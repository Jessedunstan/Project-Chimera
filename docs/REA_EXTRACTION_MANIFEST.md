# Reverse Engineering Analyst (REA) Extraction Manifest & Specification
*Project Chimera Core Architecture Standards*

This document codifies the reverse-engineered mathematical curves, state machines, and data schemas extracted directly from local game installations and verified engineering sources. All simulation modules (`Chimera.Core.Micro`, `Chimera.Core.Ecology`, `Chimera.Core.Macro`, and `Chimera.Core.MathEngine`) must implement their continuous logic according to the specifications below.

---

## 1. RimWorld Extraction Specifications (Micro-Psychology)

### 1.1 Need_Mood & Mental Breaker Continuous Dynamics
* **Time Scale Invariant:** $1\text{ RimWorld In-Game Day} = 60,000\text{ ticks}$. $1\text{ hour} = 2,500\text{ ticks}$.
* **Baseline Need Values:**
  - `Mood.baseLevel = 0.32` ($32\%$)
  - `Mood.seekerRisePerHour = 0.12`
  - `Mood.seekerFallPerHour = 0.08`
  - `Mood.freezeWhileSleeping = true`
* **Mental Break Thresholds:**
  - `Base MentalBreakThreshold = 0.35` ($35\%$ - Minor Break)
  - `Major Break Threshold = 0.20` ($20\%$, offset $-0.15$)
  - `Extreme Break Threshold = 0.05` ($5\%$, offset $-0.30$)
* **Trait Modifiers to MentalBreakThreshold:**
  - `Iron-willed`: $-0.18$ (Threshold drops to $17\%$)
  - `Steadfast`: $-0.09$ (Threshold drops to $26\%$)
  - `Nervous`: $+0.08$ (Threshold rises to $43\%$)
  - `Volatile`: $+0.15$ (Threshold rises to $50\%$)
  - `Too Smart`: $+0.12$ (Threshold rises to $47\%$)
  - `Neurotic` (Tier 1): $+0.08$ (WorkSpeed $+20\%$)
  - `Very Neurotic` (Tier 2): $+0.14$ (WorkSpeed $+40\%$)

### 1.2 Mental Break Intensities & Recovery Intervals
| Break Def | Intensity | Min Recovery (Ticks) | Max Recovery (Ticks) | In-Game Hours | Mean Time Between (MTB Days) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **FireStartingSpree** | Extreme | 6,000 | 10,000 | 2.4 - 4.0 hrs | 0.033 |
| **Slaughterer** | Extreme | 8,000 | 12,000 | 3.2 - 4.8 hrs | 0.033 |
| **Binging_DrugExtreme**| Extreme | 40,000 | 60,000 | 16.0 - 24.0 hrs | 0.133 |
| **Berserk** | Extreme | 40,000 | 60,000 | 16.0 - 24.0 hrs | 0.166 |
| **MurderousRage** | Extreme | 100,000 | 100,000 | 40.0 hrs | Fixed |
| **Tantrum** | Major | 8,000 | 12,000 | 3.2 - 4.8 hrs | 0.033 |
| **TargetedTantrum** | Major | 40,000 | 60,000 | 16.0 - 24.0 hrs | 0.166 |
| **CorpseObsession** | Major | 50,000 | 70,000 | 20.0 - 28.0 hrs | 0.166 |
| **Wander_Psychotic** | Major | 70,000 | 90,000 | 28.0 - 36.0 hrs | 0.166 |
| **Binging_Food** | Minor | 25,000 | 45,000 | 10.0 - 18.0 hrs | 0.166 |
| **InsultingSpree** | Minor | 25,000 | 45,000 | 10.0 - 18.0 hrs | 0.166 |
| **Wander_Sad** | Minor | 40,000 | 60,000 | 16.0 - 24.0 hrs | 0.166 |
| **Wander_OwnRoom** | Minor | 40,000 | 60,000 | 16.0 - 24.0 hrs | 0.166 |

### 1.3 Physical Need Curves & Mood Offsets
* **Need_Food (Nutrition & Malnutrition Stages):**
  - Stage 0 (Hungry): $-6$
  - Stage 1 (Ravenously Hungry): $-12$
  - Stage 2 (Malnourished Initial): $-20$
  - Stage 3 (Badly Malnourished): $-26$
  - Stage 4 (Starving): $-32$
  - Stage 5 (Advanced Starvation): $-38$
  - Stage 6 (Extreme Starvation): $-44$
* **Need_Rest:**
  - Drowsy: $-6$, Tired: $-12$, Exhausted: $-18$
* **Need_Joy (Recreation):**
  - Recreation-Starved: $-20$, Recreation-Deprived: $-10$, Recreation Unfulfilled: $-5$, Satisfied: $+5$, Fully Satisfied: $+10$
* **Need_Comfort:**
  - Rise/Fall: `seekerRisePerHour = 0.60`, `seekerFallPerHour = 0.04`
  - Uncomfortable: $-3$, Comfortable: $+4$, Quite Comfortable: $+6$, Extremely Comfortable: $+8$, Luxuriant: $+10$
* **Need_Beauty:**
  - Rise/Fall: `seekerRisePerHour = 0.32`, `seekerFallPerHour = 0.08`, `baseLevel = 0.40`
  - Hideous: $-15$, Ugly: $-10$, Unsightly: $-5$, Pretty: $+5$, Beautiful: $+10$, Gorgeous: $+15$
* **Need_RoomSize (Interior Volume):**
  - Confined: $-10$, Cramped: $-5$, Spacious: $+5$
* **Pain Curve:**
  - Minor: $-5$, Serious: $-10$, Intense: $-15$, Mind-Shattering: $-20$ (Inverted to $+5, +10, +15, +20$ for Masochists)
* **Temperature Discomfort:**
  - Cold Stages: Chilly ($-4$), Cold ($-8$), Numbing Cold ($-12$), Absolutely Freezing ($-16$)
  - Heat Stages: Sweaty ($-4$), Hot ($-8$), Sweltering ($-12$), Blistering Hot ($-16$)

### 1.4 Social Opinion & Memory Matrices
* **Conversational Interaction Weights:**
  - `Chitchat`: $+0.66$ opinion per occurrence, stack limit $300$
  - `DeepTalk`: $+15$ opinion, duration $20\text{ days}$, stack limit $300$, `stackedEffectMultiplier = 0.9`
  - `Slighted`: $-5$ opinion, duration $20\text{ days}$, `stackedEffectMultiplier = 0.9`
  - `Insulted`: $-15$ opinion, duration $20\text{ days}$, `stackedEffectMultiplier = 0.9`; triggers `InsultedMood` ($-5$ mood, duration $2\text{ days}$)
  - `KindWords`: $+15$ opinion, duration $20\text{ days}$; triggers `KindWordsMood` ($+5$ mood, duration $2\text{ days}$)
* **Homicide & Kin Reactions:**
  - Killed Nuclear Kin (Father, Mother, Son, Daughter, Brother, Sister): $-80$ opinion, duration $120\text{ days}$
  - Killed Spouse / Fiance: $-65$ opinion, duration $120\text{ days}$
  - Killed Lover: $-50$ opinion, duration $120\text{ days}$
  - Killed Friend: $-35$ opinion, duration $60\text{ days}$
  - Killed Rival: $+35$ opinion, duration $60\text{ days}$ (Schadenfreude inversion)
* **Romance & Bonding:**
  - Honeymoon Phase: $+40$ opinion, duration $30\text{ days}$
  - Rescued Me (Offering Help): $+25$ opinion, duration $30\text{ days}$
  - Rescued Me (Direct): $+15$ opinion, duration $30\text{ days}$
  - Crashed Together: $+25$ opinion, duration $20\text{ days}$
  - Rebuffed Romance: $-10$ opinion, duration $10\text{ days}$
  - Cheated On Me / Divorced Me: $-70$ opinion, duration $120\text{ days} / 180\text{ days}$

---

## 2. Rain World Extraction Specifications (Autonomous Ecosystem)

### 2.1 Environmental Rain Cycle & Hazard Timers
* **Cycle Duration:** Region-dependent timer between $400\text{s}$ and $800\text{s}$ ($6.6\text{ to }13.3\text{ minutes}$).
* **Rain Hazard Progression Curve:**
  1. **Pre-rain / Warning:** Sky darkening, tremor frequency rises ($t \in [T_{cycle} - 120s, T_{cycle}]$).
  2. **Drizzle / Water Accumulation:** Screen blur, light water flow ($t \in [T_{cycle}, T_{cycle} + 30s]$).
  3. **Lethal Downpour:** Heavy kinetic impact particles, instantaneous drowning/crush damage on unshielded voxels ($t > T_{cycle} + 30s$).
  4. **Shelter Logic:** Entrances seal, cycle evaluates creature survival and food consumption.

### 2.2 Creature Relationship Matrix & State Machine
Every inter-species interaction evaluates a baseline static relationship pair $\langle \text{Type}, \text{Intensity} \rangle$:
* **Relationship Types:**
  - `Ignores`: $I = 0.0$. Creature moves past without target evaluation.
  - `Eats`: $I \in [0.1, 1.0]$. Predatory tracking; consumes victim upon incapacitation.
  - `Attacks`: $I \in [0.1, 1.0]$. Territorial defense, competition, or retaliation without nutritional consumption.
  - `Afraid`: $I \in [0.1, 1.0]$. Evades target; pathfinding prioritizes maximum distance / sheltered routes.
  - `StayOutOfWay`: $I \in [0.1, 0.5]$. Yields right-of-way in tight conduits.
  - `Antagonizes`: Harassment behaviors (e.g., Scavengers vs. Lizards).
  - `PlaysWith`: Non-lethal interaction (e.g., Slugcat pups, Cicadas).
* **Dynamic Modulators:**
  - $\text{Agression}(t) = \text{BaseAggression} + \int \Delta_{\text{provocation}} \, dt - \lambda_{calm} \cdot t$
  - $\text{Fear}(t) = \text{BaseFear} \times \left( \frac{\text{AttackerMass}}{\text{SelfMass}} \right) \cdot (1 - \text{HealthRatio})$

### 2.3 Ecosystem Den Routing, Swarm Dynamics & Lineage
* **Trophic Population Capacities (Extracted from `world/su/properties.txt`):**
  - `batsPerActiveSwarmRoom = 7`
  - `batDepleteCyclesMin = 2`, `batDepleteCyclesMax = 7`
  - `batDepleteCyclesMaxIfLessThanTwoLeft = 3`
  - `overseersSpawnChance = 0.8`, `overseersMin = 0`, `overseersMax = 4`
* **Room Creature Preferences:**
  - Rooms enforce spatial weight filters: `Forbidden` (Weight $0.0$), `Avoid` (Weight $0.1$), `Neutral` (Weight $1.0$), `Like` (Weight $3.0$), `Stay` (Weight $10.0$).
* **Lineage Evolution Engine:**
  - When a creature occupying a den is eliminated, the lineage chain rolls an evolutionary replacement on subsequent cycles:
  $$\text{Den}(i, \text{cycle}+1) \sim \text{Categorical}(\{ C_k, P_k \})$$
  - Example: `Pink (0.1) -> NONE (0.5) -> Blue (0.2) -> Red (Apex, 0.0)`.

---

## 3. Dwarf Fortress Technical Specifications (Macro-Geology & Materials)

### 3.1 Material Physical Constants & Tensor Formulas
All solid matter in `Chimera.Core.Macro` is parameterized by physical tensors matching Dwarf Fortress raw standards:
* **Density & Specific Heat:**
  - `SOLID_DENSITY`: Physical density in $kg/m^3$ (e.g., Granite: $2,680$, Iron: $7,850$, Gold: $19,320$, Water: $1,000$).
  - `SPECIFIC_HEAT`: Thermal capacity in $J/(kg \cdot K)$ (e.g., Water: $4,186$, Iron: $450$, Rock: $800$).
* **Temperature Conversion:**
  - Dwarf Fortress Urist Units to Kelvin:
  $$T_K = \frac{T_U - 10000}{1.8} + 273.15$$
  - Freezing point: $10000^\circ U = 273.15\text{ K} = 0^\circ\text{ C}$.
  - Boiling point: $10180^\circ U = 373.15\text{ K} = 100^\circ\text{ C}$.
* **Mechanical Strength Tensors ($kPa = 10^3\text{ N}/m^2$):**
  - `IMPACT_YIELD` & `IMPACT_FRACTURE`: Resistance to kinetic blunt blows and falling voxel impact.
  - `COMPRESSIVE_YIELD` & `COMPRESSIVE_FRACTURE`: Structural integrity under overburden weight.
  - `TENSILE_YIELD` & `TENSILE_FRACTURE`: Resistance to pulling forces and cave-in shear.
  - `SHEAR_YIELD` & `SHEAR_FRACTURE`: Resistance to cutting and blade slicing.
  - `STRAIN_AT_YIELD`: Normalized strain $\times 10^5$. Young's Modulus is computed as:
  $$E = \frac{\text{YIELD}}{\text{STRAIN\_AT\_YIELD}} \times 10^8\text{ Pa}$$

### 3.2 Geological Stratigraphy Hierarchy
Procedural chunk generation must adhere to the 4-tier stratigraphic column:
1. **Soil & Surface Layer:** Humus, sand, clay, silt, peat ($0\text{ to }5\text{ meters}$).
2. **Sedimentary Stratum:** Exclusive home of coal (bituminous coal, lignite), flux stones (limestone, chalk, dolomite), and primary sedimentary iron (hematite, magnetite, limonite).
3. **Metamorphic Stratum:** High temperature/pressure transformation band. Primary source of marble (metamorphic flux stone), quartzite, gneiss, schist, slate.
4. **Igneous Intrusive Stratum (Deep Basal Layer):** Deep plutonic basement rock: granite, diorite, gabbro, orthoclase.
5. **Igneous Extrusive Formations (Volcanic & Surface Intrusions):** Basalt, obsidian, andesite, dacite, rhyolite.

### 3.3 Historical Ledger & Event Contracts
* **Entities:** Civilizations, tribal bands, religions, guilds.
* **Historical Figures (HF):** Lineage ID, birth/death ticks, personality sphere affinities, combat victories, titles.
* **Historical Events:**
  - `FieldBattle`, `SiteConquest`, `ArtifactCreated`, `DiplomaticTruce`, `CreatureSighting`, `DynastyExtinction`.
