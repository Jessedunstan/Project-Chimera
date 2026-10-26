using System;
using System.Numerics;
using Chimera.Core.MathEngine;
using Chimera.Core.MathEngine.Biology;
using Chimera.Core.MathEngine.Materials;
using Chimera.Core.MathEngine.Social;
using Chimera.Core.MathEngine.Thermodynamics;
using Xunit;

namespace Chimera.Tests;

public class MathEngineTests
{
    // ==========================================
    // 1. PHYSICAL MATERIAL PROPERTIES TESTS
    // ==========================================
    [Fact]
    public void MaterialMath_ThermalConductivity_HarmonicMeanIsSymmetricAndZeroSafe()
    {
        float k1 = 0.8f;
        float k2 = 0.2f;
        float eff = MaterialMath.CalculateEffectiveConductivity(k1, k2);

        // Effective conductivity harmonic mean must be strictly between 0 and 2 * min(k1, k2)
        Assert.True(eff > 0.0f);
        Assert.Equal(MaterialMath.CalculateEffectiveConductivity(k2, k1), eff, 4);

        // If one conductivity is zero, effective transfer must be zero
        float zeroEff = MaterialMath.CalculateEffectiveConductivity(0.0f, 0.5f);
        Assert.Equal(0.0f, zeroEff);
    }

    [Fact]
    public void MaterialMath_Deformation_BrittlenessInducesFracture_MalleabilityAbsorbsStrain()
    {
        // High brittleness material (glass / obsidian)
        var brittleMaterial = new PhysicalMaterialParameters(
            density: 2500.0f,
            rigidity: 0.9f,
            brittleness: 0.95f,
            malleability: 0.05f,
            thermalConductivity: 0.1f,
            specificHeatCapacity: 800.0f,
            meltingPoint: 1400.0f,
            boilingPoint: 2500.0f,
            tensileYield: 100.0f
        );

        // High malleability material (gold / lead / copper)
        var malleableMaterial = new PhysicalMaterialParameters(
            density: 8960.0f,
            rigidity: 0.3f,
            brittleness: 0.05f,
            malleability: 0.95f,
            thermalConductivity: 0.9f,
            specificHeatCapacity: 385.0f,
            meltingPoint: 1357.0f,
            boilingPoint: 2835.0f,
            tensileYield: 100.0f
        );

        // Stress well above yield (300 MPa vs ~145 MPa yield)
        float highStress = 300.0f;

        var brittleResult = MaterialMath.EvaluateStress(highStress, in brittleMaterial, deterministicRoll: 0.3f);
        var malleableResult = MaterialMath.EvaluateStress(highStress, in malleableMaterial, deterministicRoll: 0.3f);

        // Brittle fractures with high fracture probability
        Assert.True(brittleResult.FractureProbability > 0.8f);
        Assert.True(brittleResult.DidFracture);

        // Malleable absorbs overstress into plastic strain without fracturing
        Assert.True(malleableResult.PlasticStrain > brittleResult.PlasticStrain);
        Assert.False(malleableResult.DidFracture);
    }

    [Fact]
    public void MaterialMath_PhaseState_SmoothTransitionsBetweenSolidLiquidGas()
    {
        float meltK = 273.15f;
        float boilK = 373.15f;

        // Ice below melting band
        Assert.Equal(0.0f, MaterialMath.CalculateContinuousPhaseState(200.0f, meltK, boilK));

        // Water between melting and boiling
        Assert.Equal(1.0f, MaterialMath.CalculateContinuousPhaseState(320.0f, meltK, boilK));

        // Steam above boiling band
        Assert.Equal(2.0f, MaterialMath.CalculateContinuousPhaseState(450.0f, meltK, boilK));

        // Exactly at melting point should be mid-phase ~0.5
        float midMelt = MaterialMath.CalculateContinuousPhaseState(meltK, meltK, boilK, transitionBandK: 2.0f);
        Assert.InRange(midMelt, 0.49f, 0.51f);
    }

    // ==========================================
    // 2. BIOLOGY & GENERATIONAL METABOLISM TESTS
    // ==========================================
    [Fact]
    public void MetabolismMath_AllometricScaling_FollowsCubeAndKleiberLaws()
    {
        float baseMass = 70.0f;
        float size1Mass = MetabolismMath.CalculateMass(1.0f, baseMass);
        float size2Mass = MetabolismMath.CalculateMass(2.0f, baseMass);

        // Size 2 has 2^3 = 8x mass
        Assert.Equal(baseMass, size1Mass);
        Assert.Equal(baseMass * 8.0f, size2Mass, 2);

        // Surface area scales with S^2 (4x for size 2)
        float area1 = MetabolismMath.CalculateSurfaceArea(1.0f);
        float area2 = MetabolismMath.CalculateSurfaceArea(2.0f);
        Assert.Equal(area1 * 4.0f, area2, 2);

        // BMR follows Kleiber's Law (S^0.75)
        float bmr1 = MetabolismMath.CalculateBasalMetabolicRate(1.0f, 1.0f);
        float bmr2 = MetabolismMath.CalculateBasalMetabolicRate(2.0f, 1.0f);
        float expectedRatio = MathF.Pow(2.0f, 0.75f); // ~1.68179
        Assert.Equal(expectedRatio, bmr2 / bmr1, 2);
    }

    [Fact]
    public void MetabolismMath_GenerationalLoop_UnderEatingShrinks_OverEatingGrows_HypertrophyCaps()
    {
        float initialSize = 1.0f;

        // 1. Caloric deficit (eating 60% of required calories) -> Offspring shrinks
        float starvedOffspring = MetabolismMath.CalculateNextGenerationSize(initialSize, caloricIntakeRatio: 0.6f);
        Assert.True(starvedOffspring < initialSize);

        // 2. Caloric surplus (eating 150% of required calories) -> Offspring grows
        float fedOffspring = MetabolismMath.CalculateNextGenerationSize(initialSize, caloricIntakeRatio: 1.5f);
        Assert.True(fedOffspring > initialSize);

        // 3. Hypertrophy penalty test: Oversized creature (size 3.0) incurs steep maintenance cost
        var normalCreature = new PhysiologicalParameters(bodySize: 1.0f, metabolicRate: 1.0f);
        var giantCreature = new PhysiologicalParameters(bodySize: 3.0f, metabolicRate: 1.0f);

        float normalDemand = MetabolismMath.CalculateCaloricRequirementPerTick(in normalCreature);
        float giantDemand = MetabolismMath.CalculateCaloricRequirementPerTick(in giantCreature);

        // Giant must require significantly more than linear 3x due to Kleiber + Hypertrophy tax
        Assert.True(giantDemand > (normalDemand * 3.0f));
    }

    [Fact]
    public void MetabolismMath_AgilityScalesInverselyWithSize()
    {
        float smallAgility = MetabolismMath.CalculateAccelerationCapacity(bodySize: 0.5f, metabolicRate: 1.0f);
        float largeAgility = MetabolismMath.CalculateAccelerationCapacity(bodySize: 2.0f, metabolicRate: 1.0f);

        // Smaller creatures can accelerate and turn much faster than huge creatures
        Assert.True(smallAgility > largeAgility);
        Assert.Equal(4.0f, smallAgility / largeAgility, 2);
    }

    // ==========================================
    // 3. THERMODYNAMIC SPELL & REACTION TESTS
    // ==========================================
    [Fact]
    public void SpellPhysicsMath_UnifiedThermodynamics_FireAndIceShareAxis()
    {
        float ambientTemp = 300.0f; // 27 C

        // Heating (+deltaT) raises temperature
        float heated = SpellPhysicsMath.ApplyThermodynamicImpulse(ambientTemp, deltaTK: 500.0f, ambientHumidity: 0.0f);
        Assert.True(heated > ambientTemp);

        // Cooling (-deltaT) drops temperature along same function
        float frozen = SpellPhysicsMath.ApplyThermodynamicImpulse(ambientTemp, deltaTK: -200.0f, ambientHumidity: 0.0f);
        Assert.True(frozen < ambientTemp);

        // High humidity dampens fire warming, but intensifies cryogenic cooling
        float wetHeated = SpellPhysicsMath.ApplyThermodynamicImpulse(ambientTemp, deltaTK: 500.0f, ambientHumidity: 0.8f);
        float wetFrozen = SpellPhysicsMath.ApplyThermodynamicImpulse(ambientTemp, deltaTK: -200.0f, ambientHumidity: 0.8f);

        Assert.True(wetHeated < heated); // Dampened fire
        Assert.True(wetFrozen < frozen); // Enhanced cooling conduction
    }

    [Fact]
    public void SpellPhysicsMath_PlanckianParticleColor_MapsSmoothContinuousSpectrum()
    {
        // Cryogenic temp (80K) must have strong blue component
        Vector4 cryoColor = SpellPhysicsMath.MapTemperatureToParticleColor(80.0f);
        Assert.True(cryoColor.Z > cryoColor.X);

        // Flame temp (1200K) must have strong red/orange component
        Vector4 flameColor = SpellPhysicsMath.MapTemperatureToParticleColor(1200.0f);
        Assert.True(flameColor.X > flameColor.Z);

        // Arc plasma temp (5000K) must have high brightness across RGB
        Vector4 plasmaColor = SpellPhysicsMath.MapTemperatureToParticleColor(5000.0f);
        Assert.True(plasmaColor.X > 0.6f && plasmaColor.Y > 0.8f && plasmaColor.Z > 0.9f);
    }

    [Fact]
    public void SpellPhysicsMath_AcousticDampingAndStun_CalculationsAreBounded()
    {
        float loudRadius = SpellPhysicsMath.CalculateSoundPropagationRadius(kineticForceMagnitudeN: 1000.0f, acousticDamping: 0.0f);
        float silentRadius = SpellPhysicsMath.CalculateSoundPropagationRadius(kineticForceMagnitudeN: 1000.0f, acousticDamping: 1.0f);

        Assert.True(loudRadius > 0.0f);
        Assert.Equal(0.0f, silentRadius);

        // Concussive + neural stun on target
        float stunSeconds = SpellPhysicsMath.CalculateStunDuration(
            kineticForceMagnitudeN: 5000.0f,
            neuralDisruption: 0.8f,
            targetMassKg: 70.0f,
            constitution: 1.0f
        );

        Assert.True(stunSeconds > 0.0f && stunSeconds <= 10.0f);
    }

    // ==========================================
    // 4. SOCIAL OPINION & IDEOLOGICAL MATH TESTS
    // ==========================================
    [Fact]
    public void SocialMath_OpinionCalculation_AdditiveAndMultiplicativeModifiers()
    {
        // Scenario 1: Harmonious faith (+1.0), No feud (0.0), Friendly sports alignment (+0.2)
        var friendlyParams = new SocialOpinionParameters(
            religiousCompatibility: 1.0f,
            clanFeudScore: 0.0f,
            sportsRivalryAlignment: 0.2f,
            evaluatorNeuroticism: 0.1f,
            evaluatorDevotion: 0.8f,
            evaluatorTribalism: 0.5f
        );

        float positiveOpinion = SocialMath.CalculateBilateralOpinion(in friendlyParams);
        Assert.True(positiveOpinion > 20.0f);

        // Scenario 2: Opposing religion (-1.0), Blood feud (-1.0), Sports rivalry (-0.2)
        var hostileParams = new SocialOpinionParameters(
            religiousCompatibility: -1.0f,
            clanFeudScore: -1.0f,
            sportsRivalryAlignment: -0.2f,
            evaluatorNeuroticism: 0.0f,
            evaluatorDevotion: 1.0f,
            evaluatorTribalism: 1.0f
        );

        float hostileOpinion = SocialMath.CalculateBilateralOpinion(in hostileParams);
        Assert.True(hostileOpinion < -50.0f);
    }

    [Fact]
    public void SocialMath_Neuroticism_AmplifiesHostilityAndDepressesBreakThresholds()
    {
        // A negative slight (-10 points) evaluated by a calm person vs a neurotic person
        var calmParams = new SocialOpinionParameters(
            religiousCompatibility: 0.0f,
            clanFeudScore: 0.0f,
            sportsRivalryAlignment: 0.0f,
            evaluatorNeuroticism: 0.0f,
            decayedMemoriesSum: -10.0f
        );

        var neuroticParams = new SocialOpinionParameters(
            religiousCompatibility: 0.0f,
            clanFeudScore: 0.0f,
            sportsRivalryAlignment: 0.0f,
            evaluatorNeuroticism: 0.9f,
            decayedMemoriesSum: -10.0f
        );

        float calmOpinion = SocialMath.CalculateBilateralOpinion(in calmParams);
        float neuroticOpinion = SocialMath.CalculateBilateralOpinion(in neuroticParams);

        // Neurotic evaluator turns -10 into more severe hatred
        Assert.True(neuroticOpinion < calmOpinion);

        // Mental break thresholds: neurotic person breaks much sooner (higher break mood threshold)
        var calmBreak = SocialMath.CalculateMentalBreakThresholds(neuroticism: 0.1f, ironWill: 0.8f);
        var neuroticBreak = SocialMath.CalculateMentalBreakThresholds(neuroticism: 0.9f, ironWill: 0.1f);

        Assert.True(neuroticBreak.MinorThreshold > calmBreak.MinorThreshold);
        Assert.True(neuroticBreak.MajorThreshold > calmBreak.MajorThreshold);
        Assert.True(neuroticBreak.ExtremeThreshold > calmBreak.ExtremeThreshold);
    }

    [Fact]
    public void SocialMath_GenerationalFeudDecaysExponentially()
    {
        float originalFeud = -1.0f; // Maximum blood feud
        float gen1 = SocialMath.DecayGenerationalFeud(originalFeud, feudDecayRate: 0.35f, generationsElapsed: 1);
        float gen3 = SocialMath.DecayGenerationalFeud(originalFeud, feudDecayRate: 0.35f, generationsElapsed: 3);

        Assert.True(gen1 > originalFeud); // Closer to 0
        Assert.True(gen3 > gen1);         // Further softened over 3 generations
    }
}
