using System;
using Chimera.Core.MathEngine.Materials;

namespace Chimera.Core.Macro.Materials;

/// <summary>
/// Unmanaged struct representing raw Dwarf Fortress physical material constants and mechanical tensors.
/// Zero GC allocations. Provides exact conversions to SI units and projections to PhysicalMaterialParameters.
/// </summary>
public readonly struct RawMaterialProperties : IEquatable<RawMaterialProperties>
{
    public MaterialId Id { get; }
    public MaterialCategory Category { get; }

    /// <summary>
    /// Physical solid density in kg / m^3 (e.g. Iron: 7850, Granite: 2600, Gold: 19320, Adamantine: 200).
    /// </summary>
    public float SolidDensity { get; }

    /// <summary>
    /// Specific heat capacity in J / (kg * K) (e.g. Iron: 450, Rock: 800, Water: 4184).
    /// </summary>
    public float SpecificHeat { get; }

    /// <summary>
    /// Melting point in Kelvin (phase change Solid -> Liquid).
    /// </summary>
    public float MeltingPointK { get; }

    /// <summary>
    /// Boiling point in Kelvin (phase change Liquid -> Gas).
    /// </summary>
    public float BoilingPointK { get; }

    // --- Mechanical Strength Tensors (kPa = 10^3 N/m^2) ---
    public float ImpactYield { get; }
    public float ImpactFracture { get; }
    public float ImpactStrainAtYield { get; }

    public float CompressiveYield { get; }
    public float CompressiveFracture { get; }
    public float CompressiveStrainAtYield { get; }

    public float TensileYield { get; }
    public float TensileFracture { get; }
    public float TensileStrainAtYield { get; }

    public float ShearYield { get; }
    public float ShearFracture { get; }
    public float ShearStrainAtYield { get; }

    /// <summary>
    /// Sharpness multiplier for cutting edges (e.g. Iron: 10,000, Obsidian: 100,000, Diamond: 100,000).
    /// </summary>
    public float MaxEdge { get; }

    public RawMaterialProperties(
        MaterialId id,
        MaterialCategory category,
        float solidDensity,
        float specificHeat,
        float meltingPointK,
        float boilingPointK,
        float impactYield,
        float impactFracture,
        float impactStrainAtYield,
        float compressiveYield,
        float compressiveFracture,
        float compressiveStrainAtYield,
        float tensileYield,
        float tensileFracture,
        float tensileStrainAtYield,
        float shearYield,
        float shearFracture,
        float shearStrainAtYield,
        float maxEdge = 10000.0f)
    {
        Id = id;
        Category = category;
        SolidDensity = MathF.Max(0.01f, solidDensity);
        SpecificHeat = MathF.Max(1.0f, specificHeat);
        MeltingPointK = MathF.Max(0.0f, meltingPointK);
        BoilingPointK = MathF.Max(MeltingPointK, boilingPointK);

        ImpactYield = MathF.Max(1.0f, impactYield);
        ImpactFracture = MathF.Max(ImpactYield, impactFracture);
        ImpactStrainAtYield = MathF.Max(1.0f, impactStrainAtYield);

        CompressiveYield = MathF.Max(1.0f, compressiveYield);
        CompressiveFracture = MathF.Max(CompressiveYield, compressiveFracture);
        CompressiveStrainAtYield = MathF.Max(1.0f, compressiveStrainAtYield);

        TensileYield = MathF.Max(1.0f, tensileYield);
        TensileFracture = MathF.Max(TensileYield, tensileFracture);
        TensileStrainAtYield = MathF.Max(1.0f, tensileStrainAtYield);

        ShearYield = MathF.Max(1.0f, shearYield);
        ShearFracture = MathF.Max(ShearYield, shearFracture);
        ShearStrainAtYield = MathF.Max(1.0f, shearStrainAtYield);

        MaxEdge = MathF.Max(1.0f, maxEdge);
    }

    /// <summary>
    /// Computes Young's Modulus in Pascals using Dwarf Fortress standard formula:
    /// E = (Yield / StrainAtYield) * 10^8 Pa
    /// </summary>
    public static float CalculateYoungsModulusPa(float yieldKpa, float strainAtYield)
    {
        if (strainAtYield <= 1e-4f) return 0.0f;
        return (yieldKpa / strainAtYield) * 1e8f;
    }

    /// <summary>
    /// Tensile Young's Modulus in Pascals (Pa).
    /// </summary>
    public float TensileYoungsModulusPa => CalculateYoungsModulusPa(TensileYield, TensileStrainAtYield);

    /// <summary>
    /// Compressive Young's Modulus in Pascals (Pa).
    /// </summary>
    public float CompressiveYoungsModulusPa => CalculateYoungsModulusPa(CompressiveYield, CompressiveStrainAtYield);

    /// <summary>
    /// Converts Dwarf Fortress Degrees Urist to Kelvin:
    /// T_K = (T_U - 10000) / 1.8 + 273.15
    /// </summary>
    public static float UristToKelvin(float degreesUrist)
    {
        return ((degreesUrist - 10000.0f) / 1.8f) + 273.15f;
    }

    /// <summary>
    /// Converts Kelvin to Dwarf Fortress Degrees Urist:
    /// T_U = 10000 + (T_K - 273.15) * 1.8
    /// </summary>
    public static float KelvinToUrist(float kelvin)
    {
        return 10000.0f + ((kelvin - 273.15f) * 1.8f);
    }

    /// <summary>
    /// Projects raw DF material physical constants into normalized/continuous PhysicalMaterialParameters
    /// for integration with the slider-math-engine. Zero allocations.
    /// </summary>
    public PhysicalMaterialParameters ToPhysicalParameters()
    {
        // Tensile Yield in MPa (1 MPa = 1000 kPa)
        float tensileYieldMpa = TensileYield / 1000.0f;

        // Rigidity scaled to [0..1] based on Young's Modulus (e.g. Diamond ~1000 GPa = 1.0, Steel/Iron ~210 GPa = 0.6)
        float youngsGpa = MathF.Max(TensileYoungsModulusPa, CompressiveYoungsModulusPa) / 1e9f;
        float rigidity = Math.Clamp(youngsGpa / 350.0f, 0.05f, 1.0f);

        // Brittleness is the discrepancy between compressive and tensile yield,
        // and low strain at yield (e.g. stone shatters under tension, metals stretch).
        float yieldRatio = CompressiveYield / MathF.Max(1.0f, TensileYield);
        float brittleness = Category switch
        {
            MaterialCategory.Soil => 0.05f,
            MaterialCategory.SedimentaryRock => Math.Clamp(0.70f + (yieldRatio * 0.01f), 0.70f, 0.98f),
            MaterialCategory.MetamorphicRock => Math.Clamp(0.65f + (yieldRatio * 0.01f), 0.65f, 0.95f),
            MaterialCategory.IgneousExtrusiveRock => 0.85f,
            MaterialCategory.IgneousIntrusiveRock => 0.80f,
            MaterialCategory.Gem => 0.95f,
            MaterialCategory.Metal => Math.Clamp(0.10f + (TensileYield / 2_000_000.0f) * 0.2f, 0.05f, 0.40f),
            MaterialCategory.Organic => 0.30f,
            MaterialCategory.Liquid => 0.0f,
            _ => 0.5f
        };

        // Malleability is high in ductile metals (gold, copper, lead) and low in ceramics/rocks
        float malleability = Category switch
        {
            MaterialCategory.Metal => Math.Clamp(1.0f - (TensileYield / 1_500_000.0f), 0.3f, 0.95f),
            MaterialCategory.Soil => 0.60f,
            MaterialCategory.Organic => 0.50f,
            _ => 0.05f
        };

        // Thermal conductivity normalized [0..1] where 1.0 is Copper (~400 W/m*K)
        float thermalConductivity = Category switch
        {
            MaterialCategory.Metal => Id switch
            {
                MaterialId.MetalCopper => 1.0f,
                MaterialId.MetalGold => 0.78f,
                MaterialId.MetalSilver => 0.95f,
                MaterialId.MetalBronze => 0.28f,
                MaterialId.MetalIron => 0.20f,
                MaterialId.MetalSteel => 0.12f,
                MaterialId.MetalPlatinum => 0.18f,
                MaterialId.MetalAdamantine => 1.0f,
                _ => 0.20f
            },
            MaterialCategory.Gem => Id == MaterialId.GemDiamond ? 1.0f : 0.08f,
            MaterialCategory.SedimentaryRock => 0.015f,
            MaterialCategory.MetamorphicRock => 0.025f,
            MaterialCategory.IgneousExtrusiveRock => 0.018f,
            MaterialCategory.IgneousIntrusiveRock => 0.035f,
            MaterialCategory.Liquid => Id == MaterialId.LiquidWater ? 0.005f : 0.05f,
            MaterialCategory.Soil => 0.008f,
            _ => 0.01f
        };

        float friction = Category switch
        {
            MaterialCategory.Liquid => 0.05f,
            MaterialCategory.Metal => 0.45f,
            MaterialCategory.Gem => 0.25f,
            MaterialCategory.Soil => 0.70f,
            _ => 0.65f
        };

        return new PhysicalMaterialParameters(
            SolidDensity,
            rigidity,
            brittleness,
            malleability,
            thermalConductivity,
            SpecificHeat,
            MeltingPointK,
            BoilingPointK,
            friction,
            tensileYieldMpa
        );
    }

    public bool Equals(RawMaterialProperties other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is RawMaterialProperties other && Equals(other);
    public override int GetHashCode() => (int)Id;
    public static bool operator ==(RawMaterialProperties left, RawMaterialProperties right) => left.Equals(right);
    public static bool operator !=(RawMaterialProperties left, RawMaterialProperties right) => !left.Equals(right);
}
