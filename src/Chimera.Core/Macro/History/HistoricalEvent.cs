using System;
using System.Runtime.InteropServices;

namespace Chimera.Core.Macro.History;

/// <summary>
/// Unmanaged historical chronicle event struct for high-frequency world simulation.
/// Guarantees zero garbage collection allocations.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly struct HistoricalEvent : IEquatable<HistoricalEvent>
{
    public long EventId { get; }
    public long WorldTick { get; }
    public HistoricalEventType EventType { get; }
    private readonly ushort _reserved;

    public int PrimaryEntityId { get; }
    public int SecondaryEntityId { get; }

    public int LocationX { get; }
    public int LocationY { get; }
    public int LocationZ { get; }

    public uint MetadataFlags { get; }
    public FixedString64 Summary { get; }

    public HistoricalEvent(
        long eventId,
        long worldTick,
        HistoricalEventType eventType,
        int primaryEntityId,
        int secondaryEntityId,
        int locationX,
        int locationY,
        int locationZ,
        FixedString64 summary,
        uint metadataFlags = 0)
    {
        EventId = eventId;
        WorldTick = worldTick;
        EventType = eventType;
        _reserved = 0;
        PrimaryEntityId = primaryEntityId;
        SecondaryEntityId = secondaryEntityId;
        LocationX = locationX;
        LocationY = locationY;
        LocationZ = locationZ;
        MetadataFlags = metadataFlags;
        Summary = summary;
    }

    public bool Equals(HistoricalEvent other)
    {
        return EventId == other.EventId &&
               WorldTick == other.WorldTick &&
               EventType == other.EventType &&
               PrimaryEntityId == other.PrimaryEntityId &&
               SecondaryEntityId == other.SecondaryEntityId &&
               LocationX == other.LocationX &&
               LocationY == other.LocationY &&
               LocationZ == other.LocationZ &&
               MetadataFlags == other.MetadataFlags &&
               Summary.Equals(other.Summary);
    }

    public override bool Equals(object? obj) => obj is HistoricalEvent other && Equals(other);

    public override int GetHashCode()
    {
        return HashCode.Combine(EventId, WorldTick, (int)EventType, PrimaryEntityId);
    }

    public static bool operator ==(HistoricalEvent left, HistoricalEvent right) => left.Equals(right);
    public static bool operator !=(HistoricalEvent left, HistoricalEvent right) => !left.Equals(right);
}
