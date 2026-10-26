using System;
using Chimera.Core.MathEngine;

namespace Chimera.Core.Ecology.FoodWeb;

/// <summary>
/// Represents a population node in a multi-tier ecological food web.
/// Tracks biomass density, population counts, caloric yield per individual, and replenishment dynamics.
/// </summary>
public sealed class TrophicBiomassNode
{
    public string SpeciesId { get; }
    public TrophicLevel Level { get; }
    public float CarryingCapacityBiomass { get; set; }
    public float CurrentBiomass { get; private set; }
    public float CaloricYieldPerKg { get; set; }
    public float IndividualMassKg { get; set; }
    public float IntrinsicGrowthRate { get; set; }

    public int EstimatedPopulation => IndividualMassKg > 0.0f
        ? (int)MathF.Floor(CurrentBiomass / IndividualMassKg)
        : 0;

    public float BiomassSaturation => CarryingCapacityBiomass > 0.0f
        ? Math.Clamp(CurrentBiomass / CarryingCapacityBiomass, 0.0f, 2.0f)
        : 0.0f;

    public TrophicBiomassNode(
        string speciesId,
        TrophicLevel level,
        float initialBiomass,
        float carryingCapacity,
        float individualMassKg = 1.0f,
        float caloricYieldPerKg = 1500.0f,
        float intrinsicGrowthRate = 0.05f)
    {
        SpeciesId = speciesId;
        Level = level;
        CarryingCapacityBiomass = MathF.Max(1.0f, carryingCapacity);
        CurrentBiomass = Math.Clamp(initialBiomass, 0.0f, carryingCapacity * 2.0f);
        IndividualMassKg = MathF.Max(0.01f, individualMassKg);
        CaloricYieldPerKg = MathF.Max(1.0f, caloricYieldPerKg);
        IntrinsicGrowthRate = intrinsicGrowthRate;
    }

    /// <summary>
    /// Executes logistic growth over a discrete macro cycle:
    /// dBiomass = r * Biomass * (1 - Biomass / K)
    /// </summary>
    public void StepMacroGrowth(float environmentalScarcityFactor = 0.0f)
    {
        if (CurrentBiomass <= 0.001f) return;

        // Scarcity dampens carrying capacity and growth
        float effectiveCapacity = CarryingCapacityBiomass * (1.0f - Math.Clamp(environmentalScarcityFactor, 0.0f, 0.95f));
        float growth = IntrinsicGrowthRate * CurrentBiomass * (1.0f - (CurrentBiomass / effectiveCapacity));
        CurrentBiomass = MathF.Max(0.0f, CurrentBiomass + growth);
    }

    /// <summary>
    /// Consumes biomass via predation or harvesting. Returns actual biomass consumed.
    /// </summary>
    public float HarvestBiomass(float requestedBiomass)
    {
        if (requestedBiomass <= 0.0f || CurrentBiomass <= 0.0f) return 0.0f;

        float harvested = MathF.Min(CurrentBiomass, requestedBiomass);
        CurrentBiomass -= harvested;
        return harvested;
    }

    public void AddBiomass(float addedBiomass)
    {
        if (addedBiomass > 0.0f)
        {
            CurrentBiomass += addedBiomass;
        }
    }
}
