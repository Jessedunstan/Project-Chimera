using System;
using System.Numerics;
using Chimera.Core.Contracts;
using Chimera.Core.Ecology;
using Chimera.Core.Ecology.Behavior;
using Chimera.Core.Ecology.Cycle;
using Chimera.Core.Ecology.FoodWeb;
using Chimera.Core.Ecology.Memory;
using Chimera.Core.MathEngine;
using Xunit;

namespace Chimera.Tests;

public class EcologyTests
{
    [Fact]
    public void FoodWeb_LogisticGrowthAndPredation_CascadesBiomass()
    {
        var foodWeb = new FoodWebEcosystem();
        var plants = new TrophicBiomassNode("Moss", TrophicLevel.PrimaryProducer, initialBiomass: 1000f, carryingCapacity: 2000f);
        var bugs = new TrophicBiomassNode("Batflies", TrophicLevel.PrimaryConsumer, initialBiomass: 200f, carryingCapacity: 500f, individualMassKg: 0.1f);
        var lizard = new TrophicBiomassNode("PinkLizard", TrophicLevel.SecondaryConsumer, initialBiomass: 50f, carryingCapacity: 100f, individualMassKg: 10f);

        foodWeb.AddNode(plants);
        foodWeb.AddNode(bugs);
        foodWeb.AddNode(lizard);

        foodWeb.AddPredationLink("Batflies", "Moss", preferenceWeight: 1.0f, consumptionRatePerPredatorKg: 0.1f);
        foodWeb.AddPredationLink("PinkLizard", "Batflies", preferenceWeight: 1.0f, consumptionRatePerPredatorKg: 0.2f);

        float initialBugBiomass = bugs.CurrentBiomass;
        float initialLizardBiomass = lizard.CurrentBiomass;

        // Step macro cycle
        foodWeb.StepMacroCycle(seasonalScarcityFactor: 0.0f);

        // Plants grew, bugs grazed plants, lizards grazed bugs
        Assert.True(plants.CurrentBiomass > 0.0f);
        Assert.True(lizard.CurrentBiomass > 0.0f);
    }

    [Fact]
    public void SwarmDepletionTracker_FollowsRainWorldDepletionSpecs()
    {
        var rng = new Random(42);
        var swarm = new SwarmDepletionTracker(maxCapacity: 7, depleteCyclesMin: 2, depleteCyclesMax: 7, depleteCyclesMaxIfLessThanTwoLeft: 3);

        Assert.Equal(7, swarm.CurrentCount);
        Assert.False(swarm.IsDepleted);

        // Harvest 6 bats (leaves 1)
        int taken = swarm.Harvest(6, rng);
        Assert.Equal(6, taken);
        Assert.Equal(1, swarm.CurrentCount);

        // Cycle 1 recovery: still depleted
        swarm.StepCycleRecovery();
        Assert.True(swarm.CurrentCount <= 1);

        // Additional cycles to restock
        for (int i = 0; i < 4; i++)
        {
            swarm.StepCycleRecovery();
        }

        Assert.Equal(7, swarm.CurrentCount);
    }

    [Fact]
    public void FearAggressionEvaluator_MassivePredator_InducesHighFearInSmallPrey()
    {
        var preyPersonality = CreaturePersonality.SkittishPrey;
        var input = new FearAggressionEvaluator.EvaluationInput(
            BaseFear: 0.5f,
            BaseAggression: 0.0f,
            SelfMassKg: 5.0f,     // Small slugcat
            OtherMassKg: 200.0f,  // Massive vulture
            SelfHealthRatio: 1.0f,
            SelfCaloricSatisfaction: 1.0f,
            TerritoryFamiliarity: 0.0f,
            Provocation: 0.0f,
            Personality: preyPersonality);

        var result = FearAggressionEvaluator.Evaluate(input, RelationshipType.Afraid);

        Assert.True(result.DynamicFear > 0.7f, $"Expected high fear but got {result.DynamicFear}");
        Assert.Equal(RelationshipType.Afraid, result.DominantIntent);
    }

    [Fact]
    public void FearAggressionEvaluator_StarvingPredator_IncreasesAggression()
    {
        var predatorPersonality = CreaturePersonality.PredatoryApex;
        var satiatedInput = new FearAggressionEvaluator.EvaluationInput(
            BaseFear: 0.1f,
            BaseAggression: 0.5f,
            SelfMassKg: 80.0f,
            OtherMassKg: 10.0f,
            SelfHealthRatio: 1.0f,
            SelfCaloricSatisfaction: 1.0f, // Full
            TerritoryFamiliarity: 0.5f,
            Provocation: 0.0f,
            Personality: predatorPersonality);

        var starvingInput = satiatedInput with
        {
            SelfCaloricSatisfaction = 0.0f // Starving
        };

        var satiatedResult = FearAggressionEvaluator.Evaluate(satiatedInput, RelationshipType.Eats);
        var starvingResult = FearAggressionEvaluator.Evaluate(starvingInput, RelationshipType.Eats);

        Assert.True(starvingResult.DynamicAggression > satiatedResult.DynamicAggression,
            "Starving predator must exhibit higher aggression than satiated predator.");
        Assert.Equal(RelationshipType.Eats, starvingResult.DominantIntent);
    }

    [Fact]
    public void SocialMemory_TemporaryEmotionsDecay_AndBakeIntoLongTermMemory()
    {
        var memory = new CreatureSocialMemory();
        var rel = memory.GetOrInitiate("PawnAlpha");

        // Inflict sudden trauma/fear
        rel.InfluenceTempFear(0.8f);
        Assert.True(rel.EffectiveFear >= 0.8f);
        Assert.Equal(0.0f, rel.Fear); // Long term initially untouched

        // Step decay over time
        for (int i = 0; i < 20; i++)
        {
            rel.EvenOutTemps(decayRate: 0.5f, dt: 1.0f);
        }

        // TempFear should have decayed close to 0, but baked part of it into persistent Fear
        Assert.True(rel.TempFear < 0.05f, "Temp fear should decay away.");
        Assert.True(rel.Fear > 0.05f, "A fraction of temp fear should bake into permanent fear.");
    }

    [Fact]
    public void TerritoryDispute_ResidentWithHomeAdvantage_DisplacesWeakerIntruder()
    {
        var territory = new TerritoryZone("ResidentA", Vector3.Zero, coreRadiusMeters: 20.0f, initialScent: 1.0f);
        var input = new TerritoryDisputeResolver.DisputeInput(
            ResidentMassKg: 50.0f,
            IntruderMassKg: 30.0f,
            ResidentPersonality: CreaturePersonality.PredatoryApex,
            IntruderPersonality: CreaturePersonality.Neutral,
            ResidentFamiliarity: 0.9f,
            ResidentHealthRatio: 1.0f,
            IntruderHealthRatio: 0.8f,
            IntruderFearOfResident: 0.5f);

        // Intruder is 5 meters from core den
        var result = TerritoryDisputeResolver.ResolveDispute(territory, new Vector3(5, 0, 0), input);

        Assert.Equal(DisputeOutcome.IntruderYields, result.Outcome);
        Assert.True(result.ResidentDefenseScore > result.IntruderChallengeScore);
    }

    [Fact]
    public void EnvironmentalHazardCycle_PhasesTransitionDeterministicAccordingToSpecs()
    {
        // 600s cycle: warning at 480s, water accumulation at 600s, lethal downpour at 630s
        var hazard = new EnvironmentalHazardCycle(cycleDurationSeconds: 600);

        // At T = 100s: Clear
        for (ulong t = 0; t < 100; t++) hazard.StepSimulationTick(t);
        Assert.Equal(RainHazardPhase.Clear, hazard.CurrentPhase);
        Assert.Equal(0.0f, hazard.TremorIntensity);
        Assert.False(hazard.SheltersLocked);

        // At T = 500s: PrecycleTremor (warning lead time 120s started at 480s)
        for (ulong t = 100; t < 500; t++) hazard.StepSimulationTick(t);
        Assert.Equal(RainHazardPhase.PrecycleTremor, hazard.CurrentPhase);
        Assert.True(hazard.TremorIntensity > 0.0f);
        Assert.False(hazard.SheltersLocked);

        // At T = 625s: WaterAccumulation and shelters locked
        for (ulong t = 500; t < 625; t++) hazard.StepSimulationTick(t);
        Assert.Equal(RainHazardPhase.WaterAccumulation, hazard.CurrentPhase);
        Assert.True(hazard.FloodLevelMeters > 0.0f);
        Assert.True(hazard.SheltersLocked);

        // At T = 640s: LethalDownpour
        for (ulong t = 625; t < 640; t++) hazard.StepSimulationTick(t);
        Assert.Equal(RainHazardPhase.LethalDownpour, hazard.CurrentPhase);
        Assert.True(hazard.KineticCrushForceNewtons >= 15000.0f);
    }

    [Fact]
    public void AbstractCreature_SeeksDenDuringWarning_AndSurvives()
    {
        var sim = new EcosystemSimulation(deterministicSeed: 12345);
        var den = new ShelterDen("Den_Alpha", "Room_01", Vector3.Zero, maxCapacity: 2);
        sim.RegisterDen(den);

        var creature = new AbstractCreature(
            creatureId: "Lizard_01",
            speciesId: "GreenLizard",
            trophicLevel: TrophicLevel.SecondaryConsumer,
            homeDenId: "Den_Alpha",
            startingRoomId: "Room_01",
            personality: CreaturePersonality.Neutral,
            startsInDen: false);

        sim.RegisterCreature(creature);
        Assert.False(creature.IsInDen);

        // Fast-forward simulation ticks into pre-cycle warning
        for (ulong t = 0; t < 500; t++)
        {
            sim.StepSimulationTick(t);
        }

        // Creature should have sensed tremor and entered den
        Assert.True(creature.IsInDen);
        Assert.Contains(creature.CreatureId, den.Occupants);
        Assert.True(creature.IsAlive);
    }

    [Fact]
    public void AbstractCreature_StrandedOutsideInLethalRain_SuffersFatalCrush()
    {
        var sim = new EcosystemSimulation(deterministicSeed: 999);
        var den = new ShelterDen("Den_Safe", "Room_Safe", Vector3.Zero, maxCapacity: 2);
        sim.RegisterDen(den);

        // Creature has no valid path to den (different room) and starts outside
        var creature = new AbstractCreature(
            creatureId: "DoomedPrey",
            speciesId: "Batfly",
            trophicLevel: TrophicLevel.PrimaryConsumer,
            homeDenId: "NonExistentDen",
            startingRoomId: "Room_Exposed",
            personality: CreaturePersonality.SkittishPrey,
            startsInDen: false);

        sim.RegisterCreature(creature);

        // Advance to lethal downpour (> 630 seconds)
        for (ulong t = 0; t < 650; t++)
        {
            sim.StepSimulationTick(t);
        }

        Assert.False(creature.IsInDen);
        Assert.False(creature.IsAlive, "Exposed creature must perish under lethal rain crush forces.");
    }

    [Fact]
    public void Lineage_EvolvesNextTier_WhenDenOccupantDies()
    {
        var sim = new EcosystemSimulation(deterministicSeed: 777);
        var den = new ShelterDen("Den_LineageTest", "Room_Apex", Vector3.Zero, maxCapacity: 1);
        var lineage = new LineageNode("Den_LineageTest");
        lineage.AddTier("PinkLizard", probabilityWeight: 1.0f, tierRank: 1);
        lineage.AddTier("RedLizardApex", probabilityWeight: 1.0f, tierRank: 3);

        sim.RegisterDen(den, lineage);

        var occupant = new AbstractCreature(
            creatureId: "OldResident",
            speciesId: "PinkLizard",
            trophicLevel: TrophicLevel.SecondaryConsumer,
            homeDenId: "Den_LineageTest",
            startingRoomId: "Room_Apex",
            personality: CreaturePersonality.Neutral,
            startsInDen: true);

        sim.RegisterCreature(occupant);

        // Kill occupant
        occupant.Die();
        Assert.False(occupant.IsAlive);

        // Step macro tick to trigger cycle end evaluation
        sim.StepMacroTick(1UL);

        // Lineage should have evolved and spawned a replacement
        Assert.True(sim.Creatures.Count >= 2);
    }
}
