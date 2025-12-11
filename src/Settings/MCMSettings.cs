using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace TheMacedonian.Settings
{
    /// <summary>
    /// Mod Configuration Menu settings for The Macedonian.
    /// Allows players to adjust difficulty and feature toggles.
    /// </summary>
    public class MCMSettings : AttributeGlobalSettings<MCMSettings>
    {
        public override string Id => "TheMacedonian";
        public override string DisplayName => "The Macedonian";
        public override string FolderName => "TheMacedonian";
        public override string FormatType => "json2";

        #region General Settings

        [SettingPropertyBool(
            "Enable Debug Logging",
            Order = 0,
            RequireRestart = false,
            HintText = "Enable detailed logging for debugging purposes.")]
        [SettingPropertyGroup("General", GroupOrder = 0)]
        public bool EnableDebugLogging { get; set; } = false;

        [SettingPropertyBool(
            "Enable Intrigue System",
            Order = 1,
            RequireRestart = true,
            HintText = "Master toggle for all intrigue mechanics. Disable to turn off the mod without uninstalling.")]
        [SettingPropertyGroup("General", GroupOrder = 0)]
        public bool EnableIntrigueSystem { get; set; } = true;

        [SettingPropertyBool(
            "Show Intrigue Panel",
            Order = 2,
            RequireRestart = false,
            HintText = "Show the intrigue status panel in the bottom right of the campaign map.")]
        [SettingPropertyGroup("General", GroupOrder = 0)]
        public bool ShowIntriguePanel { get; set; } = true;

        #endregion

        #region Difficulty Settings

        [SettingPropertyFloatingInteger(
            "Assassination Base Success Modifier",
            0.5f, 2.0f,
            Order = 0,
            RequireRestart = false,
            HintText = "Multiplier for assassination success chances. Higher = easier assassinations.")]
        [SettingPropertyGroup("Difficulty", GroupOrder = 1)]
        public float AssassinationSuccessModifier { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger(
            "Detection Chance Modifier",
            0.5f, 2.0f,
            Order = 1,
            RequireRestart = false,
            HintText = "Multiplier for detection chances. Higher = more likely to be caught.")]
        [SettingPropertyGroup("Difficulty", GroupOrder = 1)]
        public float DetectionChanceModifier { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger(
            "Leak Risk Modifier",
            0.5f, 2.0f,
            Order = 2,
            RequireRestart = false,
            HintText = "Multiplier for conspiracy leak risk. Higher = more frequent leaks.")]
        [SettingPropertyGroup("Difficulty", GroupOrder = 1)]
        public float LeakRiskModifier { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger(
            "Legitimacy Decay Modifier",
            0.5f, 2.0f,
            Order = 3,
            RequireRestart = false,
            HintText = "Multiplier for legitimacy decay rate. Higher = harder to maintain legitimacy.")]
        [SettingPropertyGroup("Difficulty", GroupOrder = 1)]
        public float LegitimacyDecayModifier { get; set; } = 1.0f;

        [SettingPropertyInteger(
            "Assassination Cooldown (Days)",
            7, 90,
            Order = 4,
            RequireRestart = false,
            HintText = "Minimum days between assassination attempts on the same target.")]
        [SettingPropertyGroup("Difficulty", GroupOrder = 1)]
        public int AssassinationCooldownDays { get; set; } = 30;

        #endregion

        #region Right Hand System

        [SettingPropertyBool(
            "Enable Right Hand System",
            Order = 0,
            RequireRestart = true,
            HintText = "Enable the Royal Protector / Right Hand path to power.")]
        [SettingPropertyGroup("Right Hand System", GroupOrder = 2)]
        public bool EnableRightHandSystem { get; set; } = true;

        [SettingPropertyInteger(
            "Battles Required for Trust",
            3, 20,
            Order = 1,
            RequireRestart = false,
            HintText = "Number of battles fighting alongside the ruler needed to become trusted.")]
        [SettingPropertyGroup("Right Hand System", GroupOrder = 2)]
        public int BattlesRequiredForTrust { get; set; } = 5;

        [SettingPropertyInteger(
            "Companion Hostage Duration (Days)",
            30, 365,
            Order = 2,
            RequireRestart = false,
            HintText = "Days your companion must serve as 'hostage' at the ruler's court.")]
        [SettingPropertyGroup("Right Hand System", GroupOrder = 2)]
        public int HostageDurationDays { get; set; } = 90;

        #endregion

        #region Assassination Settings

        [SettingPropertyBool(
            "Enable Poison Method",
            Order = 0,
            RequireRestart = false,
            HintText = "Allow poison as an assassination method.")]
        [SettingPropertyGroup("Assassination Methods", GroupOrder = 3)]
        public bool EnablePoisonMethod { get; set; } = true;

        [SettingPropertyBool(
            "Enable Hunting Accident Method",
            Order = 1,
            RequireRestart = false,
            HintText = "Allow hunting accidents as an assassination method.")]
        [SettingPropertyGroup("Assassination Methods", GroupOrder = 3)]
        public bool EnableHuntingAccidentMethod { get; set; } = true;

        [SettingPropertyBool(
            "Enable Duel Provocation Method",
            Order = 2,
            RequireRestart = false,
            HintText = "Allow provoking duels as an assassination method.")]
        [SettingPropertyGroup("Assassination Methods", GroupOrder = 3)]
        public bool EnableDuelMethod { get; set; } = true;

        [SettingPropertyBool(
            "Enable Midnight Dagger Method",
            Order = 3,
            RequireRestart = false,
            HintText = "Allow midnight assassination attempts.")]
        [SettingPropertyGroup("Assassination Methods", GroupOrder = 3)]
        public bool EnableMidnightDaggerMethod { get; set; } = true;

        [SettingPropertyBool(
            "Enable Framing",
            Order = 4,
            RequireRestart = false,
            HintText = "Allow framing other parties for assassinations.")]
        [SettingPropertyGroup("Assassination Methods", GroupOrder = 3)]
        public bool EnableFraming { get; set; } = true;

        #endregion

        #region Conspiracy Settings

        [SettingPropertyInteger(
            "Minimum Conspirators for Coup",
            2, 10,
            Order = 0,
            RequireRestart = false,
            HintText = "Minimum number of clan leaders needed to attempt a coup.")]
        [SettingPropertyGroup("Conspiracy", GroupOrder = 4)]
        public int MinConspiratorsForcoup { get; set; } = 3;

        [SettingPropertyFloatingInteger(
            "Coup Strength Required",
            25f, 75f,
            Order = 1,
            RequireRestart = false,
            HintText = "Minimum percentage of kingdom strength needed for coup to succeed.")]
        [SettingPropertyGroup("Conspiracy", GroupOrder = 4)]
        public float CoupStrengthRequired { get; set; } = 50f;

        [SettingPropertyInteger(
            "Silence Price Base (Denars)",
            5000, 50000,
            Order = 2,
            RequireRestart = false,
            HintText = "Base cost to buy a lord's silence.")]
        [SettingPropertyGroup("Conspiracy", GroupOrder = 4)]
        public int SilencePriceBase { get; set; } = 10000;

        [SettingPropertyBool(
            "Lords Can Refuse Silence",
            Order = 3,
            RequireRestart = false,
            HintText = "Allow highly honorable lords to refuse bribes entirely.")]
        [SettingPropertyGroup("Conspiracy", GroupOrder = 4)]
        public bool LordsCanRefuseSilence { get; set; } = true;

        #endregion

        #region Post-Coronation

        [SettingPropertyBool(
            "Enable Post-Coronation Phase",
            Order = 0,
            RequireRestart = true,
            HintText = "Enable the post-coronation legitimacy and stability mechanics.")]
        [SettingPropertyGroup("Post-Coronation", GroupOrder = 5)]
        public bool EnablePostCoronation { get; set; } = true;

        [SettingPropertyFloatingInteger(
            "Initial Legitimacy (Coup)",
            10f, 50f,
            Order = 1,
            RequireRestart = false,
            HintText = "Starting legitimacy when taking power through a coup.")]
        [SettingPropertyGroup("Post-Coronation", GroupOrder = 5)]
        public float InitialLegitimacyCoup { get; set; } = 30f;

        [SettingPropertyFloatingInteger(
            "Initial Legitimacy (Legitimate)",
            40f, 80f,
            Order = 2,
            RequireRestart = false,
            HintText = "Starting legitimacy when taking power legitimately.")]
        [SettingPropertyGroup("Post-Coronation", GroupOrder = 5)]
        public float InitialLegitimacyLegitimate { get; set; } = 60f;

        [SettingPropertyBool(
            "Enable Counter-Coups",
            Order = 3,
            RequireRestart = false,
            HintText = "Allow AI lords to attempt counter-coups against you.")]
        [SettingPropertyGroup("Post-Coronation", GroupOrder = 5)]
        public bool EnableCounterCoups { get; set; } = true;

        #endregion

        #region Notifications

        [SettingPropertyBool(
            "Show Suspicion Warnings",
            Order = 0,
            RequireRestart = false,
            HintText = "Display notifications when suspicion increases.")]
        [SettingPropertyGroup("Notifications", GroupOrder = 6)]
        public bool ShowSuspicionWarnings { get; set; } = true;

        [SettingPropertyBool(
            "Show Leak Warnings",
            Order = 1,
            RequireRestart = false,
            HintText = "Display notifications when conspiracy leaks occur.")]
        [SettingPropertyGroup("Notifications", GroupOrder = 6)]
        public bool ShowLeakWarnings { get; set; } = true;

        [SettingPropertyBool(
            "Show Rivalry Updates",
            Order = 2,
            RequireRestart = false,
            HintText = "Display notifications when rival threats change significantly.")]
        [SettingPropertyGroup("Notifications", GroupOrder = 6)]
        public bool ShowRivalryUpdates { get; set; } = true;

        [SettingPropertyBool(
            "Show Legitimacy Updates",
            Order = 3,
            RequireRestart = false,
            HintText = "Display periodic legitimacy status updates.")]
        [SettingPropertyGroup("Notifications", GroupOrder = 6)]
        public bool ShowLegitimacyUpdates { get; set; } = true;

        #endregion
    }
}
