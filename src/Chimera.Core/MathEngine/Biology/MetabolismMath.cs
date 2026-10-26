using System;
using System.Numerics;

namespace Chimera.Core.MathEngine.Biology;

/// <summary>
/// Mathematical functions for continuous physiology, allometric scaling, and generational evolution.
/// Built around fixed discrete ticks with zero heap allocations.
/// </summary>
public static class MetabolismMath
{
    public const float BaseCaloricDailyRequirement = 2000.0f; // kcal per day at size 1.0, metabolic rate 1.0
    public const float BioluminescenceCaloricMultiplier = 350.0f; // kcal per day at full bioluminescent emission
    public const float BaseSurfaceAreaM2 = 1.8f;              // adult human surface area at size 1.0
    public const float DayInSeconds = 86400.0f;

    /// <summary>
    /// Allometric cube-law mass scaling: Mass(S) = BaseMass * S^3
    /// </summary>
    public static float CalculateMass(float bodySize, float baseMassKg = 70.0f)
    {
        return ContinuousCurves.Allometric(bodySize, baseMassKg, 3.0f);
    }

    /// <summary>
    /// Surface area square-law scaling: Area(S) = BaseArea * S^2
    /// </summary>
    public static float CalculateSurfaceArea(float bodySize, float baseAreaM2 = BaseSurfaceAreaM2)
    {
        return ContinuousCurves.Allometric(bodySize, baseAreaM2, 2.0f);
    }

    /// <summary>
    /// Kleiber's Law allometric Basal Metabolic Rate (BMR):
    /// BMR = BaseCalories * MetabolicRate * S^0.75 (kcal / day)
    /// </summary>
    public static float CalculateBasalMetabolicRate(float bodySize, float metabolicRate, float baseDailyCalories = BaseCaloricDailyRequirement)
    {
        float kleiberScaling = ContinuousCurves.Allometric(bodySize, 1.0f, 0.75f);
        return baseDailyCalories * metabolicRate * kleiberScaling;
    }

    /// <summary>
    /// Hypertrophy penalty acting as the natural evolutionary ceiling for giant creatures:
    /// Excess mass above size 1.0 incurs non-linear gravity and structural maintenance caloric tax.
    /// Penalty = max(0, S - 1.0)^1.5 * coefficient * BaseCalories
    /// </summary>
    public static float CalculateHypertrophicPenalty(float bodySize, float penaltyCoeff = 0.4f, float baseDailyCalories = BaseCaloricDailyRequirement)
    {
        float excessSize = MathF.Max(0.0f, bodySize - 1.0f);
        if (excessSize <= 0.0f) return 0.0f;
        return ContinuousCurves.Allometric(excessSize, penaltyCoeff * baseDailyCalories, 1.5f);
    }

    /// <summary>
    /// Calculates total caloric demand per simulation tick (or micro-tick).
    /// Incorporates BMR, hypertrophic penalty, bioluminescent tax, and current exertion.
    /// </summary>
    public static float CalculateCaloricRequirementPerTick(
        in PhysiologicalParameters physiology,
        float dt = FixedTimeStep.SimulationTickDeltaTime)
    {
        float bmrDaily = CalculateBasalMetabolicRate(physiology.BodySize, physiology.MetabolicRate);
        float hypertrophyDaily = CalculateHypertrophicPenalty(physiology.BodySize);
        float biolumDaily = physiology.BioluminescenceIntensity * BioluminescenceCaloricMultiplier;

        float totalDailyKcal = (bmrDaily + hypertrophyDaily + biolumDaily) * (1.0f + physiology.ActivityLevel);
        float kcalPerSecond = totalDailyKcal / DayInSeconds;
        return kcalPerSecond * dt;
    }

    /// <summary>
    /// Physical attack strike force: Force scales with mass * velocity.
    /// F_strike = BaseForce * S^3 * sqrt(MetabolicRate) (Newtons)
    /// </summary>
    public static float CalculateAttackForce(float bodySize, float metabolicRate, float baseForceN = 500.0f)
    {
        float massScale = MathF.Pow(bodySize, 3.0f);
        float metabolicBurst = MathF.Sqrt(metabolicRate);
        return baseForceN * massScale * metabolicBurst;
    }

    /// <summary>
    /// Agility and directional acceleration:
    /// Muscular cross-section scales with S^2, while mass scales with S^3.
    /// Therefore max acceleration a = F_musc / M = k * (S^2 / S^3) * R_m = k * (R_m / S)
    /// Smaller creatures accelerate faster; massive behemoths have heavy inertia.
    /// </summary>
    public static float CalculateAccelerationCapacity(float bodySize, float metabolicRate, float baseAcceleration = 10.0f)
    {
        if (bodySize <= 0.01f) return 0.0f;
        return baseAcceleration * (metabolicRate / bodySize);
    }

    /// <summary>
    /// Generational Size Loop:
    /// Calculates offspring body size based on parent caloric intake ratio during reproductive cycle.
    /// If CaloricIntakeRatio &lt; 1.0: Offspring shrinks (stunting).
    /// If CaloricIntakeRatio &gt; 1.0: Offspring grows (hypertrophy).
    /// Hypertrophic penalty creates a natural ceiling preventing unbounded runaway growth.
    /// </summary>
    public static float CalculateNextGenerationSize(
        float parentSize,
        float caloricIntakeRatio,
        float growthSensitivity = 0.15f,
        float deterministicMutation = 0.0f,
        float minSize = 0.1f,
        float maxSize = 5.0f)
    {
        // Smoothly saturate surplus/deficit shift using tanh
        float nutritionDelta = MathF.Tanh(caloricIntakeRatio - 1.0f);
        float scalingMultiplier = 1.0f + (growthSensitivity * nutritionDelta);
        float newSize = (parentSize * scalingMultiplier) + deterministicMutation;
        return Math.Clamp(newSize, minSize, maxSize);
    }

    /// <summary>
    /// Continuous chromatic adaptation: shifts creature RGB palette toward voxel/background color
    /// at the rate dictated by ChromaticAdaptationRate slider over fixed discrete time.
    /// </summary>
    public static Vector3 UpdateChromaticAdaptation(
        Vector3 currentColor,
        Vector3 environmentColor,
        float adaptationRate,
        float dt = FixedTimeStep.SimulationTickDeltaTime)
    {
        float t = Math.Clamp(adaptationRate * dt, 0.0f, 1.0f);
        return Vector3.Lerp(currentColor, environmentColor, t);
    }
}
