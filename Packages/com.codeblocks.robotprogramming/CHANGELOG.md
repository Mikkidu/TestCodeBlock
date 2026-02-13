# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
