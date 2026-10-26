using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Chimera.Core.Contracts;

namespace Chimera.Core.Macro.History;

/// <summary>
/// High-performance, unmanaged world history ledger.
/// Backed by contiguous native memory (NativeMemory.AllocZeroed) to record hundreds of thousands of events
/// without triggering garbage collection pauses.
/// Thread-safe append via atomic lock-free increments.
/// </summary>
public unsafe sealed class HistoricalLedger : IHistoricalEventPublisher, ISimulationTickable, IDisposable
{
    private readonly int _capacity;
    private HistoricalEvent* _buffer;
    private int _eventCount;
    private long _nextEventId;
    private bool _isDisposed;

    public int Count => Math.Min(_eventCount, _capacity);
    public int Capacity => _capacity;
    public bool IsFull => _eventCount >= _capacity;

    public ulong CurrentMicroTick { get; private set; }
    public ulong CurrentSimulationTick { get; private set; }
    public ulong CurrentMacroTick { get; private set; }

    public HistoricalLedger(int capacity = 65536)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        _capacity = capacity;
        nuint byteSize = (nuint)(sizeof(HistoricalEvent) * capacity);
        _buffer = (HistoricalEvent*)NativeMemory.AllocZeroed(byteSize);
        _eventCount = 0;
        _nextEventId = 1;
    }

    ~HistoricalLedger()
    {
        Dispose(false);
    }

    /// <summary>
    /// Thread-safe record of a strongly-typed historical chronicle event with zero heap allocations.
    /// </summary>
    public long RecordEvent(
        HistoricalEventType eventType,
        long worldTick,
        int primaryEntityId,
        int secondaryEntityId,
        int x,
        int y,
        int z,
        ReadOnlySpan<char> summary,
        uint metadataFlags = 0)
    {
        int index = Interlocked.Increment(ref _eventCount) - 1;
        long eventId = Interlocked.Increment(ref _nextEventId) - 1;

        if (index < _capacity && _buffer != null)
        {
            _buffer[index] = new HistoricalEvent(
                eventId,
                worldTick,
                eventType,
                primaryEntityId,
                secondaryEntityId,
                x,
                y,
                z,
                new FixedString64(summary),
                metadataFlags
            );
        }

        return eventId;
    }

    /// <summary>
    /// Thread-safe record of a pre-constructed HistoricalEvent struct. Zero heap allocations.
    /// </summary>
    public long RecordEvent(in HistoricalEvent evt)
    {
        int index = Interlocked.Increment(ref _eventCount) - 1;
        long eventId = evt.EventId > 0 ? evt.EventId : Interlocked.Increment(ref _nextEventId) - 1;

        if (index < _capacity && _buffer != null)
        {
            _buffer[index] = new HistoricalEvent(
                eventId,
                evt.WorldTick,
                evt.EventType,
                evt.PrimaryEntityId,
                evt.SecondaryEntityId,
                evt.LocationX,
                evt.LocationY,
                evt.LocationZ,
                evt.Summary,
                evt.MetadataFlags
            );
        }

        return eventId;
    }

    /// <summary>
    /// Implements IHistoricalEventPublisher contract. Zero heap allocations during string-to-span copy.
    /// </summary>
    public void RecordEvent(string eventType, string summary, long worldTimestamp)
    {
        HistoricalEventType type = HistoricalEventType.None;
        if (Enum.TryParse<HistoricalEventType>(eventType, true, out var parsedType))
        {
            type = parsedType;
        }

        RecordEvent(
            type,
            worldTimestamp,
            primaryEntityId: 0,
            secondaryEntityId: 0,
            x: 0,
            y: 0,
            z: 0,
            summary.AsSpan(),
            metadataFlags: 0
        );
    }

    /// <summary>
    /// Reads a historical event by index in O(1) with zero allocation.
    /// </summary>
    public ref readonly HistoricalEvent GetEvent(int index)
    {
        if ((uint)index >= (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
        return ref _buffer[index];
    }

    /// <summary>
    /// Direct read-only span view over all currently recorded unmanaged events.
    /// Zero heap allocations.
    /// </summary>
    public ReadOnlySpan<HistoricalEvent> AsSpan()
    {
        if (_buffer == null || _eventCount == 0) return ReadOnlySpan<HistoricalEvent>.Empty;
        return new ReadOnlySpan<HistoricalEvent>(_buffer, Count);
    }

    /// <summary>
    /// Copies events within [startTick, endTick] into the caller-provided span. Zero heap allocations.
    /// Returns the number of events written.
    /// </summary>
    public int CopyEvents(long startTick, long endTick, Span<HistoricalEvent> destination)
    {
        int written = 0;
        int total = Count;
        for (int i = 0; i < total && written < destination.Length; i++)
        {
            if (_buffer[i].WorldTick >= startTick && _buffer[i].WorldTick <= endTick)
            {
                destination[written++] = _buffer[i];
            }
        }
        return written;
    }

    /// <summary>
    /// Copies events involving a given entity ID into the caller-provided span. Zero heap allocations.
    /// </summary>
    public int CopyEventsByEntity(int entityId, Span<HistoricalEvent> destination)
    {
        int written = 0;
        int total = Count;
        for (int i = 0; i < total && written < destination.Length; i++)
        {
            if (_buffer[i].PrimaryEntityId == entityId || _buffer[i].SecondaryEntityId == entityId)
            {
                destination[written++] = _buffer[i];
            }
        }
        return written;
    }

    /// <summary>
    /// Copies events of a given type into the caller-provided span. Zero heap allocations.
    /// </summary>
    public int CopyEventsByType(HistoricalEventType type, Span<HistoricalEvent> destination)
    {
        int written = 0;
        int total = Count;
        for (int i = 0; i < total && written < destination.Length; i++)
        {
            if (_buffer[i].EventType == type)
            {
                destination[written++] = _buffer[i];
            }
        }
        return written;
    }

    // --- ISimulationTickable ---
    public void StepMicroTick(ulong microTickIndex) => CurrentMicroTick = microTickIndex;
    public void StepSimulationTick(ulong simulationTickIndex) => CurrentSimulationTick = simulationTickIndex;
    public void StepMacroTick(ulong macroTickIndex) => CurrentMacroTick = macroTickIndex;

    private void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (_buffer != null)
            {
                NativeMemory.Free(_buffer);
                _buffer = null;
            }
            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
