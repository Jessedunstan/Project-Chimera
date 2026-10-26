using System;
using System.Collections.Generic;

namespace Chimera.Core.Ecology.Cycle;

/// <summary>
/// Represents a lineage evolutionary branch for a den, matching Rain World's lineage engine.
/// When the current occupant is eliminated, the den rolls a replacement candidate based on categorical probabilities.
/// </summary>
public sealed class LineageNode
{
    public readonly record struct LineageTier(string SpeciesId, float ProbabilityWeight, int TierRank);

    public string DenId { get; }
    private readonly List<LineageTier> _tiers = new();
    public int CurrentTierIndex { get; private set; }

    public LineageNode(string denId)
    {
        DenId = denId;
        CurrentTierIndex = 0;
    }

    public void AddTier(string speciesId, float probabilityWeight, int tierRank)
    {
        _tiers.Add(new LineageTier(speciesId, MathF.Max(0.001f, probabilityWeight), tierRank));
    }

    public string GetCurrentSpecies()
    {
        if (_tiers.Count == 0) return "Unknown";
        int idx = Math.Clamp(CurrentTierIndex, 0, _tiers.Count - 1);
        return _tiers[idx].SpeciesId;
    }

    public int GetCurrentTierRank()
    {
        if (_tiers.Count == 0) return 0;
        int idx = Math.Clamp(CurrentTierIndex, 0, _tiers.Count - 1);
        return _tiers[idx].TierRank;
    }

    /// <summary>
    /// Executes categorical evolutionary replacement roll upon occupant death.
    /// </summary>
    public string RollEvolutionaryReplacement(Random rng)
    {
        if (_tiers.Count == 0) return "Unknown";

        // Sum weights of remaining/eligible tiers
        float totalWeight = 0.0f;
        for (int i = 0; i < _tiers.Count; i++)
        {
            totalWeight += _tiers[i].ProbabilityWeight;
        }

        float roll = (float)rng.NextDouble() * totalWeight;
        float accumulated = 0.0f;

        for (int i = 0; i < _tiers.Count; i++)
        {
            accumulated += _tiers[i].ProbabilityWeight;
            if (roll <= accumulated)
            {
                CurrentTierIndex = i;
                return _tiers[i].SpeciesId;
            }
        }

        CurrentTierIndex = _tiers.Count - 1;
        return _tiers[^1].SpeciesId;
    }
}
