using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace TheMacedonian.Models
{
    /// <summary>
    /// Types of assassination methods available.
    /// </summary>
    public enum AssassinationMethod
    {
        None,
        Poison,
        HuntingAccident,
        DuelProvocation,
        MidnightDagger,
        PraetorianCoup
    }

    /// <summary>
    /// Phase of an active assassination/conspiracy plot.
    /// </summary>
    public enum PlotPhase
    {
        Planning,
        Preparation,
        Execution,
        Completed,
        Abandoned
    }

    /// <summary>
    /// Result of a conspiracy or plot.
    /// </summary>
    public enum ConspiracyResult
    {
        Pending,
        Success,
        Failure,
        Exposed,
        Abandoned
    }

    /// <summary>
    /// Severity of a leak event.
    /// </summary>
    public enum LeakSeverity
    {
        Rumor,       // Minor rumor, small suspicion increase
        Suspicion,   // Ruler becomes suspicious
        Evidence,    // Some evidence found
        FullExposure // Complete exposure, major consequences
    }

    /// <summary>
    /// Possible outcomes of an assassination attempt.
    /// </summary>
    public enum AssassinationOutcome
    {
        Success,
        Failure,
        CriticalFailure,
        FrameJobSuccess  // Critical success with framing
    }

    /// <summary>
    /// Types of framing targets.
    /// </summary>
    public enum FrameTarget
    {
        NoFrame,
        None,
        InternalRival,
        RivalClan,
        ExternalEnemy,
        EnemyKingdom,
        Bandits
    }

    /// <summary>
    /// Suspicion level thresholds for display.
    /// </summary>
    public enum SuspicionLevel
    {
        None,       // 0-10
        Low,        // 11-30
        Medium,     // 31-60
        High,       // 61-85
        Critical    // 86-100
    }

    /// <summary>
    /// Conspiracy status for a clan.
    /// </summary>
    public enum ConspiracyStatus
    {
        Unknown,    // Haven't approached yet
        Loyalist,   // Supports the current ruler
        Neutral,    // Undecided
        Sympathizer,// Dislikes ruler but not committed
        Conspirator // In the pact
    }

    /// <summary>
    /// Tracks per-clan conspiracy and rivalry data.
    /// </summary>
    public class ClanIntrigueData
    {
        [SaveableField(1)]
        public ConspiracyStatus Status = ConspiracyStatus.Unknown;

        [SaveableField(2)]
        public float RivalryScore = 0f;

        [SaveableField(3)]
        public float LeakRiskModifier = 0f;  // Based on leader traits

        [SaveableField(4)]
        public bool HasBeenApproached = false;

        public ClanIntrigueData() { }
    }

    /// <summary>
    /// Tracks suspicion levels for the player.
    /// </summary>
    public class SuspicionData
    {
        [SaveableField(1)]
        public float RulerSuspicion = 0f;    // The ruler's personal suspicion of you

        [SaveableField(2)]
        public float CourtSuspicion = 0f;    // General court awareness

        [SaveableField(3)]
        public float KingdomSuspicion = 0f;  // Kingdom-wide rumors

        public SuspicionData() { }

        public SuspicionLevel GetRulerLevel() => GetLevel(RulerSuspicion);
        public SuspicionLevel GetCourtLevel() => GetLevel(CourtSuspicion);
        public SuspicionLevel GetKingdomLevel() => GetLevel(KingdomSuspicion);

        private static SuspicionLevel GetLevel(float value)
        {
            return value switch
            {
                <= 10f => SuspicionLevel.None,
                <= 30f => SuspicionLevel.Low,
                <= 60f => SuspicionLevel.Medium,
                <= 85f => SuspicionLevel.High,
                _ => SuspicionLevel.Critical
            };
        }

        public void DecayDaily(float amount = 0.5f)
        {
            RulerSuspicion = System.Math.Max(0, RulerSuspicion - amount);
            CourtSuspicion = System.Math.Max(0, CourtSuspicion - amount * 0.5f);
            KingdomSuspicion = System.Math.Max(0, KingdomSuspicion - amount * 0.25f);
        }
    }

    /// <summary>
    /// Tracks assassination attempt cooldowns per target.
    /// </summary>
    public class AssassinationCooldown
    {
        [SaveableField(1)]
        public float LastAttemptDay = -1000f;

        [SaveableField(2)]
        public int FailureCount = 0;

        [SaveableField(3)]
        public float TargetAwareness = 0f;  // Target's personal awareness/paranoia

        public AssassinationCooldown() { }

        public bool IsOnCooldown(float currentDay, float cooldownDays = 90f)
        {
            return (currentDay - LastAttemptDay) < cooldownDays;
        }

        public float GetCooldownRemaining(float currentDay, float cooldownDays = 90f)
        {
            return System.Math.Max(0, cooldownDays - (currentDay - LastAttemptDay));
        }
    }

    /// <summary>
    /// Result of an assassination calculation.
    /// </summary>
    public class AssassinationResult
    {
        public AssassinationOutcome Outcome { get; set; }
        public float RollValue { get; set; }
        public float SuccessThreshold { get; set; }
        public float CriticalThreshold { get; set; }
        public string? Description { get; set; }
        public FrameTarget FrameOption { get; set; } = FrameTarget.None;
    }

    /// <summary>
    /// Record of an active or completed plot.
    /// </summary>
    public class PlotRecord
    {
        [SaveableField(1)]
        public string? _targetHeroId;

        [SaveableField(2)]
        public AssassinationMethod _method = AssassinationMethod.None;

        [SaveableField(3)]
        public PlotPhase _phase = PlotPhase.Planning;

        [SaveableField(4)]
        public ConspiracyResult _result = ConspiracyResult.Pending;

        [SaveableField(5)]
        public float _startDay;

        [SaveableField(6)]
        public float _completionDay;

        [SaveableField(7)]
        public FrameTarget _frameTarget = FrameTarget.None;

        [SaveableField(8)]
        public string? _frameTargetId;

        // Properties for easy access
        public string? TargetHeroId { get => _targetHeroId; set => _targetHeroId = value; }
        public AssassinationMethod Method { get => _method; set => _method = value; }
        public PlotPhase Phase { get => _phase; set => _phase = value; }
        public ConspiracyResult Result { get => _result; set => _result = value; }
        public float StartDay { get => _startDay; set => _startDay = value; }
        public float CompletionDay { get => _completionDay; set => _completionDay = value; }
        public FrameTarget FrameTarget { get => _frameTarget; set => _frameTarget = value; }
        public string? FrameTargetId { get => _frameTargetId; set => _frameTargetId = value; }

        /// <summary>
        /// Gets or sets the target hero.
        /// </summary>
        public Hero? Target
        {
            get => !string.IsNullOrEmpty(_targetHeroId)
                ? Hero.FindFirst(h => h.StringId == _targetHeroId)
                : null;
            set => _targetHeroId = value?.StringId;
        }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public CampaignTime StartDate
        {
            get => CampaignTime.Days(_startDay);
            set => _startDay = (float)value.ToDays;
        }

        public PlotRecord() { }

        public PlotRecord(Hero target, AssassinationMethod method)
        {
            _targetHeroId = target?.StringId;
            _method = method;
            _phase = PlotPhase.Planning;
            _result = ConspiracyResult.Pending;
            _startDay = (float)TaleWorlds.CampaignSystem.CampaignTime.Now.ToDays;
        }
    }
}
