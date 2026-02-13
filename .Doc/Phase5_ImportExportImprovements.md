# Phase 5 - Level Editor Improvements (JSON Import/Export & Default Visualization)

## Summary

Three key improvements to the Level Editor workflow:

1. **Prefab visualization enabled by default**
2. **Import button disabled when no level is selected**
3. **Import JSON loads into current level instead of creating new file**

---

## Change 1: Prefab Visualization Enabled by Default

### Location
`Assets/Scripts/LevelEditor/GridVisualizer.cs:13`

### Before
```csharp
public bool usePrefabs = false;
```

### After
```csharp
public bool usePrefabs = true;
```

### Impact
- When Level Editor opens a level, prefab visualization automatically shows
- Users see 3D representation immediately (no need to toggle "usePrefabs")
- More intuitive workflow: open level → see visualization → start editing

### Workflow
```
1. Window → CodeBlocks → Level Editor
2. Load/Create level
3. ✓ Prefabs automatically visible (no extra toggle needed)
4. Enable "Scene Editing" → start placing blocks
```

---

## Change 2: Import Button Disabled Without Level

### Location
`Assets/Scripts/LevelEditor/Editor/CodeBlocksLevelEditorWindow.cs:128-137`

### Before
```csharp
GUI.backgroundColor = Color.cyan;
if (GUILayout.Button("Import from JSON", GUILayout.Height(25)))
{
    ImportLevelFromJson();
}
```

### After
```csharp
// Import button disabled if no level selected
GUI.backgroundColor = currentLevel != null ? Color.cyan : Color.gray;
GUI.enabled = currentLevel != null;
if (GUILayout.Button("Import from JSON", GUILayout.Height(25)))
{
    ImportLevelFromJson();
}
GUI.enabled = true;
```

### Visual Feedback
- **Enabled** (cyan): Level selected, import available
- **Disabled** (gray): No level selected, import unavailable

### Logic
```
if (currentLevel != null)
    → Button is cyan and clickable
else
    → Button is gray and disabled
    → Click has no effect
```

---

## Change 3: Import Into Current Level

### Location
`Assets/Scripts/LevelEditor/Editor/CodeBlocksLevelEditorWindow.cs:426-464`

### Before Behavior
1. Click "Import from JSON"
2. Select JSON file
3. **Creates new ScriptableObject file** (SaveFilePanelInProject)
4. Saves imported level as new asset

### After Behavior
1. Click "Import from JSON" (available only if level selected)
2. Select JSON file
3. **Loads directly into current open level** (no new file)
4. Updates currentLevel with imported data

### Code Comparison

**Before:**
```csharp
LevelGridData importedLevel = LevelJsonSerializer.ImportFromJson(path);

// Save as NEW asset
string assetPath = EditorUtility.SaveFilePanelInProject(...);
AssetDatabase.CreateAsset(importedLevel, assetPath);

currentLevel = importedLevel; // Switch to new level
```

**After:**
```csharp
LevelGridData importedLevel = LevelJsonSerializer.ImportFromJson(path);

// Copy data to CURRENT level
currentLevel.levelId = importedLevel.levelId;
currentLevel.levelName = importedLevel.levelName;
currentLevel.difficulty = importedLevel.difficulty;
currentLevel.hintText = importedLevel.hintText;
currentLevel.gridWidth = importedLevel.gridWidth;
currentLevel.gridHeight = importedLevel.gridHeight;
currentLevel.terrain = importedLevel.terrain;
currentLevel.objects = importedLevel.objects;
currentLevel.start = importedLevel.start;
currentLevel.finish = importedLevel.finish;

// Mark dirty and save
EditorUtility.SetDirty(currentLevel);
AssetDatabase.SaveAssets();
```

### Workflow Improvement

**Old workflow:**
```
1. Create/Load Level A
2. Import level_b.json
3. Choose save location → creates level_b.asset
4. Now editing level_b (switched away from Level A)
5. 😞 Extra file created, confusing workflow
```

**New workflow:**
```
1. Create/Load Level A
2. Import level_b.json
3. ✓ Level A now contains level_b's data
4. Continue editing Level A with imported data
5. 😊 Simple, intuitive, no extra files
```

---

## Use Cases

### Use Case 1: Share Level Between Projects
```
Project A:
  1. Export Level 1 → "my_level.json"

Project B:
  1. Create empty level "temp_level"
  2. Import "my_level.json"
  3. ✓ temp_level now contains my_level's data
  4. Can rename and continue editing
```

### Use Case 2: Backup and Restore
```
1. Working on "production_level"
2. Export to JSON → backup copy
3. Later: Edit corrupts level
4. Create new "production_level"
5. Import JSON backup → restore instantly
```

### Use Case 3: Iterate on Design
```
1. Create "level_iteration_v1"
2. Modify and save
3. Export JSON → "iteration_v1.json"
4. Load "level_iteration_v1" again
5. Import "iteration_v2.json" → test new design
6. Compare without creating files
```

---

## Compilation Status

✅ **Assembly-CSharp**: Build succeeded (0 errors, 4 warnings)
✅ **Assembly-CSharp-Editor**: Build succeeded (0 errors, 1 warning)

---

## Files Modified

1. **GridVisualizer.cs**
   - Line 13: Changed `usePrefabs = false` → `usePrefabs = true`

2. **CodeBlocksLevelEditorWindow.cs**
   - Lines 128-137: Added button disable logic
   - Lines 426-464: Rewrote ImportLevelFromJson() method

---

## Testing Checklist

- [ ] Create new level → prefabs visible immediately
- [ ] Load existing level → prefabs visible without toggling usePrefabs
- [ ] Open Editor window without level selected
  - [ ] Export button enabled (gray text, disabled)
  - [ ] Import button disabled (gray, not clickable)
- [ ] Create level "test_level"
- [ ] Export to JSON → "backup.json"
- [ ] Modify test_level (add/remove blocks)
- [ ] Click Import → select backup.json
  - [ ] test_level reverts to backup state
  - [ ] No new file created
- [ ] Try importing into different level
  - [ ] Original data replaced
  - [ ] Only one file (the original)

---

## Behavior Changes Summary

| Aspect | Before | After |
|--------|--------|-------|
| **Prefab Visualization** | Manual toggle needed | Automatic (enabled by default) |
| **Import Button** | Always clickable | Disabled without level |
| **Import Action** | Creates new file | Loads into current level |
| **User Friction** | High (multiple steps) | Low (direct import) |
| **File Management** | Creates extra assets | No extra files |

---

## Performance Impact

**Negligible.** Changes are UI/UX only:
- No additional runtime overhead
- No new serialization
- Same data structures
- Only behavior modification

---

## Next Steps

- **Task #16**: Create 5 example levels
- **Task #17** (optional): Prefab Config system for flexible BlockType → Prefab mapping
- **Documentation**: Update user guide with new workflow

---

## Summary

Three simple but impactful changes that make the Level Editor more intuitive:

1. **Visualization by default** → Users see what they're editing immediately
2. **Smart button state** → Visual feedback prevents errors
3. **Smart import** → No more file proliferation, cleaner workflow

Result: More efficient level creation workflow with less friction.
