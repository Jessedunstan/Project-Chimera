namespace Chimera.Core.Macro.Materials;

/// <summary>
/// Broad material taxonomic category based on Dwarf Fortress geological stratigraphy and classification.
/// </summary>
public enum MaterialCategory : byte
{
    Unknown = 0,
    Soil = 1,
    SedimentaryRock = 2,
    MetamorphicRock = 3,
    IgneousExtrusiveRock = 4,
    IgneousIntrusiveRock = 5,
    Metal = 6,
    Ore = 7,
    Gem = 8,
    Organic = 9,
    Liquid = 10,
    Gas = 11
}
