using System;
using Chimera.Core.MathEngine;

namespace Chimera.Core.Micro.Psychology;

/// <summary>
/// Represents a psychological or social memory that decays continuously over simulation time.
/// Uses continuous exponential decay: V(t) = V0 * exp(-lambda * t).
/// </summary>
public sealed class ThoughtMemory
{
    public string DefName { get; }
    public float InitialMoodOffset { get; }
    public float InitialOpinionOffset { get; }
    public ulong TargetEntityId { get; }
    public float HalfLifeSeconds { get; }
    public float DecayRate { get; }
    public float ElapsedSeconds { get; private set; }

    public ThoughtMemory(
        string defName,
        float moodOffset,
        float opinionOffset,
        float halfLifeSeconds,
        ulong targetEntityId = 0)
    {
        DefName = defName;
        InitialMoodOffset = moodOffset;
        InitialOpinionOffset = opinionOffset;
        TargetEntityId = targetEntityId;
        HalfLifeSeconds = MathF.Max(1.0f, halfLifeSeconds);
        // Half life formula: lambda = ln(2) / t_half
        DecayRate = MathF.Log(2.0f) / HalfLifeSeconds;
        ElapsedSeconds = 0.0f;
    }

    /// <summary>
    /// Advances memory age by delta time.
    /// </summary>
    public void StepTime(float deltaSeconds)
    {
        if (deltaSeconds > 0.0f)
        {
            ElapsedSeconds += deltaSeconds;
        }
    }

    /// <summary>
    /// Current decayed mood impact.
    /// </summary>
    public float CurrentMoodOffset =>
        ContinuousCurves.ExponentialDecay(InitialMoodOffset, DecayRate, ElapsedSeconds);

    /// <summary>
    /// Current decayed opinion impact (for social target).
    /// </summary>
    public float CurrentOpinionOffset =>
        ContinuousCurves.ExponentialDecay(InitialOpinionOffset, DecayRate, ElapsedSeconds);

    /// <summary>
    /// True if memory effect has decayed below perceptible threshold (e.g. 0.01).
    /// </summary>
    public bool IsExpired =>
        MathF.Abs(CurrentMoodOffset) < 0.01f && MathF.Abs(CurrentOpinionOffset) < 0.01f;

    /// <summary>
    /// Static factory for standard Catharsis thought (+40.0 mood buffer post-mental break).
    /// Decays over 2.5 game days (150,000 seconds / ticks).
    /// </summary>
    public static ThoughtMemory CreateCatharsis(float halfLifeSeconds = 100_000.0f)
    {
        return new ThoughtMemory("Catharsis", 40.0f, 0.0f, halfLifeSeconds);
    }
}
