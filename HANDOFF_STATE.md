# Project Chimera — Current Handoff State

**Current Date / Session:** Initial Reset & Architecture Setup Complete  
**Engine Baseline:** Godot 4 (.NET C#) + Pure C# Core (.NET 8.0)  
**Current Phase:** Phase 1: Macro-Simulation Core (Dwarf Fortress Foundation)

---

## 1. Accomplishments & Current Status
- [x] **Workspace Cleaned:** Legacy Three.js prototype completely purged.
- [x] **Day-1 Git Repository Initialized:** Baseline and modular solution committed (`master`).
- [x] **Toolchains Installed & Configured in `C:\Users\para-\chimera_tools\`:**
  - **Git 2.56.0** (Portable MinGit)
  - **.NET 8.0 SDK (8.0.427)**
  - **Godot Engine 4.7.2 Mono (.NET build)**
  - **Blockbench 5.2.2 Portable**
  - **Ollama Runner** (Background setup in progress)
- [x] **Decoupled Architecture Scaffolding (`Chimera.sln`):**
  - `src/Chimera.Core`: Pure C# (.NET 8) simulation backend (ZERO Godot dependencies).
  - `src/Chimera.Presentation.Godot`: Godot 4 C# frontend (renders voxel world & shaders).
  - `src/Chimera.Tests`: xUnit determinism and logic test suite.
  - **Build & Test Verification:** 100% compilation and passing test suite (`dotnet test`).
- [x] **Subagents Configured:**
  - `sim-architect`: Focuses on Phase 1 DF simulation, determinism, data dictionaries, history ledger.
  - `genetics-ecology`: Focuses on Phases 2 & 3 RimWorld psychology, traits, junk DNA, Rain World food webs.
  - `godot-presentation`: Focuses on Phase 4 Godot nodes, shaders, greedy meshing, voxel presentation.
- [x] **Continuous Handoff Protocol Active:** [`HANDOFF_PROTOCOL.md`](HANDOFF_PROTOCOL.md) established.

---

## 2. Invariants & Rules
1. **Model-View Decoupling:** `Chimera.Core` MUST NEVER reference any Godot classes (`Node`, `Vector3`, `ShaderMaterial`, etc.).
2. **Local Memory Integration:** Presentation communicates with Simulation via direct C# interface/method calls in local memory (zero serialization overhead).
3. **Multi-threading:** Simulation ticks run asynchronously on worker threads; presentation reads double-buffered snapshots or thread-safe state views.

---

## 3. Next Immediate Roadmap (Phase 1 Execution)
1. **Define Core Contracts & Data Dictionaries (`Chimera.Core/Macro/`):**
   - Material registry (densities, melting points, mechanical strengths).
   - Seeded deterministic noise terrain & resource grid generation.
   - World history ledger and deterministic event logging.
2. **Determinism Verification (`Chimera.Tests`):**
   - Multi-tick test ensuring identical state hashes across 10,000 tick steps from the same seed.

---

## 4. Prompt For Next Chat Session
*(When switching to a fresh chat or handoff)*
```
I am continuing development on Project Chimera. Please read HANDOFF_STATE.md and HANDOFF_PROTOCOL.md in the root workspace to sync your context. Verify the current build with `dotnet test Chimera.sln` and proceed with the next task in the roadmap.
```
