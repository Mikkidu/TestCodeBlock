# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
