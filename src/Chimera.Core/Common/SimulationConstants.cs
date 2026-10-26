namespace Chimera.Core.Common;

/// <summary>
/// Fundamental physical and simulation constants enforcing cross-track determinism.
/// </summary>
public static class SimulationConstants
{
    /// <summary>
    /// Fixed micro-tick rate in Hz (60 ticks per second).
    /// </summary>
    public const ulong MicroTicksPerSecond = 60UL;

    /// <summary>
    /// Fixed discrete micro-tick delta time in seconds (1/60s).
    /// </summary>
    public const float FixedMicroDeltaTime = 1.0f / 60.0f;

    /// <summary>
    /// Fixed number of micro-ticks per simulation-tick (1 Hz need/metabolism updates).
    /// </summary>
    public const ulong MicroTicksPerSimulationTick = 60UL;

    /// <summary>
    /// Fixed number of simulation-ticks per macro-tick (3600s = 1 in-game hour).
    /// </summary>
    public const ulong SimulationTicksPerMacroTick = 3600UL;

    /// <summary>
    /// Voxel dimension in meters (1 voxel = 1.0m cube).
    /// </summary>
    public const float VoxelSizeMeters = 1.0f;

    /// <summary>
    /// Absolute zero in Celsius.
    /// </summary>
    public const float AbsoluteZeroCelsius = -273.15f;

    /// <summary>
    /// Convert Celsius to Kelvin.
    /// </summary>
    public static float CelsiusToKelvin(float celsius) => celsius - AbsoluteZeroCelsius;

    /// <summary>
    /// Convert Kelvin to Celsius.
    /// </summary>
    public static float KelvinToCelsius(float kelvin) => kelvin + AbsoluteZeroCelsius;
}
