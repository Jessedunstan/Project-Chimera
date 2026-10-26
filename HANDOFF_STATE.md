# Project Chimera — Current Handoff State

**Current Date / Session:** Phase 1 Milestone Completed & Verified (Swarm Active)  
**Engine Baseline:** Godot 4.7.2 Mono (.NET C#) + Pure C# Core (.NET 8.0)  
**Current Phase:** Phase 1 Complete $\to$ Ready for Phase 4 Presentation & Voxel Meshing

---

## 1. Verified Architecture Status (47/47 Tests Passing)

All foundational simulation layers are implemented in pure C# (`src/Chimera.Core/`) with zero Godot dependencies, zero-allocation memory pools, and bit-identical discrete tick determinism:

1. **Macro-Simulation Core (Dwarf Fortress Foundation):**
   - [`GeologicalStratigraphyGenerator.cs`](src/Chimera.Core/Macro/Geology/GeologicalStratigraphyGenerator.cs): 4-tier geological strata (Soil $\to$ Sedimentary $\to$ Metamorphic $\to$ Igneous Intrusive/Extrusive) powered by [`DeterministicNoise3D.cs`](src/Chimera.Core/Macro/Geology/DeterministicNoise3D.cs).
   - [`MaterialRegistry.cs`](src/Chimera.Core/Macro/Materials/MaterialRegistry.cs) & [`RawMaterialProperties.cs`](src/Chimera.Core/Macro/Materials/RawMaterialProperties.cs): Bay12 raw yield tensors (Impact, Tensile, Compressive, Shear in $kPa$), Young's modulus, and Urist-to-Kelvin thermal conversions.
   - [`HistoricalLedger.cs`](src/Chimera.Core/Macro/History/HistoricalLedger.cs): Unmanaged zero-allocation chronicle ledger storing historical events using [`FixedString64.cs`](src/Chimera.Core/Macro/History/FixedString64.cs).

2. **Micro-Psychology & Social Webs (RimWorld Layer):**
   - [`ContinuousMoodModel.cs`](src/Chimera.Core/Micro/Psychology/ContinuousMoodModel.cs): Differentiable continuous curves for hunger, rest, recreation, beauty, spaciousness, and masochist pain inversion.
   - [`MentalBreakStateMachine.cs`](src/Chimera.Core/Micro/Mental/MentalBreakStateMachine.cs): Minor (0.35), Major (0.20), and Extreme (0.05) thresholds scaled by Neuroticism, Iron Will, and Pyromania, with recovery catharsis buffers.
   - [`SocialAffinityEvaluator.cs`](src/Chimera.Core/Micro/Social/SocialAffinityEvaluator.cs): Continuous relationship weights ($+1.0$ faith, $+1.0$ clan feud, $-0.2$ sports rivalry, scaled by neuroticism).

3. **Autonomous Ecosystem & Food Web (Rain World Layer):**
   - [`FoodWebEcosystem.cs`](src/Chimera.Core/Ecology/FoodWeb/FoodWebEcosystem.cs): 5-tier trophic hierarchy with logistic biomass growth and batfly swarm depletion cooldowns.
   - [`FearAggressionEvaluator.cs`](src/Chimera.Core/Ecology/Behavior/FearAggressionEvaluator.cs): Sigmoidal mass-ratio fear curves and hunger-driven aggression matrices.
   - [`EnvironmentalHazardCycle.cs`](src/Chimera.Core/Ecology/Cycle/EnvironmentalHazardCycle.cs): 400s-800s countdown, tremor warnings at $T-120s$, flood progression, lethal crushing rain ($>15,000\text{ N}$), and den shelter isolation.
   - [`LineageNode.cs`](src/Chimera.Core/Ecology/Cycle/LineageNode.cs): Evolutionary predator respawn replacement rolls on den occupant deaths.

4. **Universal Slider & Math Engine:**
   - [`SliderId.cs`](src/Chimera.Core/MathEngine/SliderId.cs): Strongly-typed 16-bit enum IDs eliminating GC allocation in micro-tick loops.
   - [`SpellPhysicsMath.cs`](src/Chimera.Core/MathEngine/Thermodynamics/SpellPhysicsMath.cs): Fire and Ice unified thermal axis ($\Delta T$), humidity dampening, Arrhenius reaction kinetics, and Planckian temperature-to-RGBA particle spectrum.
   - [`MetabolismMath.cs`](src/Chimera.Core/MathEngine/Biology/MetabolismMath.cs): Allometric size scaling ($S^3$ mass, $S^2$ surface area), Kleiber's Law, and generational starvation-to-shrinkage feedback loops.

5. **Local AI Narrator & Voice Architecture:**
   - [`docs/NARRATIVE_AND_VOICE_ARCHITECTURE.md`](docs/NARRATIVE_AND_VOICE_ARCHITECTURE.md): Decoupled Triad pattern where Ollama Director sends structured JSON intent to Sim, Sim ticks deterministically, and Piper/XTTS synthesizes voice from verified state changes.

6. **Threading & Presentation Contracts:**
   - [`DoubleBufferedSnapshot.cs`](src/Chimera.Core/Presentation/DoubleBufferedSnapshot.cs): Lock-free atomic snapshot swap (`Interlocked.Exchange`) for Godot rendering without torn reads or frame drops.

---

## 2. Invariants & Guardrails
- **Model-View Decoupling:** `Chimera.Core` contains ZERO references to Godot classes or audio runtimes.
- **Deterministic 60 Hz Micro-Ticks:** Discrete tick indexing (`ulong microTickIndex`), no variable floating-point `deltaTime` accumulation.
- **Data-Oriented Struct Buffers:** Contiguous unmanaged buffers (`Span<T>`), avoiding per-entity polymorphic class overhead.

---

## 3. Next Immediate Roadmap: Phase 4 (3D Voxel Engine & Presentation)
*Note for the incoming presentation chat: The long-term vision is a high-performance 3D voxel sandbox engine (replicating and modernizing the Minecraft Java engine: first/third-person hero controls, destructible voxel blocks, AABB physics, custom chunk shaders) powered by the deep simulation backend.*

1. **Voxel Chunk Meshing & Data Structures (`src/Chimera.Presentation.Godot/Voxel/`):**
   - $16 \times 16 \times 16$ or $32 \times 32 \times 32$ voxel chunk buffers with greedy meshing and face culling.
   - Destructible block editing (digging and placing) synced with material properties.
2. **Multi-Threaded Simulation Synchronization:**
   - Consume immutable `DoubleBufferedSnapshot` render packets from `Chimera.Core` via lock-free atomic swaps.
3. **Custom Visual Shaders:**
   - Real-time Planckian temperature particle/voxel gradient shader.
   - Bioluminescence emission and chromatic adaptation uniforms driven by creature genes.
4. **Hero & Commander Dual-Camera Rig:**
   - Dwarf Fortress Overview commander camera (orbit, zoom, slice Z-levels).
   - Minecraft Hero Mode (first/third-person toggle, WASD, jump, sprint, aim).

---

## 4. Fresh Conversation Bootstrapping Prompt
*(Whenever opening a new chat session to continue)*
```
I am continuing development on Project Chimera. Please read HANDOFF_STATE.md, HANDOFF_PROTOCOL.md, and docs/PRD_MASTER.md to sync your context. Verify the current build with `dotnet test Chimera.sln` (47/47 tests passing) and begin Phase 4: building the 3D Voxel Engine (Minecraft-style chunk greedy meshing, AABB hero controls, and custom shaders) driven by our completed Chimera.Core simulation backend.
```
