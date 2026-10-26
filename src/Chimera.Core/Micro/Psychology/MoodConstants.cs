namespace Chimera.Core.Micro.Psychology;

/// <summary>
/// Verified RimWorld psychological constants extracted by REA.
/// All time intervals use discrete tick indexing.
/// </summary>
public static class MoodConstants
{
    // Discrete tick constants
    public const long TicksPerInGameDay = 60_000L;
    public const long TicksPerInGameHour = 2_500L;

    // Mood level baseline & seeker velocities
    public const float BaselineMood = 0.32f; // 32%
    public const float SeekerRisePerHour = 0.12f;
    public const float SeekerFallPerHour = 0.08f;

    // Seeker velocity per tick (Rise/Fall per tick)
    public const float SeekerRisePerTick = SeekerRisePerHour / TicksPerInGameHour;
    public const float SeekerFallPerTick = SeekerFallPerHour / TicksPerInGameHour;

    // Base mental break thresholds
    public const float BaseBreakThresholdMinor = 0.35f;
    public const float BaseBreakThresholdMajor = 0.20f;
    public const float BaseBreakThresholdExtreme = 0.05f;

    // Trait offsets to MentalBreakThreshold
    public const float IronWilledThresholdOffset = -0.18f;
    public const float SteadfastThresholdOffset = -0.09f;
    public const float NervousThresholdOffset = 0.08f;
    public const float VolatileThresholdOffset = 0.15f;
    public const float TooSmartThresholdOffset = 0.12f;
    public const float NeuroticTier1ThresholdOffset = 0.08f;
    public const float VeryNeuroticTier2ThresholdOffset = 0.14f;

    // Trait work speed bonuses
    public const float NeuroticTier1WorkSpeedBonus = 0.20f;
    public const float VeryNeuroticTier2WorkSpeedBonus = 0.40f;

    // Catharsis recovery bonus
    public const float CatharsisMoodBonus = 40.0f;
    public const long CatharsisDurationTicks = (long)(2.5f * TicksPerInGameDay); // 150,000 ticks
}
