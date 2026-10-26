# ⚡ PROJECT CHIMERA: Emergent Voxel Sandbox & Simulation Engine (v3.1)

Project Chimera combines **Dwarf Fortress** (macro-simulation & deep history), **RimWorld** (micro-psychology & social webs), and **Rain World** (autonomous off-screen ecosystems) with a 3D destructible voxel presentation layer powered by **Godot Engine 4 (.NET C#)**.

---

## 🏛️ System Architecture

### 1. Strict Model-View Decoupling
* **`Chimera.Core` (Simulation Backend):** Pure C# (.NET 8) class library without dependencies on Godot scene trees or rendering classes. Manages economic ledgers, deterministic terrain grids, food webs, social webs, and genetic trait inheritance.
* **`Chimera.Presentation.Godot` (Presentation Layer):** Godot 4 C# nodes, multi-threaded rendering integration, greedy voxel meshing, and visual shaders (bioluminescence, chromatic adaptation).
* **Execution Model:** Tightly coupled in local memory (zero network serialization overhead) via direct C# interface/method calls.

### 2. Genetic Library & Morphological Sliders
Every creature possesses continuous morphological sliders, metabolic switches, and dormant **junk DNA** sequences that mutate or activate under environmental and psychological pressures:
* `body_size [0.1 to 5.0]`
* `limb_length [0.5 to 2.0]`
* `jaw_structure [0.0 to 1.0]`
* `bioluminescence_intensity [0.0 to 1.0]`
* `chromatic_adaptation_rate [0.0 to 1.0]`
* `metabolic_rate [0.5 to 3.0]`
* `toxin_secretion [0.0 to 1.0]`
* `neuroticism [0.0 to 1.0]`
* `sociability [0.0 to 1.0]`

---

## 🛠️ Toolchains & Environment
Installed in `C:\Users\para-\chimera_tools\`:
* **Git:** Version control & repository tracking
* **.NET 8 SDK:** Multi-threaded C# simulation compiler
* **Godot 4 (.NET / Mono):** 3D voxel renderer & presentation engine
* **Blockbench Portable:** Low-poly cuboid creature & item modeling
* **Ollama Portable:** Local LLM narrator & story pacing

---

## 🔄 Conversation Lifecycle Protocol
See [`HANDOFF_PROTOCOL.md`](HANDOFF_PROTOCOL.md) and [`HANDOFF_STATE.md`](HANDOFF_STATE.md) for context management, handoff triggers, and session continuity.
