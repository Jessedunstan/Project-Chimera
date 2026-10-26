# Project Chimera — Current Handoff State

**Current Date / Session:** Initial Reset & Architecture Setup  
**Engine Baseline:** Godot 4 (.NET C#) + Pure C# Core  
**Current Phase:** Phase 1: Macro-Simulation Core (Dwarf Fortress Foundation)

---

## 1. Architectural Summary & Status
* **Workspace Cleaned:** Legacy Three.js prototype successfully purged.
* **Toolchain Installation:**
  - Git Portable (MinGit)
  - Microsoft .NET 8.0 SDK
  - Godot 4 (.NET / Mono)
  - Blockbench Portable
  - Ollama Runner Portable
* **Subagents Registered:**
  - `sim-architect`: Pure C# macro-simulation, deterministic grids, DF history.
  - `genetics-ecology`: Gene dictionary, trait sliders, junk DNA, food webs.
  - `godot-presentation`: Godot 4 C# nodes, multi-threaded presentation, voxel meshing & shaders.

---

## 2. Invariants & Guardrails
- **Model-View Decoupling:** `Chimera.Core` MUST NOT reference any Godot classes (`Node`, `Vector3`, `ShaderMaterial`, etc.). It must remain 100% pure C# .NET 8.
- **Local Memory Integration:** Presentation communicates with Simulation via direct C# interface calls in local memory (zero network serialization overhead).
- **Multi-threading:** Simulation ticks run asynchronously on worker threads; presentation reads double-buffered snapshots or thread-safe state views.

---

## 3. Next Immediate Roadmap (Phase 1 Execution)
1. Initialize Git repository and commit initial baseline.
2. Generate `Chimera.sln` with `Chimera.Core`, `Chimera.Presentation.Godot`, and `Chimera.Tests`.
3. Build the Macro-Simulation deterministic data structures:
   - Material registry (densities, melting points, mechanical strengths).
   - Biome & resource generation grid (deterministic seeded noise).
   - Historical ledger event system.
4. Add unit test suite in `Chimera.Tests` validating determinism across 10,000 tick steps.

---

## 4. Bootstrapping Prompt For Fresh Conversation
*(Use this prompt whenever opening a new chat session)*
```
I am continuing development on Project Chimera. Please read HANDOFF_STATE.md and HANDOFF_PROTOCOL.md in the root workspace to sync your context. Verify the current build and proceed with the next task in the roadmap.
```
