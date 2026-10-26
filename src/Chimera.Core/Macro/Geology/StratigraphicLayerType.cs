namespace Chimera.Core.Macro.Geology;

/// <summary>
/// 4-tier stratigraphic hierarchy based on Dwarf Fortress geological layering.
/// </summary>
public enum StratigraphicLayerType : byte
{
    Air = 0,
    Soil = 1,
    Sedimentary = 2,
    Metamorphic = 3,
    IgneousExtrusive = 4,
    IgneousIntrusive = 5,
    PlutonicCore = 6
}
