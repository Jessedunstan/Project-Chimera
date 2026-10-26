namespace Chimera.Core.Ecology.Cycle;

/// <summary>
/// Operational phases of the environmental rain cycle extracted from Rain World's RainCycle architecture.
/// </summary>
public enum RainHazardPhase : byte
{
    /// <summary>Normal cycle operation; creatures roam, forage, and hunt freely.</summary>
    Clear = 0,

    /// <summary>Pre-cycle warning (T - 120s): Sky darkens, tremor vibrations rise, creatures urge toward dens.</summary>
    PrecycleTremor = 1,

    /// <summary>Drizzle and flood onset (T to T + 30s): Rising floodwaters, shelters initiate lockdown.</summary>
    WaterAccumulation = 2,

    /// <summary>Lethal downpour (T + 30s onward): Catastrophic kinetic crush force and instant drowning.</summary>
    LethalDownpour = 3,

    /// <summary>Cycle end: Shelters hermetically sealed; hibernation and food consumption evaluated.</summary>
    ShelterHibernation = 4
}
