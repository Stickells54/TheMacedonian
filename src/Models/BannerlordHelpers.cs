using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using System;
using System.Linq;

namespace TheMacedonian.Models
{
    /// <summary>
    /// Helper methods for interacting with Bannerlord APIs.
    /// Provides version-safe wrappers for commonly used operations.
    /// </summary>
    public static class BannerlordHelpers
    {
        #region Trait Access

        /// <summary>
        /// Get a hero's honor trait value.
        /// </summary>
        public static int GetHonor(Hero hero)
        {
            if (hero == null) return 0;
            return hero.GetTraitLevel(DefaultTraits.Honor);
        }

        /// <summary>
        /// Get a hero's valor trait value.
        /// </summary>
        public static int GetValor(Hero hero)
        {
            if (hero == null) return 0;
            return hero.GetTraitLevel(DefaultTraits.Valor);
        }

        /// <summary>
        /// Get a hero's mercy trait value.
        /// </summary>
        public static int GetMercy(Hero hero)
        {
            if (hero == null) return 0;
            return hero.GetTraitLevel(DefaultTraits.Mercy);
        }

        /// <summary>
        /// Get a hero's calculating trait value.
        /// </summary>
        public static int GetCalculating(Hero hero)
        {
            if (hero == null) return 0;
            return hero.GetTraitLevel(DefaultTraits.Calculating);
        }

        /// <summary>
        /// Get a hero's generosity trait value.
        /// </summary>
        public static int GetGenerosity(Hero hero)
        {
            if (hero == null) return 0;
            return hero.GetTraitLevel(DefaultTraits.Generosity);
        }

        #endregion

        #region Clan Strength

        /// <summary>
        /// Calculate total military strength of a clan.
        /// </summary>
        public static float GetClanStrength(Clan clan)
        {
            if (clan == null) return 0f;

            float strength = 0f;

            // Sum up party strengths
            foreach (var party in clan.WarPartyComponents)
            {
                if (party.MobileParty?.Party != null)
                {
                    strength += GetPartyStrength(party.MobileParty.Party);
                }
            }

            return strength;
        }

        /// <summary>
        /// Get strength of a party.
        /// </summary>
        public static float GetPartyStrength(PartyBase party)
        {
            if (party == null) return 0f;
            // Use NumberOfAllMembers as a simple strength proxy
            return party.NumberOfAllMembers;
        }

        /// <summary>
        /// Calculate average clan strength in a kingdom.
        /// </summary>
        public static float GetAverageKingdomClanStrength(Kingdom kingdom)
        {
            if (kingdom == null || kingdom.Clans.Count == 0) return 0f;
            return kingdom.Clans.Average(c => GetClanStrength(c));
        }

        #endregion

        #region Math Helpers

        /// <summary>
        /// Clamp a float value between min and max.
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Clamp an int value between min and max.
        /// </summary>
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        #endregion

        #region Distance Calculations

        /// <summary>
        /// Get 2D distance between two settlements.
        /// </summary>
        public static float GetDistanceBetweenSettlements(TaleWorlds.CampaignSystem.Settlements.Settlement a, TaleWorlds.CampaignSystem.Settlements.Settlement b)
        {
            if (a == null || b == null) return float.MaxValue;
            return a.GatePosition.Distance(b.GatePosition);
        }

        #endregion

        #region War Checks

        /// <summary>
        /// Check if a kingdom is at war with another faction.
        /// </summary>
        public static bool IsAtWarWith(Kingdom kingdom, IFaction otherFaction)
        {
            if (kingdom == null || otherFaction == null) return false;
            return FactionManager.IsAtWarAgainstFaction(kingdom, otherFaction);
        }

        #endregion
    }

    /// <summary>
    /// Container for hero trait values for easy passing around.
    /// </summary>
    public class HeroTraits
    {
        public int Honor { get; set; }
        public int Valor { get; set; }
        public int Mercy { get; set; }
        public int Calculating { get; set; }
        public int Generosity { get; set; }

        public HeroTraits() { }

        public HeroTraits(Hero hero)
        {
            if (hero == null) return;
            Honor = BannerlordHelpers.GetHonor(hero);
            Valor = BannerlordHelpers.GetValor(hero);
            Mercy = BannerlordHelpers.GetMercy(hero);
            Calculating = BannerlordHelpers.GetCalculating(hero);
            Generosity = BannerlordHelpers.GetGenerosity(hero);
        }
    }

    /// <summary>
    /// Extension methods for Hero to get traits easily.
    /// </summary>
    public static class HeroExtensions
    {
        public static HeroTraits GetHeroTraits(this Hero hero)
        {
            return new HeroTraits(hero);
        }
    }
}
