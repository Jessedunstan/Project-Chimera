using System;
using Xunit;
using Chimera.Core.MathEngine;
using Chimera.Core.Micro.Psychology;
using Chimera.Core.Micro.Social;
using Chimera.Core.Micro.Mental;

namespace Chimera.Tests;

public class PsychologyAndSocialTests
{
    [Fact]
    public void ContinuousMoodModel_EvaluatesHunger_SmoothlyAndMonotonically()
    {
        // Saturation 1.0 -> positive contentment
        float fullMood = ContinuousMoodModel.EvaluateHungerMoodOffset(1.0f);
        Assert.True(fullMood > 0.0f && fullMood <= 4.0f);

        // Saturation 0.7 -> neutral boundary (0.0)
        float neutralMood = ContinuousMoodModel.EvaluateHungerMoodOffset(0.70f);
        Assert.InRange(neutralMood, -0.01f, 0.01f);

        // Saturation 0.0 -> max starvation penalty (-44.0)
        float starvingMood = ContinuousMoodModel.EvaluateHungerMoodOffset(0.0f);
        Assert.Equal(-44.0f, starvingMood);

        // Monotonic decrease as saturation decreases
        float prev = fullMood;
        for (float s = 0.95f; s >= 0.0f; s -= 0.05f)
        {
            float cur = ContinuousMoodModel.EvaluateHungerMoodOffset(s);
            Assert.True(cur <= prev, $"Expected monotonic decrease at saturation {s}: cur={cur}, prev={prev}");
            prev = cur;
        }
    }

    [Fact]
    public void ContinuousMoodModel_PainInversion_Masochist()
    {
        float normalPain = ContinuousMoodModel.EvaluatePainMoodOffset(0.8f, isMasochist: false);
        float masochistPain = ContinuousMoodModel.EvaluatePainMoodOffset(0.8f, isMasochist: true);

        Assert.True(normalPain < -10.0f);
        Assert.True(masochistPain > 10.0f);
        Assert.Equal(-normalPain, masochistPain, 3);
    }

    [Fact]
    public void ThoughtMemoryPool_ExponentialDecayAndStackingMultiplier()
    {
        var pool = new ThoughtMemoryPool();
        long startTick = 1000L;
        long durationTicks = 60_000L;
        float halfLifeTicks = 20_000L;

        // Add first insult (-15 opinion)
        pool.AddMemory(ThoughtMemoryDefId.Insulted, -5.0f, -15.0f, startTick, durationTicks, halfLifeTicks, targetEntityId: 42);
        
        // Opinion at start
        float op0 = pool.GetAggregateOpinionOffset(42, startTick);
        Assert.Equal(-15.0f, op0, 2);

        // Opinion after 1 half-life (20,000 ticks) -> should decay to ~ -7.5
        float opHalf = pool.GetAggregateOpinionOffset(42, startTick + 20_000L);
        Assert.InRange(opHalf, -7.6f, -7.4f);

        // Add second insult at startTick -> should stack with 0.9 multiplier (-15 * 0.9 = -13.5)
        var poolStack = new ThoughtMemoryPool();
        poolStack.AddMemory(ThoughtMemoryDefId.Insulted, -5.0f, -15.0f, startTick, durationTicks, halfLifeTicks, targetEntityId: 42);
        poolStack.AddMemory(ThoughtMemoryDefId.Insulted, -5.0f, -15.0f, startTick, durationTicks, halfLifeTicks, targetEntityId: 42);

        float stackedOp = poolStack.GetAggregateOpinionOffset(42, startTick);
        Assert.Equal(-15.0f + (-15.0f * 0.9f), stackedOp, 2);
    }

    [Fact]
    public void SocialAffinityEvaluator_AppliesContinuousAffinityAndNeuroticGrievanceScaling()
    {
        // 1. Same religion (+1.0) with devotion 1.0 vs Calm observer (neuroticism 0)
        float calmRelHarmonious = SocialAffinityEvaluator.EvaluateReligiousCompatibility(
            religiousAlignmentScore: 1.0f,
            observerDevotion: 1.0f,
            observerNeuroticism: 0.0f);
        Assert.Equal(30.0f, calmRelHarmonious);

        // 2. Heresy (-1.0) with devotion 1.0 vs Calm observer (neuroticism 0) -> -30.0
        float calmRelHeresy = SocialAffinityEvaluator.EvaluateReligiousCompatibility(
            religiousAlignmentScore: -1.0f,
            observerDevotion: 1.0f,
            observerNeuroticism: 0.0f);
        Assert.Equal(-30.0f, calmRelHeresy);

        // 3. Heresy (-1.0) with devotion 1.0 vs Extreme Neurotic observer (neuroticism 1.0)
        // Multiplier is 2.0x -> -60.0!
        float neuroticRelHeresy = SocialAffinityEvaluator.EvaluateReligiousCompatibility(
            religiousAlignmentScore: -1.0f,
            observerDevotion: 1.0f,
            observerNeuroticism: 1.0f);
        Assert.Equal(-60.0f, neuroticRelHeresy);

        // 4. Clan blood feud (-1.0) scaled by tribalism (1.0) & neuroticism (1.0)
        float neuroticFeud = SocialAffinityEvaluator.EvaluateClanFeudAffinity(
            clanFeudScore: -1.0f,
            observerTribalism: 1.0f,
            observerNeuroticism: 1.0f);
        Assert.Equal(-70.0f, neuroticFeud);

        // 5. Sports rivalry (-0.2)
        float sportsRivalry = SocialAffinityEvaluator.EvaluateSportsRivalryAffinity(
            sportsRivalryScore: -0.20f,
            observerNeuroticism: 0.0f);
        Assert.Equal(-15.0f, sportsRivalry);

        // 6. Sports alignment (+0.2)
        float sportsAlly = SocialAffinityEvaluator.EvaluateSportsRivalryAffinity(
            sportsRivalryScore: 0.20f,
            observerNeuroticism: 0.0f);
        Assert.Equal(15.0f, sportsAlly);
    }

    [Fact]
    public void MentalBreakThresholds_MonotonicOrderAndTraitOffsets()
    {
        // Base thresholds
        MentalBreakThresholds.CalculateThresholds(
            neuroticism: 0.0f,
            ironWill: 0.0f,
            volatility: 0.0f,
            out float minor,
            out float major,
            out float extreme);

        Assert.Equal(0.35f, minor, 2);
        Assert.Equal(0.20f, major, 2);
        Assert.Equal(0.05f, extreme, 2);

        // High neuroticism raises break thresholds (breaks more easily)
        MentalBreakThresholds.CalculateThresholds(
            neuroticism: 1.0f,
            ironWill: 0.0f,
            volatility: 0.0f,
            out float nMinor,
            out float nMajor,
            out float nExtreme);

        Assert.True(nMinor > minor);
        Assert.True(nMajor > major);
        Assert.True(nExtreme > extreme);
        // Order invariant
        Assert.True(nMinor > nMajor && nMajor > nExtreme);

        // Iron willed lowers break thresholds (resists breakdown)
        MentalBreakThresholds.CalculateThresholds(
            neuroticism: 0.0f,
            ironWill: 1.0f,
            volatility: 0.0f,
            out float iwMinor,
            out float iwMajor,
            out float iwExtreme);

        Assert.True(iwMinor < minor);
        Assert.True(iwMajor < major);
        Assert.True(iwExtreme < extreme);
        Assert.True(iwMinor > iwMajor && iwMajor > iwExtreme);
    }

    [Fact]
    public void MentalBreakStateMachine_TriggersBreakAndRecoversWithCatharsis()
    {
        var tracker = new EntityPsychologyTracker(entityId: 101, initialMood: 0.02f);
        tracker.Saturation = 0.0f; // Starving
        tracker.Rest = 0.0f;       // Exhausted
        tracker.Pain = 0.9f;       // Severe agony

        // Tick simulation until break triggers
        ulong tick = 0;
        while (!tracker.BreakStateMachine.IsInMentalBreak && tick < 50_000)
        {
            tick++;
            tracker.StepSimulationTick(tick);
        }

        Assert.True(tracker.BreakStateMachine.IsInMentalBreak, "Entity should trigger mental break under extreme distress.");
        Assert.Equal(MentalBreakTier.Extreme, tracker.BreakStateMachine.CurrentTier);
        Assert.Equal(MentalBreakDefId.Berserk, tracker.BreakStateMachine.ActiveBreakDef);

        // Force complete break recovery
        tracker.BreakStateMachine.RecoverFromBreak((long)tick, tracker.MemoryPool);

        Assert.False(tracker.BreakStateMachine.IsInMentalBreak);
        // Catharsis memory should now exist in memory pool (+40 mood)
        float catharsisOffset = tracker.MemoryPool.GetAggregateMoodOffset((long)tick);
        Assert.Equal(40.0f, catharsisOffset, 1);
    }

    [Fact]
    public void MentalBreakStateMachine_Pyromaniac_TriggersFireStartingSpree()
    {
        var tracker = new EntityPsychologyTracker(entityId: 102, initialMood: 0.01f)
        {
            IsPyromaniac = true,
            Saturation = 0.0f,
            Pain = 1.0f
        };

        ulong tick = 0;
        while (!tracker.BreakStateMachine.IsInMentalBreak && tick < 50_000)
        {
            tick++;
            tracker.StepSimulationTick(tick);
        }

        Assert.True(tracker.BreakStateMachine.IsInMentalBreak);
        Assert.Equal(MentalBreakTier.Extreme, tracker.BreakStateMachine.CurrentTier);
        Assert.Equal(MentalBreakDefId.FireStartingSpree, tracker.BreakStateMachine.ActiveBreakDef);
        Assert.Equal(8_000L, tracker.BreakStateMachine.BreakDurationTicks);
    }

    [Fact]
    public void EntityPsychologyTracker_ISliderModifiable_StronglyTypedAndStringKey()
    {
        var tracker = new EntityPsychologyTracker(entityId: 200);

        // Test strongly-typed SliderId
        tracker.SetSliderValue(SliderId.SocialNeuroticismEvaluator, 0.75f);
        Assert.Equal(0.75f, tracker.GetSliderValue(SliderId.SocialNeuroticismEvaluator), 2);
        Assert.Equal(0.75f, tracker.Neuroticism, 2);

        tracker.SetSliderValue(SliderId.SocialDevotionEvaluator, 0.90f);
        Assert.Equal(0.90f, tracker.GetSliderValue(SliderId.SocialDevotionEvaluator), 2);

        // Test string interface
        tracker.SetSliderValue("iron_will", 0.65f);
        Assert.Equal(0.65f, tracker.GetSliderValue("iron_will"), 2);
        Assert.Equal(0.65f, tracker.IronWill, 2);
    }

    [Fact]
    public void EntityPsychologyTracker_MoodSeeker_AdvancesAtExactRimWorldPerHourRates()
    {
        var tracker = new EntityPsychologyTracker(entityId: 300, initialMood: 0.32f);
        // Force all needs to 100% full to make target mood high (~0.60+)
        tracker.Saturation = 1.0f;
        tracker.Rest = 1.0f;
        tracker.Joy = 1.0f;
        tracker.Comfort = 1.0f;
        tracker.Beauty = 1.0f;
        tracker.RoomSize = 1.0f;
        tracker.Pain = 0.0f;

        float target = tracker.EvaluateTargetMood(0);
        Assert.True(target > 0.32f);

        // Step 1 in-game hour (2,500 simulation ticks)
        for (ulong tick = 1; tick <= MoodConstants.TicksPerInGameHour; tick++)
        {
            tracker.StepSimulationTick(tick);
        }

        // Mood should have risen by exactly SeekerRisePerHour (0.12)
        float expectedMood = 0.32f + MoodConstants.SeekerRisePerHour;
        Assert.InRange(tracker.CurrentMood, expectedMood - 0.01f, expectedMood + 0.01f);

        // While sleeping, mood progression freezes
        tracker.IsSleeping = true;
        float moodBeforeSleep = tracker.CurrentMood;
        for (ulong tick = 1; tick <= MoodConstants.TicksPerInGameHour; tick++)
        {
            tracker.StepSimulationTick(tick);
        }
        Assert.Equal(moodBeforeSleep, tracker.CurrentMood, 3);
    }

    [Fact]
    public void SocialRelationshipPool_ConfiguresAndTracksPairwiseOpinions()
    {
        var trackerA = new EntityPsychologyTracker(entityId: 1);
        trackerA.Devotion = 1.0f;
        trackerA.Tribalism = 1.0f;
        trackerA.Neuroticism = 0.0f;

        // Ally relationship: same faith (+1.0), allied clan (+1.0), sports ally (+0.2)
        trackerA.RelationshipPool.ConfigureRelationship(
            targetEntityId: 2,
            religiousAlignment: 1.0f,
            clanFeudScore: 1.0f,
            sportsRivalryScore: 0.20f);

        // Add 5 chitchat interactions (5 * 0.66 = +3.3 opinion)
        for (int i = 0; i < 5; i++)
        {
            trackerA.RelationshipPool.AddChitchat(2);
        }

        float allyOpinion = trackerA.EvaluateOpinionTowards(2, 0);
        // 30 (religion) + 35 (feud) + 15 (sports) + 3.3 (chitchat) = 83.3
        Assert.InRange(allyOpinion, 80.0f, 85.0f);

        // Enemy relationship: heretic (-1.0), blood feud (-1.0), sports rival (-0.2)
        trackerA.RelationshipPool.ConfigureRelationship(
            targetEntityId: 3,
            religiousAlignment: -1.0f,
            clanFeudScore: -1.0f,
            sportsRivalryScore: -0.20f);

        float enemyOpinion = trackerA.EvaluateOpinionTowards(3, 0);
        // -30 (religion) - 35 (feud) - 15 (sports) = -80.0
        Assert.InRange(enemyOpinion, -85.0f, -75.0f);
    }
}
