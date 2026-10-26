using System;

namespace Chimera.Core.MathEngine.Social;

public readonly struct MentalBreakThresholds
{
    public float MinorThreshold { get; }
    public float MajorThreshold { get; }
    public float ExtremeThreshold { get; }

    public MentalBreakThresholds(float minor, float major, float extreme)
    {
        MinorThreshold = minor;
        MajorThreshold = major;
        ExtremeThreshold = extreme;
    }
}

/// <summary>
/// Mathematical engine for social opinion, ideological alignment, and mental break dynamics.
/// Pure deterministic calculations, zero heap allocations.
/// </summary>
public static class SocialMath
{
    public const float ReligionWeight = 30.0f;     // Up to +/- 30 opinion points
    public const float ClanFeudWeight = 40.0f;     // Up to -40 opinion points
    public const float SportsWeight = 25.0f;       // Up to +/- 5 opinion points (at [-0.2, 0.2])

    /// <summary>
    /// Computes bilateral opinion of Evaluator toward Target entity:
    /// Opinion = clamp( Sum(Weight_i * Factor_i) * NeuroticismBias, -100, +100 )
    /// </summary>
    public static float CalculateBilateralOpinion(in SocialOpinionParameters parameters)
    {
        // 1. Ideological & Religious Harmony / Heresy: [-1.0 to +1.0] scaled by Devotion
        float religionContribution = ReligionWeight * parameters.EvaluatorDevotion * parameters.ReligiousCompatibility;

        // 2. Clan / Family Feud: [-1.0 to 0.0] scaled by Tribalism
        float feudContribution = ClanFeudWeight * parameters.EvaluatorTribalism * parameters.ClanFeudScore;

        // 3. Cultural & Sports Alignment: [-0.2 to +0.2]
        float sportsContribution = SportsWeight * parameters.SportsRivalryAlignment;

        // 4. Decayed Personal Memories
        float memoryContribution = parameters.DecayedMemoriesSum;

        // Sum raw additive opinion components
        float rawOpinion = religionContribution + feudContribution + sportsContribution + memoryContribution;

        // Apply evaluator neuroticism bias:
        // High neuroticism magnifies grievances and dampens positive affinities
        float finalOpinion;
        if (rawOpinion < 0.0f)
        {
            float negativeAmplifier = 1.0f + (1.2f * parameters.EvaluatorNeuroticism);
            finalOpinion = rawOpinion * negativeAmplifier;
        }
        else
        {
            float positiveDamper = 1.0f - (0.25f * parameters.EvaluatorNeuroticism);
            finalOpinion = rawOpinion * positiveDamper;
        }

        return Math.Clamp(finalOpinion, -100.0f, 100.0f);
    }

    /// <summary>
    /// Calculates inter-generational decay of inherited family feuds:
    /// Feud_offspring = Feud_parent * exp(-feudDecayRate * generations)
    /// </summary>
    public static float DecayGenerationalFeud(float parentFeudScore, float feudDecayRate = 0.35f, int generationsElapsed = 1)
    {
        if (generationsElapsed <= 0) return parentFeudScore;
        return ContinuousCurves.ExponentialDecay(parentFeudScore, feudDecayRate, generationsElapsed);
    }

    /// <summary>
    /// Evaluates continuous decay of temporary social memory thoughts (insults, praise, rescues).
    /// </summary>
    public static float DecaySocialMemory(float initialMemoryOpinion, float decayPerSecond, float elapsedSeconds)
    {
        return ContinuousCurves.ExponentialDecay(initialMemoryOpinion, decayPerSecond, elapsedSeconds);
    }

    /// <summary>
    /// Calculates dynamic mental break thresholds for an individual.
    /// High neuroticism pushes break thresholds higher (easier to break).
    /// High iron will pushes break thresholds lower (resilient to breaks).
    /// </summary>
    public static MentalBreakThresholds CalculateMentalBreakThresholds(float neuroticism, float ironWill)
    {
        float nFactor = 1.0f + (0.45f * Math.Clamp(neuroticism, 0.0f, 1.0f));
        float wFactor = 1.0f - (0.40f * Math.Clamp(ironWill, 0.0f, 1.0f));
        float combinedModifier = nFactor * wFactor;

        float minor = Math.Clamp(0.35f * combinedModifier, 0.05f, 0.70f);
        float major = Math.Clamp(0.20f * combinedModifier, 0.02f, 0.50f);
        float extreme = Math.Clamp(0.05f * combinedModifier, 0.005f, 0.25f);

        return new MentalBreakThresholds(minor, major, extreme);
    }
}
