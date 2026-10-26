namespace Chimera.Core.Presentation;

/// <summary>
/// Contract for state snapshots published from the pure C# simulation to the presentation layer.
/// Snapshots must be immutable or strictly isolated from ongoing simulation worker writes.
/// </summary>
public interface IRenderSnapshot
{
    /// <summary>
    /// Discrete micro-tick index at the moment this snapshot was produced.
    /// </summary>
    ulong MicroTickIndex { get; }

    /// <summary>
    /// Discrete simulation-tick index at the moment this snapshot was produced.
    /// </summary>
    ulong SimulationTickIndex { get; }
}
