using System;

namespace Chimera.Core.MathEngine.Materials;

/// <summary>
/// Output calculation for material deformation under applied mechanical stress.
/// Deterministic struct, zero heap allocations.
/// </summary>
public readonly struct DeformationResult
{
    public float AppliedStressMpa { get; }
    public float EffectiveYieldStressMpa { get; }
    public float PlasticStrain { get; }
    public float FractureProbability { get; }
    public bool DidFracture { get; }

    public DeformationResult(
        float appliedStressMpa,
        float effectiveYieldStressMpa,
        float plasticStrain,
        float fractureProbability,
        bool didFracture)
    {
        AppliedStressMpa = appliedStressMpa;
        EffectiveYieldStressMpa = effectiveYieldStressMpa;
        PlasticStrain = plasticStrain;
        FractureProbability = fractureProbability;
        DidFracture = didFracture;
    }
}

/// <summary>
/// Continuous mathematical models governing material physics, phase transitions, and heat exchange.
/// Operates on fixed discrete timesteps with zero heap allocations.
/// </summary>
public static class MaterialMath
{
    // Scaling constant converting [0..1] thermal conductivity to W / (m * K)
    public const float ThermalConductivityScale = 400.0f; // 1.0 represents copper (~400 W/mK)

    /// <summary>
    /// Computes the effective thermal conductivity between two adjacent materials using harmonic mean.
    /// In series interface conduction: k_eff = 2 * k1 * k2 / (k1 + k2)
    /// </summary>
    public static float CalculateEffectiveConductivity(float k1, float k2)
    {
        float k1Scaled = k1 * ThermalConductivityScale;
        float k2Scaled = k2 * ThermalConductivityScale;
        return ContinuousCurves.HarmonicMean(k1Scaled, k2Scaled);
    }

    /// <summary>
    /// Calculates Fourier conductive heat transfer between two voxels/materials over a fixed discrete timestep:
    /// Q = k_eff * (Area / distance) * (TempB - TempA) * dt (Joules)
    /// </summary>
    public static float CalculateConductiveHeatTransfer(
        float kEffective,
        float contactAreaM2,
        float distanceM,
        float tempA,
        float tempB,
        float dt = FixedTimeStep.MicroTickDeltaTime)
    {
        if (distanceM <= 1e-4f) return 0.0f;
        float deltaT = tempB - tempA;
        return kEffective * (contactAreaM2 / distanceM) * deltaT * dt;
    }

    /// <summary>
    /// Computes the temperature change of a mass given an absorbed heat energy Q:
    /// deltaT = Q / (mass * specificHeat)
    /// </summary>
    public static float CalculateTemperatureDelta(float heatJoules, float massKg, float specificHeatCapacity)
    {
        float thermalMass = massKg * specificHeatCapacity;
        if (thermalMass <= 1e-4f) return 0.0f;
        return heatJoules / thermalMass;
    }

    /// <summary>
    /// Continuous structural deformation and fracture solver under applied stress.
    /// Brittleness drives catastrophic shattering.
    /// Malleability drives ductile deformation.
    /// Rigidity elevates elastic threshold.
    /// </summary>
    public static DeformationResult EvaluateStress(
        float appliedStressMpa,
        in PhysicalMaterialParameters material,
        float deterministicRoll = 0.5f)
    {
        // Rigidity enhances the effective yield threshold
        float effectiveYield = material.TensileYield * (1.0f + material.Rigidity * 0.5f);
        float overstress = MathF.Max(0.0f, appliedStressMpa - effectiveYield);

        if (overstress <= 0.0f)
        {
            return new DeformationResult(appliedStressMpa, effectiveYield, 0.0f, 0.0f, false);
        }

        // Brittleness amplifies fracture likelihood rapidly above yield
        // Plastic deformation is suppressed by high brittleness
        float brittlenessFactor = MathF.Max(0.01f, 1.0f - material.Brittleness);
        float relativeStress = overstress / (effectiveYield * brittlenessFactor);
        float fractureProb = Math.Clamp(relativeStress * material.Brittleness, 0.0f, 1.0f);

        // Malleability allows absorbing overstress into plastic strain without shattering
        float plasticStrain = (overstress / effectiveYield) * material.Malleability * (1.0f - material.Brittleness);

        bool didFracture = fractureProb > deterministicRoll;

        return new DeformationResult(
            appliedStressMpa,
            effectiveYield,
            plasticStrain,
            fractureProb,
            didFracture
        );
    }

    /// <summary>
    /// Continuous phase calculation returning a smooth value representing voxel state:
    /// 0.0 = Solid
    /// (0.0 to 1.0) = Melting slush / liquid transition
    /// 1.0 = Liquid
    /// (1.0 to 2.0) = Boiling vapor / gas transition
    /// 2.0 = Pure Gas / Superheated Plasma
    /// </summary>
    public static float CalculateContinuousPhaseState(
        float currentTempK,
        float meltingPointK,
        float boilingPointK,
        float transitionBandK = 2.0f)
    {
        float band = MathF.Max(0.01f, transitionBandK);

        // Solid regime
        if (currentTempK <= meltingPointK - band)
        {
            return 0.0f;
        }

        // Melting transition [0.0 -> 1.0]
        if (currentTempK < meltingPointK + band)
        {
            return ContinuousCurves.SmoothStep(meltingPointK - band, meltingPointK + band, currentTempK);
        }

        // Liquid regime
        if (currentTempK <= boilingPointK - band)
        {
            return 1.0f;
        }

        // Boiling transition [1.0 -> 2.0]
        if (currentTempK < boilingPointK + band)
        {
            float vaporFactor = ContinuousCurves.SmoothStep(boilingPointK - band, boilingPointK + band, currentTempK);
            return 1.0f + vaporFactor;
        }

        // Gas regime
        return 2.0f;
    }
}
