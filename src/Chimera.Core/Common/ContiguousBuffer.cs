namespace Chimera.Core.Common;

using System;

/// <summary>
/// Contiguous unmanaged memory buffer for hot simulation loops.
/// Adheres to Data-Oriented Design (DoD) invariants:
/// - Flat memory layout without pointer indirection or object headers.
/// - Direct <see cref="Span{T}"/> and <see cref="ReadOnlySpan{T}"/> slicing.
/// - Zero garbage collection allocations during inner iteration.
/// </summary>
/// <typeparam name="T">Unmanaged data structure.</typeparam>
public sealed class ContiguousBuffer<T> where T : unmanaged
{
    private readonly T[] _items;

    public ContiguousBuffer(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
        }

        _items = GC.AllocateArray<T>(capacity, pinned: true);
        Capacity = capacity;
        Count = 0;
    }

    public int Capacity { get; }

    public int Count { get; set; }

    public ref T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return ref _items[index];
        }
    }

    /// <summary>
    /// Gets a mutable Span view of the active elements for high-performance vectorized or linear processing.
    /// </summary>
    public Span<T> AsSpan() => new(_items, 0, Count);

    /// <summary>
    /// Gets a read-only Span view of the active elements.
    /// </summary>
    public ReadOnlySpan<T> AsReadOnlySpan() => new(_items, 0, Count);

    /// <summary>
    /// Adds an element by value and returns a reference to its location.
    /// </summary>
    public ref T Add(in T item)
    {
        if (Count >= Capacity)
        {
            throw new InvalidOperationException($"Buffer capacity of {Capacity} exceeded.");
        }

        int index = Count++;
        _items[index] = item;
        return ref _items[index];
    }

    /// <summary>
    /// Resets the active count without reallocating underlying memory.
    /// </summary>
    public void Clear()
    {
        Count = 0;
    }
}
