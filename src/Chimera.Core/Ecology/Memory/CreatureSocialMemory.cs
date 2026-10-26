using System;
using System.Collections.Generic;
using Chimera.Core.MathEngine;

namespace Chimera.Core.Ecology.Memory;

/// <summary>
/// Individual creature social and encounter memory, matching Rain World's SocialMemory architecture.
/// Tracks long-term baseline affinities alongside short-term temporary emotional spikes.
/// </summary>
public sealed class CreatureSocialMemory
{
    public sealed class IndividualRelationship
    {
        public string SubjectId { get; }
        public float Like { get; private set; }         // -1.0 (mortal enemy) to +1.0 (trusted ally)
        public float Fear { get; private set; }         // 0.0 (unafraid) to 1.0 (terrified)
        public float Know { get; private set; }         // 0.0 (stranger) to 1.0 (intimate familiarity)
        public float TempLike { get; private set; }     // Temporary boost (e.g. recent food gift)
        public float TempFear { get; private set; }     // Temporary spike (e.g. recent sudden attack)

        public float EffectiveLike => Math.Clamp(Like + TempLike, -1.0f, 1.0f);
        public float EffectiveFear => Math.Clamp(Fear + TempFear, 0.0f, 1.0f);

        public IndividualRelationship(string subjectId)
        {
            SubjectId = subjectId;
            Like = 0.0f;
            Fear = 0.0f;
            Know = 0.0f;
            TempLike = 0.0f;
            TempFear = 0.0f;
        }

        public void InfluenceLike(float change)
        {
            Like = Math.Clamp(Like + change, -1.0f, 1.0f);
            InfluenceKnow(MathF.Abs(change) * 0.5f);
        }

        public void InfluenceFear(float change)
        {
            Fear = Math.Clamp(Fear + change, 0.0f, 1.0f);
            InfluenceKnow(MathF.Abs(change) * 0.5f);
        }

        public void InfluenceKnow(float amountOfLerpUpwards)
        {
            Know = ContinuousCurves.Lerp(Know, 1.0f, Math.Clamp(amountOfLerpUpwards, 0.0f, 1.0f));
        }

        public void InfluenceTempLike(float change)
        {
            TempLike = Math.Clamp(TempLike + change, -1.0f, 1.0f);
            InfluenceKnow(MathF.Abs(change) * 0.2f);
        }

        public void InfluenceTempFear(float change)
        {
            TempFear = Math.Clamp(TempFear + change, 0.0f, 1.0f);
            InfluenceKnow(MathF.Abs(change) * 0.2f);
        }

        /// <summary>
        /// Evens out temporary emotion spikes toward baseline using continuous exponential decay.
        /// Part of temp feelings bake into long-term feelings over time.
        /// </summary>
        public void EvenOutTemps(float decayRate, float dt)
        {
            // Transfer 15% of decayed temporary feeling into persistent memory
            float bakedLikeDelta = TempLike * 0.15f * Math.Clamp(decayRate * dt, 0.0f, 1.0f);
            float bakedFearDelta = TempFear * 0.15f * Math.Clamp(decayRate * dt, 0.0f, 1.0f);

            Like = Math.Clamp(Like + bakedLikeDelta, -1.0f, 1.0f);
            Fear = Math.Clamp(Fear + bakedFearDelta, 0.0f, 1.0f);

            TempLike = ContinuousCurves.ExponentialDecay(TempLike, decayRate, dt);
            TempFear = ContinuousCurves.ExponentialDecay(TempFear, decayRate, dt);

            if (MathF.Abs(TempLike) < 0.001f) TempLike = 0.0f;
            if (TempFear < 0.001f) TempFear = 0.0f;
        }

        /// <summary>
        /// End-of-cycle decay: slight fade of long-term extremes unless reinforced.
        /// </summary>
        public void CycleTick()
        {
            Like *= 0.98f;
            Fear *= 0.98f;
            TempLike = 0.0f;
            TempFear = 0.0f;
        }
    }

    private readonly Dictionary<string, IndividualRelationship> _relationships = new(StringComparer.OrdinalIgnoreCase);

    public IndividualRelationship GetOrInitiate(string subjectId)
    {
        if (!_relationships.TryGetValue(subjectId, out var rel))
        {
            rel = new IndividualRelationship(subjectId);
            _relationships[subjectId] = rel;
        }
        return rel;
    }

    public bool TryGet(string subjectId, out IndividualRelationship relationship)
    {
        return _relationships.TryGetValue(subjectId, out relationship!);
    }

    public void StepSimulationTick(float decayRate, float dt)
    {
        foreach (var rel in _relationships.Values)
        {
            rel.EvenOutTemps(decayRate, dt);
        }
    }

    public void StepCycleTick()
    {
        foreach (var rel in _relationships.Values)
        {
            rel.CycleTick();
        }
    }
}
