# CodeBlocks Robot Programming

Visual block-based programming system for robot control with drag-and-drop interface, snap connections, and loop blocks.

## Features

- **BlockUIBase Architecture** — unified connector system with Dictionary-based access
- **Drag & Drop** — intuitive block placement with visual snap feedback
- **Loop Blocks** — dynamic sizing with 4 connectors (external input/output + internal flow)
- **Snap Manager** — simplified API with automatic connector detection
- **Promise-based Execution** — sequential command execution without callback hell
- **Level Editor** — visual editor with JSON export/import
- **5 Tutorial Levels** — ready-to-use examples

## Installation

### Option 1: Git URL (Recommended)

**HTTPS (public access):**
1. Open Unity Package Manager
2. Click `+` → `Add package from git URL`
3. Enter:
   ```
   https://github.com/mikkiducher/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v1.0.10
   ```

**SSH (with configured SSH key):**
```
git@github.com:mikkiducher/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v1.0.10
```

### Option 2: With Sample Assets (Recommended)

1. **Add package via UPM** (as above)
2. **Import Samples** (optional):
   - Open Package Manager → "CodeBlocks Robot Programming"
   - Click "Samples" tab
   - Click "Import" next to "Sample Assets"
   - Assets will be copied to `Assets/CodeBlocks/`

**What's included in samples:**
- UI Prefabs (BlockUI, LoopBlockUI, ProgramArea, BlockPalette)
- Robot Prefabs
- LevelEditor Prefabs (Terrain, Objects)
- Demo Levels (5 tutorial levels as JSON)
- Materials and Sprites
- Resource configurations (RobotConfig, LevelEditor configs)

### Option 3: Local Package

Add to `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.codeblocks.robotprogramming": "https://github.com/mikkiducher/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming"
  }
}
```

## Requirements

- Unity 6000.0 or later
- TextMeshPro 4.0.0-pre.2+
- UGUI 2.0.0+
- **Promises Library** (IPromise, Deferred, Timers) — must be present in your project

### ⚠️ Important: Promises Library

This package requires a custom **Promises** library that is **NOT included** to avoid conflicts with your existing installation.

**Required classes:**
- `IPromise`, `IPromise<T>`, `IPromise<T1, T2>`
- `Deferred`, `Deferred<T>`, `Deferred<T1, T2>`
- `Timers` (MonoBehaviour singleton)

**Where to get:**
- If migrating from TestCodeBlock: Keep your `Assets/Scripts/Promises/` folder
- If starting fresh: Copy `Promises/` folder from the TestCodeBlock repository

**Assembly Definition:**
- Your Promises must have an assembly definition named `Promises`
- The CodeBlocks.Runtime.asmdef references this assembly

## Project Structure (Hybrid)

```
Packages/com.codeblocks.robotprogramming/  ← Scripts only (auto-updates)
Assets/CodeBlocks/                         ← Assets (prefabs, levels, configs)
```

**Note:** This package contains only scripts. Assets (prefabs, levels) should be copied separately to `Assets/CodeBlocks/` in your project.

## Quick Start

### 1. Setup Scene

```
Hierarchy:
- Canvas
  - ProgramArea (from Assets/CodeBlocks/Prefabs/UI/)
  - BlockPalette (from Assets/CodeBlocks/Prefabs/UI/)
  - Controls (from Assets/CodeBlocks/Prefabs/UI/)
- Robot
  - (Your robot GameObject)
```

### 2. Create Block Factory

```csharp
using CodeBlocks.UI;
using CodeBlocks.Robot;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BlockFactory blockFactory;
    [SerializeField] private ProgramArea programArea;
    [SerializeField] private RobotController robotController;

    void Start()
    {
        programArea.SetBlockFactory(blockFactory);
    }
}
```

### 3. Run Program

```csharp
public void ExecuteProgram()
{
    var executor = new CommandExecutor(robotController);
    var firstBlock = programArea.GetFirstBlock();

    if (firstBlock != null)
    {
        executor.Execute(firstBlock)
            .Done(() => Debug.Log("Program completed!"))
            .Fail(error => Debug.LogError($"Error: {error}"));
    }
}
```

## Architecture

### Block Types

- **BlockUI** — simple command blocks (Move, Turn, Wait)
- **LoopBlockUI** — loop blocks with 4 connectors

### Key Classes

| Class | Purpose |
|-------|---------|
| `BlockUIBase` | Abstract base for all blocks |
| `BlockDragHandler` | Drag & drop functionality |
| `SnapManager` | Snap detection and application |
| `CommandExecutor` | Promise-based execution |
| `RobotController` | Robot movement with lerp animation |

### Connector System

```csharp
// Get connector by name
var input = block.GetConnector(BlockUIBase.INPUT);
var output = block.GetConnector(BlockUIBase.OUTPUT);

// Iterate connectors
foreach (var connector in block.GetAllConnectors())
{
    // ...
}

// Primary connectors (polymorphic)
var primaryIn = block.GetPrimaryInput();   // virtual
var primaryOut = block.GetPrimaryOutput(); // virtual
```

## Level Editor

### Create Level

1. Open: `Window → CodeBlocks → Level Editor`
2. Create: `Create → CodeBlocks → Level Grid Data`
3. Edit in Scene View
4. Export: `Export to JSON`

### Load Level

```csharp
var level = Resources.Load<LevelGridData>("Levels/tutorial_01");
if (level != null)
{
    // Use level data
}
```

## Documentation

- [MIGRATION_GUIDE_HYBRID.md](MIGRATION_GUIDE_HYBRID.md) — How to migrate from Assets to Package
- [PRIVATE_REPO_GUIDE.md](PRIVATE_REPO_GUIDE.md) — Using with private repositories
- [CHANGELOG.md](CHANGELOG.md) — Version history

## License

See [LICENSE](https://github.com/mikkiducher/TestCodeBlock/blob/master/LICENSE)

## Support

- Issues: https://github.com/mikkiducher/TestCodeBlock/issues
- Repository: https://github.com/mikkiducher/TestCodeBlock

## Version

Current: **1.0.2**

See [CHANGELOG.md](CHANGELOG.md) for version history.
