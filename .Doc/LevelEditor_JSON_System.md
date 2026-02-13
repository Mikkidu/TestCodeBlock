# JSON Level Save/Load System

## Overview

Implement JSON-based level persistence system for the Level Editor. This allows users to:
- **Export** levels to portable JSON format (human-readable, version-control friendly)
- **Import** JSON files back to Unity ScriptableObject assets
- Share levels across projects
- Edit levels in text editors (VS Code, etc.)
- Maintain readable version history in Git

---

## Architecture

### 1. **LevelJsonData.cs** - JSON-Serializable Data Structure

Location: `Assets/Scripts/LevelEditor/LevelJsonData.cs`

Provides JSON-friendly equivalents of Unity types:

```csharp
[System.Serializable]
public class LevelJsonData
{
    // Nested classes for JSON serialization
    public class Vector2Int      // Replaces UnityEngine.Vector2Int
    public class JsonTerrainCell // Replaces global TerrainCell
    public class JsonGridObject  // Replaces global GridObject
    public class StartPoint      // Matches existing structure
    public class FinishPoint     // Matches existing structure

    // Level metadata
    public string levelId;
    public string levelName;
    public int difficulty;
    public string hintText;

    // Grid configuration
    public int gridWidth;
    public int gridHeight;

    // Level content
    public List<JsonTerrainCell> terrain;
    public List<JsonGridObject> objects;
    public StartPoint start;
    public FinishPoint finish;

    // Conversion methods
    public static LevelJsonData FromLevelGridData(LevelGridData gridData);
    public LevelGridData ToLevelGridData();
}
```

**Key Features:**
- Nested classes avoid naming conflicts with global types
- Conversion methods handle bidirectional transformation
- Timestamp tracking for saved files
- Version field for future compatibility

### 2. **LevelJsonSerializer.cs** - Serialization Logic

Location: `Assets/Scripts/LevelEditor/Editor/LevelJsonSerializer.cs`

Public API for JSON operations:

```csharp
public class LevelJsonSerializer
{
    // Export level to JSON file
    public static bool ExportToJson(LevelGridData levelData, string filePath);

    // Import level from JSON file
    public static LevelGridData ImportFromJson(string filePath);

    // Convert JSON file to ScriptableObject asset
    public static LevelGridData SaveJsonAsAsset(string jsonFilePath, string assetPath);
}
```

**Operations:**
1. **Export**: Converts LevelGridData → LevelJsonData → JSON string → File
2. **Import**: Reads JSON file → Deserializes → Creates ScriptableObject
3. **Asset Conversion**: Imports JSON and saves as .asset

---

## Usage in Level Editor

### UI Integration

The Level Editor window now has two buttons in the "JSON Export/Import" section:

#### Export Button
```
Current Level Inspector
├─ Level ID
├─ Level Name
├─ Difficulty
├─ Hint Text
└─ JSON Export/Import
   ├─ [Export to JSON] ← Save current level as JSON
   └─ [Import from JSON] ← Load JSON file
```

**Export Workflow:**
1. Click "Export to JSON"
2. Choose save location
3. Level exports as `levelId.json`
4. Success dialog shows file path

**Import Workflow:**
1. Click "Import from JSON"
2. Select JSON file
3. Choose location to save as `.asset`
4. Level imported and displayed in editor

---

## JSON File Format Example

```json
{
  "levelId": "level_001",
  "levelName": "First Steps",
  "difficulty": 1,
  "hintText": "Move the robot forward to reach the finish",
  "gridWidth": 8,
  "gridHeight": 8,
  "terrain": [
    {
      "terrainType": "Ground",
      "position": {
        "x": 0,
        "y": 0
      }
    },
    {
      "terrainType": "Road",
      "position": {
        "x": 1,
        "y": 0
      }
    }
  ],
  "objects": [
    {
      "objectTypeId": "Wall",
      "position": {
        "x": 3,
        "y": 2
      },
      "objectInstanceId": "Wall_4521"
    }
  ],
  "start": {
    "position": {
      "x": 0,
      "y": 0
    },
    "direction": "North"
  },
  "finish": {
    "position": {
      "x": 7,
      "y": 7
    }
  },
  "version": "1.0",
  "savedTimestamp": 637430124800000000
}
```

---

## Type Conversion Flow

### Export: Unity → JSON

```
LevelGridData (ScriptableObject)
    ↓
LevelJsonData.FromLevelGridData()
    ↓
LevelJsonData (nested JSON-friendly types)
    ↓
JsonUtility.ToJson()
    ↓
JSON String
    ↓
File.WriteAllText()
    ↓
.json File
```

### Import: JSON → Unity

```
.json File
    ↓
File.ReadAllText()
    ↓
JSON String
    ↓
JsonUtility.FromJson<LevelJsonData>()
    ↓
LevelJsonData (nested JSON-friendly types)
    ↓
LevelJsonData.ToLevelGridData()
    ↓
LevelGridData (ScriptableObject)
    ↓
AssetDatabase.CreateAsset()
    ↓
.asset File
```

---

## Implementation Details

### Namespace Handling

JSON-specific types are nested to avoid conflicts:
- `LevelJsonData.Vector2Int` ← JSON version
- `Vector2Int` (global) ← Conversion helper
- `LevelJsonData.JsonTerrainCell` ← JSON version
- `TerrainCell` (global) ← Runtime version
- `LevelJsonData.JsonGridObject` ← JSON version
- `GridObject` (global) ← Runtime version

Conversion methods use `global::` qualifier when needed:
```csharp
var globalTerrainCell = new global::TerrainCell { ... };
```

### Error Handling

Both export and import include try-catch blocks:
```csharp
try {
    // Serialize/Deserialize operations
}
catch (System.Exception ex) {
    Debug.LogError($"✗ Failed: {ex.Message}");
    EditorUtility.DisplayDialog("Error", message, "OK");
}
```

---

## Workflow Examples

### Scenario 1: Create and Export Level

```
1. Window → CodeBlocks → Level Editor
2. Create new level (Assets → Create → CodeBlocks → Level Grid Data)
3. Name: "Tutorial Level 1"
4. Add terrain and objects using scene editor
5. Click "Export to JSON"
6. Save as: Assets/Levels/JSON/tutorial_level_1.json
7. ✓ Level exported and shareable
```

### Scenario 2: Import Existing JSON

```
1. Have tutorial_level_1.json in project
2. Open Level Editor window
3. Click "Import from JSON"
4. Select tutorial_level_1.json
5. Save as asset: Assets/Resources/RobotLevels/tutorial_level_1.asset
6. Level automatically loaded in editor
7. ✓ Level ready for editing/testing
```

### Scenario 3: Share Level Between Projects

```
Project A:
├─ Levels/JSON/my_level.json

Project B:
1. Copy my_level.json to Assets/Levels/JSON/
2. Open Level Editor
3. Import from JSON → save as .asset
4. ✓ Level now available in Project B
```

---

## Compilation Status

✅ **Assembly-CSharp**: 0 errors, 4 warnings
✅ **Assembly-CSharp-Editor**: 0 errors, 1 warning

Added to csproj:
- `LevelJsonData.cs` (runtime, in Assembly-CSharp)
- `LevelJsonSerializer.cs` (editor, in Assembly-CSharp-Editor)

---

## Files Modified/Created

### New Files
- `Assets/Scripts/LevelEditor/LevelJsonData.cs` - JSON data structures
- `Assets/Scripts/LevelEditor/Editor/LevelJsonSerializer.cs` - Serialization logic

### Modified Files
- `Assets/Scripts/LevelEditor/Editor/CodeBlocksLevelEditorWindow.cs`
  - Added `ExportLevelToJson()` method
  - Added `ImportLevelFromJson()` method
  - Added UI buttons in DrawLevelInfo()
  - Updated csproj to include LevelJsonSerializer

---

## Testing Checklist

- [ ] Create new level with terrain/objects/start/finish
- [ ] Click "Export to JSON"
- [ ] Verify JSON file is readable in VS Code
- [ ] Verify all data is present (terrain, objects, start, finish)
- [ ] Click "Import from JSON"
- [ ] Verify imported level matches original
- [ ] Try editing JSON file manually and reimport
- [ ] Verify timestamp is set correctly
- [ ] Test with complex levels (many objects)
- [ ] Verify Git can track JSON file changes

---

## Future Enhancements

- **Compression**: Add gzip option for large levels
- **Validation**: Add JSON schema validator before import
- **Batch Operations**: Export/import multiple levels at once
- **Comments**: Add optional comment field for designer notes
- **Versioning**: Track level version history
- **Diff Tool**: Show differences between JSON versions
- **Web Editor**: Support JSON format for browser-based editor
- **API Integration**: Allow game runtime to load JSON levels directly

---

## Advantages Over ScriptableObject Only

| Aspect | ScriptableObject | JSON | Both (Hybrid) |
|--------|------------------|------|--------------|
| **Performance** | Fast (native Unity) | Slower (deserialization) | ✓ Fast runtime |
| **Human-readable** | No (binary) | Yes | ✓ Readable VCS |
| **Portable** | Requires Unity | Universal | ✓ Shareable |
| **Version Control** | Difficult (metadata) | Clean diffs | ✓ Clear history |
| **Text Editing** | Impossible | VS Code friendly | ✓ Flexible |

**Recommendation**: Use JSON for storage and sharing, auto-convert to ScriptableObject for runtime.

---

## Summary

A complete JSON import/export system is now implemented, allowing:
- Human-readable level data storage
- Easy level sharing between projects
- Git-friendly version tracking
- Future integration with web tools
- Fallback to ScriptableObject for runtime performance

The system is transparent to users—they click buttons, and the conversion happens automatically.
