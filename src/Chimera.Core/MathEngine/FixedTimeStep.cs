namespace Chimera.Core.MathEngine;

/// <summary>
/// Fixed discrete time step constants ensuring 100% determinism and bit-identical cross-platform physics.
/// Variable frame deltas must never leak into the simulation layers.
/// </summary>
public static class FixedTimeStep
{
    /// <summary>
    /// Micro-tick delta time in seconds (60 Hz = 0.016666668 seconds).
    /// Used for physics, kinetic integration, particle kinematics.
    /// </summary>
    public const float MicroTickDeltaTime = 1.0f / 60.0f;

    /// <summary>
    /// 30 Hz micro-tick option if running on low-spec hardware.
    /// </summary>
    public const float LowFrequencyMicroTickDeltaTime = 1.0f / 30.0f;

    /// <summary>
    /// Number of micro-ticks per simulation tick (60 micro-ticks = 1.0 second).
    /// </summary>
    public const int MicroTicksPerSimulationTick = 60;

    /// <summary>
    /// Simulation tick delta time in seconds (1 Hz).
    /// Used for hunger, metabolism, thermal diffusion, weather decay.
    /// </summary>
    public const float SimulationTickDeltaTime = 1.0f;

    /// <summary>
    /// Number of simulation ticks per macro cycle (e.g., 3600 seconds = 1 in-game hour).
    /// </summary>
    public const int SimulationTicksPerMacroCycle = 3600;
}
