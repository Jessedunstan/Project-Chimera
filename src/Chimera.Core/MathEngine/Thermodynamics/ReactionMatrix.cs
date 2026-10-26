using System;
using System.Numerics;

namespace Chimera.Core.MathEngine.Thermodynamics;

/// <summary>
/// Continuous thermodynamic and kinetic reaction parameters for spells, particles, and voxels.
/// Everything is expressed as continuous mathematical vectors.
/// Zero heap allocations.
/// </summary>
public readonly struct ReactionState
{
    public float TemperatureK { get; }          // Absolute temperature in Kelvin
    public Vector3 ForceVector { get; }          // Applied kinetic force vector in Newtons (N)
    public float AcousticDamping { get; }        // [0..1] 1.0 = absolute silence, 0.0 = full acoustic thunder
    public float NeuralDisruption { get; }       // [0..1] concussive/neural disruption (stun/daze)
    public float PhotonicFlux { get; }           // [0..1] optical radiance / flash intensity
    public float Fuel { get; }                   // [0..1] combustible matter fraction
    public float Oxidant { get; }                // [0..1] atmospheric or chemical oxidizer fraction
    public float AmbientHumidity { get; }        // [0..1] environmental moisture / rain factor

    public ReactionState(
        float temperatureK,
        Vector3 forceVector,
        float acousticDamping = 0.0f,
        float neuralDisruption = 0.0f,
        float photonicFlux = 0.0f,
        float fuel = 0.0f,
        float oxidant = 0.21f,
        float ambientHumidity = 0.3f)
    {
        TemperatureK = MathF.Max(0.0f, temperatureK);
        ForceVector = forceVector;
        AcousticDamping = Math.Clamp(acousticDamping, 0.0f, 1.0f);
        NeuralDisruption = Math.Clamp(neuralDisruption, 0.0f, 1.0f);
        PhotonicFlux = Math.Clamp(photonicFlux, 0.0f, 1.0f);
        Fuel = Math.Clamp(fuel, 0.0f, 1.0f);
        Oxidant = Math.Clamp(oxidant, 0.0f, 1.0f);
        AmbientHumidity = Math.Clamp(ambientHumidity, 0.0f, 1.0f);
    }
}
