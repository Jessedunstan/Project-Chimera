using System;

namespace Chimera.Core.MathEngine;

/// <summary>
/// Core mathematical curves and continuous transfer functions for Project Chimera.
/// Invariants: No discontinuous jumps or hard thresholds unless explicitly parameterized as a sharp boundary.
/// </summary>
public static class ContinuousCurves
{
    public const float UniversalGasConstant = 8.314462618f; // J / (mol * K)

    /// <summary>
    /// Evaluates a generalized logistic sigmoid function:
    /// f(x) = 1.0 / (1.0 + exp(-steepness * (x - midpoint)))
    /// Output range: (0.0, 1.0)
    /// </summary>
    public static float Sigmoid(float x, float midpoint = 0.0f, float steepness = 1.0f)
    {
        float z = -steepness * (x - midpoint);
        // Numerical clamp to avoid floating point overflow in exp
        if (z > 45.0f) return 0.0f;
        if (z < -45.0f) return 1.0f;
        return 1.0f / (1.0f + MathF.Exp(z));
    }

    /// <summary>
    /// Smooth cubic Hermite interpolation between edge0 and edge1.
    /// Clamps input to [0, 1] before evaluation.
    /// </summary>
    public static float SmoothStep(float edge0, float edge1, float x)
    {
        if (MathF.Abs(edge1 - edge0) < 1e-6f)
            return x >= edge1 ? 1.0f : 0.0f;

        float t = Math.Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
        return t * t * (3.0f - 2.0f * t);
    }

    /// <summary>
    /// Allometric power law scaling: f(x) = coefficient * (x ^ exponent).
    /// Used for biological scaling laws (e.g., Kleiber's Law S^0.75, Cube Law S^3.0).
    /// </summary>
    public static float Allometric(float x, float coefficient, float exponent)
    {
        if (x <= 0.0f) return 0.0f;
        return coefficient * MathF.Pow(x, exponent);
    }

    /// <summary>
    /// Computes harmonic mean of two values: 2 * a * b / (a + b).
    /// Essential for effective thermal conductivity, interface diffusion, and serial resistance.
    /// </summary>
    public static float HarmonicMean(float a, float b, float epsilon = 1e-6f)
    {
        float sum = a + b;
        if (sum <= epsilon) return 0.0f;
        return (2.0f * a * b) / sum;
    }

    /// <summary>
    /// Arrhenius reaction kinetics rate evaluator:
    /// k = preExponential * exp(-activationEnergy / (R * temperatureK))
    /// </summary>
    public static float Arrhenius(float temperatureK, float preExponentialFactor, float activationEnergyJoulePerMol)
    {
        if (temperatureK <= 1e-3f) return 0.0f;
        float exponent = -activationEnergyJoulePerMol / (UniversalGasConstant * temperatureK);
        if (exponent < -45.0f) return 0.0f;
        return preExponentialFactor * MathF.Exp(exponent);
    }

    /// <summary>
    /// Exponential decay over elapsed time/ticks:
    /// V(t) = initialValue * exp(-decayRate * delta)
    /// </summary>
    public static float ExponentialDecay(float initialValue, float decayRate, float deltaTime)
    {
        if (deltaTime <= 0.0f) return initialValue;
        return initialValue * MathF.Exp(-MathF.Max(0.0f, decayRate) * deltaTime);
    }

    /// <summary>
    /// Linear interpolation between a and b by normalized factor t.
    /// </summary>
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * Math.Clamp(t, 0.0f, 1.0f);
    }
}
