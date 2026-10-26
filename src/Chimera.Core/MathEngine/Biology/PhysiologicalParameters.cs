using System;

namespace Chimera.Core.MathEngine.Biology;

/// <summary>
/// Continuous physiological traits and evolutionary state for biological entities.
/// Zero heap allocations, pure value struct.
/// </summary>
public readonly struct PhysiologicalParameters
{
    public float BodySize { get; }                    // continuous scalar [0.1 to 5.0]
    public float MetabolicRate { get; }               // continuous scalar [0.5 to 3.0]
    public float BaseMassKg { get; }                  // baseline adult mass at size 1.0 (e.g. 70 kg)
    public float BioluminescenceIntensity { get; }    // normalized [0..1]
    public float ChromaticAdaptationRate { get; }     // normalized [0..1]
    public float CaloricIntakeRatio { get; }          // ratio of consumed to required calories [0..3.0]
    public float GenerationalGrowthFactor { get; }    // sensitivity to caloric surplus/deficit [0..1.0]
    public float MutationVariance { get; }            // mutation sigma during reproduction [0..0.5]
    public float ActivityLevel { get; }               // current exertion multiplier [0..3.0]

    public PhysiologicalParameters(
        float bodySize,
        float metabolicRate,
        float baseMassKg = 70.0f,
        float bioluminescenceIntensity = 0.0f,
        float chromaticAdaptationRate = 0.1f,
        float caloricIntakeRatio = 1.0f,
        float generationalGrowthFactor = 0.15f,
        float mutationVariance = 0.05f,
        float activityLevel = 0.2f)
    {
        BodySize = Math.Clamp(bodySize, 0.1f, 5.0f);
        MetabolicRate = Math.Clamp(metabolicRate, 0.5f, 3.0f);
        BaseMassKg = MathF.Max(0.5f, baseMassKg);
        BioluminescenceIntensity = Math.Clamp(bioluminescenceIntensity, 0.0f, 1.0f);
        ChromaticAdaptationRate = Math.Clamp(chromaticAdaptationRate, 0.0f, 1.0f);
        CaloricIntakeRatio = MathF.Max(0.0f, caloricIntakeRatio);
        GenerationalGrowthFactor = Math.Clamp(generationalGrowthFactor, 0.0f, 1.0f);
        MutationVariance = Math.Clamp(mutationVariance, 0.0f, 0.5f);
        ActivityLevel = MathF.Max(0.0f, activityLevel);
    }

    public static PhysiologicalParameters FromParameterBank(ContinuousParameterBank bank)
    {
        return new PhysiologicalParameters(
            bank.GetSliderValue(SliderId.BioBodySize),
            bank.GetSliderValue(SliderId.BioMetabolicRate),
            bank.GetSliderValue(SliderId.BioBaseMass),
            bank.GetSliderValue(SliderId.BioBioluminescenceIntensity),
            bank.GetSliderValue(SliderId.BioChromaticAdaptationRate),
            bank.GetSliderValue(SliderId.BioCaloricIntakeRatio),
            bank.GetSliderValue(SliderId.BioGenerationalGrowthFactor),
            bank.GetSliderValue(SliderId.BioMutationRate),
            bank.GetSliderValue(SliderId.BioActivityLevel)
        );
    }
}
