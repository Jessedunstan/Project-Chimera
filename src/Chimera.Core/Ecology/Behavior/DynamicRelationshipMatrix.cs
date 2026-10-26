using System;
using System.Collections.Generic;

namespace Chimera.Core.Ecology.Behavior;

/// <summary>
/// Species-level relationship definition containing static baseline values.
/// </summary>
public readonly record struct SpeciesRelationshipPair(
    RelationshipType BaselineType,
    float BaselineIntensity,
    float BaseFear,
    float BaseAggression);

/// <summary>
/// Continuous relationship matrix between species types and individual creatures.
/// Stores static cross-species baselines and evaluates real-time dynamic relationships.
/// </summary>
public sealed class DynamicRelationshipMatrix
{
    private readonly Dictionary<(string Observer, string Target), SpeciesRelationshipPair> _matrix = new();

    public void SetBaseline(
        string observerSpecies,
        string targetSpecies,
        RelationshipType type,
        float intensity,
        float baseFear,
        float baseAggression)
    {
        _matrix[(observerSpecies, targetSpecies)] = new SpeciesRelationshipPair(
            type,
            Math.Clamp(intensity, 0.0f, 1.0f),
            Math.Clamp(baseFear, 0.0f, 1.0f),
            Math.Clamp(baseAggression, 0.0f, 1.0f));
    }

    public SpeciesRelationshipPair GetBaseline(string observerSpecies, string targetSpecies)
    {
        if (_matrix.TryGetValue((observerSpecies, targetSpecies), out var pair))
        {
            return pair;
        }

        // Default: Neutral ignore
        return new SpeciesRelationshipPair(RelationshipType.Ignores, 0.0f, 0.1f, 0.1f);
    }

    /// <summary>
    /// Computes full dynamic relationship evaluating real-time physiological and situational states.
    /// </summary>
    public FearAggressionEvaluator.EvaluationResult EvaluateDynamic(
        string observerSpecies,
        string targetSpecies,
        in FearAggressionEvaluator.EvaluationInput input)
    {
        var baseline = GetBaseline(observerSpecies, targetSpecies);

        // Merge baseline into input if not overridden
        var combinedInput = input with
        {
            BaseFear = input.BaseFear > 0.0f ? input.BaseFear : baseline.BaseFear,
            BaseAggression = input.BaseAggression > 0.0f ? input.BaseAggression : baseline.BaseAggression
        };

        return FearAggressionEvaluator.Evaluate(combinedInput, baseline.BaselineType);
    }
}
