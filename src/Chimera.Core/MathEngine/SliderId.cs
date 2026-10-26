namespace Chimera.Core.MathEngine;

/// <summary>
/// Strongly-typed 16-bit identifier for universal simulation sliders.
/// Guarantees zero heap allocation, fast O(1) array/switch lookups, and deterministic serialization.
/// Grouped in blocks of 100 per domain.
/// </summary>
public enum SliderId : ushort
{
    None = 0,

    // --- Physical Materials (1 - 99) ---
    MaterialDensity = 1,
    MaterialRigidity = 2,
    MaterialBrittleness = 3,
    MaterialMalleability = 4,
    MaterialThermalConductivity = 5,
    MaterialSpecificHeatCapacity = 6,
    MaterialMeltingPoint = 7,
    MaterialBoilingPoint = 8,
    MaterialFrictionCoefficient = 9,
    MaterialTensileYield = 10,

    // --- Biology & Generational Physiology (100 - 199) ---
    BioBodySize = 100,
    BioMetabolicRate = 101,
    BioBioluminescenceIntensity = 102,
    BioChromaticAdaptationRate = 103,
    BioCaloricIntakeRatio = 104,
    BioGenerationalGrowthFactor = 105,
    BioMutationRate = 106,
    BioActivityLevel = 107,
    BioBaseMass = 108,
    BioHypertrophicPenaltyCoeff = 109,

    // --- Thermodynamics & Spell Reaction Matrix (200 - 299) ---
    ThermoTemperatureDelta = 200,
    ThermoForceVectorX = 201,
    ThermoForceVectorY = 202,
    ThermoForceVectorZ = 203,
    ThermoAcousticDamping = 204,
    ThermoNeuralDisruption = 205,
    ThermoPhotonicFlux = 206,
    ThermoAmbientHumidity = 207,
    ThermoFuel = 208,
    ThermoOxidant = 209,
    ThermoThermalInertia = 210,
    ThermoCombustionActivationEnergy = 211,

    // --- Social Opinion & Ideological Math (300 - 399) ---
    SocialReligiousCompatibility = 300,
    SocialClanFeudScore = 301,
    SocialSportsRivalryAlignment = 302,
    SocialNeuroticismEvaluator = 303,
    SocialDevotionEvaluator = 304,
    SocialTribalismEvaluator = 305,
    SocialIronWillEvaluator = 306,
    SocialOpinionBaseline = 307,
    SocialDecayedMemorySum = 308,
    SocialMoodBreakMinorThreshold = 309,
    SocialMoodBreakMajorThreshold = 310,
    SocialMoodBreakExtremeThreshold = 311,

    // --- Autonomous Ecology & Environmental Hazards (400 - 499) ---
    EcologyFearScore = 400,
    EcologyAggressionScore = 401,
    EcologyTerritorialDominance = 402,
    EcologyTerritoryFamiliarity = 403,
    EcologyCaloricSatisfaction = 404,
    EcologyPrecycleIntensity = 405,
    EcologyRainCycleHazardIntensity = 406,
    EcologyShelterLockdownRatio = 407,
    EcologySeasonalScarcityFactor = 408,
    EcologyHypothermiaExposure = 409,
    EcologyTrophicBiomassDensity = 410,
    EcologyLineageTier = 411
}
