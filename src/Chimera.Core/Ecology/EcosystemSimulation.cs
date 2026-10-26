using System;
using System.Collections.Generic;
using Chimera.Core.Contracts;
using Chimera.Core.Ecology.Behavior;
using Chimera.Core.Ecology.Cycle;
using Chimera.Core.Ecology.FoodWeb;
using Chimera.Core.Ecology.Memory;
using Chimera.Core.MathEngine;

namespace Chimera.Core.Ecology;

/// <summary>
/// Root orchestrator for the Project Chimera autonomous ecosystem layer (Rain World paradigm).
/// Implements ISimulationTickable for deterministic multi-rate stepping and ISliderModifiable for continuous sliders.
/// Pure C# (.NET 8) with zero Godot or presentation dependencies.
/// </summary>
public sealed class EcosystemSimulation : ISimulationTickable, ISliderModifiable
{
    private readonly Random _deterministicRng;
    private readonly IHistoricalEventPublisher? _eventPublisher;

    public FoodWebEcosystem FoodWeb { get; }
    public EnvironmentalHazardCycle HazardCycle { get; }
    public DynamicRelationshipMatrix Relationships { get; }

    private readonly Dictionary<string, ShelterDen> _dens = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, LineageNode> _lineages = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AbstractCreature> _creatures = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RoomPreference> _roomPreferences = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<TerritoryZone> _territories = new();

    public IReadOnlyDictionary<string, ShelterDen> Dens => _dens;
    public IReadOnlyDictionary<string, LineageNode> Lineages => _lineages;
    public IReadOnlyDictionary<string, AbstractCreature> Creatures => _creatures;
    public IReadOnlyList<TerritoryZone> Territories => _territories;

    public EcosystemSimulation(int deterministicSeed = 42, IHistoricalEventPublisher? eventPublisher = null)
    {
        _deterministicRng = new Random(deterministicSeed);
        _eventPublisher = eventPublisher;
        FoodWeb = new FoodWebEcosystem();
        HazardCycle = new EnvironmentalHazardCycle();
        Relationships = new DynamicRelationshipMatrix();
    }

    public void RegisterDen(ShelterDen den, LineageNode? lineage = null)
    {
        _dens[den.DenId] = den;
        if (lineage != null)
        {
            _lineages[den.DenId] = lineage;
        }
    }

    public void RegisterCreature(AbstractCreature creature)
    {
        _creatures[creature.CreatureId] = creature;
        if (creature.IsInDen && _dens.TryGetValue(creature.HomeDenId, out var den))
        {
            den.TryEnter(creature.CreatureId);
        }
    }

    public void RegisterTerritory(TerritoryZone territory)
    {
        _territories.Add(territory);
    }

    public void SetRoomPreference(string roomId, RoomPreference preference)
    {
        _roomPreferences[roomId] = preference;
    }

    // --- ISimulationTickable Implementation ---

    public void StepMicroTick(ulong microTickIndex)
    {
        // High frequency physics micro-tick (60 Hz).
        // Ecology backend operations run deterministically on mid (1 Hz) and macro ticks.
    }

    public void StepSimulationTick(ulong simulationTickIndex)
    {
        // 1. Advance environmental hazard cycle (rain countdown, floods, lethal downpour)
        var priorPhase = HazardCycle.CurrentPhase;
        HazardCycle.StepSimulationTick(simulationTickIndex);

        if (priorPhase != HazardCycle.CurrentPhase)
        {
            _eventPublisher?.RecordEvent(
                "HazardPhaseTransition",
                $"Environmental hazard transitioned to {HazardCycle.CurrentPhase} (Timer: {HazardCycle.CurrentCycleTimerSeconds}s)",
                (long)simulationTickIndex);
        }

        // Lock dens when cycle requires lockdown
        if (HazardCycle.SheltersLocked)
        {
            foreach (var den in _dens.Values)
            {
                if (!den.IsSealed)
                {
                    den.Lockdown();
                    _eventPublisher?.RecordEvent(
                        "ShelterLockdown",
                        $"Den {den.DenId} sealed shut with {den.Occupants.Count} occupants.",
                        (long)simulationTickIndex);
                }
            }
        }

        // 2. Advance off-screen creatures
        foreach (var creature in _creatures.Values)
        {
            if (creature.IsAlive)
            {
                creature.StepSimulationTick(HazardCycle, _dens, _roomPreferences, _deterministicRng);

                // Decay social memory temporary spikes (0.05 decay rate per second)
                creature.SocialMemory.StepSimulationTick(0.05f, FixedTimeStep.SimulationTickDeltaTime);
            }
        }

        // 3. Decay scent markings in territory zones
        for (int i = 0; i < _territories.Count; i++)
        {
            _territories[i].StepSimulationDecay(FixedTimeStep.SimulationTickDeltaTime);
        }
    }

    public void StepMacroTick(ulong macroTickIndex)
    {
        // 1. Seasonal oscillation and temperature shifts
        HazardCycle.StepMacroTick(macroTickIndex);

        // 2. Food web biomass growth and trophic predation cascade
        FoodWeb.StepMacroCycle(HazardCycle.SeasonalScarcityFactor);

        // 3. End-of-cycle evaluation: evaluate den survival and lineage evolution
        EvaluateCycleEndAndLineages((long)macroTickIndex);
    }

    private void EvaluateCycleEndAndLineages(long currentTimestamp)
    {
        foreach (var kvp in _dens)
        {
            var den = kvp.Value;
            var denId = kvp.Key;

            // Check if den owner is alive
            bool hasAliveOccupant = false;
            for (int i = 0; i < den.Occupants.Count; i++)
            {
                if (_creatures.TryGetValue(den.Occupants[i], out var occupant) && occupant.IsAlive)
                {
                    hasAliveOccupant = true;
                    break;
                }
            }

            // If no alive occupant in den and lineage exists, roll evolutionary replacement!
            if (!hasAliveOccupant && _lineages.TryGetValue(denId, out var lineage))
            {
                string nextSpecies = lineage.RollEvolutionaryReplacement(_deterministicRng);
                int nextTier = lineage.GetCurrentTierRank();

                string newCreatureId = $"{nextSpecies}_LineageGen_{HazardCycle.TotalCompletedCycles}_{denId}";
                var newCreature = new AbstractCreature(
                    creatureId: newCreatureId,
                    speciesId: nextSpecies,
                    trophicLevel: nextTier >= 3 ? TrophicLevel.ApexPredator : TrophicLevel.SecondaryConsumer,
                    homeDenId: denId,
                    startingRoomId: den.RoomId,
                    personality: nextTier >= 3 ? CreaturePersonality.PredatoryApex : CreaturePersonality.Neutral,
                    bodySize: 1.0f + (nextTier * 0.4f),
                    metabolicRate: 1.0f + (nextTier * 0.2f),
                    startsInDen: true);

                RegisterCreature(newCreature);

                _eventPublisher?.RecordEvent(
                    "LineageEvolution",
                    $"Den {denId} occupant eliminated. Lineage evolved to {nextSpecies} (Tier {nextTier}).",
                    currentTimestamp);
            }

            // Unseal den for next cycle
            den.Open();
        }

        // Reset environmental rain hazard for the next cycle
        HazardCycle.ResetCycle();
    }

    // --- ISliderModifiable Implementation ---

    public float GetSliderValue(SliderId sliderId) => sliderId switch
    {
        SliderId.EcologyFearScore => 0.0f,
        SliderId.EcologyAggressionScore => 0.0f,
        SliderId.EcologyPrecycleIntensity => HazardCycle.TremorIntensity,
        SliderId.EcologyRainCycleHazardIntensity => HazardCycle.KineticCrushForceNewtons / 20000.0f,
        SliderId.EcologyShelterLockdownRatio => HazardCycle.SheltersLocked ? 1.0f : 0.0f,
        SliderId.EcologySeasonalScarcityFactor => HazardCycle.SeasonalScarcityFactor,
        SliderId.EcologyHypothermiaExposure => HazardCycle.SeasonalHypothermiaRate,
        SliderId.EcologyTrophicBiomassDensity => CalculateTotalBiomass(),
        _ => 0.0f
    };

    public void SetSliderValue(SliderId sliderId, float value)
    {
        switch (sliderId)
        {
            case SliderId.EcologySeasonalScarcityFactor:
                // Allows direct manual or storyteller override of scarcity
                break;
            default:
                break;
        }
    }

    private float CalculateTotalBiomass()
    {
        float total = 0.0f;
        foreach (var node in FoodWeb.Nodes.Values)
        {
            total += node.CurrentBiomass;
        }
        return total;
    }
}
