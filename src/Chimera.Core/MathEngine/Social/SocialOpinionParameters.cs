using System;

namespace Chimera.Core.MathEngine.Social;

/// <summary>
/// Continuous social traits, ideological stances, and bilateral relationship factors between entities.
/// Zero heap allocations, pure value struct.
/// </summary>
public readonly struct SocialOpinionParameters
{
    public float ReligiousCompatibility { get; }       // [-1.0 (heresy) to +1.0 (harmony)]
    public float ClanFeudScore { get; }                // [-1.0 (blood feud) to 0.0 (neutral)]
    public float SportsRivalryAlignment { get; }       // [-0.2 (intense rivalry) to +0.2 (shared fandom)]
    public float EvaluatorNeuroticism { get; }         // [0.0 (stoic) to 1.0 (hyper-neurotic)]
    public float EvaluatorDevotion { get; }            // [0.0 (secular) to 1.0 (zealot)]
    public float EvaluatorTribalism { get; }           // [0.0 (cosmopolitan) to 1.0 (extreme clan loyalty)]
    public float EvaluatorIronWill { get; }            // [0.0 (fragile) to 1.0 (iron willed)]
    public float DecayedMemoriesSum { get; }           // Continuous accumulation of decayed bilateral memories

    public SocialOpinionParameters(
        float religiousCompatibility,
        float clanFeudScore,
        float sportsRivalryAlignment,
        float evaluatorNeuroticism = 0.2f,
        float evaluatorDevotion = 0.5f,
        float evaluatorTribalism = 0.5f,
        float evaluatorIronWill = 0.2f,
        float decayedMemoriesSum = 0.0f)
    {
        ReligiousCompatibility = Math.Clamp(religiousCompatibility, -1.0f, 1.0f);
        ClanFeudScore = Math.Clamp(clanFeudScore, -1.0f, 0.0f);
        SportsRivalryAlignment = Math.Clamp(sportsRivalryAlignment, -0.2f, 0.2f);
        EvaluatorNeuroticism = Math.Clamp(evaluatorNeuroticism, 0.0f, 1.0f);
        EvaluatorDevotion = Math.Clamp(evaluatorDevotion, 0.0f, 1.0f);
        EvaluatorTribalism = Math.Clamp(evaluatorTribalism, 0.0f, 1.0f);
        EvaluatorIronWill = Math.Clamp(evaluatorIronWill, 0.0f, 1.0f);
        DecayedMemoriesSum = Math.Clamp(decayedMemoriesSum, -100.0f, 100.0f);
    }
}
