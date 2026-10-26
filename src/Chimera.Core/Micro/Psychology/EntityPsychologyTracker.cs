using System;
using Chimera.Core.Contracts;
using Chimera.Core.MathEngine;
using Chimera.Core.Micro.Mental;
using Chimera.Core.Micro.Social;

namespace Chimera.Core.Micro.Psychology;

/// <summary>
/// Unified psychology and social tracker for individual entities in Project Chimera.
/// Combines continuous needs, personality traits, memory pools, social relationships,
/// and mental break state machines.
/// Implements ISimulationTickable for fixed-tick updates and ISliderModifiable for continuous sliders.
/// </summary>
public sealed class EntityPsychologyTracker : ISimulationTickable, ISliderModifiable
{
    public ulong EntityId { get; }

    // --- Continuous Needs [0.0, 1.0] ---
    public float Saturation { get; set; } = 1.0f; // Food
    public float Rest { get; set; } = 1.0f;       // Sleep / Energy
    public float Joy { get; set; } = 1.0f;        // Recreation
    public float Comfort { get; set; } = 0.50f;   // Physical comfort
    public float Beauty { get; set; } = 0.0f;     // Environmental beauty [-1.0, 1.0]
    public float RoomSize { get; set; } = 0.50f;  // Space / room volume [0.0, 1.0]
    public float Pain { get; set; } = 0.0f;       // Physical agony [0.0, 1.0]
    public float AmbientTemperatureK { get; set; } = 294.15f; // 21°C Kelvin

    // --- Personality & Trait Continuous Sliders [0.0, 1.0] ---
    public float Neuroticism { get; set; } = 0.0f;  // SliderId 303
    public float Devotion { get; set; } = 0.5f;     // SliderId 304
    public float Tribalism { get; set; } = 0.5f;    // SliderId 305
    public float IronWill { get; set; } = 0.0f;     // SliderId 306
    public float Volatility { get; set; } = 0.0f;
    public bool IsMasochist { get; set; } = false;
    public bool IsPyromaniac { get; set; } = false;
    public bool IsSleeping { get; set; } = false;

    // --- Mood Dynamics ---
    public float CurrentMood { get; private set; } = MoodConstants.BaselineMood;
    public float TargetMood { get; private set; } = MoodConstants.BaselineMood;

    // --- Pools & State Machine ---
    public ThoughtMemoryPool MemoryPool { get; } = new();
    public SocialRelationshipPool RelationshipPool { get; } = new();
    public MentalBreakStateMachine BreakStateMachine { get; } = new();

    public EntityPsychologyTracker(ulong entityId, float initialMood = 0.50f)
    {
        EntityId = entityId;
        CurrentMood = Math.Clamp(initialMood, 0.0f, 1.0f);
        TargetMood = CurrentMood;
    }

    /// <summary>
    /// Evaluates all continuous need curves and computes the instantaneous target mood.
    /// </summary>
    public float EvaluateTargetMood(long currentTick)
    {
        float hungerOffset = ContinuousMoodModel.EvaluateHungerMoodOffset(Saturation);
        float restOffset = ContinuousMoodModel.EvaluateRestMoodOffset(Rest);
        float joyOffset = ContinuousMoodModel.EvaluateJoyMoodOffset(Joy);
        float comfortOffset = ContinuousMoodModel.EvaluateComfortMoodOffset(Comfort);
        float beautyOffset = ContinuousMoodModel.EvaluateBeautyMoodOffset(Beauty);
        float roomOffset = ContinuousMoodModel.EvaluateRoomSizeMoodOffset(RoomSize);
        float painOffset = ContinuousMoodModel.EvaluatePainMoodOffset(Pain, IsMasochist);
        float thermalOffset = ContinuousMoodModel.EvaluateThermalMoodOffset(AmbientTemperatureK);
        float memorySum = MemoryPool.GetAggregateMoodOffset(currentTick);

        TargetMood = ContinuousMoodModel.CalculateTargetMood(
            hungerOffset,
            restOffset,
            joyOffset,
            comfortOffset,
            beautyOffset,
            roomOffset,
            painOffset,
            thermalOffset,
            memorySum);

        return TargetMood;
    }

    /// <summary>
    /// Evaluates interpersonal opinion towards another entity at currentTick.
    /// </summary>
    public float EvaluateOpinionTowards(ulong targetEntityId, long currentTick)
    {
        return RelationshipPool.EvaluateOpinion(
            targetEntityId,
            Devotion,
            Tribalism,
            Neuroticism,
            MemoryPool,
            currentTick);
    }

    // --- ISimulationTickable Implementation ---

    public void StepMicroTick(ulong microTickIndex)
    {
        // 60 Hz micro physics tick
    }

    public void StepSimulationTick(ulong simulationTickIndex)
    {
        long tick = (long)simulationTickIndex;

        // 1. Evaluate target mood from continuous need curves and memories
        EvaluateTargetMood(tick);

        // 2. Advance current mood toward target using RimWorld seeker rates
        CurrentMood = ContinuousMoodModel.StepMoodTowardTarget(
            CurrentMood,
            TargetMood,
            1L,
            IsSleeping);

        // 3. Update continuous mental break thresholds
        BreakStateMachine.UpdateThresholds(Neuroticism, IronWill, Volatility);

        // 4. Tick mental break state machine
        BreakStateMachine.TickMentalState(
            tick,
            CurrentMood,
            Neuroticism,
            IsPyromaniac,
            MemoryPool);

        // 5. Clean expired memories periodically (e.g. every 500 ticks)
        if (tick % 500L == 0)
        {
            MemoryPool.CleanExpired(tick);
        }
    }

    public void StepMacroTick(ulong macroTickIndex)
    {
    }

    // --- ISliderModifiable Implementation ---

    public float GetSliderValue(string parameterKey)
    {
        return parameterKey switch
        {
            "mood" or "CurrentMood" => CurrentMood,
            "target_mood" or "TargetMood" => TargetMood,
            "hunger" or "Saturation" => Saturation,
            "rest" or "Rest" => Rest,
            "joy" or "Joy" => Joy,
            "comfort" or "Comfort" => Comfort,
            "beauty" or "Beauty" => Beauty,
            "pain" or "Pain" => Pain,
            "neuroticism" or "Neuroticism" => Neuroticism,
            "devotion" or "Devotion" => Devotion,
            "tribalism" or "Tribalism" => Tribalism,
            "iron_will" or "IronWill" => IronWill,
            "volatility" or "Volatility" => Volatility,
            "break_minor_threshold" => BreakStateMachine.MinorThreshold,
            "break_major_threshold" => BreakStateMachine.MajorThreshold,
            "break_extreme_threshold" => BreakStateMachine.ExtremeThreshold,
            _ => 0.0f
        };
    }

    public void SetSliderValue(string parameterKey, float value)
    {
        switch (parameterKey)
        {
            case "hunger" or "Saturation":
                Saturation = Math.Clamp(value, 0.0f, 1.0f);
                break;
            case "rest" or "Rest":
                Rest = Math.Clamp(value, 0.0f, 1.0f);
                break;
            case "joy" or "Joy":
                Joy = Math.Clamp(value, 0.0f, 1.0f);
                break;
            case "comfort" or "Comfort":
                Comfort = Math.Clamp(value, 0.0f, 1.0f);
                break;
            case "beauty" or "Beauty":
                Beauty = Math.Clamp(value, -1.0f, 1.0f);
                break;
            case "pain" or "Pain":
                Pain = Math.Clamp(value, 0.0f, 1.0f);
                break;
            case "neuroticism" or "Neuroticism":
                Neuroticism = Math.Clamp(value, 0.0f, 1.0f);
                break;
            case "devotion" or "Devotion":
                Devotion = Math.Clamp(value, 0.0f, 1.0f);
                break;
            case "tribalism" or "Tribalism":
                Tribalism = Math.Clamp(value, 0.0f, 1.0f);
                break;
            case "iron_will" or "IronWill":
                IronWill = Math.Clamp(value, 0.0f, 1.0f);
                break;
            case "volatility" or "Volatility":
                Volatility = Math.Clamp(value, 0.0f, 1.0f);
                break;
            case "mood" or "CurrentMood":
                CurrentMood = Math.Clamp(value, 0.0f, 1.0f);
                break;
        }
    }

    /// <summary>
    /// Strongly-typed SliderId accessor.
    /// </summary>
    public float GetSliderValue(SliderId sliderId)
    {
        return sliderId switch
        {
            SliderId.SocialNeuroticismEvaluator => Neuroticism,
            SliderId.SocialDevotionEvaluator => Devotion,
            SliderId.SocialTribalismEvaluator => Tribalism,
            SliderId.SocialIronWillEvaluator => IronWill,
            SliderId.SocialOpinionBaseline => MoodConstants.BaselineMood,
            SliderId.SocialMoodBreakMinorThreshold => BreakStateMachine.MinorThreshold,
            SliderId.SocialMoodBreakMajorThreshold => BreakStateMachine.MajorThreshold,
            SliderId.SocialMoodBreakExtremeThreshold => BreakStateMachine.ExtremeThreshold,
            _ => 0.0f
        };
    }

    /// <summary>
    /// Strongly-typed SliderId setter.
    /// </summary>
    public void SetSliderValue(SliderId sliderId, float value)
    {
        switch (sliderId)
        {
            case SliderId.SocialNeuroticismEvaluator:
                Neuroticism = Math.Clamp(value, 0.0f, 1.0f);
                break;
            case SliderId.SocialDevotionEvaluator:
                Devotion = Math.Clamp(value, 0.0f, 1.0f);
                break;
            case SliderId.SocialTribalismEvaluator:
                Tribalism = Math.Clamp(value, 0.0f, 1.0f);
                break;
            case SliderId.SocialIronWillEvaluator:
                IronWill = Math.Clamp(value, 0.0f, 1.0f);
                break;
        }
    }
}
