# Project Chimera: Master Product Requirements Document (PRD v3.1)
*Emergent Voxel Sandbox & Simulation Engine*

## 1. Executive Summary & Philosophy
Project Chimera combines three landmark simulation paradigms:
1. **Dwarf Fortress (Macro):** Deep world history, geological layering, material physical constants, civilization ledgers, deterministic procedural generation.
2. **RimWorld (Micro):** Individual psychology, mood curves, mental break thresholds, social webs, opinion modifiers, storyteller tension pacing.
3. **Rain World (Ecosystem):** Autonomous off-screen food webs, persistent creature fear/aggression memory, territorial dynamics, environmental hazard cycles.

### The Universal Slider & Math Paradigm
**"Everything is a continuous mathematical slider."**  
All game mechanics operate on normalized or physical continuous scales rather than hardcoded discrete states. Changes in size, temperature, luminosity, density, rigidity, aggression, metabolism, or magic do not require new assets—they scale the underlying mathematical functions, particle parameters, and shader uniforms.

---

## 2. Decoupled Architecture & Performance Invariants (Unbreakable Invariants)
* **`Chimera.Core` (.NET 8):** Pure C# simulation backend. ZERO dependencies on Godot scene trees, nodes, or shaders. Runs deterministically and multi-threaded on worker threads.
* **`Chimera.Presentation.Godot`:** Godot 4 C# layer. Renders active voxel chunks, greedy meshes, and shaders (bioluminescence, chromatic adaptation). Reads state via lock-free immutable snapshots (`DoubleBufferedSnapshot<T>`), with zero thread contention and zero torn reads.
* **Discrete Timestep Determinism:** All simulation layers step via fixed discrete ticks (`ulong microTickIndex` @ 60Hz, `ulong simulationTickIndex` @ 1Hz, `ulong macroTickIndex`). Variable floating-point delta times are strictly forbidden in `Chimera.Core`.
* **Zero-Allocation Data-Oriented Design (DoD):** Inner simulation loops must operate over contiguous unmanaged struct buffers (`Span<T>`) without polymorphic virtual dispatch or steady-state heap allocations. Sliders are identified via strongly-typed `SliderId : ushort`.
* **`Chimera.Tests`:** Determinism, unit, and concurrency test suite ensuring cross-machine bit-identical reproducibility.

---

## 3. Sub-Conversation & Agent Swarm Structure
To maintain razor-sharp focus and eliminate context degradation:
1. **`reference-coordinator`**: Central repository for the PRD, universal naming dictionary, roadmap, and cross-team consensus.
2. **`rea-extractor`**: Reverse Engineering Analyst extracting data from local game installations (`RimWorld`, `Rain World`) and online `Dwarf Fortress` technical data.
3. **`df-simulation`**: C# macro-simulation, geology, raw material physics, history generation.
4. **`rimworld-psychology`**: C# micro-psychology, thought workers, mental breaks, social webs.
5. **`rainworld-ecosystem`**: C# autonomous ecosystems, food webs, creature cycles, territory.
6. **`slider-math-engine`**: Mathematical models for continuous sliders, reaction matrices, particle parameters, and metabolic scaling.
7. **`qa-peer-auditor`**: Independent peer review, architecture skepticism, determinism auditing, and technical debt prevention.
8. **`narrator-voice-director`**: Local Ollama narrative director, Whisper STT, Piper/XTTS voice actor pipeline.
