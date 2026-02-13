# Repository Guidelines

## Project Structure & Module Organization
This is a Unity project. Core code and assets live under `Assets/`, with configuration in `ProjectSettings/` and packages in `Packages/`.
- `Assets/Scripts/RobotProgramming/` contains the main runtime code, grouped by `Core/`, `Commands/`, `Data/`, `Robot/`, `Execution/`, `UI/`, and `Managers/`.
- `Assets/Scenes/` includes playable scenes such as `GameScene.unity` and `Test.unity`.
- `Packages/com.codeblocks.robotprogramming/` holds the package version of the system and sample content.
- Project documentation and setup guides are under `.Doc/` (see `.Doc/ProjectStructure.md` and `.Doc/QuickSetup.md`).

## Build, Test, and Development Commands
Most development happens inside the Unity Editor. For a quick compile outside Unity:
```powershell
dotnet build Assembly-CSharp.csproj
dotnet build Assembly-CSharp-Editor.csproj
```
Open the project in Unity and press Play in `Assets/Scenes/GameScene.unity` to run the main flow. Use `Test.unity` for quick checks.

## Coding Style & Naming Conventions
Use standard Unity C# conventions:
- 4-space indentation.
- `PascalCase` for types and public members.
- `camelCase` for locals and private fields.
- Interfaces use the `I` prefix (e.g., `ICommand`).
Public APIs typically include XML doc comments; follow existing patterns in `Assets/Scripts/RobotProgramming/`.

## Testing Guidelines
No dedicated automated test runner is configured in `Packages/manifest.json`. There are ad-hoc test scripts like `Assets/Scripts/GameManagerAPITest.cs` and `Packages/com.codeblocks.robotprogramming/Runtime/Managers/LevelRuntimeManagerTest.cs`. Prefer manual validation in Play Mode and follow checklists in `.Doc/TESTING_QUICKSTART.md` when available.

## Commit & Pull Request Guidelines
Commit messages are short and imperative (e.g., “Fix…”, “Add…”, “Refactor…”). Occasional prefixes like `chore:` and versioned releases (e.g., “Release v1.0.4: …”) appear in history—use the same style.
For PRs, include:
- A concise summary of changes.
- Test steps or scene(s) exercised.
- Screenshots or short clips for UI/UX changes.
- Linked issues if applicable (see `.Doc/Issues.md`).

## Agent-Specific Instructions
- Chat language: Russian.
- Repository artifacts: code, comments, and strings must be in English; documentation and instructions in `.md` should be in Russian for new/updated content.
- Avoid editing Cyrillic text using Python scripts in this repo; use normal file edits (e.g., apply_patch) to prevent encoding issues.
- Do not create commits unless explicitly requested. Do not delete files without asking first.
- Use the task system: check `.Doc/Issues.md` for status and `.Doc/Tasks/` for detailed plans. When planning changes, read the relevant code before proposing a plan.

## Configuration Tips
Unity-generated folders like `Library/`, `Temp/`, and `Logs/` should not be committed. Keep new assets under `Assets/` and update corresponding `.meta` files.
