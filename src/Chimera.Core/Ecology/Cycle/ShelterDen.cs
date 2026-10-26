using System;
using System.Collections.Generic;
using System.Numerics;

namespace Chimera.Core.Ecology.Cycle;

/// <summary>
/// Represents a shelter or den chamber where creatures seek refuge from environmental hazards.
/// Provides physical shielding against lethal rain, floods, and cold pressure.
/// </summary>
public sealed class ShelterDen
{
    public string DenId { get; }
    public string RoomId { get; }
    public Vector3 Position { get; }
    public int MaxCapacity { get; }
    public bool IsSealed { get; private set; }
    public int FoodPipsStored { get; set; }

    private readonly List<string> _occupants = new();
    public IReadOnlyList<string> Occupants => _occupants;

    public bool IsFull => _occupants.Count >= MaxCapacity;

    public ShelterDen(string denId, string roomId, Vector3 position, int maxCapacity = 4)
    {
        DenId = denId;
        RoomId = roomId;
        Position = position;
        MaxCapacity = Math.Max(1, maxCapacity);
        IsSealed = false;
        FoodPipsStored = 0;
    }

    public bool TryEnter(string creatureId)
    {
        if (IsSealed || IsFull) return false;
        if (!_occupants.Contains(creatureId))
        {
            _occupants.Add(creatureId);
        }
        return true;
    }

    public void Exit(string creatureId)
    {
        if (!IsSealed)
        {
            _occupants.Remove(creatureId);
        }
    }

    public void Lockdown()
    {
        IsSealed = true;
    }

    public void Open()
    {
        IsSealed = false;
    }

    public void ClearOccupants()
    {
        _occupants.Clear();
    }
}
