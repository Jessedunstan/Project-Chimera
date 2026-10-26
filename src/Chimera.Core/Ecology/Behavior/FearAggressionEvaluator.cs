using System;
using Chimera.Core.MathEngine;

namespace Chimera.Core.Ecology.Behavior;

/// <summary>
/// Continuous mathematical evaluator for dynamic fear vs. aggression between creatures.
/// Combines baseline species relationship with mass ratios, health, hunger urgency, personality, and territorial claims.
/// </summary>
public static class FearAggressionEvaluator
{
    public readonly record struct EvaluationInput(
        float BaseFear,
        float BaseAggression,
        float SelfMassKg,
        float OtherMassKg,
        float SelfHealthRatio,
        float SelfCaloricSatisfaction,
        float TerritoryFamiliarity,
        float Provocation,
        CreaturePersonality Personality);

    public readonly record struct EvaluationResult(
        float DynamicFear,
        float DynamicAggression,
        RelationshipType DominantIntent,
        float DominantIntensity);

    /// <summary>
    /// Computes continuous fear and aggression vectors and determines dominant behavioral intent.
    /// Pure mathematical evaluation with zero heap allocations.
    /// </summary>
    public static EvaluationResult Evaluate(in EvaluationInput input, RelationshipType baselineType)
    {
        float selfMass = MathF.Max(0.01f, input.SelfMassKg);
        float otherMass = MathF.Max(0.01f, input.OtherMassKg);
        float massRatio = otherMass / selfMass;

        // Sigmoidal mass scaling: 1.0 at parity, asymptotically 2.0 for giants, near 0.0 for tiny creatures
        float massModifier = ContinuousCurves.Sigmoid(massRatio, midpoint: 1.0f, steepness: 2.0f) * 2.0f;

        // Health vulnerability: wounded creatures experience higher fear
        float healthVulnerability = 1.0f + (0.6f * (1.0f - Math.Clamp(input.SelfHealthRatio, 0.0f, 1.0f)));

        // Personality impact on fear
        float personalityFearMod = (1.0f + input.Personality.Nervous) / (1.0f + (2.0f * input.Personality.Bravery));

        float rawFear = input.BaseFear * massModifier * healthVulnerability * personalityFearMod;
        float dynamicFear = Math.Clamp(rawFear, 0.0f, 1.0f);

        // Hunger / Caloric deficit: starving creatures become reckless and aggressive
        float hungerDrive = 1.0f + (0.75f * (1.0f - Math.Clamp(input.SelfCaloricSatisfaction, 0.0f, 1.0f)));

        // Personality impact on aggression
        float personalityAggressionMod = 0.5f
            + (0.4f * input.Personality.Aggression)
            + (0.3f * input.Personality.Dominance)
            - (0.2f * input.Personality.Sympathy);

        // Territorial familiarity boosts aggression for dominant residents
        float territorialBonus = input.TerritoryFamiliarity * input.Personality.Dominance * 0.4f;

        float rawAggression = (input.BaseAggression + input.Provocation + territorialBonus)
            * hungerDrive
            * personalityAggressionMod;
        float dynamicAggression = Math.Clamp(rawAggression, 0.0f, 1.0f);

        // Derive dominant behavioral classification dynamically
        RelationshipType intent;
        float intensity;

        if (dynamicFear > 0.55f && dynamicFear > dynamicAggression)
        {
            intent = RelationshipType.Afraid;
            intensity = dynamicFear;
        }
        else if (dynamicAggression > 0.55f && dynamicAggression >= dynamicFear)
        {
            intent = baselineType == RelationshipType.Eats ? RelationshipType.Eats : RelationshipType.Attacks;
            intensity = dynamicAggression;
        }
        else if (dynamicAggression > 0.30f && dynamicFear > 0.30f)
        {
            intent = RelationshipType.Antagonizes;
            intensity = (dynamicAggression + dynamicFear) * 0.5f;
        }
        else if (dynamicFear < 0.25f && dynamicAggression < 0.25f)
        {
            if (input.Personality.Sympathy > 0.65f)
            {
                intent = RelationshipType.PlaysWith;
                intensity = input.Personality.Sympathy;
            }
            else
            {
                intent = RelationshipType.Ignores;
                intensity = 0.0f;
            }
        }
        else
        {
            intent = dynamicAggression > dynamicFear ? RelationshipType.StayOutOfWay : RelationshipType.Ignores;
            intensity = MathF.Abs(dynamicAggression - dynamicFear);
        }

        return new EvaluationResult(dynamicFear, dynamicAggression, intent, intensity);
    }
}
