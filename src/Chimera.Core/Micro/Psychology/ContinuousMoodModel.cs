using System;
using Chimera.Core.MathEngine;

namespace Chimera.Core.Micro.Psychology;

/// <summary>
/// Continuous mathematical transfer functions translating RimWorld's discrete psychological states
/// (hunger, exhaustion, joy, comfort, beauty, room size, pain, temperature) into smooth mood offsets.
/// Zero step-discontinuities; strictly continuous and differentiable curves.
/// </summary>
public static class ContinuousMoodModel
{
    /// <summary>
    /// Computes continuous mood offset from caloric saturation.
    /// Input: saturation in [0.0, 1.0], where 1.0 is full and 0.0 is zero calories.
    /// Maps continuous curve smoothly reflecting RimWorld's stages:
    /// Hungry (-6), Ravenous (-12), Malnourished (-20), Badly Malnourished (-26), Starving (-32), Advanced (-38), Extreme (-44).
    /// </summary>
    public static float EvaluateHungerMoodOffset(float saturation)
    {
        float s = Math.Clamp(saturation, 0.0f, 1.0f);
        if (s >= 0.70f)
        {
            // Satiated contentment boost: smooth curve up to +4.0
            float t = (s - 0.70f) / 0.30f;
            return 4.0f * (t * t);
        }

        // Deficit normalized: 0.0 at saturation=0.70, 1.0 at saturation=0.0
        float deficit = (0.70f - s) / 0.70f;
        // Continuous non-linear escalation to -44.0
        return -44.0f * MathF.Pow(deficit, 1.35f);
    }

    /// <summary>
    /// Computes continuous mood offset from rest/energy level.
    /// Input: rest in [0.0, 1.0], where 1.0 is fully rested and 0.0 is total exhaustion.
    /// RimWorld discrete: Drowsy (-6), Tired (-12), Exhausted (-18).
    /// </summary>
    public static float EvaluateRestMoodOffset(float rest)
    {
        float r = Math.Clamp(rest, 0.0f, 1.0f);
        if (r >= 0.60f)
        {
            float t = (r - 0.60f) / 0.40f;
            return 2.0f * t;
        }

        float fatigue = (0.60f - r) / 0.60f;
        return -18.0f * MathF.Pow(fatigue, 1.25f);
    }

    /// <summary>
    /// Computes continuous mood offset from recreation/joy level.
    /// Input: joy in [0.0, 1.0].
    /// RimWorld discrete: Recreation-Starved (-20), Deprived (-10), Unfulfilled (-5), Satisfied (+5), Fully Satisfied (+10).
    /// </summary>
    public static float EvaluateJoyMoodOffset(float joy)
    {
        float j = Math.Clamp(joy, 0.0f, 1.0f);
        if (j >= 0.40f)
        {
            // Range from [0.40, 1.0] maps smoothly to [0.0, +10.0]
            float t = (j - 0.40f) / 0.60f;
            return 10.0f * ContinuousCurves.SmoothStep(0.0f, 1.0f, t);
        }
        else
        {
            // Range from [0.40, 0.0] maps smoothly down to -20.0
            float deficit = (0.40f - j) / 0.40f;
            return -20.0f * MathF.Pow(deficit, 1.2f);
        }
    }

    /// <summary>
    /// Computes continuous mood offset from physical comfort (furniture, bedding, apparel).
    /// Input: comfort in [0.0, 1.0].
    /// RimWorld discrete: Uncomfortable (-3), Comfortable (+4), Quite (+6), Extremely (+8), Luxuriant (+10).
    /// </summary>
    public static float EvaluateComfortMoodOffset(float comfort)
    {
        float c = Math.Clamp(comfort, 0.0f, 1.0f);
        if (c < 0.20f)
        {
            float deficit = (0.20f - c) / 0.20f;
            return -3.0f * deficit;
        }

        float t = (c - 0.20f) / 0.80f;
        return 10.0f * ContinuousCurves.SmoothStep(0.0f, 1.0f, t);
    }

    /// <summary>
    /// Computes continuous mood offset from environmental beauty.
    /// Input: beauty in [-1.0, 1.0].
    /// RimWorld discrete: Hideous (-15), Ugly (-10), Unsightly (-5), Pretty (+5), Beautiful (+10), Gorgeous (+15).
    /// </summary>
    public static float EvaluateBeautyMoodOffset(float beauty)
    {
        float b = Math.Clamp(beauty, -1.0f, 1.0f);
        return 15.0f * MathF.Tanh(2.2f * b);
    }

    /// <summary>
    /// Computes continuous mood offset from room volume / spaciousness.
    /// Input: roomSize in [0.0, 1.0], where 0.0 is confined closet, 0.3 is normal, 1.0 is grand hall.
    /// RimWorld discrete: Confined (-10), Cramped (-5), Spacious (+5).
    /// </summary>
    public static float EvaluateRoomSizeMoodOffset(float roomSize)
    {
        float s = Math.Clamp(roomSize, 0.0f, 1.0f);
        if (s < 0.30f)
        {
            float crampedFactor = (0.30f - s) / 0.30f;
            return -10.0f * MathF.Pow(crampedFactor, 1.2f);
        }

        float spaciousFactor = (s - 0.30f) / 0.70f;
        return 5.0f * ContinuousCurves.SmoothStep(0.0f, 1.0f, spaciousFactor);
    }

    /// <summary>
    /// Computes continuous mood offset from physical pain.
    /// Input: pain in [0.0, 1.0], where 0.0 is painless and 1.0 is mind-shattering pain.
    /// In RimWorld: Minor (-5), Serious (-10), Intense (-15), Mind-Shattering (-20).
    /// If entity is Masochist, inverted to positive mood [+5.0, +20.0].
    /// </summary>
    public static float EvaluatePainMoodOffset(float pain, bool isMasochist = false)
    {
        float p = Math.Clamp(pain, 0.0f, 1.0f);
        if (p <= 1e-4f) return 0.0f;

        float magnitude = 20.0f * MathF.Pow(ContinuousCurves.SmoothStep(0.0f, 1.0f, p), 1.1f);
        return isMasochist ? magnitude : -magnitude;
    }

    /// <summary>
    /// Computes continuous mood offset from ambient temperature.
    /// Input: temperature in Kelvin.
    /// Optimal temperature: ~294.15 K (21°C).
    /// Comfort band: [289.15 K (16°C), 299.15 K (26°C)].
    /// Beyond comfort: scales smoothly down to -16.0 (Cold: -4, -8, -12, -16; Heat: -4, -8, -12, -16).
    /// </summary>
    public static float EvaluateThermalMoodOffset(float temperatureKelvin)
    {
        const float minComfortK = 289.15f; // 16°C
        const float maxComfortK = 299.15f; // 26°C

        if (temperatureKelvin >= minComfortK && temperatureKelvin <= maxComfortK)
        {
            return 0.0f; // Neutral comfort inside the safe band
        }

        if (temperatureKelvin < minComfortK)
        {
            // Cold distress
            float coldDelta = minComfortK - temperatureKelvin;
            return -16.0f * ContinuousCurves.Sigmoid(coldDelta, 12.0f, 0.25f);
        }
        else
        {
            // Heat distress
            float heatDelta = temperatureKelvin - maxComfortK;
            return -16.0f * ContinuousCurves.Sigmoid(heatDelta, 12.0f, 0.25f);
        }
    }

    /// <summary>
    /// Computes the instantaneous target mood level in [0.0, 1.0].
    /// RimWorld base level is 0.32 (32%), adjusted by sum of mood offsets / 100.
    /// </summary>
    public static float CalculateTargetMood(
        float hungerOffset,
        float restOffset,
        float joyOffset,
        float comfortOffset,
        float beautyOffset,
        float roomSizeOffset,
        float painOffset,
        float thermalOffset,
        float memoryMoodSum,
        float traitMoodOffset = 0.0f,
        float baselineMood = MoodConstants.BaselineMood)
    {
        float totalOffsetPoints = hungerOffset + restOffset + joyOffset + comfortOffset 
            + beautyOffset + roomSizeOffset + painOffset + thermalOffset 
            + memoryMoodSum + traitMoodOffset;

        float target = baselineMood + (totalOffsetPoints / 100.0f);
        return Math.Clamp(target, 0.0f, 1.0f);
    }

    /// <summary>
    /// Advances current mood toward target mood over elapsed simulation ticks using RimWorld's seeker rates.
    /// SeekerRisePerHour = 0.12, SeekerFallPerHour = 0.08.
    /// Freezes progression if pawn is sleeping.
    /// </summary>
    public static float StepMoodTowardTarget(
        float currentMood,
        float targetMood,
        long elapsedTicks,
        bool isSleeping = false)
    {
        if (isSleeping || elapsedTicks <= 0)
        {
            return currentMood;
        }

        float delta = targetMood - currentMood;
        if (MathF.Abs(delta) < 1e-6f)
        {
            return targetMood;
        }

        if (delta > 0.0f)
        {
            float maxRise = MoodConstants.SeekerRisePerTick * elapsedTicks;
            return MathF.Min(targetMood, currentMood + maxRise);
        }
        else
        {
            float maxFall = MoodConstants.SeekerFallPerTick * elapsedTicks;
            return MathF.Max(targetMood, currentMood - maxFall);
        }
    }
}
