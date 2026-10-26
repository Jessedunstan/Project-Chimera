using System;
using Chimera.Core.MathEngine;

namespace Chimera.Core.Ecology.Cycle;

/// <summary>
/// Continuous environmental hazard simulation covering rain cycles, flood accumulation, and seasonal pressure.
/// Operates on fixed 1 Hz discrete simulation ticks, with macro seasonal cycles on macro ticks.
/// </summary>
public sealed class EnvironmentalHazardCycle
{
    public const int DefaultCycleDurationSeconds = 600; // 10 minutes (within 400s - 800s specification)
    public const int WarningLeadTimeSeconds = 120;       // Tremor warning at T - 120s
    public const int FloodOnsetDurationSeconds = 30;     // Flood accumulation from T to T + 30s

    public int CycleDurationSeconds { get; private set; }
    public int CurrentCycleTimerSeconds { get; private set; }
    public ulong TotalCompletedCycles { get; private set; }

    public RainHazardPhase CurrentPhase { get; private set; }
    public float TremorIntensity { get; private set; }
    public float LightDarkeningFactor { get; private set; }
    public float FloodLevelMeters { get; private set; }
    public float KineticCrushForceNewtons { get; private set; }
    public bool SheltersLocked { get; private set; }

    // Seasonal macro properties
    public float AmbientTemperatureKelvin { get; private set; } = 293.15f; // 20 C
    public float SeasonalScarcityFactor { get; private set; } = 0.0f;     // 0.0 = abundance, 1.0 = severe winter scarcity
    public float SeasonalHypothermiaRate { get; private set; } = 0.0f;

    public EnvironmentalHazardCycle(int cycleDurationSeconds = DefaultCycleDurationSeconds)
    {
        CycleDurationSeconds = Math.Clamp(cycleDurationSeconds, 400, 800);
        CurrentCycleTimerSeconds = 0;
        CurrentPhase = RainHazardPhase.Clear;
    }

    /// <summary>
    /// Discrete 1 Hz simulation tick update.
    /// Evaluates hazard phases, tremor warnings, flood accumulation, and lethal crushing downpour forces.
    /// </summary>
    public void StepSimulationTick(ulong simulationTickIndex)
    {
        CurrentCycleTimerSeconds++;

        int warningStart = Math.Max(0, CycleDurationSeconds - WarningLeadTimeSeconds);
        int floodStart = CycleDurationSeconds;
        int lethalStart = CycleDurationSeconds + FloodOnsetDurationSeconds;

        if (CurrentCycleTimerSeconds < warningStart)
        {
            CurrentPhase = RainHazardPhase.Clear;
            TremorIntensity = 0.0f;
            LightDarkeningFactor = 0.0f;
            FloodLevelMeters = 0.0f;
            KineticCrushForceNewtons = 0.0f;
            SheltersLocked = false;
        }
        else if (CurrentCycleTimerSeconds < floodStart)
        {
            CurrentPhase = RainHazardPhase.PrecycleTremor;
            float progress = (float)(CurrentCycleTimerSeconds - warningStart) / WarningLeadTimeSeconds;
            TremorIntensity = ContinuousCurves.SmoothStep(0.0f, 1.0f, progress);
            LightDarkeningFactor = TremorIntensity * 0.85f;
            FloodLevelMeters = 0.0f;
            KineticCrushForceNewtons = 0.0f;
            SheltersLocked = false;
        }
        else if (CurrentCycleTimerSeconds < lethalStart)
        {
            CurrentPhase = RainHazardPhase.WaterAccumulation;
            float floodProgress = (float)(CurrentCycleTimerSeconds - floodStart) / FloodOnsetDurationSeconds;
            TremorIntensity = 1.0f;
            LightDarkeningFactor = 0.85f + (0.15f * floodProgress);
            FloodLevelMeters = floodProgress * 3.5f; // Rising water level
            KineticCrushForceNewtons = floodProgress * 1500.0f; // Light falling drizzle force
            SheltersLocked = floodProgress >= 0.7f; // Shelters lock down just before lethal rain
        }
        else
        {
            CurrentPhase = RainHazardPhase.LethalDownpour;
            TremorIntensity = 1.0f;
            LightDarkeningFactor = 1.0f;
            int lethalSeconds = CurrentCycleTimerSeconds - lethalStart;
            FloodLevelMeters = 3.5f + MathF.Min(15.0f, lethalSeconds * 0.25f);
            // Catastrophic kinetic impact crushing force (>10 kN)
            KineticCrushForceNewtons = 15000.0f + (lethalSeconds * 200.0f);
            SheltersLocked = true;
        }
    }

    /// <summary>
    /// Macro-tick execution for seasonal cycles (e.g. 1 macro tick per season or multi-cycle epoch).
    /// </summary>
    public void StepMacroTick(ulong macroTickIndex)
    {
        // 4-season sinusoidal temperature and resource oscillation
        float seasonalPhase = (float)(macroTickIndex % 4UL) / 4.0f * MathF.Tau;
        float tempDelta = MathF.Sin(seasonalPhase) * 15.0f; // -15 K (winter) to +15 K (summer)
        AmbientTemperatureKelvin = 293.15f + tempDelta;

        // Winter incurs food scarcity and hypothermia exposure
        if (AmbientTemperatureKelvin < 283.15f) // < 10 C
        {
            float coldSeverity = (283.15f - AmbientTemperatureKelvin) / 15.0f;
            SeasonalScarcityFactor = Math.Clamp(coldSeverity * 0.7f, 0.0f, 0.85f);
            SeasonalHypothermiaRate = Math.Clamp(coldSeverity * 0.05f, 0.0f, 0.1f);
        }
        else
        {
            SeasonalScarcityFactor = 0.0f;
            SeasonalHypothermiaRate = 0.0f;
        }
    }

    /// <summary>
    /// Resets the cycle after hibernation, picking a new deterministic cycle duration.
    /// </summary>
    public void ResetCycle(int newDurationSeconds = DefaultCycleDurationSeconds)
    {
        CycleDurationSeconds = Math.Clamp(newDurationSeconds, 400, 800);
        CurrentCycleTimerSeconds = 0;
        CurrentPhase = RainHazardPhase.Clear;
        TremorIntensity = 0.0f;
        LightDarkeningFactor = 0.0f;
        FloodLevelMeters = 0.0f;
        KineticCrushForceNewtons = 0.0f;
        SheltersLocked = false;
        TotalCompletedCycles++;
    }
}
