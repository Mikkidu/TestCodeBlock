# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.6] - 2026-01-26

    ### Added
    - InitLevel() API for multiple level loading (#24)

    ### Fixed
    - Memory leaks when switching levels

    ### Changed
    - **All code comments and strings translated to English**
      - Removed all Cyrillic text from 12 source files
      - Translated comments in CodeBlocksLevelEditorWindow, Commands, UI components
      - Translated Debug.Log messages and display names ("Вперёд" → "Forward", "Назад" → "Backward", etc.)
      - All UI status messages now in English ("Программа пуста!" → "Program is empty!", "Выполняется..." → "Executing...")
      - Code now fully complies with English-only policy

## [1.0.5] - 2026-01-23

### Fixed
- **LevelRuntimeManager coordinate system** (#18)
  - Level grid now properly centered at world origin (0, 0, 0)
  - levelContainer positioned at Vector3.zero (world center) instead of levelOrigin
  - levelOrigin calculated correctly: `(-gridWidth/2, 0, -gridHeight/2)` for centering
  - All terrain, objects, and markers positioned relative to world center
  - Background positioned at (0, -0.1, 0) below level center

### Changed
- Gizmos visualization now shows grid bounds centered at world origin
- Start point direction arrow drawn in correct direction
- White cross indicator added to mark world origin in Scene View

### Improved
- Robot spawn positioning (#19 preparation)
  - Robot will correctly position at start point center relative to world origin
  - Grid-to-world coordinate conversion more reliable for runtime operations

## [1.0.4] - 2026-01-23

### Added
- **SetWorldPosition(Vector3)** - Public method for proper block positioning in local coordinates
  - Automatically converts world position to local parent coordinates using RectTransformUtility
  - Works correctly with nested UI hierarchies (Loop containers, nested ProgramArea)
  - No parameters required - retrieves RectTransform, parent, and Canvas automatically
  - Caching ready for future optimizations

### Fixed
- **Block positioning in nested UI containers** (#23)
  - AlignToInputConnection() now uses SetWorldPosition() for correct local coordinate conversion
  - ApplySnap() converts coordinates properly when inserting blocks into chains
  - ApplySnapToInput() handles coordinate conversion for start-of-chain insertion
  - SetParent calls now use correct false parameter after SetWorldPosition establishes local coordinates
  - Blocks position correctly in Loop containers and nested ProgramArea structures

- **Loop block input chain alignment**
  - Fixed cascade alignment when Loop OUTPUT is connected to another block's INPUT
  - AlignToInputConnection cascade now properly propagates through Loop boundary
  - Blocks connected after Loop correctly align when internal blocks are added

- **Loop container resizing**
  - RecalculateSize() now called after block extraction from Loop
  - Loop container properly shrinks when internal blocks are removed or dragged out
  - BypassBlockInLoop() triggers parent Loop size recalculation

### Changed
- SetParent parameter optimization: true → false in SnapManager after SetWorldPosition calls
- Improved coordinate handling for blocks in all UI hierarchy levels

## [1.0.3] - 2026-01-22

### Added
- **BypassBlockInLoop()** - Drag & Drop improvement for Loop blocks
  - Safe block extraction from Loop chain with automatic connection collapse
  - InternalOutput reconnects to next block when extracting first block
  - Previous block reconnects to InternalInput when extracting last block
  - Debug logging for operation tracking
  - Supports single block, first block, and last block scenarios

### Changed
- BlockDragHandler now collapses Loop connections IMMEDIATELY on drag start, before disconnect
- Improved connection bypass logic - only checks primary input/output (external connectors)

### Fixed
- BlockDragHandler correctly handles block extraction from Loop without losing internal structure
- Loop connections properly maintain state when dragging first/last blocks

## [1.0.2] - 2026-01-22

### Added
- **Samples~ folder** with all sample assets, prefabs, and demo levels
  - Optional import through Package Manager
  - UI Prefabs (BlockUI, LoopBlockUI, ProgramArea, BlockPalette)
  - Robot Prefabs
  - LevelEditor Prefabs (Terrain objects and types)
  - 5 Tutorial Levels with demo configurations
  - Materials, Sprites, and Resource configurations
- **GridPositionTracker** - tracks robot position on grid and detects events:
  - OnGridPositionChanged event for position updates
  - OnMovedToImpassableTerrain event for collision detection
  - OnReachedFinish event for win condition
  - Grid verification and distance calculation
- **LevelRuntimeManager** - manages level loading and grid visualization at runtime:
  - Level data loading and validation
  - Grid position conversions (world ↔ grid coordinates)
  - Terrain passability checking
  - Scene gizmos visualization

### Changed
- Package structure: Code in `Runtime/Editor/`, Sample assets in `Samples~/Assets/`
- GameManager now fully integrated with level system:
  - Loads levels at startup
  - Positions robot at start point with correct rotation
  - Detects finish and game-over conditions
  - Displays robot position and level progress
- Improved SnapManager for better input snapping
- Enhanced RobotController with start position management

### Features
- ✅ Finish detection with visual feedback (green highlight)
- ✅ Level loading and runtime management
- ✅ Robot position tracking on grid
- ✅ Impassable terrain detection
- ✅ Sample assets importable from Package Manager

## [1.0.1] - 2026-01-21

### Fixed
- Resources.Load paths updated for LevelEditor prefabs (Terrain and Objects)
- Prefabs now correctly load from `Resources/LevelEditor/` folder

### Changed
- Namespace renamed from `RobotProgramming.*` to `CodeBlocks.*`
- Namespace `LevelEditor` changed to `CodeBlocks.LevelEditor`
- LevelEditor prefabs moved to `Assets/CodeBlocks/Resources/LevelEditor/` for proper Resources.Load support

### Important
- **Promises library excluded from package** (remains external dependency in `Assets/Scripts/Promises/`)
- Promises assembly reference required in CodeBlocks.Runtime.asmdef
- See README.md prerequisites section for details

## [1.0.0] - 2026-01-21

### Added
- Initial release
- BlockUIBase architecture with unified connector system
- BlockUI for simple command blocks
- LoopBlockUI for loop blocks with 4 connectors
- BlockDragHandler for drag-and-drop functionality
- SnapManager with simplified API
- Visual snap line feedback
- Command execution system with Promises
- Level Editor with JSON export/import
- 5 tutorial levels

### Architecture
- Dictionary<string, BlockConnector> for flexible connector access
- Inheritance-based block types (BlockUI, LoopBlockUI : BlockUIBase)
- Separated drag logic into BlockDragHandler component
