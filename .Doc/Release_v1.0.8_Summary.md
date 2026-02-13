# Release v1.0.8 Summary

## 📦 Package: CodeBlocks Robot Programming
**Version:** 1.0.8
**Date:** 2026-01-28
**Priority:** 🔴 CRITICAL (blocks play-united integration)

---

## 🎯 Key Features

### 1. **Unified Start/Finish Architecture** (#25 Refactor)
- StartPoint and FinishPoint are now regular `GridObject` instances in `objects[]` array
- No more special fields - consistent with Wall, Door, Button architecture
- Direction stored in `parameters["direction"]` as string
- **Benefits:**
  - Single spawning pipeline for all objects
  - No special cases in code
  - Easy to extend (add Trap, Key, Portal in future)
  - No marker duplication bugs

### 2. **Public API for External Control** (#25 FEATURE-1)
Integration-ready for play-united MiniGameManager:

```csharp
// Start/Stop program from external code
gameManager.StartProgram();
gameManager.StopProgram();
gameManager.ClearProgram();

// Query state
bool isRunning = gameManager.IsProgramRunning;
int blocksUsed = gameManager.GetBlocksCount();
```

**Use Cases:**
- External UI buttons in play-united
- Statistics collection
- UI state management (disable buttons during execution)

### 3. **Migration Tool**
Menu: `Tools → CodeBlocks → Migrate Levels (Start/Finish)`

- Converts legacy `start`/`finish` fields to unified `objects[]` format
- Automatic duplicate detection
- Progress bar with detailed summary
- Safe: keeps deprecated fields for backward compatibility

---

## 🐛 Bug Fixes

### Fixed Issues:
1. **Start/Finish marker duplication** - markers now properly parented to levelContainer
2. **Background positioning** - background now centered correctly regardless of window size
3. **Reset button now stops program** - `OnResetButtonClicked()` correctly calls Stop logic

---

## 📊 Changes Summary

### Files Modified (13 files):
**Runtime (7 files):**
- `GridObject.cs` - Added Parameter serialization system
- `LevelGridData.cs` - Added GetStartPoint/GetFinishPoint/GetStartDirection API
- `LevelRuntimeManager.cs` - Unified InstantiateObject() for all markers
- `GridPositionTracker.cs` - Updated to use GetFinishPoint()
- `GameManager.cs` - Added 5 public API methods + refactored OnResetButtonClicked()
- `LevelJsonData.cs` - Updated JSON serialization for unified format
- `GridVisualizer.cs` - Updated editor visualization

**Editor (4 files):**
- `LevelMigrationTool.cs` (NEW) - Migration utility
- `TutorialLevelGenerator.cs` - Updated 5 tutorial levels
- `CodeBlocksLevelEditorWindow.cs` - Updated minimap display
- `GridVisualizer.cs` - Updated object placement

**Meta (2 files):**
- `package.json` - Version 1.0.8
- `CHANGELOG.md` - Full v1.0.8 changelog

### Code Statistics:
- **Lines Added:** ~350 lines
- **Lines Removed:** ~80 lines (duplicate logic)
- **Net Change:** +270 lines
- **Compilation:** ✅ 0 errors, 7 warnings (pre-existing)

---

## 🔄 Migration Path

### For Existing Projects:
1. **Update to v1.0.8** (this version)
2. **Run Migration Tool:** `Tools → CodeBlocks → Migrate Levels (Start-Finish)`
3. **Update Custom Code:** Replace `level.start`/`level.finish` with `GetStartPoint()`/`GetFinishPoint()`
4. **Test:** Verify levels load correctly
5. ⚠️ **IMPORTANT:** In v1.1.0, deprecated fields will be removed (breaking change)

### Backward Compatibility:
- ✅ Legacy `start`/`finish` fields still work (deprecated with warnings)
- ✅ Fallback logic converts legacy fields on-the-fly
- ✅ Migration Tool safe to run multiple times

---

## 🧪 Testing Checklist

### Tested Scenarios:
- [x] Load 5 tutorial levels sequentially - no marker duplication
- [x] Run Migration Tool - all levels converted successfully
- [x] Public API methods work from external script
- [x] Reset button stops running program
- [x] Background centered correctly
- [x] Compilation passes (0 errors)
- [x] No Russian comments in code

### Regression Tests:
- [x] Existing UI buttons (Run/Stop/Clear/Reset) work correctly
- [x] Block drag-and-drop works
- [x] Loop blocks work
- [x] JSON export/import works
- [x] Level Editor works

---

## 🚀 Integration with play-united

### Example Usage:

```csharp
using CodeBlocks.Managers;

public class RobotProgrammingMiniGame : MonoBehaviour
{
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    // External UI button
    public void OnPlayButtonClick()
    {
        if (!gameManager.IsProgramRunning)
        {
            gameManager.StartProgram();
        }
    }

    // Statistics collection
    public void OnLevelComplete()
    {
        int blocksUsed = gameManager.GetBlocksCount();
        SendStatistics(blocksUsed);
    }

    // UI state management
    void Update()
    {
        playButton.interactable = !gameManager.IsProgramRunning;
    }
}
```

---

## 📝 Git Integration URL

```
https://github.com/mikkiducher/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v1.0.8
```

---

## 🎯 Next Steps (v1.1.0 - Breaking Changes)

**Planned for v1.1.0:**
- ⚠️ Remove deprecated `start`/`finish` fields (BREAKING CHANGE)
- Users MUST migrate before updating to v1.1.0
- Migration Tool will be available in v1.0.8 to prepare

**Timeline:**
- v1.0.8 (current): Deprecated fields, backward compatible, Migration Tool available
- v1.1.0 (next): Fields removed, must migrate before updating

---

## 👨‍💻 Contributors
- Implementation: Claude Code + Mikki Ducher
- Testing: Manual testing in Unity Editor
- Documentation: Full CHANGELOG.md + migration guides

---

## 📖 Documentation

- **CHANGELOG.md:** Full version history
- **Migration Tool:** `Tools → CodeBlocks → Migrate Levels (Start-Finish)`
- **Public API:** XML documentation in GameManager.cs
- **Task Plans:**
  - `.Doc/Tasks/25_Unified_StartFinish_Refactor.md`
  - `.Doc/Tasks/25_Step1_PublicAPI_StopFixes.md`
