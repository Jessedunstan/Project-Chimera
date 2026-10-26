using Chimera.Core.Macro.Materials;

namespace Chimera.Core.Macro.Geology;

/// <summary>
/// Unmanaged struct representing a sampled geological voxel in the 4-tier stratigraphic crust.
/// Zero GC allocations.
/// </summary>
public readonly struct StratumSample
{
    public StratigraphicLayerType LayerType { get; }
    public MaterialId PrimaryMaterial { get; }
    public MaterialId VeinMaterial { get; }
    public float OreGrade { get; }
    public bool IsVein { get; }
    public float Density { get; }
    public float CompressiveYieldKpa { get; }
    public float TensileYieldKpa { get; }

    public MaterialId EffectiveMaterial => IsVein ? VeinMaterial : PrimaryMaterial;

    public StratumSample(
        StratigraphicLayerType layerType,
        MaterialId primaryMaterial,
        MaterialId veinMaterial,
        float oreGrade,
        bool isVein,
        float density,
        float compressiveYieldKpa,
        float tensileYieldKpa)
    {
        LayerType = layerType;
        PrimaryMaterial = primaryMaterial;
        VeinMaterial = veinMaterial;
        OreGrade = oreGrade;
        IsVein = isVein;
        Density = density;
        CompressiveYieldKpa = compressiveYieldKpa;
        TensileYieldKpa = tensileYieldKpa;
    }
}
