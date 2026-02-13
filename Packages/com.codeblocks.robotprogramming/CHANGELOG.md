# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.1] - 2026-02-12

• ### Fixed

  - Removed package dependency on SharedData from:
      - Runtime/CodeBlocks.Runtime.asmdef
      - Editor/CodeBlocks.Editor.asmdef
  - Removed editor-only [StringEnum(...)] attribute from ReactionConfig.ReactionProfile.obstacleTypeId to eliminate hard coupling to external SharedData types.
  - Kept obstacleTypeId field unchanged; runtime reaction logic and behavior remain the same.

  ### Notes

  - This is a compatibility fix for package integration in projects without SharedData.
  - No gameplay or animation behavior changes.

## [1.1.0] - 2026-02-12

### Added
  - Документация `REACTIONS_ANIMATIONS_GUIDE.md`:
  - как устроен централизованный поток реакций/анимаций,
  - как добавлять новые реакции через конфиги без правок C#,
  - anti-pop рекомендации и чеклист для стабильного старта анимированных объектов.
  - Data-driven `ReactionAnimationConfig` по `obstacleTypeId` (добавление новых реакций без изменений кода).
  - Базовый дефолтный `ReactionAnimationConfig` для ключевых реакций:
    - `FinishPoint`, `OutOfBounds`, `NoTerrain`, `Pit`, `Spike`.
  - Поддержка тайминга запуска анимации реакции в `ReactionConfig.ReactionProfile`:
    - `AnimationTriggerTiming.Start` (триггер до завершения перемещения),
    - `AnimationTriggerTiming.End` (триггер после завершения перемещения).

### Changed
- Реакционные анимации переведены на централизованный resolver:
  - сначала lookup в `ReactionAnimationConfig`,
  - затем fallback на `ReactionProfile.animationId` (для обратной совместимости).
  - Удалена кодовая `if/else`-цепочка выбора animation key в runtime resolver.
  - Для остановки по реакциям введён явный `StopReason` (`Reaction:*`), чтобы результат реакции оставался на экране до ручного перезапуска.
  - Улучшена пакетная загрузка `RobotConfig`:
    - поддержаны пути `Resources/RobotConfig` и `Resources/Configs/RobotConfig`.

## [1.0.10] - 2026-02-03

### Fixed

  - Wrapped editor-only methods in #if UNITY_EDITOR directives
  - Moved EnsureVisualizationManager outside editor block (uses only runtime API)
  - Fixed ArrayUtility and EditorUtility usage in runtime context
  - PlaceTerrain, RemoveTerrain, PlaceObject, RemoveObject now editor-only

## [1.0.9] - 2026-01-30

### Added
  - **InputPoint API** (#26) - unified program start point with 4 access methods
  - **Chain navigation** - `GetPreviousBlock()`, `GetLastBlockInChain()` methods
  - **Auto-snap to InputPoint** - first block always snaps to start point
  - `GameManager.Reset()` - public method for game state reset
  - `TerrainBlockVisual` random rotation

  ### Changed
  - InputPoint magnetism now uses INPUT connector (was OUTPUT)
  - Debug logs cleaned up - console spam removed

  ### Removed (BREAKING)
  ⚠️ **Breaking changes from v1.0.8**
  - `LevelGridData.start` field - use `GetStartPoint()`
  - `LevelGridData.finish` field - use `GetFinishPoint()`

  **Migration:** Run Tools → CodeBlocks → Migrate Levels before updating.

  ### Notes
  - InputPoint requires manual setup in Unity Editor
  - See `.Doc/Tasks/26_Step2_InputPoint_Setup_Instructions.md`

## [1.1.0] - TBD (Planned)

### Removed (BREAKING CHANGES)
⚠️ **This release contains breaking changes. Migrate using v1.0.8 Migration Tool first.**

- `LevelGridData.start` field (deprecated in v1.0.8)
- `LevelGridData.finish` field (deprecated in v1.0.8)

### Migration Required
Before updating to v1.1.0:
1. Ensure you are on v1.0.8
2. Run: Tools → CodeBlocks → Migrate Levels (Start-Finish)
3. Update all custom code using `level.start`/`level.finish` to use `GetStartPoint()`/`GetFinishPoint()`
4. Test thoroughly on v1.0.8 before updating to v1.1.0

---

## [1.0.8] - 2026-01-28

### Changed
- **StartPoint/FinishPoint unified architecture** (#25 preparation)
  - StartPoint and FinishPoint are now regular objects in `objects[]` array
  - objectTypeId: "StartPoint" and "FinishPoint"
  - StartPoint direction stored in `parameters["direction"]` as string
  - All markers instantiated through unified `InstantiateObject()` method
  - Consistent architecture: all level objects use same spawning pipeline

### Added
- **GridObject parameter serialization**
  - `Parameter` class for Unity Inspector serialization
  - `parametersList` serialized field with Dictionary runtime accessor
  - `AddParameter(key, value)` helper method for fluent API
  - Automatic lazy initialization from serialized list
- **LevelGridData unified API**
  - `GetStartPoint()` - returns StartPoint as GridObject
  - `GetFinishPoint()` - returns FinishPoint as GridObject
  - `GetStartDirection()` - extracts direction from StartPoint parameters
- **Migration tool** (#25 Phase 4)
  - Menu: Tools → CodeBlocks → Migrate Levels (Start/Finish)
  - Converts legacy start/finish fields to objects[] array
  - Automatic duplicate detection and skipping
  - Progress bar and detailed migration summary
- **Public API for external control** (#25 FEATURE-1)
  - `GameManager.StartProgram()` - start program execution from external code
  - `GameManager.StopProgram()` - stop program execution from external code
  - `GameManager.ClearProgram()` - clear all blocks from program area
  - `GameManager.IsProgramRunning` - check if program is currently running
  - `GameManager.GetBlocksCount()` - get number of blocks in program area
  - Integration-ready for play-united MiniGameManager

### Deprecated (will be removed in v1.1.0)
- `LevelGridData.start` field (use `GetStartPoint()` or run Migration Tool)
- `LevelGridData.finish` field (use `GetFinishPoint()` or run Migration Tool)
- Note: Fields kept for backward compatibility and Migration Tool

### Removed (internal only, no breaking changes)
- `LevelRuntimeManager.InstantiateStartVisual()` (internal method, replaced by `InstantiateObject()`)
- `LevelRuntimeManager.InstantiateFinishVisual()` (internal method, replaced by `InstantiateObject()`)

### Fixed
- **Start/Finish marker duplication bug** (#25 BUG-1)
  - Fallback CreatePrimitive now correctly sets parent to levelContainer
  - All markers properly cleaned up via levelContainer.Destroy()
  - No marker accumulation on multiple InitLevel() calls
- **Background positioning** (#25 BUG-2)
  - Background now correctly parented to levelContainer
  - Centered properly regardless of window size
  - Consistent positioning across different screen resolutions
- **Reset button now stops program** (#25)
  - `OnResetButtonClicked()` now correctly calls `OnStopButtonClicked()` if program is running
  - Removed duplicate Stop logic (DRY principle)
  - Simplified from 30 lines to 16 lines

### Migration Guide
**For existing projects (before v1.1.0):**
1. Update to v1.0.8 (this version)
2. Run migration tool: Tools → CodeBlocks → Migrate Levels (Start-Finish)
3. Migration automatically moves start/finish data to objects[] array
4. Update any custom code using `level.start`/`level.finish`:
   - Replace `level.start.position` with `level.GetStartPoint().position`
   - Replace `level.start.direction` with `level.GetStartDirection()`
   - Replace `level.finish.position` with `level.GetFinishPoint().position`
5. Test your project - backward compatibility maintained in v1.0.8
6. ⚠️ **IMPORTANT:** In v1.1.0, `start`/`finish` fields will be removed (breaking change)

**Benefits:**
- Unified object spawning (no special cases)
- No marker duplication bugs
- Easier to extend (add new object types like Trap, Key, Portal)
- Consistent level data structure

**Timeline:**
- v1.0.8 (current): Deprecated fields, backward compatible, Migration Tool available
- v1.1.0 (next): Fields removed, must migrate before updating

## [1.0.7] - 2026-01-27

### Added
First level statistic
LEvelFinished event to GameManager

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
