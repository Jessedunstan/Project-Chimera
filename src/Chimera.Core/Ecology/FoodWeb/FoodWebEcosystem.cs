using System;
using System.Collections.Generic;

namespace Chimera.Core.Ecology.FoodWeb;

/// <summary>
/// Multi-tier ecological food web orchestrator.
/// Simulates trophic energy cascades, predatory feeding links, and population carrying capacity adjustments.
/// </summary>
public sealed class FoodWebEcosystem
{
    private readonly Dictionary<string, TrophicBiomassNode> _nodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<PredationLink> _predationLinks = new();

    public float TrophicTransferEfficiency { get; set; } = 0.10f; // 10% standard Lindeman efficiency

    public readonly record struct PredationLink(
        string PredatorSpeciesId,
        string PreySpeciesId,
        float PreferenceWeight,
        float ConsumptionRatePerPredatorKg);

    public IReadOnlyDictionary<string, TrophicBiomassNode> Nodes => _nodes;
    public IReadOnlyList<PredationLink> Links => _predationLinks;

    public void AddNode(TrophicBiomassNode node)
    {
        _nodes[node.SpeciesId] = node;
    }

    public void AddPredationLink(string predatorSpeciesId, string preySpeciesId, float preferenceWeight, float consumptionRatePerPredatorKg = 0.05f)
    {
        _predationLinks.Add(new PredationLink(predatorSpeciesId, preySpeciesId, preferenceWeight, consumptionRatePerPredatorKg));
    }

    /// <summary>
    /// Executes a macro-tick cycle across the food web:
    /// 1. Primary producers grow via solar/geothermal energy.
    /// 2. Consumers harvest prey according to trophic linkages and preferences.
    /// 3. Energy transfer is computed via trophic efficiency.
    /// 4. Populations update based on caloric intake vs maintenance demand.
    /// </summary>
    public void StepMacroCycle(float seasonalScarcityFactor = 0.0f)
    {
        // 1. Step growth on all nodes
        foreach (var node in _nodes.Values)
        {
            node.StepMacroGrowth(seasonalScarcityFactor);
        }

        // 2. Resolve predation links in trophic order (SecondaryConsumers, Apex, Scavengers)
        foreach (var link in _predationLinks)
        {
            if (!_nodes.TryGetValue(link.PredatorSpeciesId, out var predator) ||
                !_nodes.TryGetValue(link.PreySpeciesId, out var prey))
            {
                continue;
            }

            if (predator.CurrentBiomass <= 0.001f || prey.CurrentBiomass <= 0.001f)
            {
                continue;
            }

            // Desired biomass demand = predator biomass * consumption rate * preference weight
            float demand = predator.CurrentBiomass * link.ConsumptionRatePerPredatorKg * link.PreferenceWeight;
            float harvestedPreyBiomass = prey.HarvestBiomass(demand);

            // Energy conversion to predator biomass via trophic transfer efficiency
            float biomassGained = harvestedPreyBiomass * TrophicTransferEfficiency;
            predator.AddBiomass(biomassGained);
        }
    }

    /// <summary>
    /// Evaluates starvation pressure for a given species:
    /// Returns 0.0 (fully nourished) to 1.0 (starving to extinction).
    /// </summary>
    public float CalculateStarvationPressure(string speciesId)
    {
        if (!_nodes.TryGetValue(speciesId, out var node)) return 0.0f;
        if (node.CurrentBiomass <= 0.001f) return 1.0f;

        // Saturation below 0.3 indicates severe resource stress
        float saturation = node.BiomassSaturation;
        if (saturation >= 1.0f) return 0.0f;
        return Math.Clamp((1.0f - saturation) / 1.0f, 0.0f, 1.0f);
    }
}
