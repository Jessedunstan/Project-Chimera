using System;
using Chimera.Core.Micro.Psychology;

namespace Chimera.Core.Micro.Social;

/// <summary>
/// Zero-allocation, blittable struct representing a pairwise social bond with another entity.
/// </summary>
public struct SocialRelationshipRecord
{
    public ulong TargetEntityId;
    public float ReligiousAlignment;   // [-1.0, 1.0]
    public float ClanFeudScore;         // [-1.0, 1.0]
    public float SportsRivalryScore;    // [-0.2, 0.2]
    public float CumulativeChitchat;    // Up to maxCumulatedOpinionOffset (10.0 in RimWorld)
    public bool IsActive;

    /// <summary>
    /// Resets record to clean state.
    /// </summary>
    public void Reset()
    {
        TargetEntityId = 0;
        ReligiousAlignment = 0.0f;
        ClanFeudScore = 0.0f;
        SportsRivalryScore = 0.0f;
        CumulativeChitchat = 0.0f;
        IsActive = false;
    }
}

/// <summary>
/// Pre-allocated struct pool storing social bonds for an entity.
/// Guarantees zero heap allocation, deterministic indexing, and fast evaluation.
/// </summary>
public sealed class SocialRelationshipPool
{
    public const int MaxRelationships = 32;

    private readonly SocialRelationshipRecord[] _relationships = new SocialRelationshipRecord[MaxRelationships];

    /// <summary>
    /// Finds or creates a relationship record for the target entity with zero heap allocation.
    /// </summary>
    public int GetOrCreateRelationshipIndex(ulong targetEntityId)
    {
        if (targetEntityId == 0) return -1;

        int firstEmpty = -1;
        for (int i = 0; i < MaxRelationships; i++)
        {
            if (_relationships[i].IsActive && _relationships[i].TargetEntityId == targetEntityId)
            {
                return i;
            }
            if (!_relationships[i].IsActive && firstEmpty == -1)
            {
                firstEmpty = i;
            }
        }

        if (firstEmpty != -1)
        {
            _relationships[firstEmpty].Reset();
            _relationships[firstEmpty].TargetEntityId = targetEntityId;
            _relationships[firstEmpty].IsActive = true;
            return firstEmpty;
        }

        return -1; // Pool full
    }

    /// <summary>
    /// Configures ideological and familial baseline scores for a relationship.
    /// </summary>
    public void ConfigureRelationship(
        ulong targetEntityId,
        float religiousAlignment,
        float clanFeudScore,
        float sportsRivalryScore)
    {
        int index = GetOrCreateRelationshipIndex(targetEntityId);
        if (index >= 0)
        {
            _relationships[index].ReligiousAlignment = Math.Clamp(religiousAlignment, -1.0f, 1.0f);
            _relationships[index].ClanFeudScore = Math.Clamp(clanFeudScore, -1.0f, 1.0f);
            _relationships[index].SportsRivalryScore = Math.Clamp(sportsRivalryScore, -0.20f, 0.20f);
        }
    }

    /// <summary>
    /// Adds casual chitchat opinion (+0.66 per occurrence, capped at 10.0 in RimWorld).
    /// </summary>
    public void AddChitchat(ulong targetEntityId, float amount = 0.66f, float maxCumulative = 10.0f)
    {
        int index = GetOrCreateRelationshipIndex(targetEntityId);
        if (index >= 0)
        {
            _relationships[index].CumulativeChitchat = MathF.Min(
                maxCumulative,
                _relationships[index].CumulativeChitchat + amount);
        }
    }

    /// <summary>
    /// Computes total opinion score of the observer towards the target entity at currentTick.
    /// </summary>
    public float EvaluateOpinion(
        ulong targetEntityId,
        float observerDevotion,
        float observerTribalism,
        float observerNeuroticism,
        ThoughtMemoryPool memoryPool,
        long currentTick)
    {
        int index = -1;
        for (int i = 0; i < MaxRelationships; i++)
        {
            if (_relationships[i].IsActive && _relationships[i].TargetEntityId == targetEntityId)
            {
                index = i;
                break;
            }
        }

        float relAlign = (index >= 0) ? _relationships[index].ReligiousAlignment : 0.0f;
        float feudScore = (index >= 0) ? _relationships[index].ClanFeudScore : 0.0f;
        float sportsScore = (index >= 0) ? _relationships[index].SportsRivalryScore : 0.0f;
        float chitchat = (index >= 0) ? _relationships[index].CumulativeChitchat : 0.0f;

        float memoryOffset = memoryPool != null 
            ? memoryPool.GetAggregateOpinionOffset(targetEntityId, currentTick) 
            : 0.0f;

        float totalMemorySum = chitchat + memoryOffset;

        return SocialAffinityEvaluator.CalculateCompositeOpinion(
            relAlign,
            observerDevotion,
            feudScore,
            observerTribalism,
            sportsScore,
            observerNeuroticism,
            totalMemorySum);
    }

    /// <summary>
    /// Resets all relationships.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < MaxRelationships; i++)
        {
            _relationships[i].Reset();
        }
    }
}
