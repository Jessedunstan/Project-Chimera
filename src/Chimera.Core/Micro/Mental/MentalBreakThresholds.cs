using System;
using Chimera.Core.Micro.Psychology;

namespace Chimera.Core.Micro.Mental;

/// <summary>
/// Break intensity tiers matching RimWorld standards.
/// </summary>
public enum MentalBreakTier : byte
{
    None = 0,
    Minor = 1,
    Major = 2,
    Extreme = 3
}

/// <summary>
/// Specific mental break states extracted from RimWorld Defs.
/// </summary>
public enum MentalBreakDefId : ushort
{
    None = 0,

    // Minor (Threshold ~ 0.35)
    WanderSad = 1,
    WanderOwnRoom = 2,
    BingingFood = 3,
    InsultingSpree = 4,

    // Major (Threshold ~ 0.20)
    Tantrum = 5,
    TargetedTantrum = 6,
    CorpseObsession = 7,
    WanderPsychotic = 8,

    // Extreme (Threshold ~ 0.05)
    Berserk = 9,
    FireStartingSpree = 10,
    Slaughterer = 11,
    BingingDrugExtreme = 12,
    MurderousRage = 13
}

/// <summary>
/// Continuous calculator for mental break thresholds dynamically modulated by personality traits.
/// </summary>
public static class MentalBreakThresholds
{
    /// <summary>
    /// Computes the effective continuous break thresholds for an entity.
    /// Modulated by continuous trait sliders:
    /// - Neuroticism [0.0, 1.0]: up to +0.14 threshold increase
    /// - IronWill [0.0, 1.0]: up to -0.18 threshold decrease
    /// - Volatility [0.0, 1.0]: up to +0.15 threshold increase
    /// </summary>
    public static void CalculateThresholds(
        float neuroticism,
        float ironWill,
        float volatility,
        out float minorThreshold,
        out float majorThreshold,
        out float extremeThreshold)
    {
        float n = Math.Clamp(neuroticism, 0.0f, 1.0f);
        float w = Math.Clamp(ironWill, 0.0f, 1.0f);
        float v = Math.Clamp(volatility, 0.0f, 1.0f);

        // Net threshold offset from traits
        float traitOffset = (MoodConstants.VeryNeuroticTier2ThresholdOffset * n)
                          + (MoodConstants.VolatileThresholdOffset * v)
                          + (MoodConstants.IronWilledThresholdOffset * w);

        // Clamp thresholds to strictly maintain monotonic order: Extreme < Major < Minor <= 1.0
        minorThreshold = Math.Clamp(MoodConstants.BaseBreakThresholdMinor + traitOffset, 0.15f, 0.90f);
        majorThreshold = Math.Clamp(MoodConstants.BaseBreakThresholdMajor + traitOffset, 0.08f, minorThreshold - 0.05f);
        extremeThreshold = Math.Clamp(MoodConstants.BaseBreakThresholdExtreme + (traitOffset * 0.5f), 0.01f, majorThreshold - 0.04f);
    }
}
