using System;
using Chimera.Core.MathEngine;

namespace Chimera.Core.Micro.Social;

/// <summary>
/// Continuous mathematical model calculating interpersonal affinity and opinion scores.
/// Core equation: Affinity(A, B) = sum(w_i * Factor_i), scaled continuously by Neuroticism.
/// Factors:
/// - Religious Harmony / Heresy: +1.0 (harmonious) to -1.0 (heretical/opposing), scaled by Devotion.
/// - Clan / Family Feud: +1.0 (allied/blood kin) to -1.0 (blood feud), scaled by Tribalism.
/// - Cultural / Sports Rivalry: +0.2 (fan bonding) to -0.2 (rival team), scaled by casual affinity.
/// - Grievance Scaling: Pawn A's Neuroticism amplifies all negative factors (feuds, heresy, rivalries, insults).
/// </summary>
public static class SocialAffinityEvaluator
{
    public const float MaxOpinionScore = 100.0f;
    public const float MinOpinionScore = -100.0f;

    // Base factor weights contributing to the 100-point opinion scale
    public const float ReligionWeight = 30.0f;
    public const float FeudWeight = 35.0f;
    public const float SportsRivalryWeight = 15.0f;

    /// <summary>
    /// Computes the effective weighting factor for negative social components based on the evaluator's Neuroticism.
    /// Range: [1.0, 2.0] as Neuroticism goes from 0.0 (calm) to 1.0 (extreme neurotic).
    /// Highly neurotic pawns react up to 2x more intensely to grievances, insults, and heresy.
    /// </summary>
    public static float GetNeuroticismGrievanceMultiplier(float neuroticism)
    {
        float n = Math.Clamp(neuroticism, 0.0f, 1.0f);
        return 1.0f + n; // 1.0 at 0% neurotic, 2.0 at 100% neurotic
    }

    /// <summary>
    /// Evaluates continuous ideological/religious compatibility between two entities.
    /// Input: 
    /// - religiousAlignmentScore in [-1.0, 1.0] (+1.0 = identical faith/dogma, -1.0 = heresy)
    /// - observerDevotion in [0.0, 1.0] (importance of faith to observer)
    /// - observerNeuroticism in [0.0, 1.0]
    /// Output: opinion delta in [-30.0, +30.0].
    /// </summary>
    public static float EvaluateReligiousCompatibility(
        float religiousAlignmentScore,
        float observerDevotion,
        float observerNeuroticism)
    {
        float align = Math.Clamp(religiousAlignmentScore, -1.0f, 1.0f);
        float dev = Math.Clamp(observerDevotion, 0.0f, 1.0f);

        float rawContribution = ReligionWeight * dev * align;

        if (rawContribution < 0.0f)
        {
            // Grievance amplified by neuroticism
            rawContribution *= GetNeuroticismGrievanceMultiplier(observerNeuroticism);
        }

        return Math.Clamp(rawContribution, -ReligionWeight * 2.0f, ReligionWeight);
    }

    /// <summary>
    /// Evaluates continuous clan / family feud status between two entities.
    /// Input:
    /// - clanFeudScore in [-1.0, 1.0] (+1.0 = deep family kinship/alliance, -1.0 = multi-generational blood feud)
    /// - observerTribalism in [0.0, 1.0] (importance of family/clan allegiance to observer)
    /// - observerNeuroticism in [0.0, 1.0]
    /// Output: opinion delta in [-35.0, +35.0].
    /// </summary>
    public static float EvaluateClanFeudAffinity(
        float clanFeudScore,
        float observerTribalism,
        float observerNeuroticism)
    {
        float feud = Math.Clamp(clanFeudScore, -1.0f, 1.0f);
        float trib = Math.Clamp(observerTribalism, 0.0f, 1.0f);

        float rawContribution = FeudWeight * trib * feud;

        if (rawContribution < 0.0f)
        {
            rawContribution *= GetNeuroticismGrievanceMultiplier(observerNeuroticism);
        }

        return Math.Clamp(rawContribution, -FeudWeight * 2.0f, FeudWeight);
    }

    /// <summary>
    /// Evaluates cultural and sports rivalry alignment between two entities.
    /// Input:
    /// - sportsRivalryScore in [-0.2, +0.2] (+0.2 = shared team fandom, -0.2 = opposing team rivalry)
    /// - observerNeuroticism in [0.0, 1.0]
    /// Output: opinion delta in [-15.0, +15.0].
    /// </summary>
    public static float EvaluateSportsRivalryAffinity(
        float sportsRivalryScore,
        float observerNeuroticism)
    {
        float rivalry = Math.Clamp(sportsRivalryScore, -0.20f, 0.20f);
        // Normalize [-0.2, 0.2] to [-1.0, 1.0]
        float normalized = rivalry / 0.20f;
        float rawContribution = SportsRivalryWeight * normalized;

        if (rawContribution < 0.0f)
        {
            rawContribution *= GetNeuroticismGrievanceMultiplier(observerNeuroticism);
        }

        return Math.Clamp(rawContribution, -SportsRivalryWeight * 2.0f, SportsRivalryWeight);
    }

    /// <summary>
    /// Evaluates the total aggregate opinion of Pawn A towards Pawn B.
    /// Opinion is clamped to [-100.0, +100.0].
    /// </summary>
    public static float CalculateCompositeOpinion(
        float religiousAlignment,
        float devotionA,
        float clanFeudScore,
        float tribalismA,
        float sportsRivalryScore,
        float neuroticismA,
        float decayedMemorySum)
    {
        float relScore = EvaluateReligiousCompatibility(religiousAlignment, devotionA, neuroticismA);
        float feudScore = EvaluateClanFeudAffinity(clanFeudScore, tribalismA, neuroticismA);
        float sportsScore = EvaluateSportsRivalryAffinity(sportsRivalryScore, neuroticismA);

        float memoryComponent = decayedMemorySum;
        if (memoryComponent < 0.0f)
        {
            memoryComponent *= GetNeuroticismGrievanceMultiplier(neuroticismA);
        }

        float totalOpinion = relScore + feudScore + sportsScore + memoryComponent;
        return Math.Clamp(totalOpinion, MinOpinionScore, MaxOpinionScore);
    }
}
