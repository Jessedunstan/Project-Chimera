using System;
using Chimera.Core.MathEngine;

namespace Chimera.Core.Micro.Psychology;

/// <summary>
/// Identifiers for common psychological and social thought memories.
/// </summary>
public enum ThoughtMemoryDefId : ushort
{
    None = 0,
    Catharsis = 1,
    Chitchat = 2,
    DeepTalk = 3,
    Slighted = 4,
    Insulted = 5,
    KindWords = 6,
    KilledNuclearKin = 7,
    KilledSpouse = 8,
    KilledLover = 9,
    KilledFriend = 10,
    KilledRival = 11,
    HoneymoonPhase = 12,
    RescuedMe = 13,
    CrashedTogether = 14,
    RebuffedRomance = 15,
    CheatedOrDivorced = 16,
    AteLavishMeal = 17,
    WitnessedDeath = 18,
    ObservedCorpse = 19
}

/// <summary>
/// Zero-allocation, blittable struct representing a single decaying thought memory.
/// </summary>
public struct ThoughtMemoryRecord
{
    public ThoughtMemoryDefId DefId;
    public ulong TargetEntityId;     // 0 if non-social/situational
    public float InitialMoodOffset;
    public float InitialOpinionOffset;
    public long StartTick;
    public long DurationTicks;
    public float HalfLifeTicks;
    public float StackMultiplier;    // Defaults to 1.0; 0.9 for stacked social interactions
    public bool IsActive;

    /// <summary>
    /// Computes the current decayed mood offset at a specific simulation tick.
    /// Uses ContinuousCurves.ExponentialDecay based on half-life.
    /// </summary>
    public readonly float GetCurrentMoodOffset(long currentTick)
    {
        if (!IsActive || currentTick < StartTick) return 0.0f;
        long elapsed = currentTick - StartTick;
        if (DurationTicks > 0 && elapsed >= DurationTicks) return 0.0f;

        float decayRate = MathF.Log(2.0f) / MathF.Max(1.0f, HalfLifeTicks);
        float decayed = ContinuousCurves.ExponentialDecay(InitialMoodOffset, decayRate, elapsed);
        return decayed * StackMultiplier;
    }

    /// <summary>
    /// Computes the current decayed opinion offset at a specific simulation tick.
    /// </summary>
    public readonly float GetCurrentOpinionOffset(long currentTick)
    {
        if (!IsActive || currentTick < StartTick) return 0.0f;
        long elapsed = currentTick - StartTick;
        if (DurationTicks > 0 && elapsed >= DurationTicks) return 0.0f;

        float decayRate = MathF.Log(2.0f) / MathF.Max(1.0f, HalfLifeTicks);
        float decayed = ContinuousCurves.ExponentialDecay(InitialOpinionOffset, decayRate, elapsed);
        return decayed * StackMultiplier;
    }

    /// <summary>
    /// Checks if this memory has expired.
    /// </summary>
    public readonly bool CheckIsExpired(long currentTick)
    {
        if (!IsActive) return true;
        long elapsed = currentTick - StartTick;
        if (DurationTicks > 0 && elapsed >= DurationTicks) return true;
        // Check if decayed below perceptible epsilon
        return MathF.Abs(GetCurrentMoodOffset(currentTick)) < 0.05f 
            && MathF.Abs(GetCurrentOpinionOffset(currentTick)) < 0.05f;
    }
}

/// <summary>
/// Fixed-capacity, zero-allocation pool storing active thought memories for an entity.
/// Guarantees determinism, zero GC pressure, and cache-friendly contiguous struct memory.
/// </summary>
public sealed class ThoughtMemoryPool
{
    public const int MaxMemories = 32;

    private readonly ThoughtMemoryRecord[] _records = new ThoughtMemoryRecord[MaxMemories];

    /// <summary>
    /// Adds a thought memory into the pool with zero allocation.
    /// Automatically applies RimWorld's 0.9 stackedEffectMultiplier for repeat interactions with the same target.
    /// </summary>
    public bool AddMemory(
        ThoughtMemoryDefId defId,
        float moodOffset,
        float opinionOffset,
        long currentTick,
        long durationTicks,
        float halfLifeTicks,
        ulong targetEntityId = 0)
    {
        // Calculate stacking multiplier if existing memories of same def & target exist
        int existingCount = 0;
        int emptyIndex = -1;

        for (int i = 0; i < MaxMemories; i++)
        {
            if (_records[i].IsActive)
            {
                if (_records[i].CheckIsExpired(currentTick))
                {
                    _records[i].IsActive = false;
                    if (emptyIndex == -1) emptyIndex = i;
                }
                else if (_records[i].DefId == defId && _records[i].TargetEntityId == targetEntityId)
                {
                    existingCount++;
                }
            }
            else if (emptyIndex == -1)
            {
                emptyIndex = i;
            }
        }

        if (emptyIndex == -1)
        {
            // Pool is saturated; find the oldest expired or weakest memory to overwrite
            float minImpact = float.MaxValue;
            for (int i = 0; i < MaxMemories; i++)
            {
                float impact = MathF.Abs(_records[i].GetCurrentMoodOffset(currentTick)) 
                             + MathF.Abs(_records[i].GetCurrentOpinionOffset(currentTick));
                if (impact < minImpact)
                {
                    minImpact = impact;
                    emptyIndex = i;
                }
            }
        }

        float stackMultiplier = MathF.Pow(0.9f, existingCount);

        _records[emptyIndex] = new ThoughtMemoryRecord
        {
            DefId = defId,
            TargetEntityId = targetEntityId,
            InitialMoodOffset = moodOffset,
            InitialOpinionOffset = opinionOffset,
            StartTick = currentTick,
            DurationTicks = durationTicks,
            HalfLifeTicks = halfLifeTicks,
            StackMultiplier = stackMultiplier,
            IsActive = true
        };

        return true;
    }

    /// <summary>
    /// Computes the aggregate mood offset of all active memories at the given tick.
    /// </summary>
    public float GetAggregateMoodOffset(long currentTick)
    {
        float total = 0.0f;
        for (int i = 0; i < MaxMemories; i++)
        {
            if (_records[i].IsActive)
            {
                total += _records[i].GetCurrentMoodOffset(currentTick);
            }
        }
        return total;
    }

    /// <summary>
    /// Computes the aggregate opinion offset towards a specific target pawn at the given tick.
    /// </summary>
    public float GetAggregateOpinionOffset(ulong targetEntityId, long currentTick)
    {
        if (targetEntityId == 0) return 0.0f;
        float total = 0.0f;
        for (int i = 0; i < MaxMemories; i++)
        {
            if (_records[i].IsActive && _records[i].TargetEntityId == targetEntityId)
            {
                total += _records[i].GetCurrentOpinionOffset(currentTick);
            }
        }
        return total;
    }

    /// <summary>
    /// Prunes expired memories at the current tick. Zero heap allocations.
    /// </summary>
    public void CleanExpired(long currentTick)
    {
        for (int i = 0; i < MaxMemories; i++)
        {
            if (_records[i].IsActive && _records[i].CheckIsExpired(currentTick))
            {
                _records[i].IsActive = false;
            }
        }
    }

    /// <summary>
    /// Clears all memories.
    /// </summary>
    public void Clear()
    {
        Array.Clear(_records);
    }
}
