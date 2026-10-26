using System;
using System.Collections.Generic;
using Chimera.Core.Ecology.Behavior;
using Chimera.Core.Ecology.FoodWeb;
using Chimera.Core.Ecology.Memory;
using Chimera.Core.MathEngine;
using Chimera.Core.MathEngine.Biology;

namespace Chimera.Core.Ecology.Cycle;

public enum AbstractCreatureState : byte
{
    InDen = 0,
    RoamInRoom = 1,
    TravelBetweenRooms = 2,
    HuntingPrey = 3,
    FleeingToDen = 4,
    Hibernating = 5,
    Dead = 6
}

/// <summary>
/// Off-screen abstract creature simulation entity matching Rain World's AbstractCreature / AbstractCreatureAI.
/// Simulates off-screen lifecycle, hunger depletion, den routing, room-to-room migration, and hazard flight.
/// </summary>
public sealed class AbstractCreature
{
    public string CreatureId { get; }
    public string SpeciesId { get; }
    public TrophicLevel TrophicLevel { get; }
    public float BodySize { get; set; }
    public float MetabolicRate { get; set; }
    public float BaseMassKg => MetabolismMath.CalculateMass(BodySize);

    public CreaturePersonality Personality { get; }
    public CreatureSocialMemory SocialMemory { get; }

    public float HealthRatio { get; private set; } = 1.0f;
    public float CaloricSatisfaction { get; private set; } = 1.0f;
    public float HypothermiaRatio { get; private set; } = 0.0f;

    public string CurrentRoomId { get; private set; }
    public string HomeDenId { get; private set; }
    public bool IsInDen { get; private set; }
    public AbstractCreatureState State { get; private set; }
    public float OffscreenSpeedFactor { get; set; } = 1.0f;

    public bool IsAlive => State != AbstractCreatureState.Dead && HealthRatio > 0.0f;

    public AbstractCreature(
        string creatureId,
        string speciesId,
        TrophicLevel trophicLevel,
        string homeDenId,
        string startingRoomId,
        CreaturePersonality personality,
        float bodySize = 1.0f,
        float metabolicRate = 1.0f,
        bool startsInDen = true)
    {
        CreatureId = creatureId;
        SpeciesId = speciesId;
        TrophicLevel = trophicLevel;
        HomeDenId = homeDenId;
        CurrentRoomId = startingRoomId;
        Personality = personality;
        SocialMemory = new CreatureSocialMemory();
        BodySize = Math.Clamp(bodySize, 0.1f, 5.0f);
        MetabolicRate = Math.Clamp(metabolicRate, 0.5f, 3.0f);
        IsInDen = startsInDen;
        State = startsInDen ? AbstractCreatureState.InDen : AbstractCreatureState.RoamInRoom;
    }

    /// <summary>
    /// Executes discrete 1 Hz simulation tick update for off-screen creature.
    /// Handles metabolic energy consumption, hazard reaction, den entry/exit, and damage.
    /// </summary>
    public void StepSimulationTick(
        EnvironmentalHazardCycle hazardCycle,
        IReadOnlyDictionary<string, ShelterDen> dens,
        IReadOnlyDictionary<string, RoomPreference> roomPreferences,
        Random rng)
    {
        if (!IsAlive) return;

        // 1. Metabolic burn: rate depends on MetabolicRate and BodySize allometric scaling
        float bmrBurnPerSecond = 0.0001f * MetabolicRate * MathF.Pow(BodySize, 0.75f);
        CaloricSatisfaction = MathF.Max(0.0f, CaloricSatisfaction - bmrBurnPerSecond);

        // Starvation causes gradual health loss
        if (CaloricSatisfaction <= 0.0f)
        {
            HealthRatio = MathF.Max(0.0f, HealthRatio - 0.001f);
            if (HealthRatio <= 0.0f)
            {
                Die();
                return;
            }
        }

        // 2. Cold / Hypothermia handling during winter macro conditions
        if (hazardCycle.SeasonalHypothermiaRate > 0.0f && !IsInDen)
        {
            HypothermiaRatio = Math.Clamp(HypothermiaRatio + hazardCycle.SeasonalHypothermiaRate * 0.01f, 0.0f, 1.0f);
            if (HypothermiaRatio >= 1.0f)
            {
                HealthRatio = MathF.Max(0.0f, HealthRatio - 0.005f);
                if (HealthRatio <= 0.0f)
                {
                    Die();
                    return;
                }
            }
        }
        else if (IsInDen && HypothermiaRatio > 0.0f)
        {
            // Recover from hypothermia in den
            HypothermiaRatio = MathF.Max(0.0f, HypothermiaRatio - 0.02f);
        }

        // 3. Environmental hazard response
        if (hazardCycle.CurrentPhase >= RainHazardPhase.PrecycleTremor && !IsInDen)
        {
            // Sense danger: rush to den!
            State = AbstractCreatureState.FleeingToDen;
            if (dens.TryGetValue(HomeDenId, out var homeDen))
            {
                // If in same room as den, attempt entry
                if (CurrentRoomId == homeDen.RoomId)
                {
                    if (homeDen.TryEnter(CreatureId))
                    {
                        IsInDen = true;
                        State = AbstractCreatureState.InDen;
                    }
                }
                else
                {
                    // Move towards den room
                    CurrentRoomId = homeDen.RoomId;
                }
            }
        }
        else if (hazardCycle.CurrentPhase == RainHazardPhase.Clear && IsInDen)
        {
            // Exit den to forage when cycle is clear
            if (rng.NextDouble() < (0.1f * Personality.Energy))
            {
                if (dens.TryGetValue(HomeDenId, out var homeDen))
                {
                    homeDen.Exit(CreatureId);
                }
                IsInDen = false;
                State = AbstractCreatureState.RoamInRoom;
            }
        }

        // 4. Lethal crush damage if caught outside during lethal downpour
        if (!IsInDen && hazardCycle.CurrentPhase == RainHazardPhase.LethalDownpour)
        {
            float kineticDamage = hazardCycle.KineticCrushForceNewtons / 100000.0f; // Rapid crush damage
            HealthRatio = MathF.Max(0.0f, HealthRatio - (0.15f + kineticDamage));
            if (HealthRatio <= 0.0f)
            {
                Die();
                return;
            }
        }

        // 5. Normal foraging / roaming if outside during clear weather
        if (!IsInDen && State == AbstractCreatureState.RoamInRoom)
        {
            // Slight chance to move to adjacent room based on preference
            if (rng.NextDouble() < (0.05f * OffscreenSpeedFactor))
            {
                State = AbstractCreatureState.TravelBetweenRooms;
            }
        }
    }

    /// <summary>
    /// Feed the creature, replenishing caloric satisfaction.
    /// </summary>
    public void Feed(float nutritionRatio)
    {
        CaloricSatisfaction = Math.Clamp(CaloricSatisfaction + nutritionRatio, 0.0f, 1.0f);
    }

    /// <summary>
    /// Move directly to a target room.
    /// </summary>
    public void MoveToRoom(string newRoomId)
    {
        CurrentRoomId = newRoomId;
        State = IsInDen ? AbstractCreatureState.InDen : AbstractCreatureState.RoamInRoom;
    }

    public void Die()
    {
        HealthRatio = 0.0f;
        State = AbstractCreatureState.Dead;
    }
}
