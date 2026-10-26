using System;
using System.Numerics;
using Chimera.Core.Ecology.Behavior;

namespace Chimera.Core.Ecology.Memory;

public enum DisputeOutcome : byte
{
    MutualTolerance = 0,
    IntruderYields = 1,
    ResidentYields = 2,
    TerritorialClash = 3
}

/// <summary>
/// Mathematical resolver for territorial intrusions and disputes.
/// Evaluates resident home-field defensive investment against intruder dominance and mass.
/// </summary>
public static class TerritoryDisputeResolver
{
    public readonly record struct DisputeInput(
        float ResidentMassKg,
        float IntruderMassKg,
        CreaturePersonality ResidentPersonality,
        CreaturePersonality IntruderPersonality,
        float ResidentFamiliarity,
        float ResidentHealthRatio,
        float IntruderHealthRatio,
        float IntruderFearOfResident);

    public readonly record struct DisputeResult(
        DisputeOutcome Outcome,
        float ResidentDefenseScore,
        float IntruderChallengeScore,
        float ConflictIntensity);

    public static DisputeResult ResolveDispute(
        TerritoryZone territory,
        Vector3 intruderPosition,
        in DisputeInput input)
    {
        float intrusionDepth = territory.EvaluateIntrusionFactor(intruderPosition);
        if (intrusionDepth <= 0.001f)
        {
            return new DisputeResult(DisputeOutcome.MutualTolerance, 0.0f, 0.0f, 0.0f);
        }

        float resMass = MathF.Max(0.01f, input.ResidentMassKg);
        float intMass = MathF.Max(0.01f, input.IntruderMassKg);

        // Resident home advantage scales with familiarity, scent mark, and proximity to core den
        float homeFieldAdvantage = intrusionDepth * input.ResidentFamiliarity * territory.ScentIntensity;

        float residentDefense = (input.ResidentPersonality.Dominance * 1.5f + homeFieldAdvantage)
            * (resMass / intMass)
            * input.ResidentHealthRatio
            * input.ResidentPersonality.Bravery;

        float intruderChallenge = input.IntruderPersonality.Dominance
            * (intMass / resMass)
            * input.IntruderHealthRatio
            * (1.0f - Math.Clamp(input.IntruderFearOfResident, 0.0f, 0.95f))
            * input.IntruderPersonality.Aggression;

        float delta = residentDefense - intruderChallenge;
        float conflictIntensity = (residentDefense + intruderChallenge) * 0.5f * intrusionDepth;

        DisputeOutcome outcome;
        if (input.ResidentPersonality.Sympathy > 0.7f && input.IntruderPersonality.Sympathy > 0.7f && intrusionDepth < 0.5f)
        {
            outcome = DisputeOutcome.MutualTolerance;
        }
        else if (delta > 0.4f)
        {
            // Resident significantly stronger: intruder yields and flees
            outcome = DisputeOutcome.IntruderYields;
        }
        else if (delta < -0.4f)
        {
            // Intruder significantly stronger: resident displaced
            outcome = DisputeOutcome.ResidentYields;
        }
        else
        {
            // Close contest near den core: physical territorial clash
            outcome = DisputeOutcome.TerritorialClash;
        }

        return new DisputeResult(outcome, residentDefense, intruderChallenge, conflictIntensity);
    }
}
