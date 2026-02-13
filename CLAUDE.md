# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 6.0.2.6f2 project configured for PC/Windows standalone development with support for mobile platforms. The project uses the Universal Render Pipeline (URP) for graphics and includes Visual Scripting capabilities.

**Key Technologies:**
- Unity Version: 6.0.2.6f2
- Render Pipeline: Universal Render Pipeline (URP)
- Input System: New Input System (1.14.2)
- Visual Scripting: Enabled (1.9.7)
- Target Platforms: Standalone (Windows) with Mobile support configured

## Project Structure

```
Assets/
├── Scenes/               # Game scenes
├── Settings/             # URP renderer & graphics settings
├── TutorialInfo/         # Tutorial and editor UI components
└── Editor/               # Editor-only assets and tools

ProjectSettings/          # Unity project configuration
Packages/                 # Package manifest and dependencies
```

## Development Commands

### Opening the Project
Open the project in Unity Editor using the solution file or directly through Unity Hub:
```
TestCodeBlock.sln
```

### Compiling C# Code
- **In Editor:** Code compiles automatically when files are saved
- **Command Line:** Use `dotnet` or Visual Studio to compile the `.csproj` files:
  ```
  dotnet build Assembly-CSharp.csproj
  dotnet build Assembly-CSharp-Editor.csproj
  ```

### Running the Project
- **Play Mode:** Press Play in the Unity Editor
- **Standalone Build:** Build → Build Settings → Build and Run in Unity Editor

## Code Architecture

### Assembly Organization
The project uses Unity's default assembly setup:
- **Assembly-CSharp.asmdef:** Runtime scripts (compiled from `Assets/` excluding Editor folders)
- **Assembly-CSharp-Editor.asmdef:** Editor-only scripts (from `Assets/*/Editor` folders)

### C# Language Features
- C# Language Version: 9.0 (.NET 4.7.1 equivalent)
- Unsafe code blocks are disabled
- Warnings treated as warnings (not errors)

### Core Systems

#### Promises Library (`Assets/Scripts/Promises/`)
The foundation for the block-based command execution system. Implements a JavaScript-style Promise pattern with support for deferred resolution/rejection.

**Key Components:**
- **IPromise/IPromise<T>/IPromise<T1,T2>:** Interface contracts for promise behavior. Support method chaining through `Done()`, `Fail()`, `Always()`, and `Then()`.
- **BaseDeferred:** Abstract base implementing core promise state machine (Pending → Resolved/Rejected). Manages callback queues and state transitions.
- **Deferred/Deferred<T>/Deferred<T1,T2>:** Concrete implementations with object pooling to minimize GC allocation. Support static factory methods (`Resolved()`, `Rejected()`, `All()`, `Race()`, `Sequence()`).
- **Timers:** MonoBehaviour singleton managing time-based promises. Supports `Wait()` (scaled), `WaitUnscaled()`, `WaitForTrue()`, and main-thread action queuing for thread safety.

**Usage Pattern:** Promises enable sequential command execution through chaining: `promise.Then(() => nextPromise()).Done(() => onComplete())`. Each block in the UI command system returns a promise representing command completion.

#### Block-Based Command System (In Development)
Architecture for visual programming robot control:
- **Block Structure:** Each block represents a command, selected from UI and stored in a linked-list-like structure.
- **Command Flow:** Block → Controller receives command + reference to next block(s) → Command executes → On completion, calls next block via promise chain.
- **Selection Handling:** Blocks with multiple options pass selection state to controller alongside the command.
- **Promise Integration:** Promises handle asynchronous command execution and sequential chaining without callbacks hell.

**Example Flow:**
1. User selects "Move Forward" block
2. Block passes command to robot controller with reference to next block
3. Controller returns promise that resolves when movement completes
4. Promise resolution triggers next block's command execution
5. Chain continues until all blocks processed or error occurs

### Key Classes

**Readme.cs:** ScriptableObject used for displaying tutorial/readme information in the Inspector with configurable sections and links.

**ReadmeEditor.cs:** Custom Editor that renders the Readme asset with formatted text, links, and a removal button. Automatically displays the readme on first load and loads a custom window layout defined in `Assets/TutorialInfo/Layout.wlt`.

## Package Dependencies

**Core Rendering:**
- com.unity.render-pipelines.universal: 17.2.0
- com.unity.render-pipelines.core

**Editor Integration:**
- com.unity.ide.visualstudio: 2.0.23
- com.unity.ide.rider: 3.0.38

**Input & UI:**
- com.unity.inputsystem: 1.14.2
- com.unity.ugui: 2.0.0

**Features:**
- com.unity.feature.mobile: 1.0.0 (Android/iOS support)
- com.unity.visualscripting: 1.9.7
- com.unity.timeline: 1.8.9

**Development Tools:**
- com.unity.test-framework.performance: Performance testing
- com.unity.multiplayer.center: Multiplayer infrastructure
- com.unity.collab-proxy: Unity Collaborate version control

**Standard Modules:** Full set of Unity modules (physics, audio, animation, etc.) are included in `Packages/manifest.json`.

## Project Management

### Task Documentation System
This project maintains a **two-level task tracking approach** for clarity and visibility:

**1. Central Task Registry (`.Doc/Issues.md`)**
- Single source of truth for all project work
- Each entry contains: issue number, title, one-sentence description, status, blockers, and link to detailed plan
- Status markers: `[ ] Pending`, `[→] In Progress`, `[✓] Done`
- Updated to reflect current project state

**2. Detailed Task Documents (`.Doc/Tasks/[Number]_[TaskName].md`)**
- Comprehensive implementation plans for each task
- Contains: Goal, Context, Key Steps (5-7 concrete actions), Blockers & Risks, Acceptance Criteria, Notes
- Prevents scope creep by defining clear success criteria
- Named as: `1_SetupSystem.md`, `2_ImplementBlock.md`, etc.

**Task Workflow:**
- Starting a task: Read `.Doc/Issues.md` for overview, then `.Doc/Tasks/` for detailed plan
- **Planning implementation:** ALWAYS read relevant existing code files BEFORE planning changes. Understand current implementation, then create concrete plan based on real code state. Never plan based only on documentation.
- During implementation: Use TodoWrite tool for actionable subtask lists
- Completing a task: Mark `[✓] Done` in both files
- If blocked: Update Blockers field, do NOT close task—keep it visible

**Documentation Structure:**
```
.Doc/
├── Architecture/           # System design documents
├── Tasks/                  # Detailed task plans (one per task)
├── Issues.md              # Central task registry (brief)
└── [ProjectName]_Guide.md # Quick reference
```

### Git Workflow
- **Commits:** Only create commits when explicitly requested by the user
- Do NOT proactively create commits
- User will ask: "commit this" or "/commit" when they want changes saved
- **File Deletion:** Always check with user before deleting files (can be lost permanently)
- **Safe Deletion:** Use git restore or git revert if file was tracked

### Language Rules - STRICT
- **Chat communication with user:** Russian (Русский) only
- **Code comments:** English only - NO Russian comments in code files
- **Variable/Function names:** English only
- **Documentation files (.md):** Russian for content, English for code blocks

## Important Notes

- The project is configured for Windows Standalone with mobile rendering options available
- Graphics settings support both PC and mobile configurations via `Settings/PC_Renderer.asset` and `Settings/Mobile_Renderer.asset`
- Input is configured to use the New Input System with an `InputSystem_Actions.inputactions` asset
- No custom build scripts or test configurations are present—use Unity Editor for all builds and testing
