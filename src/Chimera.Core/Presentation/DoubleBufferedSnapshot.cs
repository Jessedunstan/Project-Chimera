namespace Chimera.Core.Presentation;

using System;
using System.Threading;

/// <summary>
/// Thread-safe, lock-free double-buffered snapshot exchange pattern using <see cref="Interlocked.Exchange{T}(ref T, T)"/>.
/// Decouples the multi-threaded simulation worker thread from the Godot presentation/render thread:
/// - Guarantees the presentation layer never reads torn/partial frames.
/// - Guarantees the presentation layer never blocks or pauses simulation stepping.
/// - Produces zero memory allocation during steady-state publishing when paired with recycled/reusable buffers.
/// </summary>
/// <typeparam name="TSnapshot">The immutable or snapshot state payload reference type.</typeparam>
public sealed class DoubleBufferedSnapshot<TSnapshot> where TSnapshot : class
{
    private TSnapshot _publishedSnapshot;

    /// <summary>
    /// Initializes the snapshot container with an initial baseline snapshot.
    /// </summary>
    /// <param name="initialSnapshot">Non-null baseline snapshot.</param>
    public DoubleBufferedSnapshot(TSnapshot initialSnapshot)
    {
        _publishedSnapshot = initialSnapshot ?? throw new ArgumentNullException(nameof(initialSnapshot));
    }

    /// <summary>
    /// Acquired by the presentation thread (e.g., Godot `_Process` / render step).
    /// Atomic, lock-free, zero allocation.
    /// </summary>
    public TSnapshot Current => Volatile.Read(ref _publishedSnapshot);

    /// <summary>
    /// Invoked by the simulation worker thread upon completing a simulation tick or frame packet.
    /// Atomically publishes the new snapshot reference.
    /// </summary>
    /// <param name="newSnapshot">The newly completed immutable snapshot to publish.</param>
    /// <returns>The previous snapshot reference, which can optionally be recycled to avoid heap churn.</returns>
    public TSnapshot Publish(TSnapshot newSnapshot)
    {
        ArgumentNullException.ThrowIfNull(newSnapshot);
        return Interlocked.Exchange(ref _publishedSnapshot, newSnapshot);
    }
}
