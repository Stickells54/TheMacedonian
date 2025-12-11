using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TheMacedonian.Behaviors;
using TheMacedonian.Models;
using TheMacedonian.Settings;

namespace TheMacedonian.Patches
{
    /// <summary>
    /// Harmony patches for intercepting game events relevant to intrigue mechanics.
    /// </summary>
    [HarmonyPatch]
    public static class IntriguePatches
    {
        // Note: Kingdom election patching is handled via CampaignBehavior events
        // rather than direct patching, as the election API varies between versions.

        #region Hero Death Patches

        /// <summary>
        /// Patch to handle ruler death and trigger succession events.
        /// </summary>
        [HarmonyPatch(typeof(KillCharacterAction), nameof(KillCharacterAction.ApplyByMurder))]
        [HarmonyPostfix]
        public static void ApplyByMurder_Postfix(Hero victim, Hero killer, bool showNotification)
        {
            try
            {
                if (!MCMSettings.Instance?.EnableIntrigueSystem ?? true)
                    return;

                var behavior = Campaign.Current?.GetCampaignBehavior<MacedonianBehavior>();
                if (behavior == null)
                    return;

                // Check if the victim was the ruler of player's kingdom
                var kingdom = Clan.PlayerClan?.Kingdom;
                if (kingdom != null && victim == kingdom.Leader)
                {
                    // Ruler was murdered - this could be our doing
                    if (MCMSettings.Instance?.EnableDebugLogging ?? false)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            $"[TheMacedonian] Ruler {victim.Name} was murdered. Succession triggered.",
                            Colors.Magenta));
                    }

                    // The game will handle succession, but we track it
                    behavior.OnRulerDeath(victim, true);
                }
            }
            catch (Exception ex)
            {
                if (MCMSettings.Instance?.EnableDebugLogging ?? false)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        $"[TheMacedonian] Murder patch error: {ex.Message}",
                        Colors.Red));
                }
            }
        }

        /// <summary>
        /// Patch for natural death to track succession.
        /// </summary>
        [HarmonyPatch(typeof(KillCharacterAction), nameof(KillCharacterAction.ApplyByOldAge))]
        [HarmonyPostfix]
        public static void ApplyByOldAge_Postfix(Hero victim, bool showNotification)
        {
            try
            {
                if (!MCMSettings.Instance?.EnableIntrigueSystem ?? true)
                    return;

                var behavior = Campaign.Current?.GetCampaignBehavior<MacedonianBehavior>();
                if (behavior == null)
                    return;

                var kingdom = Clan.PlayerClan?.Kingdom;
                if (kingdom != null && victim == kingdom.Leader)
                {
                    // Natural death - good opportunity
                    behavior.OnRulerDeath(victim, false);
                }
            }
            catch (Exception ex)
            {
                if (MCMSettings.Instance?.EnableDebugLogging ?? false)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        $"[TheMacedonian] Death patch error: {ex.Message}",
                        Colors.Red));
                }
            }
        }

        #endregion

        #region Relation Change Patches

        /// <summary>
        /// Track significant relation changes that might affect conspiracy status.
        /// Parameter names updated for Bannerlord 1.3.9 API
        /// </summary>
        [HarmonyPatch(typeof(ChangeRelationAction), nameof(ChangeRelationAction.ApplyRelationChangeBetweenHeroes))]
        [HarmonyPostfix]
        public static void ApplyRelationChange_Postfix(Hero hero, Hero gainedRelationWith, int relationChange, bool showQuickNotification)
        {
            try
            {
                if (!MCMSettings.Instance?.EnableIntrigueSystem ?? true)
                    return;

                // Only care about significant changes involving the player
                if (hero != Hero.MainHero && gainedRelationWith != Hero.MainHero)
                    return;

                if (Math.Abs(relationChange) < 10)
                    return;

                var behavior = Campaign.Current?.GetCampaignBehavior<MacedonianBehavior>();
                if (behavior == null)
                    return;

                var otherHero = hero == Hero.MainHero ? gainedRelationWith : hero;

                // If relation drops significantly with a conspirator, they might leave
                if (relationChange < -20 && otherHero.Clan != null)
                {
                    var clanData = behavior.GetClanData(otherHero.Clan);
                    if (clanData.Status == ConspiracyStatus.Conspirator)
                    {
                        // Risk of conspiracy falling apart
                        if (MBRandom.RandomFloat < 0.3f)
                        {
                            // They're reconsidering
                            InformationManager.DisplayMessage(new InformationMessage(
                                $"{otherHero.Name} seems uncertain about your plans...",
                                Colors.Yellow));
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail - relation tracking is non-critical
            }
        }

        #endregion

        #region Battle Participation

        // NOTE: Battle participation tracking is now handled via CampaignEvents in MacedonianBehavior
        // The MapEventSide.OnFinish method doesn't exist in the current API
        // Instead, we register for MapEventEnded event in the behavior

        #endregion

        #region Companion/Prisoner Tracking

        // NOTE: Hostage/prisoner tracking is handled via CampaignEvents.HeroPrisonerTaken
        // in MacedonianBehavior rather than patching TakePrisonerAction which may vary by version

        #endregion
    }

    /// <summary>
    /// Additional patches for game menu interactions.
    /// </summary>
    [HarmonyPatch]
    public static class GameMenuPatches
    {
        // Placeholder for game menu hooks
        // These would intercept menu options to add intrigue actions
    }
}
