# Universal Slider & Mathematical Parameter Architecture

## Core Philosophy
Every entity, material, particle, spell, and social interaction in Project Chimera is defined by **continuous mathematical sliders** rather than discrete hardcoded states. 
Systems modify these values dynamically via direct simulation calls, instantly updating physics, behaviors, and visual presentation without requiring new graphical assets.

---

## 1. Material & Physical Properties
Materials exist along continuous physical dimensions:
* **`Density` ($kg/m^3$):** Determines mass, buoyancy in liquids/air, and structural weight.
* **`Rigidity` / `Young's Modulus` [0.0 to 1.0]:** Resistance to deformation under stress.
* **`Brittleness` [0.0 to 1.0]:** Probability of catastrophic fracturing vs bending.
* **`Malleability` [0.0 to 1.0]:** Capacity for plastic deformation when worked or hammered.
* **`Thermal Conductivity` [0.0 to 1.0]:** Heat transfer rate across adjacent voxels.
* **`Melting & Boiling Points` ($K$):** Phase transitions calculated per tick based on ambient and local temperature.

---

## 2. Physiological & Evolutionary Pressures
Creatures scale dynamically through continuous biological sliders:
* **`Body Size` [0.1 to 5.0]:** Directly scales mass, physical attack force, constitution, and movement inertia.
* **`Metabolic Rate` [0.5 to 3.0]:** Dictates energy consumption rate, hunger tick frequency, body heat output, and sprint endurance.
* **`Generational Size Loop`:**
  - Caloric deficit during reproductive maturity reduces offspring `body_size` slider.
  - Caloric abundance allows upward size mutation.
  - Hypertrophy penalty: oversized individuals face exponential caloric requirements, acting as a natural evolutionary ceiling.
* **`Bioluminescence Intensity` [0.0 to 1.0]:** Emission intensity sent directly to shader uniform and lighting grid.
* **`Chromatic Adaptation Rate` [0.0 to 1.0]:** Rate at which creature palette shifts toward background voxel color.

---

## 3. Unified Reaction Matrix & Spell Mechanics
Spells and environmental interactions are not arbitrary scripts; they are **thermodynamic and kinetic force vectors**:
* **Thermal Spectrum ($\Delta T$):**
  - High positive $\Delta T$ = Ignition, combustion, fire particles (red -> orange -> white-hot).
  - High negative $\Delta T$ = Freezing, crystallization, ice particles (cyan -> deep blue).
* **Kinetic Vector ($\vec{F}$):** Force applied to surrounding entities and destructible voxels (blast shockwave, suction, or concussive stun).
* **Status Vectors:** Continuous sliders for `acoustic_damping` (silence/whisper), `neural_disruption` (confusion/stun), and `photonic_flux` (illumination/flashbang).
* **Environmental Dampening:** Ambient humidity and rain attenuate thermal transfer and extinguish combustion while accelerating electrical/cold conduction.

---

## 4. Social & Ideological Affinity Math
Inter-pawn relationships are calculated as a continuous weighted sum:
$$\text{Affinity}(A, B) = \sum_{i} w_i \cdot \text{Factor}_i$$
* **Religious Harmony / Heresy:** $+1.0$ (same faith) to $-1.0$ (opposing dogmas).
* **Clan / Family Feud:** Inherited negative bias spanning generations.
* **Cultural / Sports Alignment:** $+0.2$ to $-0.2$ casual bonding or rivalry.
* **Stress & Neuroticism Multiplier:** High neuroticism scales negative opinion shifts, lowering mental break thresholds.
