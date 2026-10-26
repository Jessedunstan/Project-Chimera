namespace Chimera.Core.Macro.Materials;

/// <summary>
/// Strongly-typed 16-bit identifier for Dwarf Fortress raw materials.
/// Guarantees O(1) array indexing, zero heap allocations, and deterministic serialization.
/// </summary>
public enum MaterialId : ushort
{
    None = 0,

    // --- Soils & Loose Sediments (1 - 49) ---
    SoilLoam = 1,
    SoilSand = 2,
    SoilClay = 3,
    SoilSilt = 4,
    SoilPeat = 5,
    SoilHumus = 6,

    // --- Sedimentary Rocks (50 - 99) ---
    RockLimestone = 50,
    RockSandstone = 51,
    RockShale = 52,
    RockChalk = 53,
    RockDolomite = 54,
    RockChert = 55,
    RockMudstone = 56,
    RockBituminousCoal = 57,
    RockLignite = 58,
    RockRockSalt = 59,

    // --- Metamorphic Rocks (100 - 149) ---
    RockMarble = 100,
    RockQuartzite = 101,
    RockSlate = 102,
    RockPhyllite = 103,
    RockSchist = 104,
    RockGneiss = 105,

    // --- Igneous Extrusive Rocks (150 - 199) ---
    RockBasalt = 150,
    RockObsidian = 151,
    RockAndesite = 152,
    RockDacite = 153,
    RockRhyolite = 154,

    // --- Igneous Intrusive Rocks (200 - 249) ---
    RockGranite = 200,
    RockDiorite = 201,
    RockGabbro = 202,

    // --- Common & Precious Metals (250 - 299) ---
    MetalIron = 250,
    MetalSteel = 251,
    MetalPigIron = 252,
    MetalCopper = 253,
    MetalBronze = 254,
    MetalBismuthBronze = 255,
    MetalBrass = 256,
    MetalGold = 257,
    MetalSilver = 258,
    MetalPlatinum = 259,
    MetalLead = 260,
    MetalTin = 261,
    MetalZinc = 262,
    MetalNickel = 263,
    MetalElectrum = 264,
    MetalAdamantine = 270, // Legendary DF metal

    // --- Ores & Vein Minerals (300 - 349) ---
    OreHematite = 300,
    OreMagnetite = 301,
    OreLimonite = 302,
    OreMalachite = 303,
    OreTetrahedrite = 304,
    OreNativeGold = 305,
    OreNativeSilver = 306,
    OreNativePlatinum = 307,
    OreCassiterite = 308,
    OreGalena = 309,
    OreBauxite = 310,
    OreKimberlite = 311,

    // --- Gems (350 - 399) ---
    GemDiamond = 350,
    GemRuby = 351,
    GemSapphire = 352,
    GemEmerald = 353,
    GemAmethyst = 354,

    // --- Organics & Fluids (400 - 450) ---
    WoodOak = 400,
    WoodPine = 401,
    Bone = 410,
    LiquidWater = 420,
    SolidIce = 421,
    LiquidMagma = 430
}
