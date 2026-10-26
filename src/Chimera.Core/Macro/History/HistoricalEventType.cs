namespace Chimera.Core.Macro.History;

/// <summary>
/// Strongly-typed 16-bit historical event category for Dwarf Fortress world chronicle records.
/// </summary>
public enum HistoricalEventType : ushort
{
    None = 0,

    // --- Warfare & Conflict (1 - 49) ---
    WarDeclared = 1,
    FieldBattle = 2,
    SiteConquest = 3,
    SiteRallied = 4,
    SiteDestroyed = 5,
    PeaceTreatySigned = 6,
    DiplomaticTruce = 7,

    // --- Dynasties & Political Rulers (50 - 99) ---
    Coronation = 50,
    RulerAbdication = 51,
    RulerAssassinated = 52,
    DynastyExtinction = 53,
    SuccessionDispute = 54,

    // --- Settlements & Sites (100 - 149) ---
    SiteFounded = 100,
    SiteAbandoned = 101,
    TradeAccordEstablished = 102,
    GuildFormed = 103,

    // --- Artifacts & Masterpieces (150 - 199) ---
    ArtifactCreated = 150,
    ArtifactStolen = 151,
    ArtifactLost = 152,
    ArtifactRecovered = 153,

    // --- Megabeasts, Titans & Disasters (200 - 249) ---
    CreatureSighting = 200,
    TitanAwakened = 201,
    MegabeastSlain = 202,
    GreatFamine = 203,
    PlagueOutbreak = 204,
    VolcanicEruption = 205,

    // --- Excavation & Discoveries (250 - 299) ---
    VeinDiscovered = 250,
    AdamantineSpireBreached = 251,
    AncientVaultUnsealed = 252
}
