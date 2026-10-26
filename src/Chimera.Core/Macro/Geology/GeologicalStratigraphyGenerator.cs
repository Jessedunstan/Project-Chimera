using System;
using Chimera.Core.Macro.Materials;

namespace Chimera.Core.Macro.Geology;

/// <summary>
/// Deterministic geological stratigraphy and seeded mineral vein generator.
/// Enforces the Dwarf Fortress 4-tier geological hierarchy:
/// Surface Soil (0-5m) -> Sedimentary -> Metamorphic -> Igneous Intrusive / Extrusive -> Plutonic Core.
/// Zero heap allocations during sampling.
/// </summary>
public sealed class GeologicalStratigraphyGenerator
{
    private readonly ulong _worldSeed;

    public GeologicalStratigraphyGenerator(ulong worldSeed)
    {
        _worldSeed = worldSeed;
    }

    /// <summary>
    /// Computes the surface terrain elevation in voxel units (meters) at the given (worldX, worldZ) coordinates.
    /// </summary>
    public int GetSurfaceElevation(int worldX, int worldZ)
    {
        float continental = DeterministicNoise3D.OctaveNoise2D(_worldSeed + 101UL, worldX * 0.002f, worldZ * 0.002f, 3, 0.5f);
        float mountains = DeterministicNoise3D.OctaveNoise2D(_worldSeed + 202UL, worldX * 0.01f, worldZ * 0.01f, 4, 0.45f);

        // Elevation baseline 64m, ranging between 16m (ocean/basin) to 180m (mountain peaks)
        float elevation = 64.0f + (continental * 40.0f) + (mountains * 40.0f);
        return Math.Clamp((int)elevation, 16, 220);
    }

    /// <summary>
    /// Computes surface soil layer thickness (0 to 5 meters) adhering to DF specification.
    /// </summary>
    public int GetSoilThickness(int worldX, int worldZ, int surfaceElevation)
    {
        // High steep mountain peaks have zero or eroded soil; valleys have deep soil
        if (surfaceElevation > 130)
        {
            return 0;
        }

        float noise = DeterministicNoise3D.Noise2D(_worldSeed + 303UL, worldX * 0.03f, worldZ * 0.03f);
        int thickness = (int)((noise + 1.0f) * 2.5f); // 0 to 5 meters
        return Math.Clamp(thickness, 0, 5);
    }

    /// <summary>
    /// Samples a single voxel at (worldX, worldY, worldZ) with zero heap allocations.
    /// Returns the exact stratum type, primary host rock, mineral vein intrusion (if any), and physical strength.
    /// </summary>
    public StratumSample SampleVoxel(int worldX, int worldY, int worldZ)
    {
        int surfaceY = GetSurfaceElevation(worldX, worldZ);

        // Above surface -> Air
        if (worldY > surfaceY)
        {
            return new StratumSample(
                StratigraphicLayerType.Air,
                MaterialId.None,
                MaterialId.None,
                oreGrade: 0.0f,
                isVein: false,
                density: 1.2f, // Air density
                compressiveYieldKpa: 0.0f,
                tensileYieldKpa: 0.0f
            );
        }

        int depthBelowSurface = surfaceY - worldY;
        int soilThickness = GetSoilThickness(worldX, worldZ, surfaceY);

        // ----------------------------------------------------
        // TIER 1: SURFACE SOIL (0 to 5 meters)
        // ----------------------------------------------------
        if (depthBelowSurface < soilThickness)
        {
            MaterialId soilMat = PickSoilMaterial(worldX, worldZ);
            var soilProps = MaterialRegistry.GetProperties(soilMat);

            return new StratumSample(
                StratigraphicLayerType.Soil,
                soilMat,
                MaterialId.None,
                oreGrade: 0.0f,
                isVein: false,
                density: soilProps.SolidDensity,
                compressiveYieldKpa: soilProps.CompressiveYield,
                tensileYieldKpa: soilProps.TensileYield
            );
        }

        // Deep bedrock / plutonic threshold
        if (worldY <= 5)
        {
            // Check for deep raw adamantine spires
            float adamantineNoise = DeterministicNoise3D.Noise2D(_worldSeed + 999UL, worldX * 0.05f, worldZ * 0.05f);
            if (adamantineNoise > 0.85f && worldY <= 8)
            {
                var adamantineProps = MaterialRegistry.GetProperties(MaterialId.MetalAdamantine);
                return new StratumSample(
                    StratigraphicLayerType.PlutonicCore,
                    MaterialId.MetalAdamantine,
                    MaterialId.MetalAdamantine,
                    oreGrade: 1.0f,
                    isVein: true,
                    density: adamantineProps.SolidDensity,
                    compressiveYieldKpa: adamantineProps.CompressiveYield,
                    tensileYieldKpa: adamantineProps.TensileYield
                );
            }

            var plutonicProps = MaterialRegistry.GetProperties(MaterialId.RockGabbro);
            return new StratumSample(
                StratigraphicLayerType.PlutonicCore,
                MaterialId.RockGabbro,
                MaterialId.None,
                oreGrade: 0.0f,
                isVein: false,
                density: plutonicProps.SolidDensity,
                compressiveYieldKpa: plutonicProps.CompressiveYield,
                tensileYieldKpa: plutonicProps.TensileYield
            );
        }

        // Kimberlite Pipe check: vertical cylindrical diamond conduits cutting through the crust
        float kimberliteX = (worldX % 256) - 128;
        float kimberliteZ = (worldZ % 256) - 128;
        float distSqFromPipe = (kimberliteX * kimberliteX) + (kimberliteZ * kimberliteZ);
        if (distSqFromPipe < 16.0f) // 4-meter radius pipe
        {
            float diamondRoll = DeterministicNoise3D.Noise3D(_worldSeed + 777UL, worldX * 0.4f, worldY * 0.4f, worldZ * 0.4f);
            bool isDiamond = diamondRoll > 0.65f;
            MaterialId pipeMat = isDiamond ? MaterialId.GemDiamond : MaterialId.OreKimberlite;
            var props = MaterialRegistry.GetProperties(pipeMat);

            return new StratumSample(
                StratigraphicLayerType.IgneousIntrusive,
                MaterialId.OreKimberlite,
                pipeMat,
                oreGrade: isDiamond ? 0.95f : 0.3f,
                isVein: isDiamond,
                density: props.SolidDensity,
                compressiveYieldKpa: props.CompressiveYield,
                tensileYieldKpa: props.TensileYield
            );
        }

        // Determine geological strata thresholds
        float volcanicActivity = DeterministicNoise3D.Noise2D(_worldSeed + 404UL, worldX * 0.005f, worldZ * 0.005f);
        bool isVolcanicRegion = volcanicActivity > 0.45f;

        // Stratum thickness modulations
        int sedimentaryThickness = isVolcanicRegion ? 6 : 28;
        int metamorphicThickness = 32;

        int sedimentaryBottom = surfaceY - soilThickness - sedimentaryThickness;
        int metamorphicBottom = sedimentaryBottom - metamorphicThickness;

        StratigraphicLayerType currentLayer;
        MaterialId hostRock;

        if (isVolcanicRegion && depthBelowSurface < (soilThickness + 12))
        {
            // Volcanic extrusive cap
            currentLayer = StratigraphicLayerType.IgneousExtrusive;
            hostRock = PickExtrusiveRock(worldX, worldY, worldZ);
        }
        else if (worldY > sedimentaryBottom)
        {
            // TIER 2: SEDIMENTARY
            currentLayer = StratigraphicLayerType.Sedimentary;
            hostRock = PickSedimentaryRock(worldX, worldY, worldZ);
        }
        else if (worldY > metamorphicBottom)
        {
            // TIER 3: METAMORPHIC
            currentLayer = StratigraphicLayerType.Metamorphic;
            hostRock = PickMetamorphicRock(worldX, worldY, worldZ);
        }
        else
        {
            // TIER 4: IGNEOUS INTRUSIVE
            currentLayer = StratigraphicLayerType.IgneousIntrusive;
            hostRock = PickIntrusiveRock(worldX, worldY, worldZ);
        }

        // ----------------------------------------------------
        // MINERAL VEINS & CLUSTERS (DF Rules)
        // ----------------------------------------------------
        bool hasVein = CheckMineralVein(currentLayer, hostRock, worldX, worldY, worldZ, out MaterialId veinMaterial, out float oreGrade);

        var effectiveProps = hasVein ? MaterialRegistry.GetProperties(veinMaterial) : MaterialRegistry.GetProperties(hostRock);

        return new StratumSample(
            currentLayer,
            hostRock,
            veinMaterial,
            oreGrade,
            hasVein,
            effectiveProps.SolidDensity,
            effectiveProps.CompressiveYield,
            effectiveProps.TensileYield
        );
    }

    private MaterialId PickSoilMaterial(int worldX, int worldZ)
    {
        float moisture = DeterministicNoise3D.Noise2D(_worldSeed + 505UL, worldX * 0.015f, worldZ * 0.015f);
        if (moisture > 0.4f) return MaterialId.SoilPeat;
        if (moisture > 0.1f) return MaterialId.SoilClay;
        if (moisture > -0.2f) return MaterialId.SoilLoam;
        if (moisture > -0.5f) return MaterialId.SoilSilt;
        return MaterialId.SoilSand;
    }

    private MaterialId PickSedimentaryRock(int worldX, int worldY, int worldZ)
    {
        float noise = DeterministicNoise3D.Noise3D(_worldSeed + 606UL, worldX * 0.02f, worldY * 0.05f, worldZ * 0.02f);
        if (noise > 0.4f) return MaterialId.RockLimestone;
        if (noise > 0.1f) return MaterialId.RockSandstone;
        if (noise > -0.2f) return MaterialId.RockShale;
        if (noise > -0.5f) return MaterialId.RockDolomite;
        return MaterialId.RockChalk;
    }

    private MaterialId PickMetamorphicRock(int worldX, int worldY, int worldZ)
    {
        float noise = DeterministicNoise3D.Noise3D(_worldSeed + 707UL, worldX * 0.02f, worldY * 0.05f, worldZ * 0.02f);
        if (noise > 0.35f) return MaterialId.RockMarble;
        if (noise > 0.1f) return MaterialId.RockQuartzite;
        if (noise > -0.2f) return MaterialId.RockSlate;
        if (noise > -0.5f) return MaterialId.RockGneiss;
        return MaterialId.RockSchist;
    }

    private MaterialId PickExtrusiveRock(int worldX, int worldY, int worldZ)
    {
        float noise = DeterministicNoise3D.Noise3D(_worldSeed + 808UL, worldX * 0.03f, worldY * 0.05f, worldZ * 0.03f);
        if (noise > 0.3f) return MaterialId.RockObsidian;
        if (noise > 0.0f) return MaterialId.RockBasalt;
        if (noise > -0.3f) return MaterialId.RockAndesite;
        return MaterialId.RockDacite;
    }

    private MaterialId PickIntrusiveRock(int worldX, int worldY, int worldZ)
    {
        float noise = DeterministicNoise3D.Noise3D(_worldSeed + 909UL, worldX * 0.02f, worldY * 0.05f, worldZ * 0.02f);
        if (noise > 0.2f) return MaterialId.RockGranite;
        if (noise > -0.2f) return MaterialId.RockDiorite;
        return MaterialId.RockGabbro;
    }

    private bool CheckMineralVein(
        StratigraphicLayerType layer,
        MaterialId hostRock,
        int x, int y, int z,
        out MaterialId veinMaterial,
        out float oreGrade)
    {
        veinMaterial = MaterialId.None;
        oreGrade = 0.0f;

        // 3D tubular/ridge noise for veins
        float veinNoise = MathF.Abs(DeterministicNoise3D.Noise3D(_worldSeed + 1212UL, x * 0.06f, y * 0.06f, z * 0.06f));

        // Dwarf Fortress rule: Coal and Sedimentary Iron (Hematite/Limonite) ONLY occur in Sedimentary rock
        if (layer == StratigraphicLayerType.Sedimentary)
        {
            if (veinNoise < 0.045f) // Thin planar/tubular vein cut
            {
                float typeRoll = DeterministicNoise3D.Noise3D(_worldSeed + 1313UL, x * 0.02f, y * 0.02f, z * 0.02f);
                if (typeRoll > 0.2f)
                {
                    veinMaterial = MaterialId.RockBituminousCoal;
                    oreGrade = 0.85f;
                    return true;
                }
                if (typeRoll > -0.2f)
                {
                    veinMaterial = MaterialId.OreHematite;
                    oreGrade = 0.75f;
                    return true;
                }
                veinMaterial = MaterialId.OreLimonite;
                oreGrade = 0.60f;
                return true;
            }
        }
        else if (layer == StratigraphicLayerType.Metamorphic)
        {
            // Metamorphic hosts Quartzite veins, Native Gold, Native Silver, and Gems
            if (veinNoise < 0.035f)
            {
                float typeRoll = DeterministicNoise3D.Noise3D(_worldSeed + 1414UL, x * 0.03f, y * 0.03f, z * 0.03f);
                if (typeRoll > 0.4f)
                {
                    veinMaterial = MaterialId.MetalGold;
                    oreGrade = 0.90f;
                    return true;
                }
                if (typeRoll > 0.1f)
                {
                    veinMaterial = MaterialId.MetalSilver;
                    oreGrade = 0.80f;
                    return true;
                }
                if (typeRoll < -0.3f)
                {
                    veinMaterial = MaterialId.GemRuby;
                    oreGrade = 0.95f;
                    return true;
                }
            }
        }
        else if (layer == StratigraphicLayerType.IgneousIntrusive)
        {
            // Gabbro specifically hosts Magnetite and Native Platinum in DF
            if (hostRock == MaterialId.RockGabbro && veinNoise < 0.040f)
            {
                float typeRoll = DeterministicNoise3D.Noise3D(_worldSeed + 1515UL, x * 0.03f, y * 0.03f, z * 0.03f);
                if (typeRoll > 0.3f)
                {
                    veinMaterial = MaterialId.OreMagnetite;
                    oreGrade = 0.80f;
                    return true;
                }
                if (typeRoll > 0.0f)
                {
                    veinMaterial = MaterialId.MetalPlatinum;
                    oreGrade = 0.70f;
                    return true;
                }
            }
            else if (hostRock == MaterialId.RockGranite && veinNoise < 0.035f)
            {
                veinMaterial = MaterialId.OreMalachite;
                oreGrade = 0.65f;
                return true;
            }
        }

        return false;
    }
}
