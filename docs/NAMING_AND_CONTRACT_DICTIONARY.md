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
