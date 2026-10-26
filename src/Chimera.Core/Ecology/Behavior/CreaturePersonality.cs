using System;

namespace Chimera.Core.Ecology.Behavior;

/// <summary>
/// Continuous personality traits for individual creatures, matching Rain World's personality architecture.
/// All parameters are normalized continuous sliders [0.0..1.0].
/// </summary>
public readonly struct CreaturePersonality
{
    public float Energy { get; init; }
    public float Bravery { get; init; }
    public float Sympathy { get; init; }
    public float Dominance { get; init; }
    public float Nervous { get; init; }
    public float Aggression { get; init; }

    public CreaturePersonality(
        float energy = 0.5f,
        float bravery = 0.5f,
        float sympathy = 0.5f,
        float dominance = 0.5f,
        float nervous = 0.5f,
        float aggression = 0.5f)
    {
        Energy = Math.Clamp(energy, 0.0f, 1.0f);
        Bravery = Math.Clamp(bravery, 0.0f, 1.0f);
        Sympathy = Math.Clamp(sympathy, 0.0f, 1.0f);
        Dominance = Math.Clamp(dominance, 0.0f, 1.0f);
        Nervous = Math.Clamp(nervous, 0.0f, 1.0f);
        Aggression = Math.Clamp(aggression, 0.0f, 1.0f);
    }

    public static CreaturePersonality Neutral => new(0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f);

    public static CreaturePersonality PredatoryApex => new(
        energy: 0.8f,
        bravery: 0.9f,
        sympathy: 0.05f,
        dominance: 0.95f,
        nervous: 0.1f,
        aggression: 0.9f);

    public static CreaturePersonality SkittishPrey => new(
        energy: 0.7f,
        bravery: 0.1f,
        sympathy: 0.4f,
        dominance: 0.1f,
        nervous: 0.9f,
        aggression: 0.05f);

    public static CreaturePersonality SocialPack => new(
        energy: 0.6f,
        bravery: 0.6f,
        sympathy: 0.85f,
        dominance: 0.5f,
        nervous: 0.3f,
        aggression: 0.4f);
}
