using System;
using Chimera.Core.Contracts;
using Chimera.Core.Micro.Psychology;

namespace Chimera.Core.Micro.Mental;

/// <summary>
/// Deterministic fixed-tick state machine controlling psychological breakdown,
/// break hazard accumulation, duration countdown, and post-break catharsis recovery.
/// Driven directly by the simulation tick loop (ISimulationTickable).
/// </summary>
public sealed class MentalBreakStateMachine : ISimulationTickable
{
    public MentalBreakTier CurrentTier { get; private set; } = MentalBreakTier.None;
    public MentalBreakDefId ActiveBreakDef { get; private set; } = MentalBreakDefId.None;
    public long BreakStartTick { get; private set; } = 0;
    public long BreakDurationTicks { get; private set; } = 0;
    public long BreakRemainingTicks { get; private set; } = 0;
    public float HazardAccumulator { get; private set; } = 0.0f;

    // Last evaluated thresholds
    public float MinorThreshold { get; private set; } = MoodConstants.BaseBreakThresholdMinor;
    public float MajorThreshold { get; private set; } = MoodConstants.BaseBreakThresholdMajor;
    public float ExtremeThreshold { get; private set; } = MoodConstants.BaseBreakThresholdExtreme;

    public bool IsInMentalBreak => CurrentTier != MentalBreakTier.None;

    /// <summary>
    /// Event invoked when a mental break begins.
    /// </summary>
    public event Action<MentalBreakTier, MentalBreakDefId, long>? OnBreakStarted;

    /// <summary>
    /// Event invoked when an entity recovers from a mental break.
    /// </summary>
    public event Action<MentalBreakDefId, long>? OnBreakEnded;

    /// <summary>
    /// Updates continuous thresholds from trait sliders.
    /// </summary>
    public void UpdateThresholds(float neuroticism, float ironWill, float volatility)
    {
        MentalBreakThresholds.CalculateThresholds(
            neuroticism,
            ironWill,
            volatility,
            out float minor,
            out float major,
            out float extreme);

        MinorThreshold = minor;
        MajorThreshold = major;
        ExtremeThreshold = extreme;
    }

    /// <summary>
    /// Evaluates mental state progression for this simulation tick (1 Hz).
    /// </summary>
    public void TickMentalState(
        long currentTick,
        float currentMood,
        float neuroticism,
        bool isPyromaniac,
        ThoughtMemoryPool memoryPool)
    {
        if (IsInMentalBreak)
        {
            BreakRemainingTicks--;
            if (BreakRemainingTicks <= 0)
            {
                RecoverFromBreak(currentTick, memoryPool);
            }
            return;
        }

        // Determine break tier risk
        MentalBreakTier threatenedTier = MentalBreakTier.None;
        float depth = 0.0f;
        float baseThreshold = 0.0f;

        if (currentMood < ExtremeThreshold)
        {
            threatenedTier = MentalBreakTier.Extreme;
            depth = ExtremeThreshold - currentMood;
            baseThreshold = ExtremeThreshold;
        }
        else if (currentMood < MajorThreshold)
        {
            threatenedTier = MentalBreakTier.Major;
            depth = MajorThreshold - currentMood;
            baseThreshold = MajorThreshold;
        }
        else if (currentMood < MinorThreshold)
        {
            threatenedTier = MentalBreakTier.Minor;
            depth = MinorThreshold - currentMood;
            baseThreshold = MinorThreshold;
        }

        if (threatenedTier == MentalBreakTier.None)
        {
            // Calm recovery of hazard pressure
            HazardAccumulator = MathF.Max(0.0f, HazardAccumulator - 0.002f);
            return;
        }

        // Mean Time Between (MTB) days scaled by depth below threshold
        // MTB days in RimWorld: Extreme ~0.166 days (10,000 ticks), Major ~0.166 days, Minor ~0.166 days
        // Hazard increment per tick: lambda = (1.0 / MTB) * (depth / baseThreshold)^1.5
        float relativeDepth = Math.Clamp(depth / MathF.Max(0.01f, baseThreshold), 0.01f, 1.0f);
        float baseMtbTicks = threatenedTier switch
        {
            MentalBreakTier.Extreme => 5_000.0f,
            MentalBreakTier.Major => 10_000.0f,
            _ => 15_000.0f
        };

        float neuroticismMod = 1.0f + (neuroticism * 0.5f);
        float tickHazard = (1.0f / baseMtbTicks) * MathF.Pow(relativeDepth, 1.25f) * neuroticismMod;
        
        // Accumulate deterministic break hazard
        HazardAccumulator += tickHazard;

        if (HazardAccumulator >= 1.0f)
        {
            TriggerBreak(threatenedTier, isPyromaniac, currentTick);
        }
    }

    /// <summary>
    /// Deterministically selects and activates a mental break based on tier and personality.
    /// </summary>
    public void TriggerBreak(MentalBreakTier tier, bool isPyromaniac, long currentTick)
    {
        CurrentTier = tier;
        HazardAccumulator = 0.0f;

        switch (tier)
        {
            case MentalBreakTier.Extreme:
                if (isPyromaniac)
                {
                    ActiveBreakDef = MentalBreakDefId.FireStartingSpree;
                    BreakDurationTicks = 8_000L; // REA: 6,000 - 10,000 ticks
                }
                else
                {
                    ActiveBreakDef = MentalBreakDefId.Berserk;
                    BreakDurationTicks = 50_000L; // REA: 40,000 - 60,000 ticks
                }
                break;

            case MentalBreakTier.Major:
                ActiveBreakDef = MentalBreakDefId.Tantrum;
                BreakDurationTicks = 10_000L; // REA: 8,000 - 12,000 ticks
                break;

            case MentalBreakTier.Minor:
            default:
                ActiveBreakDef = MentalBreakDefId.WanderSad;
                BreakDurationTicks = 50_000L; // REA: 40,000 - 60,000 ticks
                break;
        }

        BreakStartTick = currentTick;
        BreakRemainingTicks = BreakDurationTicks;

        OnBreakStarted?.Invoke(CurrentTier, ActiveBreakDef, currentTick);
    }

    /// <summary>
    /// Recovers from active break and grants the Catharsis thought (+40.0 mood buffer).
    /// </summary>
    public void RecoverFromBreak(long currentTick, ThoughtMemoryPool? memoryPool)
    {
        var finishedBreak = ActiveBreakDef;
        CurrentTier = MentalBreakTier.None;
        ActiveBreakDef = MentalBreakDefId.None;
        BreakRemainingTicks = 0;
        HazardAccumulator = 0.0f;

        // Add Catharsis thought memory (+40 mood, half life ~2.5 days)
        if (memoryPool != null)
        {
            memoryPool.AddMemory(
                ThoughtMemoryDefId.Catharsis,
                MoodConstants.CatharsisMoodBonus,
                0.0f,
                currentTick,
                MoodConstants.CatharsisDurationTicks,
                MoodConstants.CatharsisDurationTicks * 0.5f);
        }

        OnBreakEnded?.Invoke(finishedBreak, currentTick);
    }

    /// <summary>
    /// Forces premature recovery (e.g. arrested, knocked unconscious, sedated).
    /// </summary>
    public void InterruptBreak(long currentTick, ThoughtMemoryPool? memoryPool, bool grantCatharsis = true)
    {
        if (!IsInMentalBreak) return;

        if (grantCatharsis)
        {
            RecoverFromBreak(currentTick, memoryPool);
        }
        else
        {
            var finishedBreak = ActiveBreakDef;
            CurrentTier = MentalBreakTier.None;
            ActiveBreakDef = MentalBreakDefId.None;
            BreakRemainingTicks = 0;
            HazardAccumulator = 0.0f;
            OnBreakEnded?.Invoke(finishedBreak, currentTick);
        }
    }

    // --- ISimulationTickable implementation ---
    public void StepMicroTick(ulong microTickIndex)
    {
        // Micro ticks (60 Hz) - deterministic sub-stepping
    }

    public void StepSimulationTick(ulong simulationTickIndex)
    {
        // State updates are explicitly parameterized by TickMentalState
    }

    public void StepMacroTick(ulong macroTickIndex)
    {
    }
}
