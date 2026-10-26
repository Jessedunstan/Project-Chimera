using System;
using System.Collections.Generic;

namespace Chimera.Core.Macro.Materials;

/// <summary>
/// Static high-performance registry containing Dwarf Fortress canonical material definitions.
/// Backed by a flat direct-lookup array indexed by (ushort)MaterialId for O(1) zero-allocation access.
/// </summary>
public static class MaterialRegistry
{
    private const int MaxMaterialCapacity = 512;
    private static readonly RawMaterialProperties[] Table = new RawMaterialProperties[MaxMaterialCapacity];
    private static readonly Dictionary<string, MaterialId> NameLookup = new(StringComparer.OrdinalIgnoreCase);

    static MaterialRegistry()
    {
        InitializeRegistry();
    }

    public static RawMaterialProperties GetProperties(MaterialId id)
    {
        ushort idx = (ushort)id;
        return idx < MaxMaterialCapacity ? Table[idx] : Table[0];
    }

    public static bool TryGetProperties(MaterialId id, out RawMaterialProperties properties)
    {
        ushort idx = (ushort)id;
        if (idx < MaxMaterialCapacity && Table[idx].Id != MaterialId.None)
        {
            properties = Table[idx];
            return true;
        }

        properties = default;
        return false;
    }

    public static bool TryGetIdByName(string name, out MaterialId id)
    {
        return NameLookup.TryGetValue(name, out id);
    }

    private static void Register(RawMaterialProperties props, string name)
    {
        ushort idx = (ushort)props.Id;
        if (idx < MaxMaterialCapacity)
        {
            Table[idx] = props;
            NameLookup[name] = props.Id;
            NameLookup[props.Id.ToString()] = props.Id;
        }
    }

    private static void InitializeRegistry()
    {
        // ----------------------------------------------------
        // SOILS & SEDIMENTS
        // ----------------------------------------------------
        Register(new RawMaterialProperties(
            MaterialId.SoilLoam, MaterialCategory.Soil,
            solidDensity: 1600f, specificHeat: 1100f, meltingPointK: 1400f, boilingPointK: 2500f,
            impactYield: 500f, impactFracture: 800f, impactStrainAtYield: 5000f,
            compressiveYield: 500f, compressiveFracture: 800f, compressiveStrainAtYield: 5000f,
            tensileYield: 50f, tensileFracture: 100f, tensileStrainAtYield: 5000f,
            shearYield: 100f, shearFracture: 200f, shearStrainAtYield: 5000f,
            maxEdge: 100f), "Loam");

        Register(new RawMaterialProperties(
            MaterialId.SoilSand, MaterialCategory.Soil,
            solidDensity: 1600f, specificHeat: 830f, meltingPointK: 1980f, boilingPointK: 2500f,
            impactYield: 300f, impactFracture: 500f, impactStrainAtYield: 7000f,
            compressiveYield: 300f, compressiveFracture: 500f, compressiveStrainAtYield: 7000f,
            tensileYield: 10f, tensileFracture: 30f, tensileStrainAtYield: 7000f,
            shearYield: 50f, shearFracture: 100f, shearStrainAtYield: 7000f,
            maxEdge: 100f), "Sand");

        Register(new RawMaterialProperties(
            MaterialId.SoilClay, MaterialCategory.Soil,
            solidDensity: 1800f, specificHeat: 920f, meltingPointK: 1600f, boilingPointK: 2500f,
            impactYield: 800f, impactFracture: 1200f, impactStrainAtYield: 4000f,
            compressiveYield: 800f, compressiveFracture: 1200f, compressiveStrainAtYield: 4000f,
            tensileYield: 80f, tensileFracture: 150f, tensileStrainAtYield: 4000f,
            shearYield: 150f, shearFracture: 300f, shearStrainAtYield: 4000f,
            maxEdge: 100f), "Clay");

        Register(new RawMaterialProperties(
            MaterialId.SoilSilt, MaterialCategory.Soil,
            solidDensity: 1500f, specificHeat: 1050f, meltingPointK: 1450f, boilingPointK: 2500f,
            impactYield: 400f, impactFracture: 600f, impactStrainAtYield: 6000f,
            compressiveYield: 400f, compressiveFracture: 600f, compressiveStrainAtYield: 6000f,
            tensileYield: 30f, tensileFracture: 60f, tensileStrainAtYield: 6000f,
            shearYield: 80f, shearFracture: 150f, shearStrainAtYield: 6000f,
            maxEdge: 100f), "Silt");

        Register(new RawMaterialProperties(
            MaterialId.SoilPeat, MaterialCategory.Soil,
            solidDensity: 1200f, specificHeat: 1800f, meltingPointK: 550f, boilingPointK: 800f,
            impactYield: 250f, impactFracture: 400f, impactStrainAtYield: 8000f,
            compressiveYield: 250f, compressiveFracture: 400f, compressiveStrainAtYield: 8000f,
            tensileYield: 20f, tensileFracture: 50f, tensileStrainAtYield: 8000f,
            shearYield: 40f, shearFracture: 80f, shearStrainAtYield: 8000f,
            maxEdge: 50f), "Peat");

        // ----------------------------------------------------
        // SEDIMENTARY ROCKS
        // ----------------------------------------------------
        Register(new RawMaterialProperties(
            MaterialId.RockLimestone, MaterialCategory.SedimentaryRock,
            solidDensity: 2710f, specificHeat: 880f, meltingPointK: 1600f, boilingPointK: 2800f,
            impactYield: 80000f, impactFracture: 80000f, impactStrainAtYield: 133f,
            compressiveYield: 80000f, compressiveFracture: 80000f, compressiveStrainAtYield: 133f,
            tensileYield: 4000f, tensileFracture: 4000f, tensileStrainAtYield: 133f,
            shearYield: 4000f, shearFracture: 4000f, shearStrainAtYield: 133f,
            maxEdge: 10000f), "Limestone");

        Register(new RawMaterialProperties(
            MaterialId.RockSandstone, MaterialCategory.SedimentaryRock,
            solidDensity: 2200f, specificHeat: 920f, meltingPointK: 1450f, boilingPointK: 2500f,
            impactYield: 60000f, impactFracture: 60000f, impactStrainAtYield: 150f,
            compressiveYield: 60000f, compressiveFracture: 60000f, compressiveStrainAtYield: 150f,
            tensileYield: 3000f, tensileFracture: 3000f, tensileStrainAtYield: 150f,
            shearYield: 3000f, shearFracture: 3000f, shearStrainAtYield: 150f,
            maxEdge: 8000f), "Sandstone");

        Register(new RawMaterialProperties(
            MaterialId.RockShale, MaterialCategory.SedimentaryRock,
            solidDensity: 2600f, specificHeat: 900f, meltingPointK: 1400f, boilingPointK: 2500f,
            impactYield: 50000f, impactFracture: 50000f, impactStrainAtYield: 140f,
            compressiveYield: 50000f, compressiveFracture: 50000f, compressiveStrainAtYield: 140f,
            tensileYield: 2500f, tensileFracture: 2500f, tensileStrainAtYield: 140f,
            shearYield: 2500f, shearFracture: 2500f, shearStrainAtYield: 140f,
            maxEdge: 8000f), "Shale");

        Register(new RawMaterialProperties(
            MaterialId.RockChalk, MaterialCategory.SedimentaryRock,
            solidDensity: 2300f, specificHeat: 910f, meltingPointK: 1550f, boilingPointK: 2700f,
            impactYield: 40000f, impactFracture: 40000f, impactStrainAtYield: 160f,
            compressiveYield: 40000f, compressiveFracture: 40000f, compressiveStrainAtYield: 160f,
            tensileYield: 2000f, tensileFracture: 2000f, tensileStrainAtYield: 160f,
            shearYield: 2000f, shearFracture: 2000f, shearStrainAtYield: 160f,
            maxEdge: 6000f), "Chalk");

        Register(new RawMaterialProperties(
            MaterialId.RockDolomite, MaterialCategory.SedimentaryRock,
            solidDensity: 2840f, specificHeat: 890f, meltingPointK: 1650f, boilingPointK: 2850f,
            impactYield: 90000f, impactFracture: 90000f, impactStrainAtYield: 130f,
            compressiveYield: 90000f, compressiveFracture: 90000f, compressiveStrainAtYield: 130f,
            tensileYield: 4500f, tensileFracture: 4500f, tensileStrainAtYield: 130f,
            shearYield: 4500f, shearFracture: 4500f, shearStrainAtYield: 130f,
            maxEdge: 10000f), "Dolomite");

        Register(new RawMaterialProperties(
            MaterialId.RockBituminousCoal, MaterialCategory.SedimentaryRock,
            solidDensity: 1340f, specificHeat: 1260f, meltingPointK: 650f, boilingPointK: 900f,
            impactYield: 30000f, impactFracture: 30000f, impactStrainAtYield: 200f,
            compressiveYield: 30000f, compressiveFracture: 30000f, compressiveStrainAtYield: 200f,
            tensileYield: 1500f, tensileFracture: 1500f, tensileStrainAtYield: 200f,
            shearYield: 1500f, shearFracture: 1500f, shearStrainAtYield: 200f,
            maxEdge: 5000f), "Bituminous Coal");

        Register(new RawMaterialProperties(
            MaterialId.RockLignite, MaterialCategory.SedimentaryRock,
            solidDensity: 1200f, specificHeat: 1350f, meltingPointK: 600f, boilingPointK: 850f,
            impactYield: 20000f, impactFracture: 20000f, impactStrainAtYield: 220f,
            compressiveYield: 20000f, compressiveFracture: 20000f, compressiveStrainAtYield: 220f,
            tensileYield: 1000f, tensileFracture: 1000f, tensileStrainAtYield: 220f,
            shearYield: 1000f, shearFracture: 1000f, shearStrainAtYield: 220f,
            maxEdge: 4000f), "Lignite");

        // ----------------------------------------------------
        // METAMORPHIC ROCKS
        // ----------------------------------------------------
        Register(new RawMaterialProperties(
            MaterialId.RockMarble, MaterialCategory.MetamorphicRock,
            solidDensity: 2700f, specificHeat: 880f, meltingPointK: 1570f, boilingPointK: 2800f,
            impactYield: 90000f, impactFracture: 90000f, impactStrainAtYield: 120f,
            compressiveYield: 90000f, compressiveFracture: 90000f, compressiveStrainAtYield: 120f,
            tensileYield: 5000f, tensileFracture: 5000f, tensileStrainAtYield: 120f,
            shearYield: 5000f, shearFracture: 5000f, shearStrainAtYield: 120f,
            maxEdge: 10000f), "Marble");

        Register(new RawMaterialProperties(
            MaterialId.RockQuartzite, MaterialCategory.MetamorphicRock,
            solidDensity: 2650f, specificHeat: 750f, meltingPointK: 1950f, boilingPointK: 3000f,
            impactYield: 180000f, impactFracture: 180000f, impactStrainAtYield: 110f,
            compressiveYield: 180000f, compressiveFracture: 180000f, compressiveStrainAtYield: 110f,
            tensileYield: 12000f, tensileFracture: 12000f, tensileStrainAtYield: 110f,
            shearYield: 12000f, shearFracture: 12000f, shearStrainAtYield: 110f,
            maxEdge: 15000f), "Quartzite");

        Register(new RawMaterialProperties(
            MaterialId.RockSlate, MaterialCategory.MetamorphicRock,
            solidDensity: 2750f, specificHeat: 820f, meltingPointK: 1500f, boilingPointK: 2700f,
            impactYield: 100000f, impactFracture: 100000f, impactStrainAtYield: 125f,
            compressiveYield: 100000f, compressiveFracture: 100000f, compressiveStrainAtYield: 125f,
            tensileYield: 6000f, tensileFracture: 6000f, tensileStrainAtYield: 125f,
            shearYield: 6000f, shearFracture: 6000f, shearStrainAtYield: 125f,
            maxEdge: 12000f), "Slate");

        Register(new RawMaterialProperties(
            MaterialId.RockGneiss, MaterialCategory.MetamorphicRock,
            solidDensity: 2800f, specificHeat: 800f, meltingPointK: 1550f, boilingPointK: 2750f,
            impactYield: 140000f, impactFracture: 140000f, impactStrainAtYield: 115f,
            compressiveYield: 140000f, compressiveFracture: 140000f, compressiveStrainAtYield: 115f,
            tensileYield: 8000f, tensileFracture: 8000f, tensileStrainAtYield: 115f,
            shearYield: 8000f, shearFracture: 8000f, shearStrainAtYield: 115f,
            maxEdge: 11000f), "Gneiss");

        Register(new RawMaterialProperties(
            MaterialId.RockSchist, MaterialCategory.MetamorphicRock,
            solidDensity: 2750f, specificHeat: 810f, meltingPointK: 1520f, boilingPointK: 2700f,
            impactYield: 110000f, impactFracture: 110000f, impactStrainAtYield: 120f,
            compressiveYield: 110000f, compressiveFracture: 110000f, compressiveStrainAtYield: 120f,
            tensileYield: 6500f, tensileFracture: 6500f, tensileStrainAtYield: 120f,
            shearYield: 6500f, shearFracture: 6500f, shearStrainAtYield: 120f,
            maxEdge: 10000f), "Schist");

        // ----------------------------------------------------
        // IGNEOUS EXTRUSIVE ROCKS
        // ----------------------------------------------------
        Register(new RawMaterialProperties(
            MaterialId.RockBasalt, MaterialCategory.IgneousExtrusiveRock,
            solidDensity: 3000f, specificHeat: 840f, meltingPointK: 1473f, boilingPointK: 2600f,
            impactYield: 150000f, impactFracture: 150000f, impactStrainAtYield: 110f,
            compressiveYield: 150000f, compressiveFracture: 150000f, compressiveStrainAtYield: 110f,
            tensileYield: 6000f, tensileFracture: 6000f, tensileStrainAtYield: 110f,
            shearYield: 6000f, shearFracture: 6000f, shearStrainAtYield: 110f,
            maxEdge: 12000f), "Basalt");

        Register(new RawMaterialProperties(
            MaterialId.RockObsidian, MaterialCategory.IgneousExtrusiveRock,
            solidDensity: 2600f, specificHeat: 830f, meltingPointK: 1373f, boilingPointK: 2500f,
            impactYield: 110000f, impactFracture: 110000f, impactStrainAtYield: 100f,
            compressiveYield: 110000f, compressiveFracture: 110000f, compressiveStrainAtYield: 100f,
            tensileYield: 7000f, tensileFracture: 7000f, tensileStrainAtYield: 100f,
            shearYield: 7000f, shearFracture: 7000f, shearStrainAtYield: 100f,
            maxEdge: 100000f), "Obsidian");

        Register(new RawMaterialProperties(
            MaterialId.RockAndesite, MaterialCategory.IgneousExtrusiveRock,
            solidDensity: 2700f, specificHeat: 820f, meltingPointK: 1450f, boilingPointK: 2550f,
            impactYield: 130000f, impactFracture: 130000f, impactStrainAtYield: 115f,
            compressiveYield: 130000f, compressiveFracture: 130000f, compressiveStrainAtYield: 115f,
            tensileYield: 5500f, tensileFracture: 5500f, tensileStrainAtYield: 115f,
            shearYield: 5500f, shearFracture: 5500f, shearStrainAtYield: 115f,
            maxEdge: 10000f), "Andesite");

        Register(new RawMaterialProperties(
            MaterialId.RockDacite, MaterialCategory.IgneousExtrusiveRock,
            solidDensity: 2650f, specificHeat: 810f, meltingPointK: 1420f, boilingPointK: 2500f,
            impactYield: 120000f, impactFracture: 120000f, impactStrainAtYield: 118f,
            compressiveYield: 120000f, compressiveFracture: 120000f, compressiveStrainAtYield: 118f,
            tensileYield: 5000f, tensileFracture: 5000f, tensileStrainAtYield: 118f,
            shearYield: 5000f, shearFracture: 5000f, shearStrainAtYield: 118f,
            maxEdge: 10000f), "Dacite");

        Register(new RawMaterialProperties(
            MaterialId.RockRhyolite, MaterialCategory.IgneousExtrusiveRock,
            solidDensity: 2500f, specificHeat: 800f, meltingPointK: 1400f, boilingPointK: 2450f,
            impactYield: 115000f, impactFracture: 115000f, impactStrainAtYield: 120f,
            compressiveYield: 115000f, compressiveFracture: 115000f, compressiveStrainAtYield: 120f,
            tensileYield: 4800f, tensileFracture: 4800f, tensileStrainAtYield: 120f,
            shearYield: 4800f, shearFracture: 4800f, shearStrainAtYield: 120f,
            maxEdge: 10000f), "Rhyolite");

        // ----------------------------------------------------
        // IGNEOUS INTRUSIVE ROCKS
        // ----------------------------------------------------
        Register(new RawMaterialProperties(
            MaterialId.RockGranite, MaterialCategory.IgneousIntrusiveRock,
            solidDensity: 2680f, specificHeat: 790f, meltingPointK: 1488f, boilingPointK: 2600f,
            impactYield: 130000f, impactFracture: 130000f, impactStrainAtYield: 120f,
            compressiveYield: 130000f, compressiveFracture: 130000f, compressiveStrainAtYield: 120f,
            tensileYield: 4800f, tensileFracture: 4800f, tensileStrainAtYield: 120f,
            shearYield: 4800f, shearFracture: 4800f, shearStrainAtYield: 120f,
            maxEdge: 10000f), "Granite");

        Register(new RawMaterialProperties(
            MaterialId.RockDiorite, MaterialCategory.IgneousIntrusiveRock,
            solidDensity: 2850f, specificHeat: 810f, meltingPointK: 1500f, boilingPointK: 2650f,
            impactYield: 140000f, impactFracture: 140000f, impactStrainAtYield: 118f,
            compressiveYield: 140000f, compressiveFracture: 140000f, compressiveStrainAtYield: 118f,
            tensileYield: 5200f, tensileFracture: 5200f, tensileStrainAtYield: 118f,
            shearYield: 5200f, shearFracture: 5200f, shearStrainAtYield: 118f,
            maxEdge: 10000f), "Diorite");

        Register(new RawMaterialProperties(
            MaterialId.RockGabbro, MaterialCategory.IgneousIntrusiveRock,
            solidDensity: 3000f, specificHeat: 820f, meltingPointK: 1520f, boilingPointK: 2700f,
            impactYield: 160000f, impactFracture: 160000f, impactStrainAtYield: 112f,
            compressiveYield: 160000f, compressiveFracture: 160000f, compressiveStrainAtYield: 112f,
            tensileYield: 6500f, tensileFracture: 6500f, tensileStrainAtYield: 112f,
            shearYield: 6500f, shearFracture: 6500f, shearStrainAtYield: 112f,
            maxEdge: 10000f), "Gabbro");

        // ----------------------------------------------------
        // METALS
        // ----------------------------------------------------
        Register(new RawMaterialProperties(
            MaterialId.MetalIron, MaterialCategory.Metal,
            solidDensity: 7850f, specificHeat: 450f, meltingPointK: 1811f, boilingPointK: 3134f,
            impactYield: 540000f, impactFracture: 1080000f, impactStrainAtYield: 257f,
            compressiveYield: 540000f, compressiveFracture: 1080000f, compressiveStrainAtYield: 257f,
            tensileYield: 270000f, tensileFracture: 540000f, tensileStrainAtYield: 257f,
            shearYield: 270000f, shearFracture: 540000f, shearStrainAtYield: 654f,
            maxEdge: 10000f), "Iron");

        Register(new RawMaterialProperties(
            MaterialId.MetalSteel, MaterialCategory.Metal,
            solidDensity: 7850f, specificHeat: 500f, meltingPointK: 1800f, boilingPointK: 3100f,
            impactYield: 1505000f, impactFracture: 2520000f, impactStrainAtYield: 717f,
            compressiveYield: 1505000f, compressiveFracture: 2520000f, compressiveStrainAtYield: 717f,
            tensileYield: 752500f, tensileFracture: 1260000f, tensileStrainAtYield: 717f,
            shearYield: 752500f, shearFracture: 1260000f, shearStrainAtYield: 1823f,
            maxEdge: 10000f), "Steel");

        Register(new RawMaterialProperties(
            MaterialId.MetalPigIron, MaterialCategory.Metal,
            solidDensity: 7500f, specificHeat: 460f, meltingPointK: 1473f, boilingPointK: 2800f,
            impactYield: 350000f, impactFracture: 700000f, impactStrainAtYield: 300f,
            compressiveYield: 700000f, compressiveFracture: 1400000f, compressiveStrainAtYield: 300f,
            tensileYield: 140000f, tensileFracture: 280000f, tensileStrainAtYield: 300f,
            shearYield: 140000f, shearFracture: 280000f, shearStrainAtYield: 700f,
            maxEdge: 8000f), "Pig Iron");

        Register(new RawMaterialProperties(
            MaterialId.MetalCopper, MaterialCategory.Metal,
            solidDensity: 8930f, specificHeat: 385f, meltingPointK: 1358f, boilingPointK: 2835f,
            impactYield: 245000f, impactFracture: 770000f, impactStrainAtYield: 217f,
            compressiveYield: 245000f, compressiveFracture: 770000f, compressiveStrainAtYield: 217f,
            tensileYield: 70000f, tensileFracture: 220000f, tensileStrainAtYield: 217f,
            shearYield: 70000f, shearFracture: 220000f, shearStrainAtYield: 551f,
            maxEdge: 10000f), "Copper");

        Register(new RawMaterialProperties(
            MaterialId.MetalBronze, MaterialCategory.Metal,
            solidDensity: 8250f, specificHeat: 380f, meltingPointK: 1223f, boilingPointK: 2600f,
            impactYield: 602000f, impactFracture: 1008000f, impactStrainAtYield: 528f,
            compressiveYield: 602000f, compressiveFracture: 1008000f, compressiveStrainAtYield: 528f,
            tensileYield: 301000f, tensileFracture: 504000f, tensileStrainAtYield: 528f,
            shearYield: 301000f, shearFracture: 504000f, shearStrainAtYield: 1342f,
            maxEdge: 10000f), "Bronze");

        Register(new RawMaterialProperties(
            MaterialId.MetalGold, MaterialCategory.Metal,
            solidDensity: 19320f, specificHeat: 129f, meltingPointK: 1337f, boilingPointK: 3129f,
            impactYield: 175000f, impactFracture: 350000f, impactStrainAtYield: 224f,
            compressiveYield: 175000f, compressiveFracture: 350000f, compressiveStrainAtYield: 224f,
            tensileYield: 50000f, tensileFracture: 100000f, tensileStrainAtYield: 224f,
            shearYield: 50000f, shearFracture: 100000f, shearStrainAtYield: 570f,
            maxEdge: 5000f), "Gold");

        Register(new RawMaterialProperties(
            MaterialId.MetalSilver, MaterialCategory.Metal,
            solidDensity: 10490f, specificHeat: 235f, meltingPointK: 1235f, boilingPointK: 2435f,
            impactYield: 350000f, impactFracture: 700000f, impactStrainAtYield: 422f,
            compressiveYield: 350000f, compressiveFracture: 700000f, compressiveStrainAtYield: 422f,
            tensileYield: 100000f, tensileFracture: 200000f, tensileStrainAtYield: 422f,
            shearYield: 100000f, shearFracture: 200000f, shearStrainAtYield: 1073f,
            maxEdge: 10000f), "Silver");

        Register(new RawMaterialProperties(
            MaterialId.MetalPlatinum, MaterialCategory.Metal,
            solidDensity: 21450f, specificHeat: 130f, meltingPointK: 2041f, boilingPointK: 4098f,
            impactYield: 350000f, impactFracture: 700000f, impactStrainAtYield: 208f,
            compressiveYield: 350000f, compressiveFracture: 700000f, compressiveStrainAtYield: 208f,
            tensileYield: 100000f, tensileFracture: 200000f, tensileStrainAtYield: 208f,
            shearYield: 100000f, shearFracture: 200000f, shearStrainAtYield: 529f,
            maxEdge: 10000f), "Platinum");

        Register(new RawMaterialProperties(
            MaterialId.MetalLead, MaterialCategory.Metal,
            solidDensity: 11340f, specificHeat: 130f, meltingPointK: 600f, boilingPointK: 2022f,
            impactYield: 50000f, impactFracture: 100000f, impactStrainAtYield: 312f,
            compressiveYield: 50000f, compressiveFracture: 100000f, compressiveStrainAtYield: 312f,
            tensileYield: 10000f, tensileFracture: 20000f, tensileStrainAtYield: 312f,
            shearYield: 10000f, shearFracture: 20000f, shearStrainAtYield: 795f,
            maxEdge: 1000f), "Lead");

        Register(new RawMaterialProperties(
            MaterialId.MetalTin, MaterialCategory.Metal,
            solidDensity: 7310f, specificHeat: 228f, meltingPointK: 505f, boilingPointK: 2875f,
            impactYield: 75000f, impactFracture: 150000f, impactStrainAtYield: 150f,
            compressiveYield: 75000f, compressiveFracture: 150000f, compressiveStrainAtYield: 150f,
            tensileYield: 15000f, tensileFracture: 30000f, tensileStrainAtYield: 150f,
            shearYield: 15000f, shearFracture: 30000f, shearStrainAtYield: 382f,
            maxEdge: 2000f), "Tin");

        Register(new RawMaterialProperties(
            MaterialId.MetalAdamantine, MaterialCategory.Metal,
            solidDensity: 200f, specificHeat: 7500f, meltingPointK: 14000f, boilingPointK: 28000f,
            impactYield: 5000000f, impactFracture: 9000000f, impactStrainAtYield: 1000f,
            compressiveYield: 5000000f, compressiveFracture: 9000000f, compressiveStrainAtYield: 1000f,
            tensileYield: 5000000f, tensileFracture: 9000000f, tensileStrainAtYield: 1000f,
            shearYield: 5000000f, shearFracture: 9000000f, shearStrainAtYield: 2500f,
            maxEdge: 100000f), "Adamantine");

        // ----------------------------------------------------
        // ORES
        // ----------------------------------------------------
        Register(new RawMaterialProperties(
            MaterialId.OreHematite, MaterialCategory.Ore,
            solidDensity: 5260f, specificHeat: 650f, meltingPointK: 1838f, boilingPointK: 3000f,
            impactYield: 120000f, impactFracture: 120000f, impactStrainAtYield: 120f,
            compressiveYield: 120000f, compressiveFracture: 120000f, compressiveStrainAtYield: 120f,
            tensileYield: 5000f, tensileFracture: 5000f, tensileStrainAtYield: 120f,
            shearYield: 5000f, shearFracture: 5000f, shearStrainAtYield: 120f,
            maxEdge: 10000f), "Hematite");

        Register(new RawMaterialProperties(
            MaterialId.OreMagnetite, MaterialCategory.Ore,
            solidDensity: 5180f, specificHeat: 650f, meltingPointK: 1870f, boilingPointK: 3000f,
            impactYield: 130000f, impactFracture: 130000f, impactStrainAtYield: 120f,
            compressiveYield: 130000f, compressiveFracture: 130000f, compressiveStrainAtYield: 120f,
            tensileYield: 5500f, tensileFracture: 5500f, tensileStrainAtYield: 120f,
            shearYield: 5500f, shearFracture: 5500f, shearStrainAtYield: 120f,
            maxEdge: 10000f), "Magnetite");

        Register(new RawMaterialProperties(
            MaterialId.OreLimonite, MaterialCategory.Ore,
            solidDensity: 3800f, specificHeat: 700f, meltingPointK: 1700f, boilingPointK: 2800f,
            impactYield: 70000f, impactFracture: 70000f, impactStrainAtYield: 140f,
            compressiveYield: 70000f, compressiveFracture: 70000f, compressiveStrainAtYield: 140f,
            tensileYield: 3500f, tensileFracture: 3500f, tensileStrainAtYield: 140f,
            shearYield: 3500f, shearFracture: 3500f, shearStrainAtYield: 140f,
            maxEdge: 8000f), "Limonite");

        Register(new RawMaterialProperties(
            MaterialId.OreMalachite, MaterialCategory.Ore,
            solidDensity: 4000f, specificHeat: 720f, meltingPointK: 1300f, boilingPointK: 2600f,
            impactYield: 80000f, impactFracture: 80000f, impactStrainAtYield: 130f,
            compressiveYield: 80000f, compressiveFracture: 80000f, compressiveStrainAtYield: 130f,
            tensileYield: 4000f, tensileFracture: 4000f, tensileStrainAtYield: 130f,
            shearYield: 4000f, shearFracture: 4000f, shearStrainAtYield: 130f,
            maxEdge: 9000f), "Malachite");

        Register(new RawMaterialProperties(
            MaterialId.OreKimberlite, MaterialCategory.Ore,
            solidDensity: 2900f, specificHeat: 800f, meltingPointK: 1550f, boilingPointK: 2700f,
            impactYield: 140000f, impactFracture: 140000f, impactStrainAtYield: 115f,
            compressiveYield: 140000f, compressiveFracture: 140000f, compressiveStrainAtYield: 115f,
            tensileYield: 6000f, tensileFracture: 6000f, tensileStrainAtYield: 115f,
            shearYield: 6000f, shearFracture: 6000f, shearStrainAtYield: 115f,
            maxEdge: 10000f), "Kimberlite");

        // ----------------------------------------------------
        // GEMS
        // ----------------------------------------------------
        Register(new RawMaterialProperties(
            MaterialId.GemDiamond, MaterialCategory.Gem,
            solidDensity: 3510f, specificHeat: 516f, meltingPointK: 4000f, boilingPointK: 5100f,
            impactYield: 5000000f, impactFracture: 5000000f, impactStrainAtYield: 500f,
            compressiveYield: 5000000f, compressiveFracture: 5000000f, compressiveStrainAtYield: 500f,
            tensileYield: 2800000f, tensileFracture: 2800000f, tensileStrainAtYield: 500f,
            shearYield: 2800000f, shearFracture: 2800000f, shearStrainAtYield: 500f,
            maxEdge: 100000f), "Diamond");

        // ----------------------------------------------------
        // FLUIDS & ORGANICS
        // ----------------------------------------------------
        Register(new RawMaterialProperties(
            MaterialId.LiquidWater, MaterialCategory.Liquid,
            solidDensity: 1000f, specificHeat: 4184f, meltingPointK: 273.15f, boilingPointK: 373.15f,
            impactYield: 1f, impactFracture: 1f, impactStrainAtYield: 10000f,
            compressiveYield: 1f, compressiveFracture: 1f, compressiveStrainAtYield: 10000f,
            tensileYield: 1f, tensileFracture: 1f, tensileStrainAtYield: 10000f,
            shearYield: 1f, shearFracture: 1f, shearStrainAtYield: 10000f,
            maxEdge: 1f), "Water");

        Register(new RawMaterialProperties(
            MaterialId.SolidIce, MaterialCategory.Liquid,
            solidDensity: 917f, specificHeat: 2050f, meltingPointK: 273.15f, boilingPointK: 373.15f,
            impactYield: 3000f, impactFracture: 3000f, impactStrainAtYield: 300f,
            compressiveYield: 3000f, compressiveFracture: 3000f, compressiveStrainAtYield: 300f,
            tensileYield: 1500f, tensileFracture: 1500f, tensileStrainAtYield: 300f,
            shearYield: 1500f, shearFracture: 1500f, shearStrainAtYield: 300f,
            maxEdge: 5000f), "Ice");

        Register(new RawMaterialProperties(
            MaterialId.WoodOak, MaterialCategory.Organic,
            solidDensity: 700f, specificHeat: 1700f, meltingPointK: 573.15f, boilingPointK: 700f,
            impactYield: 40000f, impactFracture: 70000f, impactStrainAtYield: 350f,
            compressiveYield: 40000f, compressiveFracture: 70000f, compressiveStrainAtYield: 350f,
            tensileYield: 90000f, tensileFracture: 120000f, tensileStrainAtYield: 350f,
            shearYield: 10000f, shearFracture: 20000f, shearStrainAtYield: 800f,
            maxEdge: 1000f), "Oak Wood");
    }
}
