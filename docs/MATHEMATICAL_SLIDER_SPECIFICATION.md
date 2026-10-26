# Universal Mathematical Slider Specification & Parameter Schemas
*Project Chimera Mathematical Sim Shaping Engine*

## Overview
Project Chimera eliminates arbitrary discrete state flags in favor of **continuous mathematical sliders** and **differential transfer functions**. Every entity, voxel, reaction, and social bond exists along calibrated physical and continuous scales.

All sliders are mapped to a strongly typed 16-bit identifier (`SliderId : ushort`) to enforce zero-allocation, deterministic execution across all micro-ticks (60 Hz), simulation-ticks (1 Hz), and macro-ticks (cycles).

---

## 1. Parameter Identification & Memory Architecture

### Schema Identifier: `SliderId : ushort`
```csharp
public enum SliderId : ushort
{
    // Physical Materials (1 - 99)
    MaterialDensity = 1,
    MaterialRigidity = 2,
    MaterialBrittleness = 3,
    MaterialMalleability = 4,
    MaterialThermalConductivity = 5,
    MaterialSpecificHeatCapacity = 6,
    MaterialMeltingPoint = 7,
    MaterialBoilingPoint = 8,
    MaterialFrictionCoefficient = 9,
    MaterialTensileYield = 10,

    // Biological & Generational Physiology (100 - 199)
    BioBodySize = 100,
    BioMetabolicRate = 101,
    BioBioluminescenceIntensity = 102,
    BioChromaticAdaptationRate = 103,
    BioCaloricIntakeRatio = 104,
    BioGenerationalGrowthFactor = 105,
    BioMutationRate = 106,
    BioActivityLevel = 107,
    BioBaseMass = 108,
    BioHypertrophicPenaltyCoeff = 109,

    // Thermodynamics & Spell Reaction Matrix (200 - 299)
    ThermoTemperatureDelta = 200,
    ThermoForceVectorX = 201,
    ThermoForceVectorY = 202,
    ThermoForceVectorZ = 203,
    ThermoAcousticDamping = 204,
    ThermoNeuralDisruption = 205,
    ThermoPhotonicFlux = 206,
    ThermoAmbientHumidity = 207,
    ThermoFuel = 208,
    ThermoOxidant = 209,
    ThermoThermalInertia = 210,
    ThermoCombustionActivationEnergy = 211,

    // Social Opinion & Ideological Math (300 - 399)
    SocialReligiousCompatibility = 300,
    SocialClanFeudScore = 301,
    SocialSportsRivalryAlignment = 302,
    SocialNeuroticismEvaluator = 303,
    SocialDevotionEvaluator = 304,
    SocialTribalismEvaluator = 305,
    SocialIronWillEvaluator = 306,
    SocialOpinionBaseline = 307,
    SocialDecayedMemorySum = 308,
    SocialMoodBreakMinorThreshold = 309,
    SocialMoodBreakMajorThreshold = 310,
    SocialMoodBreakExtremeThreshold = 311
}
```

---

## 2. Mathematical Models & Formulas

### 2.1 Physical Material Properties

#### Continuous Sliders:
* **`Density` ($\rho$):** $[1.0, 30000.0]\, kg/m^3$
* **`Rigidity` ($R$):** $[0.0, 1.0]$ (Resistance to elastic deformation)
* **`Brittleness` ($B$):** $[0.0, 1.0]$ (Tendency toward fracture vs bending)
* **`Malleability` ($M$):** $[0.0, 1.0]$ (Capacity to absorb plastic strain)
* **`Thermal Conductivity` ($k$):** $[0.0, 1.0]$ (Normalized to $400\, W/(m\cdot K)$)
* **`Specific Heat Capacity` ($c_p$):** $[50.0, 5000.0]\, J/(kg\cdot K)$
* **`Phase Points` ($T_{melt}, T_{boil}$):** Kelvin ($K$)

#### Governing Equations:
1. **Interface Thermal Conductance (Harmonic Mean):**
   $$k_{eff} = \frac{2 \cdot k_1 \cdot k_2}{k_1 + k_2 + \epsilon}$$
   $$Q = k_{eff} \cdot \frac{A_{contact}}{d} \cdot (T_B - T_A) \cdot \Delta t$$
   $$\Delta T = \frac{Q}{\text{Mass} \cdot c_p}$$

2. **Mechanical Stress & Fracture Solver:**
   $$\sigma_{yield} = \text{TensileYield} \cdot (1.0 + 0.5 \cdot R)$$
   $$\Delta \sigma = \max(0, \sigma_{applied} - \sigma_{yield})$$
   $$P_{fracture} = \min\left(1.0, \frac{\Delta \sigma}{\sigma_{yield} \cdot (1.0 - B + 0.01)}\right) \cdot B$$
   $$\epsilon_{plastic} = \frac{\Delta \sigma}{\sigma_{yield}} \cdot M \cdot (1.0 - B)$$

3. **Continuous Phase State Function:**
   $$S_{phase}(T) \in [0.0, 2.0]$$
   - $[0.0 \dots 1.0]$: Smooth transition through $T_{melt} \pm \Delta T_{trans}$ (solid $\to$ liquid slush).
   - $[1.0 \dots 2.0]$: Smooth transition through $T_{boil} \pm \Delta T_{trans}$ (liquid $\to$ vapor).

---

### 2.2 Biological & Generational Metabolism

#### Continuous Sliders:
* **`BodySize` ($S$):** $[0.1, 5.0]$
* **`MetabolicRate` ($R_m$):** $[0.5, 3.0]$
* **`BioluminescenceIntensity` ($\lambda$):** $[0.0, 1.0]$
* **`CaloricIntakeRatio` ($E_{ratio}$):** $[0.0, 3.0]$ ($\frac{\text{Consumed}}{\text{Required}}$)

#### Governing Equations:
1. **Allometric Scaling:**
   $$\text{Mass}(S) = M_{base} \cdot S^3$$
   $$\text{SurfaceArea}(S) = A_{base} \cdot S^2$$
   $$\text{AttackForce}(S, R_m) = F_{base} \cdot S^3 \cdot \sqrt{R_m}$$
   $$\text{Agility}(S, R_m) = a_{base} \cdot \frac{R_m}{S}$$

2. **Kleiber's Basal Metabolic Rate & Hypertrophic Penalty:**
   $$BMR(S, R_m) = C_{base} \cdot R_m \cdot S^{0.75}$$
   $$\text{HypertrophyTax}(S) = \max(0, S - 1.0)^{1.5} \cdot k_{hyper} \cdot C_{base}$$
   $$\text{TotalDemandPerTick} = \frac{(BMR + \text{HypertrophyTax} + \lambda \cdot C_{lum}) \cdot (1.0 + \text{Activity})}{86400} \cdot \Delta t$$

3. **Generational Evolution Loop:**
   $$S_{offspring} = \text{clamp}\left(S_{parent} \cdot \left(1.0 + \alpha_{growth} \cdot \tanh(E_{ratio} - 1.0)\right) + \mathcal{N}(0, \sigma^2), 0.1, 5.0\right)$$
   - Caloric deficit ($E_{ratio} < 1.0$) shrinks the next generation.
   - Caloric abundance ($E_{ratio} > 1.0$) drives gigantism.
   - The non-linear hypertrophy tax acts as an ecological carrying ceiling, starving oversized populations during famine.

---

### 2.3 Thermodynamic Spell & Reaction Matrix

#### Continuous Sliders:
* **`TemperatureDelta` ($\Delta T$):** $[-1000.0, +3000.0]\, K$
* **`ForceVector` ($\vec{F}$):** $(F_x, F_y, F_z)$
* **`AcousticDamping` ($\alpha$):** $[0.0, 1.0]$
* **`NeuralDisruption` ($\nu$):** $[0.0, 1.0]$
* **`PhotonicFlux` ($\Phi$):** $[0.0, 1.0]$
* **`AmbientHumidity` ($H$):** $[0.0, 1.0]$

#### Governing Equations:
1. **Unified Thermal Spectrum ($\Delta T$):**
   - Fire and Ice exist on the same continuous temperature axis.
   - Humidity attenuation:
     $$T_{new} = T_{cur} + \frac{\Delta T \cdot \left(\Delta T \ge 0 ? (1 - 0.65 H) : (1 + 0.50 H)\right)}{\text{ThermalInertia}}$$
2. **Arrhenius Combustion Reaction:**
   $$R_{combust} = A \cdot \exp\left(-\frac{E_a}{R_{gas} \cdot T}\right) \cdot [\text{Fuel}] \cdot [\text{Oxidant}] \cdot (1.0 - 0.85 H)$$
3. **Continuous Planckian / Cryogenic Particle Color Mapping ($T \to RGBA$):**
   - $T \le 150K$: Deep cryogenic violet-blue `(0.10, 0.15, 0.90)`.
   - $150K < T \le 273.15K$: Sub-zero cyan frost `(0.20, 0.65, 1.00)`.
   - $273.15K < T \le 450K$: Ambient gray smoke `(0.55, 0.45, 0.35)`.
   - $450K < T \le 850K$: Cherry smolder `(0.90, 0.15, 0.02)`.
   - $850K < T \le 1600K$: Fiery orange `(1.00, 0.55, 0.05)`.
   - $1600K < T \le 2800K$: Incandescent yellow-white `(1.00, 0.98, 0.50)`.
   - $T > 2800K$: Hyper-thermal arc plasma blue-white `(0.70, 0.85, 1.00)`.
4. **Status & Field Vectors:**
   $$\tau_{stun} = \frac{|\vec{F}| \cdot \Delta t \cdot 0.05 + \nu \cdot 3.5}{(M_{target} + \text{Constitution} \cdot 50) \cdot 0.015}$$
   $$R_{sound} = R_0 \cdot \sqrt{|\vec{F}|} \cdot (1 - \alpha)$$
   $$\tau_{blind} = \Phi \cdot \tau_{max} \cdot \frac{1}{\max(1, d)} \cdot (1 - \text{Resistance})$$

---

### 2.4 Social & Ideological Opinion Math

#### Continuous Sliders:
* **`ReligiousCompatibility`:** $[-1.0, +1.0]$ (+1 harmony, -1 heresy)
* **`ClanFeudScore`:** $[-1.0, 0.0]$ (-1 mortal blood feud, 0 peace)
* **`SportsRivalryAlignment`:** $[-0.2, +0.2]$ (+0.2 ally, -0.2 rival)
* **`Neuroticism` ($N$):** $[0.0, 1.0]$
* **`IronWill` ($W$):** $[0.0, 1.0]$

#### Governing Equations:
1. **Weighted Affinity Sum:**
   $$\text{RawOpinion}(A, B) = w_{rel} \cdot \Delta_{rel} \cdot D_{rel} + w_{feud} \cdot \Gamma_{tribal} \cdot G_{feud} + w_{sport} \cdot S_{sport} + \sum \text{DecayedMemories}$$
2. **Neuroticism Multiplier:**
   $$\text{FinalOpinion} = \begin{cases}
   \text{clamp}(\text{RawOpinion} \cdot (1.0 + 1.2 \cdot N_A), -100, 100) & \text{RawOpinion} < 0 \\
   \text{clamp}(\text{RawOpinion} \cdot (1.0 - 0.25 \cdot N_A), -100, 100) & \text{RawOpinion} \ge 0
   \end{cases}$$
3. **Generational Feud Decay:**
   $$G_{offspring} = G_{parent} \cdot \exp(-\lambda_{feud} \cdot \text{Generations})$$
4. **Mental Break Thresholds:**
   $$\text{ThresholdModifier} = (1.0 + 0.45 \cdot N_A) \cdot (1.0 - 0.40 \cdot W_A)$$
   - Minor: $0.35 \times \text{ThresholdModifier}$
   - Major: $0.20 \times \text{ThresholdModifier}$
   - Extreme: $0.05 \times \text{ThresholdModifier}$

---

## 3. REA Value Ingestion Readiness

The mathematical engine is fully wired and verified to receive parameter tables extracted by REA:
* **Dwarf Fortress Raws:** `IMPACT_YIELD`, `COMPRESSIVE_YIELD`, `SOLID_DENSITY`, `SPECIFIC_HEAT`, `MELTING_POINT` map directly to `Material*` sliders.
* **RimWorld XMLs:** `Thoughts_Situation_General.xml`, `Thoughts_Memory_Social.xml`, `MentalBreakDefs` map directly to `Social*` sliders and decay constants.
* **Rain World Assemblies:** `CreatureTemplate.Relationship`, `AbstractCreature`, trophic level caloric demands map directly to `Bio*` allometric sliders and `Thermo*` vector impacts.
