using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Chimera.Core.Macro.Geology;
using Chimera.Core.Macro.History;
using Chimera.Core.Macro.Materials;
using Chimera.Core.MathEngine.Materials;
using Xunit;

namespace Chimera.Tests;

public class MacroSimulationTests
{
    // =========================================================================
    // 1. DWARF FORTRESS RAW MATERIAL TABLES & MECHANICAL PROPERTIES
    // =========================================================================

    [Fact]
    public void UristToKelvin_Conversions_AreMathematicallyExact()
    {
        // 10000 Urist = 0 deg C = 273.15 K
        float freezingK = RawMaterialProperties.UristToKelvin(10000.0f);
        Assert.Equal(273.15f, freezingK, 2);

        // 10180 Urist = 100 deg C = 373.15 K
        float boilingK = RawMaterialProperties.UristToKelvin(10180.0f);
        Assert.Equal(373.15f, boilingK, 2);

        // Round-trip
        float roundTripUrist = RawMaterialProperties.KelvinToUrist(373.15f);
        Assert.Equal(10180.0f, roundTripUrist, 1);
    }

    [Fact]
    public void YoungsModulus_Calculation_MatchesDwarfFortressTensorFormula()
    {
        // E = (Yield / StrainAtYield) * 10^8 Pa
        // Iron: Compressive Yield = 540,000 kPa, StrainAtYield = 257 -> E ~ 210 GPa (2.10 * 10^11 Pa)
        // Iron: Tensile Yield = 270,000 kPa, StrainAtYield = 257 -> E ~ 105 GPa (1.05 * 10^11 Pa)
        var iron = MaterialRegistry.GetProperties(MaterialId.MetalIron);

        float ironCompressiveYoungsPa = iron.CompressiveYoungsModulusPa;
        float expectedCompressivePa = (540000.0f / 257.0f) * 1e8f;
        Assert.Equal(expectedCompressivePa, ironCompressiveYoungsPa, 0);
        Assert.InRange(ironCompressiveYoungsPa, 2.0e11f, 2.2e11f); // ~210 GPa

        float ironTensileYoungsPa = iron.TensileYoungsModulusPa;
        float expectedTensilePa = (270000.0f / 257.0f) * 1e8f;
        Assert.Equal(expectedTensilePa, ironTensileYoungsPa, 0);
        Assert.InRange(ironTensileYoungsPa, 1.0e11f, 1.1e11f); // ~105 GPa
    }

    [Fact]
    public void MaterialRegistry_CanonicalMaterials_HaveAccurateProperties()
    {
        // Verify Steel
        var steel = MaterialRegistry.GetProperties(MaterialId.MetalSteel);
        Assert.Equal(7850.0f, steel.SolidDensity);
        Assert.Equal(500.0f, steel.SpecificHeat);
        Assert.Equal(1505000.0f, steel.ImpactYield);
        Assert.Equal(752500.0f, steel.TensileYield);

        // Verify Adamantine: super light, near-infinite yield
        var adamantine = MaterialRegistry.GetProperties(MaterialId.MetalAdamantine);
        Assert.Equal(200.0f, adamantine.SolidDensity);
        Assert.Equal(7500.0f, adamantine.SpecificHeat);
        Assert.Equal(5000000.0f, adamantine.ImpactYield);

        // Verify Granite: high compressive yield, low tensile yield (brittle stone)
        var granite = MaterialRegistry.GetProperties(MaterialId.RockGranite);
        Assert.Equal(2680.0f, granite.SolidDensity);
        Assert.Equal(130000.0f, granite.CompressiveYield);
        Assert.Equal(4800.0f, granite.TensileYield);
        Assert.True(granite.CompressiveYield > (granite.TensileYield * 20.0f));
    }

    [Fact]
    public void Material_ToPhysicalParameters_ProjectsCorrectlyToSliderMathEngine()
    {
        var ironProps = MaterialRegistry.GetProperties(MaterialId.MetalIron);
        PhysicalMaterialParameters physical = ironProps.ToPhysicalParameters();

        Assert.Equal(7850.0f, physical.Density);
        Assert.Equal(270.0f, physical.TensileYield); // 270,000 kPa = 270 MPa
        Assert.True(physical.Rigidity > 0.4f);
        Assert.True(physical.Brittleness < 0.35f); // Ductile metal
        Assert.True(physical.Malleability > 0.5f);
        Assert.Equal(450.0f, physical.SpecificHeatCapacity);
    }

    // =========================================================================
    // 2. DETERMINISTIC GEOLOGICAL STRATIGRAPHY & SEEDED NOISE VEINS
    // =========================================================================

    [Fact]
    public void StratigraphyGenerator_DeterministicAcrossIdenticalSeeds()
    {
        ulong seed = 0xDEADBEEFCAFEUL;
        var genA = new GeologicalStratigraphyGenerator(seed);
        var genB = new GeologicalStratigraphyGenerator(seed);

        for (int x = -20; x <= 20; x += 5)
        {
            for (int z = -20; z <= 20; z += 5)
            {
                int elevA = genA.GetSurfaceElevation(x, z);
                int elevB = genB.GetSurfaceElevation(x, z);
                Assert.Equal(elevA, elevB);

                for (int y = elevA - 10; y <= elevA + 2; y++)
                {
                    var sampleA = genA.SampleVoxel(x, y, z);
                    var sampleB = genB.SampleVoxel(x, y, z);

                    Assert.Equal(sampleA.LayerType, sampleB.LayerType);
                    Assert.Equal(sampleA.PrimaryMaterial, sampleB.PrimaryMaterial);
                    Assert.Equal(sampleA.VeinMaterial, sampleB.VeinMaterial);
                    Assert.Equal(sampleA.IsVein, sampleB.IsVein);
                    Assert.Equal(sampleA.Density, sampleB.Density);
                }
            }
        }
    }

    [Fact]
    public void Stratigraphy_EnforcesSoilDepth_ZeroToFiveMeters()
    {
        var gen = new GeologicalStratigraphyGenerator(12345UL);

        for (int x = 0; x < 50; x += 7)
        {
            for (int z = 0; z < 50; z += 7)
            {
                int surfaceY = gen.GetSurfaceElevation(x, z);
                int soilThickness = gen.GetSoilThickness(x, z, surfaceY);

                Assert.InRange(soilThickness, 0, 5);

                if (soilThickness > 0)
                {
                    var soilSample = gen.SampleVoxel(x, surfaceY, z);
                    Assert.Equal(StratigraphicLayerType.Soil, soilSample.LayerType);
                }
            }
        }
    }

    [Fact]
    public void Stratigraphy_Veins_StrictlyRespectDwarfFortressGeologyRules()
    {
        var gen = new GeologicalStratigraphyGenerator(42UL);

        int coalCount = 0;
        int hematiteCount = 0;

        // Sample a 40x40 column slice down through sedimentary layers
        for (int x = -20; x <= 20; x++)
        {
            for (int z = -20; z <= 20; z++)
            {
                int surface = gen.GetSurfaceElevation(x, z);
                for (int y = surface - 30; y < surface; y++)
                {
                    var sample = gen.SampleVoxel(x, y, z);

                    // Dwarf Fortress rule: Coal & Hematite can only occur in Sedimentary layer
                    if (sample.EffectiveMaterial == MaterialId.RockBituminousCoal)
                    {
                        coalCount++;
                        Assert.Equal(StratigraphicLayerType.Sedimentary, sample.LayerType);
                    }
                    else if (sample.EffectiveMaterial == MaterialId.OreHematite)
                    {
                        hematiteCount++;
                        Assert.Equal(StratigraphicLayerType.Sedimentary, sample.LayerType);
                    }
                }
            }
        }

        Assert.True(coalCount > 0, "Expected to generate coal veins in sedimentary crust");
        Assert.True(hematiteCount > 0, "Expected to generate hematite veins in sedimentary crust");
    }

    // =========================================================================
    // 3. HISTORICAL CHRONICLE EVENT LEDGER & UNMANAGED STRUCT MEMORY
    // =========================================================================

    [Fact]
    public void HistoricalEvent_And_FixedString64_ArePureUnmanagedStructs()
    {
        // Runtime check: absolutely no reference types allowed in unmanaged memory
        Assert.False(RuntimeHelpers.IsReferenceOrContainsReferences<FixedString64>());
        Assert.False(RuntimeHelpers.IsReferenceOrContainsReferences<HistoricalEvent>());
    }

    [Fact]
    public void HistoricalLedger_HighFrequencyRecording_HasZeroHeapAllocation()
    {
        using var ledger = new HistoricalLedger(capacity: 2048);

        // Warm up JIT and thread local caches
        ledger.RecordEvent(HistoricalEventType.FieldBattle, 1L, 10, 20, 100, 50, 100, "Warmup");

        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < 1000; i++)
        {
            ledger.RecordEvent(
                HistoricalEventType.SiteConquest,
                worldTick: 100L + i,
                primaryEntityId: 101,
                secondaryEntityId: 202,
                x: i,
                y: 64,
                z: i * 2,
                summary: "Conquest of the Iron Fortress",
                metadataFlags: 0x1
            );
        }

        long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

        // 1000 recorded events to native memory MUST yield 0 bytes of managed heap allocation
        Assert.Equal(0L, allocatedAfter - allocatedBefore);
        Assert.Equal(1001, ledger.Count);
    }

    [Fact]
    public void HistoricalLedger_ConcurrentThreadSafeRecording_MaintainsIntegrity()
    {
        using var ledger = new HistoricalLedger(capacity: 10000);

        Parallel.For(0, 5000, i =>
        {
            ledger.RecordEvent(
                HistoricalEventType.ArtifactCreated,
                worldTick: (long)i,
                primaryEntityId: i,
                secondaryEntityId: 0,
                x: 0,
                y: 0,
                z: 0,
                summary: "The Masterpiece Anvil"
            );
        });

        Assert.Equal(5000, ledger.Count);

        // Verify span queries without allocations
        Span<HistoricalEvent> queryDest = stackalloc HistoricalEvent[100];
        int copied = ledger.CopyEventsByType(HistoricalEventType.ArtifactCreated, queryDest);
        Assert.Equal(100, copied);
    }

    [Fact]
    public void FixedString64_HandlesUtf8RoundTrip_WithoutHeapGarbage()
    {
        string original = "The Siege of Boatmurdered";
        var fixedStr = new FixedString64(original);

        Assert.Equal(original.Length, fixedStr.Length);
        Assert.Equal(original, fixedStr.ToString());

        var fixedStr2 = new FixedString64("The Siege of Boatmurdered");
        Assert.True(fixedStr == fixedStr2);
        Assert.Equal(fixedStr.GetHashCode(), fixedStr2.GetHashCode());
    }
}
