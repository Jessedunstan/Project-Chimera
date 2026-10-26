using System;

namespace Chimera.Core.Ecology.FoodWeb;

/// <summary>
/// Simulates localized prey swarm dynamics and multi-cycle exhaustion based on Rain World's batfly hive logic.
/// Swarms support an active quota that depletes under harvesting pressure and requires multiple idle cycles to recover.
/// </summary>
public sealed class SwarmDepletionTracker
{
    public int MaxSwarmCapacity { get; }
    public int CurrentCount { get; private set; }
    public int DepleteCyclesMin { get; }
    public int DepleteCyclesMax { get; }
    public int DepleteCyclesMaxIfLessThanTwoLeft { get; }

    private int _remainingDepletionCycles;

    public bool IsDepleted => CurrentCount <= 0;

    public SwarmDepletionTracker(
        int maxCapacity = 7,
        int depleteCyclesMin = 2,
        int depleteCyclesMax = 7,
        int depleteCyclesMaxIfLessThanTwoLeft = 3)
    {
        MaxSwarmCapacity = maxCapacity;
        CurrentCount = maxCapacity;
        DepleteCyclesMin = depleteCyclesMin;
        DepleteCyclesMax = depleteCyclesMax;
        DepleteCyclesMaxIfLessThanTwoLeft = depleteCyclesMaxIfLessThanTwoLeft;
        _remainingDepletionCycles = 0;
    }

    /// <summary>
    /// Harvests prey individuals from the swarm during an active cycle.
    /// </summary>
    public int Harvest(int count, Random rng)
    {
        if (count <= 0 || CurrentCount <= 0) return 0;

        int taken = Math.Min(CurrentCount, count);
        CurrentCount -= taken;

        if (CurrentCount < 2)
        {
            _remainingDepletionCycles = Math.Max(_remainingDepletionCycles,
                rng.Next(DepleteCyclesMin, DepleteCyclesMaxIfLessThanTwoLeft + 1));
        }
        else if (taken > 0)
        {
            _remainingDepletionCycles = Math.Max(_remainingDepletionCycles,
                rng.Next(DepleteCyclesMin, DepleteCyclesMax + 1));
        }

        return taken;
    }

    /// <summary>
    /// Executes end-of-cycle replenishment. If currently depleted or cooling down, ticks down recovery cycles.
    /// </summary>
    public void StepCycleRecovery()
    {
        if (_remainingDepletionCycles > 0)
        {
            _remainingDepletionCycles--;
            if (_remainingDepletionCycles == 0)
            {
                // Restocked upon recovery cycle expiration
                CurrentCount = MaxSwarmCapacity;
            }
        }
        else if (CurrentCount < MaxSwarmCapacity)
        {
            // Gradual incremental replenishment when not heavily depleted
            CurrentCount = Math.Min(MaxSwarmCapacity, CurrentCount + 2);
        }
    }
}
