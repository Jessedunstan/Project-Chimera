using System;
using System.Numerics;

namespace Chimera.Core.MathEngine.Thermodynamics;

/// <summary>
/// Unified thermodynamic and kinetic calculation engine for spells, explosions, and environmental reactions.
/// Fire and Ice are treated as unified vector deltas along a single thermodynamic axis.
/// Fixed discrete timesteps, zero heap allocations.
/// </summary>
public static class SpellPhysicsMath
{
    public const float IgnitionThresholdK = 573.15f; // ~300 C typical wood/foliage flash point
    public const float FreezingThresholdK = 273.15f; // 0 C water crystallization point
    public const float PreExponentialCombustionFactor = 1000.0f;

    /// <summary>
    /// Applies a thermodynamic impulse to a voxel or target entity.
    /// Fire (+deltaT) is quenched by high ambient humidity.
    /// Ice (-deltaT) cooling is accelerated by high ambient humidity (water thermal conduction).
    /// </summary>
    public static float ApplyThermodynamicImpulse(
        float currentTemperatureK,
        float deltaTK,
        float ambientHumidity,
        float thermalInertia = 1.0f)
    {
        float clampedHumidity = Math.Clamp(ambientHumidity, 0.0f, 1.0f);
        float effectiveDeltaT;

        if (deltaTK >= 0.0f)
        {
            // Humidity resists warming / quenches fire
            float heatAttenuation = 1.0f - (0.65f * clampedHumidity);
            effectiveDeltaT = deltaTK * heatAttenuation;
        }
        else
        {
            // Humidity intensifies cryogenic heat drain
            float coldAmplification = 1.0f + (0.5f * clampedHumidity);
            effectiveDeltaT = deltaTK * coldAmplification;
        }

        float inertia = MathF.Max(0.1f, thermalInertia);
        float finalTemp = currentTemperatureK + (effectiveDeltaT / inertia);
        return MathF.Max(0.0f, finalTemp);
    }

    /// <summary>
    /// Arrhenius combustion reaction rate:
    /// Rate = A * exp(-Ea / (R * T)) * [Fuel] * [Oxidant] * (1.0 - 0.85 * Humidity)
    /// Returns fuel consumption rate per simulation tick.
    /// </summary>
    public static float CalculateCombustionRate(
        float temperatureK,
        float fuel,
        float oxidant,
        float ambientHumidity,
        float activationEnergyKJ = 50.0f)
    {
        if (fuel <= 1e-4f || oxidant <= 1e-4f || temperatureK < 300.0f)
            return 0.0f;

        float eaJoules = activationEnergyKJ * 1000.0f;
        float baseRate = ContinuousCurves.Arrhenius(temperatureK, PreExponentialCombustionFactor, eaJoules);

        float humidityDamping = MathF.Max(0.0f, 1.0f - (0.85f * Math.Clamp(ambientHumidity, 0.0f, 1.0f)));
        return baseRate * fuel * oxidant * humidityDamping;
    }

    /// <summary>
    /// Unified continuous Planckian blackbody / cryogenic particle color mapping.
    /// Converts continuous Kelvin temperature to an RGBA visual vector.
    /// Fire and Ice are opposite ends of the same thermodynamic slider.
    /// </summary>
    public static Vector4 MapTemperatureToParticleColor(float temperatureK)
    {
        // Cryogenic Deep Freeze (0K to 150K) -> Deep Violet-Blue
        if (temperatureK <= 150.0f)
        {
            float t = Math.Clamp(temperatureK / 150.0f, 0.0f, 1.0f);
            return Vector4.Lerp(
                new Vector4(0.10f, 0.15f, 0.90f, 0.95f),
                new Vector4(0.20f, 0.65f, 1.00f, 0.85f),
                t);
        }

        // Sub-zero Frost / Crystallization (150K to 273.15K) -> Cyan to Pale Frost
        if (temperatureK < FreezingThresholdK)
        {
            float t = (temperatureK - 150.0f) / (FreezingThresholdK - 150.0f);
            return Vector4.Lerp(
                new Vector4(0.20f, 0.65f, 1.00f, 0.85f),
                new Vector4(0.70f, 0.90f, 1.00f, 0.60f),
                t);
        }

        // Ambient / Neutral (273.15K to 450K) -> Soft Gray Dust / Smoke
        if (temperatureK <= 450.0f)
        {
            float t = (temperatureK - FreezingThresholdK) / (450.0f - FreezingThresholdK);
            return Vector4.Lerp(
                new Vector4(0.70f, 0.90f, 1.00f, 0.40f),
                new Vector4(0.55f, 0.45f, 0.35f, 0.25f),
                t);
        }

        // Incipient Smolder (450K to 850K) -> Deep Cherry Red
        if (temperatureK <= 850.0f)
        {
            float t = (temperatureK - 450.0f) / (850.0f - 450.0f);
            return Vector4.Lerp(
                new Vector4(0.55f, 0.45f, 0.35f, 0.30f),
                new Vector4(0.90f, 0.15f, 0.02f, 0.90f),
                t);
        }

        // Active Flame (850K to 1600K) -> Fiery Orange
        if (temperatureK <= 1600.0f)
        {
            float t = (temperatureK - 850.0f) / (1600.0f - 850.0f);
            return Vector4.Lerp(
                new Vector4(0.90f, 0.15f, 0.02f, 0.90f),
                new Vector4(1.00f, 0.55f, 0.05f, 0.95f),
                t);
        }

        // Incandescent Heat (1600K to 2800K) -> Brilliant Yellow-White
        if (temperatureK <= 2800.0f)
        {
            float t = (temperatureK - 1600.0f) / (2800.0f - 1600.0f);
            return Vector4.Lerp(
                new Vector4(1.00f, 0.55f, 0.05f, 0.95f),
                new Vector4(1.00f, 0.98f, 0.50f, 1.00f),
                t);
        }

        // Hyper-Thermal Plasma (> 2800K) -> Arc Blue-White
        float plasmaT = Math.Clamp((temperatureK - 2800.0f) / 2200.0f, 0.0f, 1.0f);
        return Vector4.Lerp(
            new Vector4(1.00f, 0.98f, 0.50f, 1.00f),
            new Vector4(0.70f, 0.85f, 1.00f, 1.00f),
            plasmaT);
    }

    /// <summary>
    /// Concussive and neural disruption stun calculation.
    /// Stun duration in seconds = (KineticForce * k_f + NeuralDisruption * k_n) / (TargetMass + Constitution)
    /// </summary>
    public static float CalculateStunDuration(
        float kineticForceMagnitudeN,
        float neuralDisruption,
        float targetMassKg,
        float constitution = 1.0f)
    {
        float totalResistance = MathF.Max(1.0f, targetMassKg + (constitution * 50.0f));
        float kineticImpulse = kineticForceMagnitudeN * FixedTimeStep.MicroTickDeltaTime;
        float kineticContribution = kineticImpulse * 0.05f;
        float neuralContribution = neuralDisruption * 3.5f;

        float rawDuration = (kineticContribution + neuralContribution) / (totalResistance * 0.015f);
        return Math.Clamp(rawDuration, 0.0f, 10.0f); // Max 10s stun
    }

    /// <summary>
    /// Acoustic sound propagation radius:
    /// R = BaseRadius * sqrt(KineticForce) * (1.0 - AcousticDamping)
    /// </summary>
    public static float CalculateSoundPropagationRadius(
        float kineticForceMagnitudeN,
        float acousticDamping,
        float baseRadiusM = 5.0f)
    {
        float kineticScale = MathF.Sqrt(MathF.Max(0.0f, kineticForceMagnitudeN));
        float soundFactor = 1.0f - Math.Clamp(acousticDamping, 0.0f, 1.0f);
        return baseRadiusM * kineticScale * soundFactor * 0.1f;
    }

    /// <summary>
    /// Photonic flux optical blindness calculation:
    /// Duration = PhotonicFlux * (1.0 - FlashResistance) * (BaseRadius / (Distance + 1.0))
    /// </summary>
    public static float CalculateBlindDuration(
        float photonicFlux,
        float distanceM,
        float flashResistance = 0.0f,
        float maxDurationSeconds = 8.0f)
    {
        if (photonicFlux <= 1e-4f) return 0.0f;
        float distanceAttenuation = 1.0f / MathF.Max(1.0f, distanceM);
        float resistanceFactor = 1.0f - Math.Clamp(flashResistance, 0.0f, 1.0f);
        return Math.Clamp(photonicFlux * maxDurationSeconds * distanceAttenuation * resistanceFactor, 0.0f, maxDurationSeconds);
    }
}
