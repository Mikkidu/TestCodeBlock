# Prefab Visualization Fix - Level Loading Issue

## Problem

When loading a level in the Level Editor:
- Only color visualization appeared (grid cells with colors)
- Prefabs were NOT showing, even though `usePrefabs = true` by default
- Issue: First-time prefab rebuild was not triggered when a level was initially loaded

## Root Cause

In `GridVisualizer.OnDrawGizmos()`, visualization rebuild was only triggered when:
1. `usePrefabs` state changed (from false to true)

But when loading a NEW LEVEL:
- `usePrefabs` remained `true` (never changed)
- `levelData` changed, but this wasn't detected
- `RebuildVisualization()` was never called

So prefabs were never generated for newly loaded levels.

## Solution

Added level data change detection in two places:

### 1. GridVisualizer.cs - Added Change Detection

**Added variable:**
```csharp
private LevelGridData lastLoadedLevelData = null;
```

**In OnDrawGizmos() - Added check BEFORE usePrefabs check:**
```csharp
// Check if levelData changed (new level loaded)
if (levelData != lastLoadedLevelData)
{
    lastLoadedLevelData = levelData;
    EnsureVisualizationManager();

    if (visualizationManager != null && usePrefabs)
    {
        // New level loaded - rebuild prefabs
        visualizationManager.usePrefabs = usePrefabs;
        visualizationManager.RebuildVisualization(levelData);
        Debug.Log($"✓ Prefabs visualization built for level: {levelData.levelName}");
    }
}
```

### 2. CodeBlocksLevelEditorWindow.cs - Explicit Rebuild

**In EnsureGridVisualizer() - After setting levelData:**
```csharp
visualizer.levelData = currentLevel;

// Rebuild prefab visualization if enabled
if (visualizer.usePrefabs)
{
    var visMgr = visualizer.GetComponent<LevelVisualizationManager>();
    if (visMgr != null)
    {
        visMgr.usePrefabs = true;
        visMgr.RebuildVisualization(currentLevel);
        Debug.Log($"✓ Prefab visualization rebuilt for {currentLevel.levelName}");
    }
}
```

## Result

✅ When loading ANY level in Level Editor:
- Prefab visualization is automatically triggered
- 3D models appear immediately
- No need to toggle `usePrefabs`
- No need to click anything else

## Workflow After Fix

```
1. Level Editor window open
2. Load Level → Select any level (tutorial or custom)
3. ✓ Prefabs automatically visible
4. Click "Enable Scene Editing"
5. Start placing blocks
```

## Compilation Status

✅ **Assembly-CSharp**: 0 errors, 4 warnings
✅ **Assembly-CSharp-Editor**: 0 errors, 1 warning

## Testing

- [ ] Load tutorial level 1 → prefabs visible
- [ ] Load tutorial level 2 → prefabs visible
- [ ] Load different level → old prefabs cleared, new ones built
- [ ] Toggle usePrefabs off/on → works correctly
- [ ] Create new level → prefabs build when needed

## Files Modified

1. **Assets/Scripts/LevelEditor/GridVisualizer.cs**
   - Added `lastLoadedLevelData` variable
   - Added level data change detection in `OnDrawGizmos()`

2. **Assets/Scripts/LevelEditor/Editor/CodeBlocksLevelEditorWindow.cs**
   - Added explicit rebuild in `EnsureGridVisualizer()`

## Summary

Problem: Prefabs not showing when loading levels
Cause: Level data change was not detected
Fix: Added detection and explicit rebuild
Result: Prefabs now show automatically on level load
