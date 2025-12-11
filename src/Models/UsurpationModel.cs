using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TheMacedonian.Behaviors;

namespace TheMacedonian.Models
{
    /// <summary>
    /// Contains all probability calculations for assassination, conspiracy, and usurpation.
    /// All values are percentages (0-100) unless otherwise noted.
    /// </summary>
    public static class UsurpationModel
    {
        #region Constants

        // Assassination base chances by method
        private const float POISON_BASE_CHANCE = 35f;
        private const float HUNTING_ACCIDENT_BASE_CHANCE = 25f;
        private const float DUEL_BASE_CHANCE = 20f;
        private const float MIDNIGHT_DAGGER_BASE_CHANCE = 40f;

        // Legitimacy thresholds
        private const float LEGITIMACY_STABLE = 75f;
        private const float LEGITIMACY_UNEASY = 50f;
        private const float LEGITIMACY_CRISIS = 25f;

        // Coup strength thresholds
        private const float COUP_STRENGTH_WEAK = 25f;
        private const float COUP_STRENGTH_MODERATE = 50f;
        private const float COUP_STRENGTH_STRONG = 75f;

        #endregion

        #region Assassination Calculations

        /// <summary>
        /// Calculate success chance for assassination based on method and circumstances.
        /// </summary>
        public static float CalculateAssassinationChance(
            Hero target,
            AssassinationMethod method,
            bool hasAlchemySkill = false,
            bool targetHuntsRegularly = false,
            bool targetHasBodyguards = false)
        {
            float baseChance = GetBaseChance(method);

            // Method-specific modifiers
            switch (method)
            {
                case AssassinationMethod.Poison:
                    baseChance = CalculatePoisonChance(target, hasAlchemySkill, targetHasBodyguards);
                    break;

                case AssassinationMethod.HuntingAccident:
                    baseChance = CalculateHuntingAccidentChance(target, targetHuntsRegularly);
                    break;

                case AssassinationMethod.DuelProvocation:
                    baseChance = CalculateDuelChance(target);
                    break;

                case AssassinationMethod.MidnightDagger:
                    baseChance = CalculateMidnightDaggerChance(target, targetHasBodyguards);
                    break;
            }

            return MathF.Clamp(baseChance, 5f, 95f);
        }

        private static float GetBaseChance(AssassinationMethod method)
        {
            return method switch
            {
                AssassinationMethod.Poison => POISON_BASE_CHANCE,
                AssassinationMethod.HuntingAccident => HUNTING_ACCIDENT_BASE_CHANCE,
                AssassinationMethod.DuelProvocation => DUEL_BASE_CHANCE,
                AssassinationMethod.MidnightDagger => MIDNIGHT_DAGGER_BASE_CHANCE,
                _ => 20f
            };
        }

        private static float CalculatePoisonChance(Hero target, bool hasAlchemySkill, bool hasBodyguards)
        {
            float chance = POISON_BASE_CHANCE;

            // Medicine skill helps create better poisons
            int medicineSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Medicine);
            chance += medicineSkill / 10f; // +0.1% per skill point

            if (hasAlchemySkill)
                chance += 15f;

            // Target's medicine skill helps detect poison
            int targetMedicine = target.GetSkillValue(DefaultSkills.Medicine);
            chance -= targetMedicine / 15f; // Target can detect

            // Bodyguards/tasters reduce chance
            if (hasBodyguards)
                chance -= 20f;

            // Charm helps get close to target
            int charm = Hero.MainHero.GetSkillValue(DefaultSkills.Charm);
            chance += charm / 20f;

            return chance;
        }

        private static float CalculateHuntingAccidentChance(Hero target, bool targetHuntsRegularly)
        {
            float chance = HUNTING_ACCIDENT_BASE_CHANCE;

            // Only works if target actually hunts
            if (!targetHuntsRegularly)
                return 0f; // Cannot use this method

            // Riding skill affects setup
            int riding = Hero.MainHero.GetSkillValue(DefaultSkills.Riding);
            chance += riding / 15f;

            // Roguery affects cover-up
            int roguery = Hero.MainHero.GetSkillValue(DefaultSkills.Roguery);
            chance += roguery / 10f;

            // Target's athletics affects survival
            int targetAthletics = target.GetSkillValue(DefaultSkills.Athletics);
            chance -= targetAthletics / 20f;

            // Target's vigor makes them harder to kill
            var targetTraits = target.GetHeroTraits();
            if (targetTraits.Valor > 0)
                chance -= 5f * targetTraits.Valor;

            return chance;
        }

        private static float CalculateDuelChance(Hero target)
        {
            float chance = DUEL_BASE_CHANCE;

            // Combat skills comparison
            int playerCombat = GetCombatScore(Hero.MainHero);
            int targetCombat = GetCombatScore(target);

            float combatDiff = (playerCombat - targetCombat) / 5f;
            chance += combatDiff;

            // Athletics affects stamina
            int playerAthletics = Hero.MainHero.GetSkillValue(DefaultSkills.Athletics);
            int targetAthletics = target.GetSkillValue(DefaultSkills.Athletics);
            chance += (playerAthletics - targetAthletics) / 10f;

            // Player's cunning trait gives dirty tricks advantage
            var playerTraits = Hero.MainHero.GetHeroTraits();
            if (playerTraits.Calculating > 0)
                chance += 10f;

            // Target's honor makes them predictable
            var targetTraits = target.GetHeroTraits();
            if (targetTraits.Honor > 0)
                chance += 5f * targetTraits.Honor;

            // But their valor makes them dangerous
            if (targetTraits.Valor > 0)
                chance -= 5f * targetTraits.Valor;

            return chance;
        }

        private static float CalculateMidnightDaggerChance(Hero target, bool hasBodyguards)
        {
            float chance = MIDNIGHT_DAGGER_BASE_CHANCE;

            // Roguery is king for stealth kills
            int roguery = Hero.MainHero.GetSkillValue(DefaultSkills.Roguery);
            chance += roguery / 8f;

            // Athletics for agility
            int athletics = Hero.MainHero.GetSkillValue(DefaultSkills.Athletics);
            chance += athletics / 15f;

            // One-handed for the kill
            int oneHanded = Hero.MainHero.GetSkillValue(DefaultSkills.OneHanded);
            chance += oneHanded / 20f;

            // Bodyguards drastically reduce chance
            if (hasBodyguards)
                chance -= 30f;

            // Target's scouting might detect approach
            int targetScouting = target.GetSkillValue(DefaultSkills.Scouting);
            chance -= targetScouting / 15f;

            // Higher suspicion means more guards
            var behavior = Campaign.Current?.GetCampaignBehavior<MacedonianBehavior>();
            if (behavior != null)
            {
                float rulerSuspicion = behavior.Suspicion.RulerSuspicion;
                chance -= rulerSuspicion / 5f; // -0.2% per suspicion point
            }

            return chance;
        }

        private static int GetCombatScore(Hero hero)
        {
            return (hero.GetSkillValue(DefaultSkills.OneHanded) +
                    hero.GetSkillValue(DefaultSkills.TwoHanded) +
                    hero.GetSkillValue(DefaultSkills.Polearm)) / 3;
        }

        #endregion

        #region Detection & Suspicion Calculations

        /// <summary>
        /// Calculate detection chance when an assassination plot is executed.
        /// </summary>
        public static float CalculateDetectionChance(AssassinationMethod method, bool succeeded)
        {
            float baseDetection = method switch
            {
                AssassinationMethod.Poison => 20f,
                AssassinationMethod.HuntingAccident => 15f,
                AssassinationMethod.DuelProvocation => 40f, // Many witnesses
                AssassinationMethod.MidnightDagger => succeeded ? 10f : 60f, // Failed = caught
                _ => 25f
            };

            // Roguery helps cover tracks
            int roguery = Hero.MainHero.GetSkillValue(DefaultSkills.Roguery);
            baseDetection -= roguery / 10f;

            // Failed attempts are more likely to be traced
            if (!succeeded)
                baseDetection += 25f;

            return MathF.Clamp(baseDetection, 5f, 90f);
        }

        /// <summary>
        /// Calculate suspicion increase based on circumstances.
        /// </summary>
        public static float CalculateSuspicionIncrease(
            AssassinationMethod method,
            bool detected,
            bool hasAlibi,
            float currentSuspicion)
        {
            if (!detected)
                return 0f;

            float increase = method switch
            {
                AssassinationMethod.Poison => 25f,
                AssassinationMethod.HuntingAccident => 15f,
                AssassinationMethod.DuelProvocation => 10f, // Duels are "legal"
                AssassinationMethod.MidnightDagger => 40f,
                _ => 20f
            };

            if (hasAlibi)
                increase *= 0.5f;

            // High charm reduces suspicion
            int charm = Hero.MainHero.GetSkillValue(DefaultSkills.Charm);
            increase -= charm / 20f;

            // Diminishing suspicion if already high (they already suspect)
            if (currentSuspicion > 50f)
                increase *= 0.7f;

            return MathF.Max(increase, 0f);
        }

        #endregion

        #region Conspiracy Calculations

        /// <summary>
        /// Calculate the chance a lord will join the conspiracy.
        /// </summary>
        public static float CalculateJoinConspiracyChance(Hero lord, Hero currentRuler)
        {
            if (lord == null || currentRuler == null)
                return 0f;

            float chance = 25f; // Base chance

            // Relation with player
            int playerRelation = lord.GetRelation(Hero.MainHero);
            chance += playerRelation / 2f; // +0.5% per relation point

            // Relation with current ruler
            int rulerRelation = lord.GetRelation(currentRuler);
            chance -= rulerRelation / 2f; // -0.5% per relation point with ruler

            // Lord's traits
            var traits = lord.GetHeroTraits();

            // Honorable lords are less likely to betray
            if (traits.Honor > 0)
                chance -= 15f * traits.Honor;
            else if (traits.Honor < 0)
                chance += 10f * Math.Abs(traits.Honor);

            // Calculating lords see opportunity
            if (traits.Calculating > 0)
                chance += 10f * traits.Calculating;

            // Merciful lords don't want bloodshed
            if (traits.Mercy > 0)
                chance -= 5f * traits.Mercy;

            // Clan power - weaker clans are more desperate
            float clanStrength = lord.Clan != null ? BannerlordHelpers.GetClanStrength(lord.Clan) : 0f;
            var kingdom = Clan.PlayerClan?.Kingdom;
            if (kingdom != null)
            {
                float avgStrength = BannerlordHelpers.GetAverageKingdomClanStrength(kingdom);
                if (clanStrength < avgStrength * 0.5f)
                    chance += 15f; // Weak clan desperate for change
                else if (clanStrength > avgStrength * 1.5f)
                    chance -= 10f; // Strong clan has less to gain
            }

            // Player's coup strength affects perception
            var behavior = Campaign.Current?.GetCampaignBehavior<MacedonianBehavior>();
            if (behavior != null)
            {
                float coupStrength = behavior.CoupStrength;
                chance += coupStrength / 4f; // Lords join winners
            }

            return MathF.Clamp(chance, 5f, 85f);
        }

        /// <summary>
        /// Calculate leak risk for the conspiracy.
        /// </summary>
        public static float CalculateLeakRisk(int conspiratorsCount, float totalLeakModifiers)
        {
            // Base risk increases with size
            float risk = conspiratorsCount * 5f;

            // Add individual leak modifiers
            risk += totalLeakModifiers;

            return MathF.Clamp(risk, 0f, 100f);
        }

        /// <summary>
        /// Check if conspiracy leaks this week.
        /// </summary>
        public static bool DoesConspiracyLeak(float leakRisk)
        {
            float roll = MBRandom.RandomFloat * 100f;
            return roll < leakRisk / 10f; // Weekly check, so divide by 10
        }

        /// <summary>
        /// Calculate outcome of a leaked conspiracy.
        /// </summary>
        public static LeakSeverity CalculateLeakSeverity(float currentSuspicion)
        {
            float roll = MBRandom.RandomFloat * 100f;

            // High suspicion makes things worse
            roll += currentSuspicion / 2f;

            if (roll < 30f)
                return LeakSeverity.Rumor;
            else if (roll < 60f)
                return LeakSeverity.Suspicion;
            else if (roll < 85f)
                return LeakSeverity.Evidence;
            else
                return LeakSeverity.FullExposure;
        }

        #endregion

        #region Buying Silence

        /// <summary>
        /// Calculate gold cost to buy a lord's silence.
        /// </summary>
        public static int CalculateSilencePrice(Hero lord, float currentSuspicion)
        {
            if (lord == null)
                return int.MaxValue;

            int basePrice = 10000;

            // Higher suspicion = higher price
            basePrice += (int)(currentSuspicion * 100);

            // Lord's traits affect price
            var traits = lord.GetHeroTraits();

            // Greedy/calculating lords charge more
            if (traits.Calculating > 0)
                basePrice += 5000 * traits.Calculating;

            // Honorable lords won't take bribes
            if (traits.Honor > 1)
                return int.MaxValue; // Cannot bribe

            // Lord's clan wealth affects expectations
            if (lord.Clan != null)
            {
                if (lord.Clan.Gold > 100000)
                    basePrice += 10000;
            }

            // Relation discount
            int relation = lord.GetRelation(Hero.MainHero);
            if (relation > 0)
                basePrice -= relation * 50;

            return Math.Max(basePrice, 5000);
        }

        /// <summary>
        /// Calculate reliability of a lord keeping silent.
        /// </summary>
        public static float CalculateSilenceReliability(Hero lord, int goldPaid)
        {
            if (lord == null)
                return 0f;

            float reliability = 60f; // Base

            var traits = lord.GetHeroTraits();

            // Payment generosity
            int expectedPrice = CalculateSilencePrice(lord, 0);
            if (goldPaid >= expectedPrice * 2)
                reliability += 20f;
            else if (goldPaid >= expectedPrice)
                reliability += 10f;

            // Relation affects loyalty
            int relation = lord.GetRelation(Hero.MainHero);
            reliability += relation / 3f;

            // Honor affects trustworthiness (ironically, honorable lords keep their word even in crime)
            if (traits.Honor > 0)
                reliability += 15f;

            // Calculating lords might betray for better offer
            if (traits.Calculating > 0)
                reliability -= 10f * traits.Calculating;

            return MathF.Clamp(reliability, 10f, 95f);
        }

        #endregion

        #region Rivalry Calculations

        /// <summary>
        /// Calculate rivalry score for a hero.
        /// Higher score = bigger threat to player's ambitions.
        /// </summary>
        public static float CalculateRivalryScore(Hero hero, Hero currentRuler)
        {
            if (hero == null || hero == Hero.MainHero)
                return 0f;

            float score = 0f;

            var kingdom = Clan.PlayerClan?.Kingdom;
            if (kingdom == null)
                return 0f;

            // Political power (clan tier and influence)
            if (hero.Clan != null)
            {
                score += hero.Clan.Tier * 10f;
                score += Math.Min(hero.Clan.Influence / 100f, 50f);
            }

            // Relation with ruler
            if (currentRuler != null)
            {
                int rulerRelation = hero.GetRelation(currentRuler);
                score += rulerRelation / 2f; // Ruler's allies are rivals
            }

            // Relation with player (enemies are bigger threats)
            int playerRelation = hero.GetRelation(Hero.MainHero);
            score -= playerRelation / 3f;

            // Military strength
            if (hero.Clan != null)
            {
                float avgStrength = BannerlordHelpers.GetAverageKingdomClanStrength(kingdom);
                float clanStrength = BannerlordHelpers.GetClanStrength(hero.Clan);
                float relativeStrength = avgStrength > 0 ? (clanStrength / avgStrength) * 20f : 0f;
                score += relativeStrength;
            }

            // Personal combat prowess
            int combat = GetCombatScore(hero);
            score += combat / 10f;

            // Heir/succession position
            if (hero == currentRuler?.Spouse)
                score += 25f;
            if (hero.Father == currentRuler || hero.Mother == currentRuler)
                score += 30f;

            // Ambition trait
            var traits = hero.GetHeroTraits();
            if (traits.Calculating > 0)
                score += 10f * traits.Calculating;

            return MathF.Max(score, 0f);
        }

        #endregion

        #region Legitimacy Calculations

        /// <summary>
        /// Calculate initial legitimacy when taking the throne.
        /// </summary>
        public static float CalculateInitialLegitimacy(bool throughCoup, float coupStrength, float priorSuspicion)
        {
            float legitimacy;

            if (throughCoup)
            {
                // Violent takeover starts with lower legitimacy
                legitimacy = 30f;

                // Strong coup support helps
                legitimacy += coupStrength / 4f;

                // If you were already suspected, even worse
                legitimacy -= priorSuspicion / 3f;
            }
            else
            {
                // Legitimate succession (ruler died, election, etc.)
                legitimacy = 60f;
            }

            return MathF.Clamp(legitimacy, 10f, 80f);
        }

        /// <summary>
        /// Calculate weekly legitimacy change.
        /// </summary>
        public static float CalculateWeeklyLegitimacyChange(
            float currentLegitimacy,
            int victoriesThisWeek,
            int defeatsThisWeek,
            bool holdingFeast,
            int denariDonated)
        {
            float change = 0f;

            // Time naturally heals wounds (drift toward 50)
            if (currentLegitimacy < 50f)
                change += 0.5f;
            else if (currentLegitimacy > 80f)
                change -= 0.1f; // Harder to maintain very high legitimacy

            // Military victories
            change += victoriesThisWeek * 3f;
            change -= defeatsThisWeek * 2f;

            // Feasting builds support
            if (holdingFeast)
                change += 2f;

            // Generosity
            change += Math.Min(denariDonated / 10000f, 5f);

            return change;
        }

        /// <summary>
        /// Get the current stability state based on legitimacy.
        /// </summary>
        public static string GetLegitimacyState(float legitimacy)
        {
            if (legitimacy >= LEGITIMACY_STABLE)
                return "Stable Rule";
            else if (legitimacy >= LEGITIMACY_UNEASY)
                return "Uneasy Crown";
            else if (legitimacy >= LEGITIMACY_CRISIS)
                return "Legitimacy Crisis";
            else
                return "Tyrant's Grip";
        }

        #endregion

        #region Framing Calculations

        /// <summary>
        /// Calculate success chance when framing another party for assassination.
        /// </summary>
        public static float CalculateFrameSuccessChance(FrameTarget frameType, Clan targetClan = null, Kingdom targetKingdom = null)
        {
            float chance = 40f; // Base chance

            // Roguery helps plant evidence
            int roguery = Hero.MainHero.GetSkillValue(DefaultSkills.Roguery);
            chance += roguery / 8f;

            // Charm helps with the cover story
            int charm = Hero.MainHero.GetSkillValue(DefaultSkills.Charm);
            chance += charm / 12f;

            switch (frameType)
            {
                case FrameTarget.EnemyKingdom:
                    if (targetKingdom != null)
                    {
                        // Easier if already at war
                        var playerKingdom = Clan.PlayerClan?.Kingdom;
                        if (playerKingdom != null && playerKingdom.IsAtWarWith(targetKingdom))
                            chance += 25f;

                        // Easier if target kingdom is already distrusted
                        // (simplified - would need relation tracking)
                        chance += 10f;
                    }
                    break;

                case FrameTarget.RivalClan:
                    if (targetClan != null)
                    {
                        // Easier if target clan is already unpopular
                        var ruler = Clan.PlayerClan?.Kingdom?.Leader;
                        if (ruler != null)
                        {
                            int rulerOpinionOfTarget = 0;
                            foreach (var hero in targetClan.Heroes.Where(h => h.IsAlive && h.IsLord))
                            {
                                rulerOpinionOfTarget += ruler.GetRelation(hero);
                            }
                            rulerOpinionOfTarget /= Math.Max(1, targetClan.Heroes.Count(h => h.IsAlive && h.IsLord));

                            if (rulerOpinionOfTarget < 0)
                                chance += 15f;
                        }
                    }
                    break;

                case FrameTarget.Bandits:
                    chance += 20f; // Easy to blame bandits
                    break;

                case FrameTarget.NoFrame:
                default:
                    return 0f;
            }

            return MathF.Clamp(chance, 10f, 85f);
        }

        #endregion

        #region Usurpation Final Check

        /// <summary>
        /// Calculate final usurpation success chance.
        /// </summary>
        public static float CalculateUsurpationSuccess(
            float coupStrength,
            float legitimacy,
            bool rulerAlive,
            int armySizeAdvantage)
        {
            float chance = coupStrength; // Start with political support

            // Legitimacy of the claim
            chance += legitimacy / 4f;

            // Ruler being dead makes it much easier
            if (!rulerAlive)
                chance += 30f;

            // Military might matters
            chance += MathF.Clamp(armySizeAdvantage / 1000f, -20f, 20f);

            return MathF.Clamp(chance, 5f, 95f);
        }

        #endregion
    }
}
