using System;
using System.Collections.Generic;
using Chimera.Core.Contracts;

namespace Chimera.Core.MathEngine;

public enum SliderCategory : byte
{
    Material = 1,
    Biology = 2,
    Thermodynamics = 3,
    Social = 4,
    Ecology = 5
}

public enum SliderCurveType : byte
{
    Linear = 0,
    Sigmoid = 1,
    AllometricPower = 2,
    Exponential = 3,
    Harmonic = 4
}

/// <summary>
/// Immutable schema definition for a continuous slider parameter.
/// </summary>
public readonly struct SliderDefinition
{
    public SliderId Id { get; }
    public SliderCategory Category { get; }
    public string Name { get; }
    public string Unit { get; }
    public float MinValue { get; }
    public float MaxValue { get; }
    public float DefaultValue { get; }
    public SliderCurveType CurveType { get; }

    public SliderDefinition(
        SliderId id,
        SliderCategory category,
        string name,
        string unit,
        float minValue,
        float maxValue,
        float defaultValue,
        SliderCurveType curveType = SliderCurveType.Linear)
    {
        Id = id;
        Category = category;
        Name = name;
        Unit = unit;
        MinValue = minValue;
        MaxValue = maxValue;
        DefaultValue = Math.Clamp(defaultValue, minValue, maxValue);
        CurveType = curveType;
    }

    public float Clamp(float value) => Math.Clamp(value, MinValue, MaxValue);
}

/// <summary>
/// High-performance, zero-allocation parameter bank for storing and modifying continuous sliders.
/// Uses fixed-size flat array indexed by SliderId (ushort) with O(1) performance.
/// </summary>
public sealed class ContinuousParameterBank : ISliderModifiable
{
    private const int MaxSliderCapacity = 512;
    private readonly float[] _values = new float[MaxSliderCapacity];
    private static readonly Dictionary<string, SliderId> StringToIdMap = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<SliderId, SliderDefinition> Registry = new();

    static ContinuousParameterBank()
    {
        RegisterAllDefaults();
    }

    public ContinuousParameterBank()
    {
        // Initialize all parameters to their registered default values
        foreach (var kvp in Registry)
        {
            ushort idx = (ushort)kvp.Key;
            if (idx < MaxSliderCapacity)
            {
                _values[idx] = kvp.Value.DefaultValue;
            }
        }
    }

    public static void Register(SliderDefinition def)
    {
        Registry[def.Id] = def;
        StringToIdMap[def.Name] = def.Id;
        StringToIdMap[def.Id.ToString()] = def.Id;
    }

    public static bool TryGetDefinition(SliderId id, out SliderDefinition def)
    {
        return Registry.TryGetValue(id, out def);
    }

    public static IReadOnlyCollection<SliderDefinition> AllDefinitions => Registry.Values;

    public float GetSliderValue(SliderId sliderId)
    {
        ushort idx = (ushort)sliderId;
        return idx < MaxSliderCapacity ? _values[idx] : 0.0f;
    }

    public void SetSliderValue(SliderId sliderId, float value)
    {
        ushort idx = (ushort)sliderId;
        if (idx < MaxSliderCapacity)
        {
            if (Registry.TryGetValue(sliderId, out var def))
            {
                _values[idx] = def.Clamp(value);
            }
            else
            {
                _values[idx] = value;
            }
        }
    }

    public float GetSliderValue(string parameterKey)
    {
        if (StringToIdMap.TryGetValue(parameterKey, out var id))
        {
            return GetSliderValue(id);
        }
        return 0.0f;
    }

    public void SetSliderValue(string parameterKey, float value)
    {
        if (StringToIdMap.TryGetValue(parameterKey, out var id))
        {
            SetSliderValue(id, value);
        }
    }

    private static void RegisterAllDefaults()
    {
        // Material Sliders
        Register(new SliderDefinition(SliderId.MaterialDensity, SliderCategory.Material, "Density", "kg/m^3", 1.0f, 30000.0f, 2500.0f));
        Register(new SliderDefinition(SliderId.MaterialRigidity, SliderCategory.Material, "Rigidity", "normalized [0..1]", 0.0f, 1.0f, 0.5f));
        Register(new SliderDefinition(SliderId.MaterialBrittleness, SliderCategory.Material, "Brittleness", "normalized [0..1]", 0.0f, 1.0f, 0.3f));
        Register(new SliderDefinition(SliderId.MaterialMalleability, SliderCategory.Material, "Malleability", "normalized [0..1]", 0.0f, 1.0f, 0.4f));
        Register(new SliderDefinition(SliderId.MaterialThermalConductivity, SliderCategory.Material, "ThermalConductivity", "normalized [0..1]", 0.0f, 1.0f, 0.5f, SliderCurveType.Harmonic));
        Register(new SliderDefinition(SliderId.MaterialSpecificHeatCapacity, SliderCategory.Material, "SpecificHeatCapacity", "J/(kg*K)", 50.0f, 5000.0f, 900.0f));
        Register(new SliderDefinition(SliderId.MaterialMeltingPoint, SliderCategory.Material, "MeltingPoint", "K", 0.0f, 6000.0f, 1350.0f));
        Register(new SliderDefinition(SliderId.MaterialBoilingPoint, SliderCategory.Material, "BoilingPoint", "K", 0.0f, 10000.0f, 2500.0f));
        Register(new SliderDefinition(SliderId.MaterialFrictionCoefficient, SliderCategory.Material, "FrictionCoefficient", "normalized [0..1]", 0.0f, 1.0f, 0.6f));
        Register(new SliderDefinition(SliderId.MaterialTensileYield, SliderCategory.Material, "TensileYield", "MPa", 0.1f, 5000.0f, 250.0f));

        // Biological Sliders
        Register(new SliderDefinition(SliderId.BioBodySize, SliderCategory.Biology, "BodySize", "scalar [0.1..5.0]", 0.1f, 5.0f, 1.0f, SliderCurveType.AllometricPower));
        Register(new SliderDefinition(SliderId.BioMetabolicRate, SliderCategory.Biology, "MetabolicRate", "scalar [0.5..3.0]", 0.5f, 3.0f, 1.0f));
        Register(new SliderDefinition(SliderId.BioBioluminescenceIntensity, SliderCategory.Biology, "BioluminescenceIntensity", "normalized [0..1]", 0.0f, 1.0f, 0.0f));
        Register(new SliderDefinition(SliderId.BioChromaticAdaptationRate, SliderCategory.Biology, "ChromaticAdaptationRate", "normalized [0..1]", 0.0f, 1.0f, 0.1f));
        Register(new SliderDefinition(SliderId.BioCaloricIntakeRatio, SliderCategory.Biology, "CaloricIntakeRatio", "ratio [0..3.0]", 0.0f, 3.0f, 1.0f));
        Register(new SliderDefinition(SliderId.BioGenerationalGrowthFactor, SliderCategory.Biology, "GenerationalGrowthFactor", "scalar [0..1.0]", 0.0f, 1.0f, 0.15f));
        Register(new SliderDefinition(SliderId.BioMutationRate, SliderCategory.Biology, "MutationRate", "std dev", 0.0f, 0.5f, 0.05f));
        Register(new SliderDefinition(SliderId.BioActivityLevel, SliderCategory.Biology, "ActivityLevel", "scalar [0..3.0]", 0.0f, 3.0f, 0.2f));
        Register(new SliderDefinition(SliderId.BioBaseMass, SliderCategory.Biology, "BaseMass", "kg", 1.0f, 500.0f, 70.0f));
        Register(new SliderDefinition(SliderId.BioHypertrophicPenaltyCoeff, SliderCategory.Biology, "HypertrophicPenaltyCoeff", "scalar", 0.0f, 2.0f, 0.4f));

        // Thermodynamic Sliders
        Register(new SliderDefinition(SliderId.ThermoTemperatureDelta, SliderCategory.Thermodynamics, "TemperatureDelta", "K", -1000.0f, 3000.0f, 0.0f));
        Register(new SliderDefinition(SliderId.ThermoForceVectorX, SliderCategory.Thermodynamics, "ForceVectorX", "N", -10000.0f, 10000.0f, 0.0f));
        Register(new SliderDefinition(SliderId.ThermoForceVectorY, SliderCategory.Thermodynamics, "ForceVectorY", "N", -10000.0f, 10000.0f, 0.0f));
        Register(new SliderDefinition(SliderId.ThermoForceVectorZ, SliderCategory.Thermodynamics, "ForceVectorZ", "N", -10000.0f, 10000.0f, 0.0f));
        Register(new SliderDefinition(SliderId.ThermoAcousticDamping, SliderCategory.Thermodynamics, "AcousticDamping", "normalized [0..1]", 0.0f, 1.0f, 0.0f));
        Register(new SliderDefinition(SliderId.ThermoNeuralDisruption, SliderCategory.Thermodynamics, "NeuralDisruption", "normalized [0..1]", 0.0f, 1.0f, 0.0f));
        Register(new SliderDefinition(SliderId.ThermoPhotonicFlux, SliderCategory.Thermodynamics, "PhotonicFlux", "normalized [0..1]", 0.0f, 1.0f, 0.0f));
        Register(new SliderDefinition(SliderId.ThermoAmbientHumidity, SliderCategory.Thermodynamics, "AmbientHumidity", "normalized [0..1]", 0.0f, 1.0f, 0.3f));
        Register(new SliderDefinition(SliderId.ThermoFuel, SliderCategory.Thermodynamics, "Fuel", "normalized [0..1]", 0.0f, 1.0f, 0.0f));
        Register(new SliderDefinition(SliderId.ThermoOxidant, SliderCategory.Thermodynamics, "Oxidant", "normalized [0..1]", 0.0f, 1.0f, 0.21f));
        Register(new SliderDefinition(SliderId.ThermoThermalInertia, SliderCategory.Thermodynamics, "ThermalInertia", "scalar", 0.1f, 10.0f, 1.0f));
        Register(new SliderDefinition(SliderId.ThermoCombustionActivationEnergy, SliderCategory.Thermodynamics, "CombustionActivationEnergy", "kJ/mol", 10.0f, 200.0f, 50.0f));

        // Social Sliders
        Register(new SliderDefinition(SliderId.SocialReligiousCompatibility, SliderCategory.Social, "ReligiousCompatibility", "normalized [-1..1]", -1.0f, 1.0f, 0.0f));
        Register(new SliderDefinition(SliderId.SocialClanFeudScore, SliderCategory.Social, "ClanFeudScore", "normalized [-1..0]", -1.0f, 0.0f, 0.0f));
        Register(new SliderDefinition(SliderId.SocialSportsRivalryAlignment, SliderCategory.Social, "SportsRivalryAlignment", "normalized [-0.2..0.2]", -0.2f, 0.2f, 0.0f));
        Register(new SliderDefinition(SliderId.SocialNeuroticismEvaluator, SliderCategory.Social, "NeuroticismEvaluator", "normalized [0..1]", 0.0f, 1.0f, 0.2f));
        Register(new SliderDefinition(SliderId.SocialDevotionEvaluator, SliderCategory.Social, "DevotionEvaluator", "normalized [0..1]", 0.0f, 1.0f, 0.5f));
        Register(new SliderDefinition(SliderId.SocialTribalismEvaluator, SliderCategory.Social, "TribalismEvaluator", "normalized [0..1]", 0.0f, 1.0f, 0.5f));
        Register(new SliderDefinition(SliderId.SocialIronWillEvaluator, SliderCategory.Social, "IronWillEvaluator", "normalized [0..1]", 0.0f, 1.0f, 0.2f));
        Register(new SliderDefinition(SliderId.SocialOpinionBaseline, SliderCategory.Social, "OpinionBaseline", "points [-100..100]", -100.0f, 100.0f, 0.0f));
        Register(new SliderDefinition(SliderId.SocialDecayedMemorySum, SliderCategory.Social, "DecayedMemorySum", "points [-100..100]", -100.0f, 100.0f, 0.0f));
        Register(new SliderDefinition(SliderId.SocialMoodBreakMinorThreshold, SliderCategory.Social, "MoodBreakMinorThreshold", "normalized [0..1]", 0.05f, 0.6f, 0.35f));
        Register(new SliderDefinition(SliderId.SocialMoodBreakMajorThreshold, SliderCategory.Social, "MoodBreakMajorThreshold", "normalized [0..1]", 0.02f, 0.4f, 0.20f));
        Register(new SliderDefinition(SliderId.SocialMoodBreakExtremeThreshold, SliderCategory.Social, "MoodBreakExtremeThreshold", "normalized [0..1]", 0.0f, 0.2f, 0.05f));

        // Ecology & Environmental Hazard Sliders
        Register(new SliderDefinition(SliderId.EcologyFearScore, SliderCategory.Ecology, "EcologyFearScore", "normalized [0..1]", 0.0f, 1.0f, 0.0f));
        Register(new SliderDefinition(SliderId.EcologyAggressionScore, SliderCategory.Ecology, "EcologyAggressionScore", "normalized [0..1]", 0.0f, 1.0f, 0.0f));
        Register(new SliderDefinition(SliderId.EcologyTerritorialDominance, SliderCategory.Ecology, "TerritorialDominance", "normalized [0..1]", 0.0f, 1.0f, 0.5f));
        Register(new SliderDefinition(SliderId.EcologyTerritoryFamiliarity, SliderCategory.Ecology, "TerritoryFamiliarity", "normalized [0..1]", 0.0f, 1.0f, 0.5f));
        Register(new SliderDefinition(SliderId.EcologyCaloricSatisfaction, SliderCategory.Ecology, "CaloricSatisfaction", "normalized [0..1]", 0.0f, 1.0f, 1.0f));
        Register(new SliderDefinition(SliderId.EcologyPrecycleIntensity, SliderCategory.Ecology, "PrecycleIntensity", "normalized [0..1]", 0.0f, 1.0f, 0.0f));
        Register(new SliderDefinition(SliderId.EcologyRainCycleHazardIntensity, SliderCategory.Ecology, "RainCycleHazardIntensity", "normalized [0..1]", 0.0f, 1.0f, 0.0f));
        Register(new SliderDefinition(SliderId.EcologyShelterLockdownRatio, SliderCategory.Ecology, "ShelterLockdownRatio", "normalized [0..1]", 0.0f, 1.0f, 0.0f));
        Register(new SliderDefinition(SliderId.EcologySeasonalScarcityFactor, SliderCategory.Ecology, "SeasonalScarcityFactor", "normalized [0..1]", 0.0f, 1.0f, 0.0f));
        Register(new SliderDefinition(SliderId.EcologyHypothermiaExposure, SliderCategory.Ecology, "HypothermiaExposure", "normalized [0..1]", 0.0f, 1.0f, 0.0f));
        Register(new SliderDefinition(SliderId.EcologyTrophicBiomassDensity, SliderCategory.Ecology, "TrophicBiomassDensity", "biomass units", 0.0f, 10000.0f, 100.0f));
        Register(new SliderDefinition(SliderId.EcologyLineageTier, SliderCategory.Ecology, "LineageTier", "tier index [0..5]", 0.0f, 5.0f, 0.0f));
    }
}
