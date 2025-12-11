using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using TheMacedonian.Models;
using TheMacedonian.Settings;

namespace TheMacedonian.Behaviors
{
    /// <summary>
    /// Core behavior tracking all intrigue state: conspiracy, suspicion, legitimacy, Right Hand status.
    /// Full implementation with game menus, assassination execution, and post-coronation events.
    /// </summary>
    public class MacedonianBehavior : CampaignBehaviorBase
    {
        #region Persisted State

        // Per-clan intrigue data (conspiracy status, rivalry, leak risk)
        private Dictionary<string, ClanIntrigueData> _clanData = new Dictionary<string, ClanIntrigueData>();

        // Player suspicion levels
        private SuspicionData _suspicion = new SuspicionData();

        // Assassination cooldowns per hero ID
        private Dictionary<string, AssassinationCooldown> _cooldowns = new Dictionary<string, AssassinationCooldown>();

        // Right Hand status
        [SaveableField(1)]
        private bool _isRightHand = false;

        [SaveableField(2)]
        private float _rightHandAppointedDay = 0f;

        // Legitimacy (only relevant once player becomes ruler)
        [SaveableField(3)]
        private float _legitimacy = 50f;  // 0-100

        // Battles fought in ruler's army (for loyal service tracking)
        [SaveableField(4)]
        private int _battlesFoughtWithRuler = 0;

        // Hostage tracking
        [SaveableField(5)]
        private string _hostageHeroId = null;

        // Hostage companion reference (transient)
        private Hero _hostageCompanion = null;
        private CampaignTime _hostageStartDate = CampaignTime.Zero;

        // Ruler status
        [SaveableField(6)]
        private bool _isRuler = false;

        [SaveableField(7)]
        private bool _becameRulerThroughCoup = false;

        // Active assassination plots
        private List<PlotRecord> _activePlots = new List<PlotRecord>();

        // Current intrigue target (for menu flow)
        private Hero _currentIntrigueTarget = null;
        private AssassinationMethod _selectedMethod = AssassinationMethod.None;
        private FrameTarget _selectedFrameTarget = FrameTarget.NoFrame;

        // Feast/donation tracking for legitimacy
        [SaveableField(8)]
        private int _victoriesThisWeek = 0;

        [SaveableField(9)]
        private int _defeatsThisWeek = 0;

        [SaveableField(10)]
        private bool _holdingFeast = false;

        [SaveableField(11)]
        private int _denariDonatedThisWeek = 0;

        #endregion

        #region Properties

        public bool IsRightHand => _isRightHand;
        public float Legitimacy => _legitimacy;
        public SuspicionData Suspicion => _suspicion;
        public int BattlesFoughtWithRuler => _battlesFoughtWithRuler;

        public float CoupStrength
        {
            get
            {
                var kingdom = Clan.PlayerClan?.Kingdom;
                if (kingdom == null) return 0f;

                float conspiratorsStrength = 0f;
                float totalStrength = 0f;

                foreach (var clan in kingdom.Clans)
                {
                    float clanStrength = BannerlordHelpers.GetClanStrength(clan);
                    totalStrength += clanStrength;
                    if (GetClanData(clan).Status == ConspiracyStatus.Conspirator)
                    {
                        conspiratorsStrength += clanStrength;
                    }
                }

                return totalStrength > 0 ? (conspiratorsStrength / totalStrength) * 100f : 0f;
            }
        }

        public float LeakRisk
        {
            get
            {
                var kingdom = Clan.PlayerClan?.Kingdom;
                if (kingdom == null) return 0f;

                float risk = 0f;
                foreach (var clan in kingdom.Clans)
                {
                    var data = GetClanData(clan);
                    if (data.Status == ConspiracyStatus.Conspirator)
                    {
                        risk += 5f + data.LeakRiskModifier;
                    }
                }
                return risk;
            }
        }

        #endregion

        #region CampaignBehaviorBase Implementation

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, OnWeeklyTick);
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
            CampaignEvents.RulingClanChanged.AddNonSerializedListener(this, OnRulingClanChanged);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
        }

        // Hotkey cooldown to prevent spam
        private float _lastHotkeyTime = 0f;

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("macedonian_isRightHand", ref _isRightHand);
            dataStore.SyncData("macedonian_rightHandDay", ref _rightHandAppointedDay);
            dataStore.SyncData("macedonian_legitimacy", ref _legitimacy);
            dataStore.SyncData("macedonian_battlesWithRuler", ref _battlesFoughtWithRuler);
            dataStore.SyncData("macedonian_hostageHeroId", ref _hostageHeroId);
            dataStore.SyncData("macedonian_isRuler", ref _isRuler);
            dataStore.SyncData("macedonian_coupRuler", ref _becameRulerThroughCoup);
            dataStore.SyncData("macedonian_victoriesWeek", ref _victoriesThisWeek);
            dataStore.SyncData("macedonian_defeatsWeek", ref _defeatsThisWeek);
            dataStore.SyncData("macedonian_holdingFeast", ref _holdingFeast);
            dataStore.SyncData("macedonian_donationsWeek", ref _denariDonatedThisWeek);

            // Restore hostage companion reference
            if (!string.IsNullOrEmpty(_hostageHeroId))
            {
                _hostageCompanion = Hero.FindFirst(h => h.StringId == _hostageHeroId);
            }
        }

        #endregion

        #region Event Handlers

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            AddGameMenus(starter);
            AddDialogs(starter);

            if (MCMSettings.Instance?.EnableDebugLogging ?? false)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[TheMacedonian] Session launched, intrigue system active.",
                    Colors.Green));
            }
        }

        private void OnDailyTick()
        {
            // Daily suspicion decay
            _suspicion.DecayDaily(0.5f);

            // Check if player became ruler
            CheckRulerStatus();
        }

        private void OnWeeklyTick()
        {
            // Check for conspiracy leaks
            CheckForLeaks();

            // Update rivalry scores for all lords in kingdom
            UpdateRivalryScores();

            // Legitimacy updates if player is ruler
            if (_isRuler)
            {
                UpdateLegitimacy();
                // Reset weekly counters
                _victoriesThisWeek = 0;
                _defeatsThisWeek = 0;
                _denariDonatedThisWeek = 0;
                _holdingFeast = false;
            }
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
        {
            // Check if this was our assassination target
            var activePlot = _activePlots.FirstOrDefault(p => p.Target == victim && p.Phase == PlotPhase.Execution);
            if (activePlot != null)
            {
                activePlot.Phase = PlotPhase.Completed;
                activePlot.Result = ConspiracyResult.Success;

                if (MCMSettings.Instance?.EnableDebugLogging ?? false)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        $"[DEBUG] Assassination plot against {victim.Name} completed.",
                        Colors.Gray));
                }
            }

            // Check if ruler was killed
            var kingdom = Clan.PlayerClan?.Kingdom;
            if (kingdom != null && victim == kingdom.Leader)
            {
                OnRulerDeath(victim, detail == KillCharacterAction.KillCharacterActionDetail.Murdered);
            }
        }

        private void OnRulingClanChanged(Kingdom kingdom, Clan newRulingClan)
        {
            if (kingdom == Clan.PlayerClan?.Kingdom && newRulingClan == Clan.PlayerClan)
            {
                // Player became ruler!
                bool throughCoup = CoupStrength > 30f; // If we had significant conspiracy support
                OnBecameRuler(throughCoup);
            }
        }

        private void CheckRulerStatus()
        {
            var kingdom = Clan.PlayerClan?.Kingdom;
            if (kingdom != null && !_isRuler && kingdom.RulingClan == Clan.PlayerClan)
            {
                OnBecameRuler(_becameRulerThroughCoup);
            }
        }

        private void OnMapEventEnded(MapEvent mapEvent)
        {
            // Only track field battles for Right Hand purposes
            if (mapEvent.EventType != MapEvent.BattleTypes.FieldBattle) return;

            var kingdom = Clan.PlayerClan?.Kingdom;
            if (kingdom?.Leader == null) return;

            // Already Right Hand? No need to track
            if (_isRightHand) return;

            // Check if both player and ruler participated on the same side
            bool playerParticipated = false;
            bool rulerParticipated = false;
            bool sameSide = false;

            foreach (var party in mapEvent.InvolvedParties)
            {
                var leader = party.LeaderHero;
                if (leader == Hero.MainHero)
                {
                    playerParticipated = true;
                }
                if (leader == kingdom.Leader)
                {
                    rulerParticipated = true;
                }
            }

            // Check if they were on the same side
            if (playerParticipated && rulerParticipated)
            {
                var playerSide = mapEvent.GetMapEventSide(mapEvent.PlayerSide);
                foreach (var party in playerSide.Parties)
                {
                    if (party.Party?.LeaderHero == kingdom.Leader)
                    {
                        sameSide = true;
                        break;
                    }
                }
            }

            if (playerParticipated && rulerParticipated && sameSide && mapEvent.WinningSide == mapEvent.PlayerSide)
            {
                OnBattleWithRuler();

                if (MCMSettings.Instance?.EnableDebugLogging ?? false)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        $"[DEBUG] Battle with ruler! Total: {_battlesFoughtWithRuler}/{RequiredBattlesWithRuler}",
                        Colors.Gray));
                }
            }
        }

        private void OnTick(float dt)
        {
            // Check for Ctrl+I hotkey press for Intrigue stats
            // Cooldown of 0.5 seconds to prevent spam
            _lastHotkeyTime += dt;
            
            if (_lastHotkeyTime >= 0.5f && IsIntrigueHotkeyPressed())
            {
                // Only show if not in a menu/conversation
                if (Campaign.Current != null && 
                    !Campaign.Current.ConversationManager.IsConversationInProgress)
                {
                    _lastHotkeyTime = 0f;
                    ShowIntrigueStatsPanel();
                }
            }
        }

        private bool IsIntrigueHotkeyPressed()
        {
            // Ctrl+I to open intrigue stats
            bool ctrlHeld = Input.IsKeyDown(TaleWorlds.InputSystem.InputKey.LeftControl) || 
                            Input.IsKeyDown(TaleWorlds.InputSystem.InputKey.RightControl);
            bool iPressed = Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.I);
            return ctrlHeld && iPressed;
        }

        /// <summary>
        /// Shows a comprehensive popup with all intrigue-related stats.
        /// Accessible via configurable hotkey on the campaign map.
        /// </summary>
        private void ShowIntrigueStatsPanel()
        {
            var kingdom = Clan.PlayerClan?.Kingdom;
            var ruler = kingdom?.Leader;

            // Build the stats text
            var sb = new System.Text.StringBuilder();

            // === HEADER ===
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine("         THE MACEDONIAN - INTRIGUE STATUS");
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine();

            // === KINGDOM STATUS ===
            if (kingdom != null)
            {
                sb.AppendLine($"Kingdom: {kingdom.Name}");
                sb.AppendLine($"Ruler: {(ruler != null ? ruler.Name.ToString() : "None")}");
                if (ruler != null && !_isRuler)
                {
                    int rulerRelation = Hero.MainHero.GetRelation(ruler);
                    sb.AppendLine($"Your Relation with Ruler: {rulerRelation}");
                }
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("You are not part of a kingdom.");
                sb.AppendLine();
            }

            // === RIGHT HAND STATUS ===
            sb.AppendLine("─── RIGHT HAND STATUS ───");
            if (_isRuler)
            {
                sb.AppendLine("You ARE the ruler!");
            }
            else if (_isRightHand)
            {
                sb.AppendLine("Status: ROYAL PROTECTOR ✓");
                float daysSinceAppointed = (float)CampaignTime.Now.ToDays - _rightHandAppointedDay;
                sb.AppendLine($"Days in Position: {(int)daysSinceAppointed}");
                if (_hostageCompanion != null)
                {
                    sb.AppendLine($"Hostage at Court: {_hostageCompanion.Name}");
                }
            }
            else
            {
                sb.AppendLine("Status: Not Yet Right Hand");
                sb.AppendLine($"Battles with Ruler: {_battlesFoughtWithRuler} / {RequiredBattlesWithRuler}");
                if (ruler != null)
                {
                    int relation = Hero.MainHero.GetRelation(ruler);
                    string relationStatus = relation >= 30 ? "✓" : $"(need 30+)";
                    sb.AppendLine($"Relation with Ruler: {relation} {relationStatus}");
                }
                string tierStatus = Clan.PlayerClan.Tier >= 3 ? "✓" : $"(need 3+)";
                sb.AppendLine($"Clan Tier: {Clan.PlayerClan.Tier} {tierStatus}");
                int companionCount = Clan.PlayerClan?.Companions?.Count ?? 0;
                string companionStatus = companionCount > 0 ? "✓" : "(need 1+)";
                sb.AppendLine($"Companions: {companionCount} {companionStatus}");
            }
            sb.AppendLine();

            // === CONSPIRACY STATUS ===
            sb.AppendLine("─── CONSPIRACY STATUS ───");
            float coupStrength = CoupStrength;
            float leakRisk = LeakRisk;
            int conspirators = 0;

            if (kingdom != null)
            {
                foreach (var clan in kingdom.Clans)
                {
                    if (GetClanData(clan).Status == ConspiracyStatus.Conspirator)
                    {
                        conspirators++;
                    }
                }
            }

            sb.AppendLine($"Conspirator Clans: {conspirators}");
            sb.AppendLine($"Coup Strength: {coupStrength:F1}%");
            
            string coupReadiness = coupStrength >= 60 ? "READY" : 
                                   coupStrength >= 40 ? "Risky" : 
                                   coupStrength >= 20 ? "Building" : "Weak";
            sb.AppendLine($"Coup Readiness: {coupReadiness}");
            sb.AppendLine($"Leak Risk: {leakRisk:F1}%");
            sb.AppendLine();

            // === SUSPICION LEVELS ===
            sb.AppendLine("─── SUSPICION LEVELS ───");
            string GetSuspicionLevel(float value) => 
                value >= 75 ? "CRITICAL" :
                value >= 50 ? "High" :
                value >= 25 ? "Moderate" : "Low";

            sb.AppendLine($"Ruler Suspicion: {_suspicion.RulerSuspicion:F0}/100 ({GetSuspicionLevel(_suspicion.RulerSuspicion)})");
            sb.AppendLine($"Court Suspicion: {_suspicion.CourtSuspicion:F0}/100 ({GetSuspicionLevel(_suspicion.CourtSuspicion)})");
            sb.AppendLine($"Kingdom Suspicion: {_suspicion.KingdomSuspicion:F0}/100 ({GetSuspicionLevel(_suspicion.KingdomSuspicion)})");
            sb.AppendLine();

            // === LEGITIMACY (if ruler) ===
            if (_isRuler)
            {
                sb.AppendLine("─── LEGITIMACY STATUS ───");
                string legStatus = _legitimacy >= 75 ? "Secure" :
                                   _legitimacy >= 50 ? "Stable" :
                                   _legitimacy >= 25 ? "Fragile" : "CRITICAL";
                sb.AppendLine($"Legitimacy: {_legitimacy:F0}/100 ({legStatus})");
                sb.AppendLine($"Came to Power via: {(_becameRulerThroughCoup ? "Coup" : "Succession")}");
                sb.AppendLine($"Victories This Week: {_victoriesThisWeek}");
                sb.AppendLine($"Defeats This Week: {_defeatsThisWeek}");
                if (_holdingFeast) sb.AppendLine("Currently Hosting Feast: Yes");
                if (_denariDonatedThisWeek > 0) sb.AppendLine($"Donations This Week: {_denariDonatedThisWeek:N0} denars");
                sb.AppendLine();
            }

            // === ACTIVE PLOTS ===
            if (_activePlots.Any())
            {
                sb.AppendLine("─── ACTIVE PLOTS ───");
                foreach (var plot in _activePlots.Where(p => p.Phase != PlotPhase.Completed))
                {
                    sb.AppendLine($"• Target: {(plot.Target != null ? plot.Target.Name.ToString() : "Unknown")}");
                    sb.AppendLine($"  Method: {plot.Method}, Phase: {plot.Phase}");
                }
                sb.AppendLine();
            }

            // === RIVAL INFO ===
            if (kingdom != null && !_isRuler)
            {
                var rival = IdentifyGreatestRival(kingdom);
                if (rival != null)
                {
                    var rivalData = GetClanData(rival.Clan);
                    sb.AppendLine("─── GREATEST RIVAL ───");
                    sb.AppendLine($"Name: {rival.Name}");
                    sb.AppendLine($"Clan: {rival.Clan?.Name}");
                    sb.AppendLine($"Rivalry Score: {rivalData.RivalryScore:F0}");
                    int rivalRelation = ruler != null ? rival.GetRelation(ruler) : 0;
                    sb.AppendLine($"Their Relation with Ruler: {rivalRelation}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine("Press 'Ctrl+I' anytime on the map to view this panel.");

            // Show as an inquiry popup
            InformationManager.ShowInquiry(
                new InquiryData(
                    "Intrigue Status",
                    sb.ToString(),
                    true,
                    false,
                    "Close",
                    null,
                    null,
                    null),
                true);
        }

        private Hero IdentifyGreatestRival(Kingdom kingdom)
        {
            if (kingdom == null) return null;

            Hero greatestRival = null;
            float highestRivalry = 0f;

            foreach (var clan in kingdom.Clans)
            {
                if (clan == Clan.PlayerClan || clan.Leader == null) continue;
                if (clan.Leader == kingdom.Leader) continue; // Ruler is target, not rival

                var data = GetClanData(clan);
                if (data.RivalryScore > highestRivalry)
                {
                    highestRivalry = data.RivalryScore;
                    greatestRival = clan.Leader;
                }
            }

            return greatestRival;
        }

        #endregion

        #region Clan Data Management

        public ClanIntrigueData GetClanData(Clan clan)
        {
            if (clan == null) return new ClanIntrigueData();

            string key = clan.StringId;
            if (!_clanData.TryGetValue(key, out var data))
            {
                data = new ClanIntrigueData();
                _clanData[key] = data;

                // Initialize leak risk based on leader traits
                if (clan.Leader != null)
                {
                    data.LeakRiskModifier = CalculateLeakRiskModifier(clan);
                }
            }
            return data;
        }

        public void SetClanConspiracyStatus(Clan clan, ConspiracyStatus status)
        {
            if (clan == null) return;
            var data = GetClanData(clan);
            data.Status = status;
            data.HasBeenApproached = true;
        }

        private float CalculateLeakRiskModifier(Clan clan)
        {
            if (clan?.Leader == null) return 0f;

            float modifier = 0f;
            var traits = clan.Leader.GetHeroTraits();

            // Paranoid/calculating lords are less likely to leak
            if (traits.Calculating > 0) modifier -= traits.Calculating * 2f;
            // Honest lords might slip up
            if (traits.Honor > 0) modifier += traits.Honor * 1.5f;
            // Naive lords are risky
            if (traits.Calculating < 0) modifier += 3f;

            return modifier;
        }

        #endregion

        #region Rivalry Calculations

        private void UpdateRivalryScores()
        {
            var kingdom = Clan.PlayerClan?.Kingdom;
            if (kingdom == null) return;

            foreach (var clan in kingdom.Clans)
            {
                if (clan == Clan.PlayerClan) continue;

                var data = GetClanData(clan);
                data.RivalryScore = CalculateRivalryScore(clan.Leader);
            }
        }

        public float CalculateRivalryScore(Hero hero)
        {
            if (hero == null || hero == Hero.MainHero) return 0f;

            var kingdom = Clan.PlayerClan?.Kingdom;
            if (kingdom == null) return 0f;

            float score = 0f;
            var ruler = kingdom.Leader;

            // Political power (clan tier and influence)
            if (hero.Clan != null)
            {
                score += hero.Clan.Tier * 10f;
                score += Math.Min(hero.Clan.Influence / 100f, 50f);
            }

            // Relation with ruler
            if (ruler != null)
            {
                int rulerRelation = hero.GetRelation(ruler);
                score += rulerRelation / 2f;
            }

            // Relation with player
            int playerRelation = hero.GetRelation(Hero.MainHero);
            score -= playerRelation / 3f;

            // Military strength
            if (hero.Clan != null && kingdom.Clans.Count > 0)
            {
                float avgStrength = BannerlordHelpers.GetAverageKingdomClanStrength(kingdom);
                float clanStrength = BannerlordHelpers.GetClanStrength(hero.Clan);
                if (avgStrength > 0)
                {
                    float relativeStrength = (clanStrength / avgStrength) * 20f;
                    score += relativeStrength;
                }
            }

            // Ambition trait
            var traits = hero.GetHeroTraits();
            if (traits.Calculating > 0)
                score += 10f * traits.Calculating;

            return Math.Max(score, 0f);
        }

        public Hero GetTopRival()
        {
            var kingdom = Clan.PlayerClan?.Kingdom;
            if (kingdom == null) return null;

            Hero topRival = null;
            float topScore = 0f;

            foreach (var clan in kingdom.Clans)
            {
                if (clan == Clan.PlayerClan || clan.Leader == null) continue;

                var data = GetClanData(clan);
                if (data.RivalryScore > topScore)
                {
                    topScore = data.RivalryScore;
                    topRival = clan.Leader;
                }
            }

            return topRival;
        }

        #endregion

        #region Conspiracy Management

        private void CheckForLeaks()
        {
            float leakRisk = LeakRisk;
            if (leakRisk <= 0) return;

            float roll = MBRandom.RandomFloat * 100f;
            if (roll < leakRisk / 10f) // Weekly check
            {
                // Leak occurred!
                var severity = UsurpationModel.CalculateLeakSeverity(_suspicion.RulerSuspicion);
                HandleLeak(severity);
            }
        }

        private void HandleLeak(LeakSeverity severity)
        {
            float suspicionIncrease = severity switch
            {
                LeakSeverity.Rumor => 5f,
                LeakSeverity.Suspicion => 15f,
                LeakSeverity.Evidence => 30f,
                LeakSeverity.FullExposure => 50f,
                _ => 10f
            };

            _suspicion.RulerSuspicion += suspicionIncrease;
            _suspicion.CourtSuspicion += suspicionIncrease * 0.7f;

            string message = severity switch
            {
                LeakSeverity.Rumor => "Whispers about your ambitions have begun to circulate...",
                LeakSeverity.Suspicion => "The ruler seems more suspicious of you lately...",
                LeakSeverity.Evidence => "Someone has found evidence of your scheming!",
                LeakSeverity.FullExposure => "Your conspiracy has been exposed!",
                _ => "Something has leaked..."
            };

            if (MCMSettings.Instance?.ShowLeakWarnings ?? true)
            {
                InformationManager.DisplayMessage(new InformationMessage(message, Colors.Red));
            }
        }

        #endregion

        #region Legitimacy Management

        private void UpdateLegitimacy()
        {
            // Use the calculation model for weekly changes
            float change = UsurpationModel.CalculateWeeklyLegitimacyChange(
                _legitimacy,
                _victoriesThisWeek,
                _defeatsThisWeek,
                _holdingFeast,
                _denariDonatedThisWeek);

            _legitimacy += change;
            _legitimacy = BannerlordHelpers.Clamp(_legitimacy, 0f, 100f);

            // Notify player of legitimacy state
            if (MCMSettings.Instance?.ShowLegitimacyUpdates ?? true)
            {
                string state = UsurpationModel.GetLegitimacyState(_legitimacy);
                if (_legitimacy < 25f)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        $"Your rule is in crisis! Legitimacy: {_legitimacy:F0}% ({state})",
                        Colors.Red));
                }
                else if (_legitimacy < 50f && change < 0)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        $"Your legitimacy is wavering. {_legitimacy:F0}% ({state})",
                        Colors.Yellow));
                }
            }

            // Check for counter-coup if legitimacy is very low
            if (_legitimacy < 20f && (MCMSettings.Instance?.EnableCounterCoups ?? true))
            {
                CheckForCounterCoup();
            }
        }

        private void CheckForCounterCoup()
        {
            // Calculate chance of counter-coup based on low legitimacy
            float counterCoupChance = (20f - _legitimacy) / 2f; // Max 10% at 0 legitimacy

            if (MBRandom.RandomFloat * 100f < counterCoupChance)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "Discontent with your rule is reaching dangerous levels. A faction plots against you!",
                    Colors.Red));

                // Could trigger a rebellion event here
            }
        }

        /// <summary>
        /// Called when player wins a battle while ruler.
        /// </summary>
        public void OnBattleVictory()
        {
            if (_isRuler)
            {
                _victoriesThisWeek++;
            }
        }

        /// <summary>
        /// Called when player loses a battle while ruler.
        /// </summary>
        public void OnBattleDefeat()
        {
            if (_isRuler)
            {
                _defeatsThisWeek++;
            }
        }

        #endregion

        #region Game Menus

        private void AddGameMenus(CampaignGameStarter starter)
        {
            // Main intrigue menu - using simpler overload
            starter.AddGameMenu(
                "macedonian_intrigue_menu",
                "{=MACED_INTRIGUE_MENU}The shadows hold many secrets. What schemes shall you pursue?",
                (MenuCallbackArgs args) => { InitializeIntrigueMenu(args); });

            // Add intrigue option to town menu
            starter.AddGameMenuOption(
                "town",
                "macedonian_intrigue_option",
                "{=MACED_INTRIGUE_OPT}Pursue dark intrigues...",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return CanAccessIntrigueMenu();
                },
                (MenuCallbackArgs args) =>
                {
                    GameMenu.SwitchToMenu("macedonian_intrigue_menu");
                },
                false,
                -1,
                false);

            // Add intrigue option to castle menu
            starter.AddGameMenuOption(
                "castle",
                "macedonian_intrigue_option_castle",
                "{=MACED_INTRIGUE_OPT}Pursue dark intrigues...",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return CanAccessIntrigueMenu();
                },
                (MenuCallbackArgs args) =>
                {
                    GameMenu.SwitchToMenu("macedonian_intrigue_menu");
                },
                false,
                -1,
                false);

            // Option: Plan assassination
            starter.AddGameMenuOption(
                "macedonian_intrigue_menu",
                "macedonian_plan_assassination",
                "{=MACED_PLAN_ASSASS}Plan an assassination...",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
                    return CanPlanAssassination();
                },
                (MenuCallbackArgs args) =>
                {
                    SelectAssassinationTarget();
                },
                false,
                0,
                false);

            // Option: View conspiracy status
            starter.AddGameMenuOption(
                "macedonian_intrigue_menu",
                "macedonian_view_conspiracy",
                "{=MACED_VIEW_CONSPIR}Review your conspiracy ({COUP_STRENGTH}% strength)",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    MBTextManager.SetTextVariable("COUP_STRENGTH", ((int)CoupStrength).ToString());
                    return Clan.PlayerClan?.Kingdom != null;
                },
                (MenuCallbackArgs args) =>
                {
                    ShowConspiracyStatus();
                },
                false,
                1,
                false);

            // Option: Check suspicion levels
            starter.AddGameMenuOption(
                "macedonian_intrigue_menu",
                "macedonian_check_suspicion",
                "{=MACED_CHECK_SUSP}Gauge suspicions about you ({SUSPICION_LEVEL})",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    MBTextManager.SetTextVariable("SUSPICION_LEVEL", _suspicion.GetRulerLevel().ToString());
                    return true;
                },
                (MenuCallbackArgs args) =>
                {
                    ShowSuspicionStatus();
                },
                false,
                2,
                false);

            // Option: Identify top rival
            starter.AddGameMenuOption(
                "macedonian_intrigue_menu",
                "macedonian_identify_rival",
                "{=MACED_RIVAL}Identify your greatest rival",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return Clan.PlayerClan?.Kingdom != null;
                },
                (MenuCallbackArgs args) =>
                {
                    IdentifyAndShowRival();
                },
                false,
                3,
                false);

            // Option: Host a feast (if ruler)
            starter.AddGameMenuOption(
                "macedonian_intrigue_menu",
                "macedonian_host_feast",
                "{=MACED_FEAST}Host a grand feast (10000 denars)",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                    return _isRuler && Hero.MainHero.Gold >= 10000 && !_holdingFeast;
                },
                (MenuCallbackArgs args) =>
                {
                    HostFeast();
                },
                false,
                4,
                false);

            // Option: Make donation (if ruler)
            starter.AddGameMenuOption(
                "macedonian_intrigue_menu",
                "macedonian_donation",
                "{=MACED_DONATE}Donate to the people (5000 denars)",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                    return _isRuler && Hero.MainHero.Gold >= 5000;
                },
                (MenuCallbackArgs args) =>
                {
                    MakeDonation(5000);
                },
                false,
                5,
                false);

            // Option: Leave menu
            starter.AddGameMenuOption(
                "macedonian_intrigue_menu",
                "macedonian_leave",
                "{=MACED_LEAVE}Leave",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                (MenuCallbackArgs args) =>
                {
                    GameMenu.ExitToLast();
                },
                true,
                99,
                false);

            // Assassination method selection menu
            AddAssassinationMethodMenu(starter);

            // Assassination confirmation menu
            AddAssassinationConfirmMenu(starter);
        }

        private void AddAssassinationMethodMenu(CampaignGameStarter starter)
        {
            starter.AddGameMenu(
                "macedonian_assassination_method",
                "{=MACED_ASSASS_METHOD}Choose your method to eliminate {TARGET_NAME}. Each method has different risks and chances of success.",
                (MenuCallbackArgs args) =>
                {
                    if (_currentIntrigueTarget != null)
                    {
                        MBTextManager.SetTextVariable("TARGET_NAME", _currentIntrigueTarget.Name);
                    }
                });

            // Poison option
            starter.AddGameMenuOption(
                "macedonian_assassination_method",
                "macedonian_method_poison",
                "{=MACED_POISON}Poison ({POISON_CHANCE}% success, {POISON_DETECT}% detection)",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
                    if (_currentIntrigueTarget == null) return false;
                    float chance = UsurpationModel.CalculateAssassinationChance(_currentIntrigueTarget, AssassinationMethod.Poison);
                    float detect = UsurpationModel.CalculateDetectionChance(AssassinationMethod.Poison, true);
                    MBTextManager.SetTextVariable("POISON_CHANCE", ((int)chance).ToString());
                    MBTextManager.SetTextVariable("POISON_DETECT", ((int)detect).ToString());
                    return MCMSettings.Instance?.EnablePoisonMethod ?? true;
                },
                (MenuCallbackArgs args) =>
                {
                    _selectedMethod = AssassinationMethod.Poison;
                    GameMenu.SwitchToMenu("macedonian_assassination_confirm");
                },
                false,
                0,
                false);

            // Hunting accident option
            starter.AddGameMenuOption(
                "macedonian_assassination_method",
                "macedonian_method_hunting",
                "{=MACED_HUNTING}Hunting accident ({HUNTING_CHANCE}% success, {HUNTING_DETECT}% detection)",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
                    if (_currentIntrigueTarget == null) return false;
                    float chance = UsurpationModel.CalculateAssassinationChance(_currentIntrigueTarget, AssassinationMethod.HuntingAccident, false, true);
                    float detect = UsurpationModel.CalculateDetectionChance(AssassinationMethod.HuntingAccident, true);
                    MBTextManager.SetTextVariable("HUNTING_CHANCE", ((int)chance).ToString());
                    MBTextManager.SetTextVariable("HUNTING_DETECT", ((int)detect).ToString());
                    return MCMSettings.Instance?.EnableHuntingAccidentMethod ?? true;
                },
                (MenuCallbackArgs args) =>
                {
                    _selectedMethod = AssassinationMethod.HuntingAccident;
                    GameMenu.SwitchToMenu("macedonian_assassination_confirm");
                },
                false,
                1,
                false);

            // Midnight dagger option
            starter.AddGameMenuOption(
                "macedonian_assassination_method",
                "macedonian_method_dagger",
                "{=MACED_DAGGER}Midnight dagger ({DAGGER_CHANCE}% success, {DAGGER_DETECT}% detection)",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
                    if (_currentIntrigueTarget == null) return false;
                    bool hasGuards = _currentIntrigueTarget.IsKingdomLeader;
                    float chance = UsurpationModel.CalculateAssassinationChance(_currentIntrigueTarget, AssassinationMethod.MidnightDagger, false, false, hasGuards);
                    float detect = UsurpationModel.CalculateDetectionChance(AssassinationMethod.MidnightDagger, true);
                    MBTextManager.SetTextVariable("DAGGER_CHANCE", ((int)chance).ToString());
                    MBTextManager.SetTextVariable("DAGGER_DETECT", ((int)detect).ToString());
                    return MCMSettings.Instance?.EnableMidnightDaggerMethod ?? true;
                },
                (MenuCallbackArgs args) =>
                {
                    _selectedMethod = AssassinationMethod.MidnightDagger;
                    GameMenu.SwitchToMenu("macedonian_assassination_confirm");
                },
                false,
                2,
                false);

            // Go back option
            starter.AddGameMenuOption(
                "macedonian_assassination_method",
                "macedonian_method_back",
                "{=MACED_BACK}Reconsider...",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                (MenuCallbackArgs args) =>
                {
                    _currentIntrigueTarget = null;
                    _selectedMethod = AssassinationMethod.None;
                    GameMenu.SwitchToMenu("macedonian_intrigue_menu");
                },
                true,
                99,
                false);
        }

        private void AddAssassinationConfirmMenu(CampaignGameStarter starter)
        {
            starter.AddGameMenu(
                "macedonian_assassination_confirm",
                "{=MACED_CONFIRM}You are about to attempt to assassinate {TARGET_NAME} using {METHOD_NAME}.\n\nSuccess chance: {SUCCESS_CHANCE}%\nDetection risk: {DETECT_CHANCE}%\n\nAre you certain?",
                (MenuCallbackArgs args) =>
                {
                    if (_currentIntrigueTarget != null)
                    {
                        MBTextManager.SetTextVariable("TARGET_NAME", _currentIntrigueTarget.Name);
                        MBTextManager.SetTextVariable("METHOD_NAME", GetMethodDisplayName(_selectedMethod));
                        bool hasGuards = _currentIntrigueTarget.IsKingdomLeader;
                        float chance = UsurpationModel.CalculateAssassinationChance(_currentIntrigueTarget, _selectedMethod, false, true, hasGuards);
                        float detect = UsurpationModel.CalculateDetectionChance(_selectedMethod, true);
                        MBTextManager.SetTextVariable("SUCCESS_CHANCE", ((int)chance).ToString());
                        MBTextManager.SetTextVariable("DETECT_CHANCE", ((int)detect).ToString());
                    }
                });

            // Confirm with framing
            starter.AddGameMenuOption(
                "macedonian_assassination_confirm",
                "macedonian_confirm_frame",
                "{=MACED_FRAME}Proceed and frame an enemy kingdom",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
                    return MCMSettings.Instance?.EnableFraming ?? true;
                },
                (MenuCallbackArgs args) =>
                {
                    _selectedFrameTarget = FrameTarget.EnemyKingdom;
                    ExecuteAssassination();
                },
                false,
                0,
                false);

            // Confirm without framing
            starter.AddGameMenuOption(
                "macedonian_assassination_confirm",
                "macedonian_confirm_no_frame",
                "{=MACED_NO_FRAME}Proceed without framing",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
                    return true;
                },
                (MenuCallbackArgs args) =>
                {
                    _selectedFrameTarget = FrameTarget.NoFrame;
                    ExecuteAssassination();
                },
                false,
                1,
                false);

            // Cancel
            starter.AddGameMenuOption(
                "macedonian_assassination_confirm",
                "macedonian_confirm_cancel",
                "{=MACED_CANCEL}Cancel the attempt",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                (MenuCallbackArgs args) =>
                {
                    _currentIntrigueTarget = null;
                    _selectedMethod = AssassinationMethod.None;
                    GameMenu.SwitchToMenu("macedonian_intrigue_menu");
                },
                true,
                99,
                false);
        }

        private bool CanAccessIntrigueMenu()
        {
            // Must be in a kingdom
            if (Clan.PlayerClan?.Kingdom == null) return false;
            // Must have intrigue system enabled
            if (!(MCMSettings.Instance?.EnableIntrigueSystem ?? true)) return false;
            return true;
        }

        private bool CanPlanAssassination()
        {
            var kingdom = Clan.PlayerClan?.Kingdom;
            if (kingdom == null) return false;
            // Can't assassinate if you're the ruler (use other means)
            if (kingdom.RulingClan == Clan.PlayerClan) return false;
            return true;
        }

        private void InitializeIntrigueMenu(MenuCallbackArgs args)
        {
            // Any setup needed when entering the intrigue menu
        }

        private void SelectAssassinationTarget()
        {
            var kingdom = Clan.PlayerClan?.Kingdom;
            if (kingdom == null) return;

            // Default target is the ruler
            _currentIntrigueTarget = kingdom.Leader;

            // Could expand this to show a selection of targets
            if (_currentIntrigueTarget != null)
            {
                GameMenu.SwitchToMenu("macedonian_assassination_method");
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "No suitable target found.",
                    Colors.Red));
                GameMenu.SwitchToMenu("macedonian_intrigue_menu");
            }
        }

        private string GetMethodDisplayName(AssassinationMethod method)
        {
            return method switch
            {
                AssassinationMethod.Poison => "Poison",
                AssassinationMethod.HuntingAccident => "Hunting Accident",
                AssassinationMethod.MidnightDagger => "Midnight Dagger",
                AssassinationMethod.DuelProvocation => "Provoked Duel",
                AssassinationMethod.PraetorianCoup => "Praetorian Coup",
                _ => "Unknown"
            };
        }

        #endregion

        #region Assassination Execution

        private void ExecuteAssassination()
        {
            if (_currentIntrigueTarget == null || _selectedMethod == AssassinationMethod.None)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "Assassination failed: Invalid target or method.",
                    Colors.Red));
                GameMenu.SwitchToMenu("macedonian_intrigue_menu");
                return;
            }

            Hero target = _currentIntrigueTarget;
            AssassinationMethod method = _selectedMethod;
            FrameTarget frameTarget = _selectedFrameTarget;

            // Check cooldown
            string targetId = target.StringId;
            if (_cooldowns.TryGetValue(targetId, out var cooldown))
            {
                float currentDay = (float)CampaignTime.Now.ToDays;
                float cooldownDays = MCMSettings.Instance?.AssassinationCooldownDays ?? 90f;
                if (cooldown.IsOnCooldown(currentDay, cooldownDays))
                {
                    float remaining = cooldown.GetCooldownRemaining(currentDay, cooldownDays);
                    InformationManager.DisplayMessage(new InformationMessage(
                        $"You must wait {(int)remaining} more days before attempting this again.",
                        Colors.Red));
                    GameMenu.SwitchToMenu("macedonian_intrigue_menu");
                    return;
                }
            }

            // Calculate success chance
            bool hasGuards = target.IsKingdomLeader;
            bool huntsRegularly = true; // Simplified - could check traits
            float successChance = UsurpationModel.CalculateAssassinationChance(target, method, false, huntsRegularly, hasGuards);

            // Apply difficulty modifier from settings
            float modifier = MCMSettings.Instance?.AssassinationSuccessModifier ?? 1.0f;
            successChance *= modifier;
            successChance = BannerlordHelpers.Clamp(successChance, 5f, 95f);

            // Roll for success
            float roll = MBRandom.RandomFloat * 100f;
            bool success = roll < successChance;

            // Create plot record
            var plot = new PlotRecord
            {
                Target = target,
                Method = method,
                Phase = PlotPhase.Execution,
                StartDate = CampaignTime.Now
            };
            _activePlots.Add(plot);

            // Update cooldown
            if (!_cooldowns.ContainsKey(targetId))
            {
                _cooldowns[targetId] = new AssassinationCooldown();
            }
            _cooldowns[targetId].LastAttemptDay = (float)CampaignTime.Now.ToDays;

            if (success)
            {
                HandleAssassinationSuccess(target, method, frameTarget);
            }
            else
            {
                _cooldowns[targetId].FailureCount++;
                HandleAssassinationFailure(target, method);
            }

            // Clear state
            _currentIntrigueTarget = null;
            _selectedMethod = AssassinationMethod.None;
            _selectedFrameTarget = FrameTarget.NoFrame;

            GameMenu.ExitToLast();
        }

        private void HandleAssassinationSuccess(Hero target, AssassinationMethod method, FrameTarget frameTarget)
        {
            // Kill the target
            KillCharacterAction.ApplyByMurder(target, Hero.MainHero, false);

            // Update plot record
            var plot = _activePlots.LastOrDefault(p => p.Target == target);
            if (plot != null)
            {
                plot.Phase = PlotPhase.Completed;
                plot.Result = ConspiracyResult.Success;
            }

            string methodMessage = method switch
            {
                AssassinationMethod.Poison => $"{target.Name} has died suddenly after falling ill...",
                AssassinationMethod.HuntingAccident => $"{target.Name} suffered a fatal accident during a hunt.",
                AssassinationMethod.MidnightDagger => $"{target.Name} was found dead in their chambers.",
                AssassinationMethod.DuelProvocation => $"{target.Name} was slain in a duel.",
                _ => $"{target.Name} has met an untimely end."
            };

            InformationManager.DisplayMessage(new InformationMessage(methodMessage, Colors.Magenta));

            // Handle detection
            float detectionChance = UsurpationModel.CalculateDetectionChance(method, true);
            float detectionModifier = MCMSettings.Instance?.DetectionChanceModifier ?? 1.0f;
            detectionChance *= detectionModifier;

            float detectionRoll = MBRandom.RandomFloat * 100f;
            bool detected = detectionRoll < detectionChance;

            if (detected)
            {
                // Handle framing attempt
                if (frameTarget != FrameTarget.NoFrame && frameTarget != FrameTarget.None)
                {
                    float frameChance = UsurpationModel.CalculateFrameSuccessChance(frameTarget);
                    float frameRoll = MBRandom.RandomFloat * 100f;

                    if (frameRoll < frameChance)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            "Evidence points to an enemy faction. Your involvement remains hidden.",
                            Colors.Green));
                        return;
                    }
                    else
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            "Your attempt to frame another party has failed!",
                            Colors.Red));
                    }
                }

                // Suspicion increase
                float suspicionIncrease = UsurpationModel.CalculateSuspicionIncrease(method, true, false, _suspicion.RulerSuspicion);
                _suspicion.RulerSuspicion += suspicionIncrease;
                _suspicion.CourtSuspicion += suspicionIncrease * 0.7f;
                _suspicion.KingdomSuspicion += suspicionIncrease * 0.3f;

                InformationManager.DisplayMessage(new InformationMessage(
                    "Rumors have begun to circulate about your involvement...",
                    Colors.Yellow));
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "No evidence connects you to the deed.",
                    Colors.Green));
            }

            // Log for debug
            if (MCMSettings.Instance?.EnableDebugLogging ?? false)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    $"[DEBUG] Assassination success. Detection roll: {detectionRoll:F1}/{detectionChance:F1}",
                    Colors.Gray));
            }
        }

        private void HandleAssassinationFailure(Hero target, AssassinationMethod method)
        {
            // Update plot record
            var plot = _activePlots.LastOrDefault(p => p.Target == target);
            if (plot != null)
            {
                plot.Phase = PlotPhase.Completed;
                plot.Result = ConspiracyResult.Failure;
            }

            string failureMessage = method switch
            {
                AssassinationMethod.Poison => "The poison was detected before it could take effect!",
                AssassinationMethod.HuntingAccident => "The hunting accident was foiled - the target escaped unharmed!",
                AssassinationMethod.MidnightDagger => "Your assassin was discovered and had to flee!",
                AssassinationMethod.DuelProvocation => "You failed to provoke a duel.",
                _ => "The assassination attempt has failed!"
            };

            InformationManager.DisplayMessage(new InformationMessage(failureMessage, Colors.Red));

            // Failed attempts have higher detection chance
            float detectionChance = UsurpationModel.CalculateDetectionChance(method, false);
            float detectionModifier = MCMSettings.Instance?.DetectionChanceModifier ?? 1.0f;
            detectionChance *= detectionModifier;

            float detectionRoll = MBRandom.RandomFloat * 100f;
            bool detected = detectionRoll < detectionChance;

            if (detected)
            {
                // Major suspicion increase on failed detection
                float suspicionIncrease = UsurpationModel.CalculateSuspicionIncrease(method, true, false, _suspicion.RulerSuspicion);
                suspicionIncrease *= 1.5f; // Penalty for failure
                _suspicion.RulerSuspicion += suspicionIncrease;
                _suspicion.CourtSuspicion += suspicionIncrease * 0.8f;
                _suspicion.KingdomSuspicion += suspicionIncrease * 0.5f;

                InformationManager.DisplayMessage(new InformationMessage(
                    "You have been identified as a suspect in the assassination attempt!",
                    Colors.Red));

                // Target becomes more paranoid
                if (_cooldowns.TryGetValue(target.StringId, out var cooldown))
                {
                    cooldown.TargetAwareness += 20f;
                }

                // Relation penalty with target
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.MainHero, target, -30, true);
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "Fortunately, you have not been connected to the attempt.",
                    Colors.Yellow));
            }

            // Debug logging
            if (MCMSettings.Instance?.EnableDebugLogging ?? false)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    $"[DEBUG] Assassination failed. Detection roll: {detectionRoll:F1}/{detectionChance:F1}",
                    Colors.Gray));
            }
        }

        #endregion

        #region Status Display Methods

        private void ShowConspiracyStatus()
        {
            var kingdom = Clan.PlayerClan?.Kingdom;
            if (kingdom == null) return;

            int conspirators = 0;
            int total = 0;
            float conspiratorsStrength = 0f;
            float totalStrength = 0f;

            foreach (var clan in kingdom.Clans)
            {
                if (clan == Clan.PlayerClan) continue;
                total++;
                float clanStrength = BannerlordHelpers.GetClanStrength(clan);
                totalStrength += clanStrength;

                var data = GetClanData(clan);
                if (data.Status == ConspiracyStatus.Conspirator)
                {
                    conspirators++;
                    conspiratorsStrength += clanStrength;
                }
            }

            float coupStrength = totalStrength > 0 ? (conspiratorsStrength / totalStrength) * 100f : 0f;

            InformationManager.DisplayMessage(new InformationMessage(
                $"Conspiracy Status: {conspirators}/{total} clans joined",
                Colors.Cyan));
            InformationManager.DisplayMessage(new InformationMessage(
                $"Coup Strength: {coupStrength:F1}% of kingdom military power",
                Colors.Cyan));
            InformationManager.DisplayMessage(new InformationMessage(
                $"Leak Risk: {LeakRisk:F1}% weekly chance",
                Colors.Yellow));

            GameMenu.SwitchToMenu("macedonian_intrigue_menu");
        }

        private void ShowSuspicionStatus()
        {
            InformationManager.DisplayMessage(new InformationMessage(
                $"Ruler Suspicion: {_suspicion.RulerSuspicion:F0} ({_suspicion.GetRulerLevel()})",
                _suspicion.GetRulerLevel() >= SuspicionLevel.High ? Colors.Red : Colors.Yellow));
            InformationManager.DisplayMessage(new InformationMessage(
                $"Court Suspicion: {_suspicion.CourtSuspicion:F0} ({_suspicion.GetCourtLevel()})",
                Colors.Yellow));
            InformationManager.DisplayMessage(new InformationMessage(
                $"Kingdom Rumors: {_suspicion.KingdomSuspicion:F0} ({_suspicion.GetKingdomLevel()})",
                Colors.Yellow));

            GameMenu.SwitchToMenu("macedonian_intrigue_menu");
        }

        private void IdentifyAndShowRival()
        {
            var topRival = GetTopRival();
            if (topRival != null)
            {
                var rivalData = GetClanData(topRival.Clan);
                InformationManager.DisplayMessage(new InformationMessage(
                    $"Your greatest rival is {topRival.Name} of {topRival.Clan?.Name}",
                    Colors.Magenta));
                InformationManager.DisplayMessage(new InformationMessage(
                    $"Rivalry Score: {rivalData.RivalryScore:F0}",
                    Colors.Magenta));

                int rulerRelation = 0;
                var kingdom = Clan.PlayerClan?.Kingdom;
                if (kingdom?.Leader != null)
                {
                    rulerRelation = topRival.GetRelation(kingdom.Leader);
                }
                InformationManager.DisplayMessage(new InformationMessage(
                    $"Ruler's favor: {rulerRelation}",
                    rulerRelation > 0 ? Colors.Red : Colors.Green));
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "You have no significant rivals at present.",
                    Colors.Green));
            }

            GameMenu.SwitchToMenu("macedonian_intrigue_menu");
        }

        #endregion

        #region Post-Coronation Actions

        private void HostFeast()
        {
            if (Hero.MainHero.Gold < 10000)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "You cannot afford to host a feast.",
                    Colors.Red));
                return;
            }

            Hero.MainHero.ChangeHeroGold(-10000);
            _holdingFeast = true;

            InformationManager.DisplayMessage(new InformationMessage(
                "You host a grand feast! Your legitimacy grows as lords enjoy your hospitality.",
                Colors.Green));

            // Immediate small legitimacy boost
            _legitimacy += 2f;
            _legitimacy = BannerlordHelpers.Clamp(_legitimacy, 0f, 100f);

            // Small relation boost with kingdom lords
            var kingdom = Clan.PlayerClan?.Kingdom;
            if (kingdom != null)
            {
                foreach (var clan in kingdom.Clans)
                {
                    if (clan != Clan.PlayerClan && clan.Leader != null)
                    {
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.MainHero, clan.Leader, 2, false);
                    }
                }
            }

            GameMenu.SwitchToMenu("macedonian_intrigue_menu");
        }

        private void MakeDonation(int amount)
        {
            if (Hero.MainHero.Gold < amount)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "You cannot afford this donation.",
                    Colors.Red));
                return;
            }

            Hero.MainHero.ChangeHeroGold(-amount);
            _denariDonatedThisWeek += amount;

            InformationManager.DisplayMessage(new InformationMessage(
                $"You donate {amount} denars to the people. Your reputation as a generous ruler grows.",
                Colors.Green));

            // Immediate small legitimacy boost
            float boost = Math.Min(amount / 5000f, 3f);
            _legitimacy += boost;
            _legitimacy = BannerlordHelpers.Clamp(_legitimacy, 0f, 100f);

            GameMenu.SwitchToMenu("macedonian_intrigue_menu");
        }

        #endregion

        #region Dialogs

        private void AddDialogs(CampaignGameStarter starter)
        {
            // ========== RIGHT HAND DIALOGS (with Ruler) ==========
            
            // Player initiates Right Hand petition with the ruler
            starter.AddPlayerLine(
                "macedonian_righthand_petition_start",
                "lord_talk_speak_diplomacy_2",
                "macedonian_righthand_ruler_response",
                "{=MACED_RH_PETITION}My liege, I wish to serve you more closely. I would be honored to be your Royal Protector.",
                () => CanPetitionForRightHand(),
                null,
                110); // Higher priority than conspiracy

            // Ruler accepts (if requirements met)
            starter.AddDialogLine(
                "macedonian_righthand_ruler_accept",
                "macedonian_righthand_ruler_response",
                "macedonian_righthand_accept_confirm",
                "{=MACED_RH_ACCEPT}Your loyalty in battle has not gone unnoticed. I would welcome you as my Right Hand... but first, I require a token of your commitment.",
                () => CanBecomeRightHand(),
                null);

            // Ruler rejects (requirements not met)
            starter.AddDialogLine(
                "macedonian_righthand_ruler_reject_battles",
                "macedonian_righthand_ruler_response",
                "lord_pretalk",
                "{=MACED_RH_REJECT_BATTLES}You have not yet proven yourself in battle by my side. Fight alongside me more, and we shall speak again. ({BATTLES_CURRENT}/{BATTLES_REQUIRED} battles)",
                () => !HasEnoughBattlesWithRuler(),
                () => SetBattleCountVariables());

            // Ruler rejects (relation too low)
            starter.AddDialogLine(
                "macedonian_righthand_ruler_reject_relation",
                "macedonian_righthand_ruler_response",
                "lord_pretalk",
                "{=MACED_RH_REJECT_RELATION}I do not yet trust you enough for such a position. Improve our relations first.",
                () => HasEnoughBattlesWithRuler() && !HasEnoughRelationWithRuler(),
                null);

            // Player confirms acceptance (send companion as hostage)
            starter.AddPlayerLine(
                "macedonian_righthand_accept_yes",
                "macedonian_righthand_accept_confirm",
                "macedonian_righthand_hostage_demand",
                "{=MACED_RH_ACCEPT_YES}I am honored, my liege. What token do you require?",
                null,
                null);

            // Player declines
            starter.AddPlayerLine(
                "macedonian_righthand_accept_no",
                "macedonian_righthand_accept_confirm",
                "lord_pretalk",
                "{=MACED_RH_ACCEPT_NO}Perhaps another time, my liege.",
                null,
                null);

            // Ruler demands hostage
            starter.AddDialogLine(
                "macedonian_righthand_hostage_demand",
                "macedonian_righthand_hostage_demand",
                "macedonian_righthand_hostage_response",
                "{=MACED_RH_HOSTAGE}Send one of your trusted companions to serve at my court for a time. This will cement our bond of trust.",
                null,
                null);

            // Player agrees to hostage
            starter.AddPlayerLine(
                "macedonian_righthand_hostage_yes",
                "macedonian_righthand_hostage_response",
                "macedonian_righthand_complete",
                "{=MACED_RH_HOSTAGE_YES}It shall be done, my liege. I will send someone worthy.",
                () => HasCompanionToSend(),
                () => OnBecomeRightHand());

            // Player has no companion
            starter.AddPlayerLine(
                "macedonian_righthand_hostage_none",
                "macedonian_righthand_hostage_response",
                "lord_pretalk",
                "{=MACED_RH_HOSTAGE_NONE}I... have no companions to send at this time.",
                () => !HasCompanionToSend(),
                null);

            // Player refuses hostage
            starter.AddPlayerLine(
                "macedonian_righthand_hostage_no",
                "macedonian_righthand_hostage_response",
                "lord_pretalk",
                "{=MACED_RH_HOSTAGE_NO}I cannot agree to such terms.",
                null,
                null);

            // Completion
            starter.AddDialogLine(
                "macedonian_righthand_complete",
                "macedonian_righthand_complete",
                "lord_pretalk",
                "{=MACED_RH_COMPLETE}Excellent. From this day, you are my Right Hand. Serve me well, and greater rewards shall follow.",
                null,
                null);

            // ========== CONSPIRACY DIALOGS (with Lords) ==========
            
            // Conspiracy recruitment dialog
            starter.AddDialogLine(
                "macedonian_conspiracy_start",
                "lord_talk_speak_diplomacy_2",
                "macedonian_conspiracy_player_response",
                "{=MACED_CONSPIRE_INTRO}I have been thinking about the state of our kingdom...",
                () => CanStartConspiracyDialog(),
                null,
                100);

            starter.AddPlayerLine(
                "macedonian_conspiracy_player_proposal",
                "macedonian_conspiracy_player_response",
                "macedonian_conspiracy_lord_response",
                "{=MACED_CONSPIRE_PROPOSE}Perhaps it is time for new leadership. Would you support such a change?",
                null,
                null);

            starter.AddPlayerLine(
                "macedonian_conspiracy_player_nevermind",
                "macedonian_conspiracy_player_response",
                "lord_pretalk",
                "{=MACED_NEVERMIND}Never mind, forget I said anything.",
                null,
                null);

            starter.AddDialogLine(
                "macedonian_conspiracy_lord_accept",
                "macedonian_conspiracy_lord_response",
                "lord_pretalk",
                "{=MACED_CONSPIRE_ACCEPT}You can count on my support. The current ruler has lost my confidence.",
                () => GetConversationLordDisposition() > 0,
                () => OnLordJoinsConspiracy());

            starter.AddDialogLine(
                "macedonian_conspiracy_lord_refuse",
                "macedonian_conspiracy_lord_response",
                "lord_pretalk",
                "{=MACED_CONSPIRE_REFUSE}I will not betray my liege. This conversation never happened.",
                () => GetConversationLordDisposition() <= 0,
                () => OnLordRefusesConspiracy());
        }

        private void OnLordJoinsConspiracy()
        {
            var lord = Hero.OneToOneConversationHero;
            if (lord?.Clan != null)
            {
                SetClanConspiracyStatus(lord.Clan, ConspiracyStatus.Conspirator);
                InformationManager.DisplayMessage(new InformationMessage(
                    $"{lord.Clan.Name} has joined your conspiracy!",
                    Colors.Green));
            }
        }

        private void OnLordRefusesConspiracy()
        {
            var lord = Hero.OneToOneConversationHero;
            if (lord?.Clan != null)
            {
                SetClanConspiracyStatus(lord.Clan, ConspiracyStatus.Loyalist);
                // Small suspicion increase
                _suspicion.RulerSuspicion += 5f;
            }
        }

        #region Right Hand Helper Methods

        private const int RequiredBattlesWithRuler = 3;

        private bool CanPetitionForRightHand()
        {
            // Can only petition the ruler
            var hero = Hero.OneToOneConversationHero;
            if (hero == null) return false;

            var kingdom = Clan.PlayerClan?.Kingdom;
            if (kingdom == null) return false;
            if (hero != kingdom.Leader) return false;

            // Already Right Hand?
            if (_isRightHand) return false;

            // Basic tier requirement
            return Clan.PlayerClan.Tier >= 3;
        }

        private bool CanBecomeRightHand()
        {
            return HasEnoughBattlesWithRuler() && HasEnoughRelationWithRuler();
        }

        private bool HasEnoughBattlesWithRuler()
        {
            return _battlesFoughtWithRuler >= RequiredBattlesWithRuler;
        }

        private bool HasEnoughRelationWithRuler()
        {
            var kingdom = Clan.PlayerClan?.Kingdom;
            if (kingdom?.Leader == null) return false;

            int relation = Hero.MainHero.GetRelation(kingdom.Leader);
            return relation >= 30;
        }

        private void SetBattleCountVariables()
        {
            MBTextManager.SetTextVariable("BATTLES_CURRENT", _battlesFoughtWithRuler);
            MBTextManager.SetTextVariable("BATTLES_REQUIRED", RequiredBattlesWithRuler);
        }

        private bool HasCompanionToSend()
        {
            // Check if player has any companions
            return Clan.PlayerClan?.Companions?.Any() == true;
        }

        private void OnBecomeRightHand()
        {
            _isRightHand = true;
            _rightHandAppointedDay = (float)CampaignTime.Now.ToDays;

            InformationManager.DisplayMessage(new InformationMessage(
                "You are now the Royal Protector - the Right Hand of the ruler!",
                Colors.Green));

            // Grant influence bonus
            Clan.PlayerClan.AddRenown(50, true);
            Hero.MainHero.AddSkillXp(DefaultSkills.Charm, 1000);

            // Select and send a companion as hostage
            var companion = Clan.PlayerClan.Companions?.FirstOrDefault();
            if (companion != null)
            {
                _hostageCompanion = companion;
                _hostageHeroId = companion.StringId;
                _hostageStartDate = CampaignTime.Now;
                InformationManager.DisplayMessage(new InformationMessage(
                    $"{companion.Name} has been sent to serve at the ruler's court.",
                    Colors.Cyan));
            }
        }

        /// <summary>
        /// Called from battle events to track fights alongside ruler
        /// </summary>
        public void OnBattleWithRuler()
        {
            _battlesFoughtWithRuler++;
            if (_battlesFoughtWithRuler == RequiredBattlesWithRuler)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "You have proven yourself in battle alongside your liege. You may now petition to become the Royal Protector.",
                    Colors.Green));
            }
        }

        #endregion

        private bool CanStartConspiracyDialog()
        {
            var lord = Hero.OneToOneConversationHero;
            if (lord == null) return false;

            var kingdom = Clan.PlayerClan?.Kingdom;
            if (kingdom == null) return false;

            // Must be same kingdom, not the ruler, not already a conspirator
            if (lord.Clan?.Kingdom != kingdom) return false;
            if (lord == kingdom.Leader) return false;

            var data = GetClanData(lord.Clan);
            if (data.Status == ConspiracyStatus.Conspirator) return false;

            return true;
        }

        private int GetConversationLordDisposition()
        {
            var lord = Hero.OneToOneConversationHero;
            if (lord == null) return 0;

            var kingdom = Clan.PlayerClan?.Kingdom;
            var ruler = kingdom?.Leader;
            if (ruler == null) return 0;

            int score = 0;

            // Relation with player
            int playerRelation = lord.GetRelation(Hero.MainHero);
            if (playerRelation > 50) score += 2;
            else if (playerRelation > 20) score += 1;
            else if (playerRelation < -20) score -= 2;

            // Relation with ruler
            int rulerRelation = lord.GetRelation(ruler);
            if (rulerRelation < -30) score += 2;
            else if (rulerRelation < 0) score += 1;
            else if (rulerRelation > 50) score -= 2;

            // Traits
            var traits = lord.GetHeroTraits();
            if (traits.Honor > 0) score -= traits.Honor;
            if (traits.Calculating > 0) score += 1;

            return score;
        }

        #endregion

        #region External Event Handlers

        /// <summary>
        /// Called when the ruler dies (murdered or natural).
        /// </summary>
        public void OnRulerDeath(Hero ruler, bool wasMurdered)
        {
            if (ruler == null) return;

            // Check if we had an active plot against them
            var activePlot = _activePlots.FirstOrDefault(p =>
                p.Target == ruler &&
                p.Phase != PlotPhase.Completed);

            if (activePlot != null)
            {
                activePlot.Phase = PlotPhase.Completed;
                activePlot.Result = wasMurdered ? ConspiracyResult.Success : ConspiracyResult.Abandoned;

                InformationManager.DisplayMessage(new InformationMessage(
                    wasMurdered
                        ? "Your plot has reached its conclusion..."
                        : "The ruler has died. Your plot is no longer necessary.",
                    Colors.Magenta));
            }

            if (_suspicion.RulerSuspicion > 50f && wasMurdered)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "Whispers circulate about your involvement in the ruler's death...",
                    Colors.Red));
            }
        }

        /// <summary>
        /// Called when player fights in battle alongside the ruler.
        /// </summary>
        public void OnBattleFoughtWithRuler()
        {
            _battlesFoughtWithRuler++;

            var settings = MCMSettings.Instance;
            int requiredBattles = settings?.BattlesRequiredForTrust ?? 5;

            if (_battlesFoughtWithRuler == requiredBattles)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "The ruler has come to trust you as a loyal warrior.",
                    Colors.Green));
            }
            else if (_battlesFoughtWithRuler == requiredBattles / 2)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "The ruler has noticed your valor in battle.",
                    Colors.Cyan));
            }

            if (settings?.EnableDebugLogging ?? false)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    $"[DEBUG] Battles with ruler: {_battlesFoughtWithRuler}/{requiredBattles}",
                    Colors.Gray));
            }
        }

        /// <summary>
        /// Called when a companion becomes a hostage at the ruler's court.
        /// </summary>
        public void OnCompanionBecameHostage(Hero companion)
        {
            if (companion == null || !companion.IsPlayerCompanion)
                return;

            _hostageCompanion = companion;
            _hostageHeroId = companion.StringId;
            _hostageStartDate = CampaignTime.Now;

            InformationManager.DisplayMessage(new InformationMessage(
                $"{companion.Name} has been taken to the ruler's court as a sign of your loyalty.",
                Colors.Yellow));
        }

        /// <summary>
        /// Check if the hostage requirement has been met.
        /// </summary>
        public bool IsHostageRequirementMet()
        {
            if (_hostageCompanion == null || _hostageStartDate == CampaignTime.Zero)
                return false;

            var settings = MCMSettings.Instance;
            int requiredDays = settings?.HostageDurationDays ?? 90;

            float daysElapsed = (float)(CampaignTime.Now - _hostageStartDate).ToDays;
            return daysElapsed >= requiredDays;
        }

        /// <summary>
        /// Called when player becomes ruler of the kingdom.
        /// </summary>
        public void OnBecameRuler(bool throughCoup)
        {
            _isRuler = true;
            _becameRulerThroughCoup = throughCoup;

            // Calculate initial legitimacy
            float coupStrength = CoupStrength;
            float priorSuspicion = _suspicion.RulerSuspicion;

            _legitimacy = UsurpationModel.CalculateInitialLegitimacy(
                throughCoup, coupStrength, priorSuspicion);

            // Reset suspicion (new reign)
            _suspicion = new SuspicionData();

            string message = throughCoup
                ? $"You have seized the throne! Your legitimacy: {_legitimacy:F0}%"
                : $"You have ascended to the throne. Your legitimacy: {_legitimacy:F0}%";

            InformationManager.DisplayMessage(new InformationMessage(message, Colors.Yellow));
        }

        #endregion
    }
}
