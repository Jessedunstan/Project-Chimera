# Universal Naming and Contract Dictionary
*Project Chimera Core Architecture Standards*

To ensure seamless collaboration across all sub-conversations and simulation layers, all code must adhere to these unified naming standards and shared contract interfaces.

---

## 1. Unit & Measurement Invariants
- **Distance:** 1 voxel = $1.0\text{ meter}$.
- **Mass:** Kilograms ($kg$).
- **Time / Ticks:**
  - Micro-tick (Physics / Entity): 20 or 60 Hz.
  - Simulation-tick (Needs / Metabolism / Weather): 1 Hz (or 20 ticks).
  - Macro-tick (Off-screen ecosystem / History / Ledgers): Low-frequency (e.g., 1 tick per in-game hour/day).
- **Temperature:** Kelvin ($K$) internally; convertible to Celsius for UI.
- **Continuous Sliders:** Normalized `[0.0, 1.0]` for ratios/intensities, or explicit physical units (e.g. $kg/m^3$ for density).

---

## 2. Universal Namespace Hierarchy
```csharp
namespace Chimera.Core
{
    namespace Macro       // DF Foundation: History, geology, materials, world ledgers
    namespace Micro       // RimWorld Layer: Psychology, thoughts, social webs, needs
    namespace Ecology     // Rain World Layer: Food webs, creature cycles, territory
    namespace Genetics    // Gene pool, morphological sliders, junk DNA
    namespace MathEngine  // Continuous curves, slider evaluators, reaction matrices
    namespace Director    // LLM narrative parameters, event contracts
}
```

---

## 3. Core Interface Contracts
```csharp
namespace Chimera.Core.Contracts
{
    public interface ISimulationTickable
    {
        void StepMicroTick(float deltaTime);
        void StepSimulationTick(long currentTick);
        void StepMacroTick(long currentMacroCycle);
    }

    public interface ISliderModifiable
    {
        float GetSliderValue(string parameterKey);
        void SetSliderValue(string parameterKey, float value);
    }

    public interface IHistoricalEventPublisher
    {
        void RecordEvent(string eventType, string summary, long worldTimestamp);
    }
}
```

---

## 4. Macro-Simulation Standards (Dwarf Fortress Foundation)

### 4.1 Raw Material Yield Tensors & Thermal Properties (`Chimera.Core.Macro.Materials`)
All raw materials are parameterized by exact Dwarf Fortress mechanical tensors and SI thermal units:
- **`RawMaterialProperties`**: Unmanaged struct containing `SolidDensity` ($kg/m^3$), `SpecificHeat` ($J/(kg \cdot K)$), `MeltingPointK` ($K$), `BoilingPointK` ($K$), and mechanical strength tensors in $kPa$: `ImpactYield`, `ImpactFracture`, `ImpactStrainAtYield`, `CompressiveYield`, `CompressiveFracture`, `TensileYield`, `TensileFracture`, `ShearYield`, `ShearFracture`, and `MaxEdge`.
- **Young's Modulus Formula:**
  $$E = \frac{\text{Yield}}{\text{StrainAtYield}} \times 10^8\text{ Pa}$$
- **Urist to Kelvin Conversion:**
  $$T_K = \frac{T_U - 10000}{1.8} + 273.15$$
- **Projection to Sliders:** `RawMaterialProperties.ToPhysicalParameters()` seamlessly projects canonical DF material tensors into normalized continuous `PhysicalMaterialParameters` for the `slider-math-engine`.

### 4.2 4-Tier Geological Stratigraphy (`Chimera.Core.Macro.Geology`)
- **`StratigraphicLayerType`**:
  1. `Soil`: Humus, loam, sand, clay, silt, peat ($0\text{ to }5\text{ meters}$).
  2. `Sedimentary`: Primary source of coal (bituminous coal, lignite), flux stones (limestone, chalk, dolomite), and iron ores (hematite, limonite).
  3. `Metamorphic`: High-pressure transformation band hosting marble, quartzite, slate, gneiss, schist, native gold/silver, and gems.
  4. `IgneousExtrusive`: Volcanic surface caps (basalt, obsidian, andesite, dacite).
  5. `IgneousIntrusive`: Deep basement rock (granite, diorite, gabbro with magnetite and native platinum).
  6. `PlutonicCore`: Deep mantle/magma and raw adamantine spires.
- **Seeded Resource Distribution (`GeologicalStratigraphyGenerator`)**: Fully deterministic, zero-allocation 3D coherent noise and cellular sampling enforcing Dwarf Fortress geological host-rock rules.

### 4.3 Historical Chronicle Ledger (`Chimera.Core.Macro.History`)
- **`FixedString64`**: Cache-friendly 64-byte unmanaged UTF-8 string buffer for event descriptions. Zero GC allocations.
- **`HistoricalEvent`**: Sequential unmanaged struct (`EventId`, `WorldTick`, `EventType`, `PrimaryEntityId`, `SecondaryEntityId`, `LocationX`, `LocationY`, `LocationZ`, `Summary`, `MetadataFlags`).
- **`HistoricalLedger`**: Backed by unmanaged native memory (`NativeMemory.AllocZeroed`) with atomic lock-free parallel append (`Interlocked.Increment`). Implements `IHistoricalEventPublisher` and `ISimulationTickable`. Supports zero-allocation span queries over historical epochs.
