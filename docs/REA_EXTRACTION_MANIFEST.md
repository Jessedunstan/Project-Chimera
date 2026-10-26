# Reverse Engineering Analyst (REA) Extraction Manifest

## 1. Local RimWorld Extraction Target
**Root Directory:** `C:\Program Files (x86)\Steam\steamapps\common\RimWorld\`

### Key Targets:
* **`Data/Core/Defs/ThoughtDefs/`**:
  - `Thoughts_Situation_General.xml`: Continuous comfort, beauty, temperature satisfaction curves.
  - `Thoughts_Memory_Social.xml`: Social opinion modifiers, insulted/praised decay rates.
  - `Thoughts_Conditions_Special.xml`: Pain, starvation, confinement psychological penalties.
* **`Data/Core/Defs/TraitDefs/`**:
  - Trait polarities (e.g., Pyromaniac, Neurotic, Depressive, Bloodlust) and their mathematical offsets.
* **`Data/Core/Defs/MentalBreakDefs/`**:
  - Minor, Major, and Extreme break thresholds, break intensities, and recovery tick intervals.
* **`Source/`**:
  - Core C# state machines for `Pawn_NeedsTracker`, `ThoughtWorker`, and `MentalStateHandler`.

---

## 2. Local Rain World Extraction Target
**Root Directory:** `C:\Program Files (x86)\Steam\steamapps\common\Rain World\`

### Key Targets:
* **`RainWorld_Data/Managed/Assembly-CSharp.dll`**:
  - `CreatureTemplate.Relationship`: Continuous fear vs. aggression matrix between species.
  - `AbstractCreature`: Off-screen cycle simulation, den routing, and persistent memory.
  - `Ecosystem`: Food chain trophic levels, hunger replenishment, dynamic population caps.
  - `RainCycle`: Environmental hazard countdown and shelter triggers.

---

## 3. Dwarf Fortress Technical Specification Targets (Online & Raws)
* **Material Property Tables (`material_template_default.txt`):**
  - `IMPACT_YIELD`, `COMPRESSIVE_YIELD`, `TENSILE_YIELD`, `SHEAR_YIELD`
  - `SPECIFIC_HEAT`, `MELTING_POINT`, `BOILING_POINT`, `SOLID_DENSITY`
* **Geological Stratigraphy:**
  - Soil layer -> Sedimentary layer -> Metamorphic layer -> Igneous intrusive/extrusive strata.
* **World History Generation:**
  - Historical figures, civilization territorial claims, conflict ledgers, artifact genesis.
