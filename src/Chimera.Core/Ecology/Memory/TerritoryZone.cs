using System;
using System.Numerics;
using Chimera.Core.MathEngine;

namespace Chimera.Core.Ecology.Memory;

/// <summary>
/// Continuous territorial zone anchored around a den or resource hub.
/// Tracks resident claim strength, scent pheromone intensity, and spatial coverage.
/// </summary>
public sealed class TerritoryZone
{
    public string OwnerCreatureId { get; set; }
    public Vector3 AnchorPosition { get; set; }
    public float CoreRadiusMeters { get; set; }
    public float ScentIntensity { get; private set; } // 0.0 to 1.0
    public float ScentDecayRate { get; set; }          // rate per second

    public TerritoryZone(
        string ownerCreatureId,
        Vector3 anchorPosition,
        float coreRadiusMeters = 25.0f,
        float initialScent = 1.0f,
        float scentDecayRate = 0.005f)
    {
        OwnerCreatureId = ownerCreatureId;
        AnchorPosition = anchorPosition;
        CoreRadiusMeters = MathF.Max(1.0f, coreRadiusMeters);
        ScentIntensity = Math.Clamp(initialScent, 0.0f, 1.0f);
        ScentDecayRate = MathF.Max(0.0f, scentDecayRate);
    }

    /// <summary>
    /// Checks spatial proximity and returns continuous intrusion factor [0.0..1.0].
    /// 1.0 = at the very core / den entrance; 0.0 = outside territorial perimeter.
    /// </summary>
    public float EvaluateIntrusionFactor(Vector3 intruderPos)
    {
        float distance = Vector3.Distance(AnchorPosition, intruderPos);
        if (distance >= CoreRadiusMeters) return 0.0f;

        // Smooth cubic falloff from core to boundary
        return 1.0f - ContinuousCurves.SmoothStep(0.0f, CoreRadiusMeters, distance);
    }

    /// <summary>
    /// Re-marks territory by resident, restoring scent intensity.
    /// </summary>
    public void RefreshMark(float addedStrength = 0.5f)
    {
        ScentIntensity = Math.Clamp(ScentIntensity + addedStrength, 0.0f, 1.0f);
    }

    /// <summary>
    /// Decays scent marking over discrete simulation time.
    /// </summary>
    public void StepSimulationDecay(float dt)
    {
        ScentIntensity = ContinuousCurves.ExponentialDecay(ScentIntensity, ScentDecayRate, dt);
        if (ScentIntensity < 0.001f) ScentIntensity = 0.0f;
    }
}
