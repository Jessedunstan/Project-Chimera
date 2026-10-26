namespace Chimera.Core.Contracts;

/// <summary>
/// Core simulation tick contract enforcing deterministic, discrete multi-rate execution.
/// Variable floating-point delta times are strictly excluded from the simulation backend to guarantee
/// cross-platform bit-identical reproducibility.
/// </summary>
public interface ISimulationTickable
{
    /// <summary>
    /// Discrete micro-tick for high-frequency physics, particle kinematics, and entity integration (fixed 60 Hz).
    /// </summary>
    /// <param name="microTickIndex">Monotonically increasing 60 Hz discrete tick index.</param>
    void StepMicroTick(ulong microTickIndex);

    /// <summary>
    /// Discrete simulation-tick for mid-frequency needs, metabolism, and local environment updates (fixed 1 Hz = 60 micro-ticks).
    /// </summary>
    /// <param name="simulationTickIndex">Monotonically increasing 1 Hz discrete tick index.</param>
    void StepSimulationTick(ulong simulationTickIndex);

    /// <summary>
    /// Discrete macro-tick for low-frequency off-screen ecosystems, history, and ledgers (e.g., 3600 simulation-ticks = 1 in-game hour).
    /// </summary>
    /// <param name="macroTickIndex">Monotonically increasing discrete macro cycle index.</param>
    void StepMacroTick(ulong macroTickIndex);
}
