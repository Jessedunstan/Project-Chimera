# Project Chimera — Conversation Lifecycle & Handoff Protocol

## 1. Purpose & Core Principle
Conversations with LLM assistants have an optimal context window. As conversations stretch past ~30-40 turns or accumulate extensive code and logs, context degradation ("drift") can cause hallucinations, forgotten invariants, or regression of previously verified code.

To prevent this, Project Chimera operates under a **Strict Continuous Handoff Protocol**:
1. When approaching context boundaries or finishing a key phase milestone, the AI initiates an immediate Handoff State Snapshot.
2. The user is prompted to archive the current conversation and open a fresh session.
3. The new conversation boots with the state snapshot and resumes development without context loss.

---

## 2. Conversation "Best-By" Trigger Indicators
A conversation reaches its Best-By date when any of the following occur:
- **Milestone Completion:** A distinct phase module (e.g., Phase 1 Macro-Sim Foundation) is fully implemented and passes all unit tests.
- **Turn Density:** 25–35 conversational turns have elapsed with multi-file modifications.
- **Context Compaction Signal:** When tool execution history or debug logs begin dominating context memory.

---

## 3. The 3-Step Handoff Execution Sequence

### Step 1: Snapshot Generation
The active AI generates/updates `HANDOFF_STATE.md` at the root of the workspace. This file must contain:
* Active Project Phase & Objective
* Completed Architectural Components
* In-Flight Tasks & Next Immediate Actions
* System Invariants & Unbreakable Rules (e.g. Model-View separation, zero Godot dependencies in `Chimera.Core`)
* Testing Status & Verification Commands
* Ready-to-use Prompt for the Next Session

### Step 2: User Notification
The AI notifies the user with a formatted alert:
> 📦 **CONVERSATION HANDOFF READY**
> This chat has reached its optimal milestone threshold. `HANDOFF_STATE.md` has been compiled. Please open a new chat in this workspace and paste the prompt below.

### Step 3: Fast-Booting the Next Chat
In the new conversation, the user pastes the handoff prompt. The new AI reads `HANDOFF_STATE.md`, runs the test verification command, and begins execution with zero friction.
