using UnityEngine;
using UnityEditor;
//using UnityEditor.SceneHierarchy;

public class CodeBlocksLevelEditorWindow : EditorWindow
{
    private LevelGridData currentLevel;
    private Vector2 scrollPosition;
    private Vector2 terrainScrollPosition;
    private Vector2 objectScrollPosition;

    private string selectedTerrainType = "Ground";
    private string selectedObjectType = "Wall";

    private float cellSize = 1f;
    private Color groundColor = Color.green;
    private Color roadColor = Color.gray;
    private Color pitColor = Color.red;
    private Color wallColor = Color.black;

    private LevelEditorPaletteConfig paletteConfig;

    public static bool isWindowOpen = false;

    [MenuItem("Window/CodeBlocks/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<CodeBlocksLevelEditorWindow>("Level Editor");
    }

    private void OnEnable()
    {
        isWindowOpen = true;
    }

    private void OnDisable()
    {
        isWindowOpen = false;
        GridVisualizer.isEditing = false;
    }

    private void OnGUI()
    {
        // Load palette config if not already loaded
        if (paletteConfig == null)
        {
            LoadPaletteConfig();
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        DrawHeader();
        GUILayout.Space(10);

        DrawLevelInfo();
        GUILayout.Space(10);

        DrawPalette();
        GUILayout.Space(10);

        DrawGridSettings();
        GUILayout.Space(10);

        DrawMiniMap();

        GUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        GUILayout.Label("Level Editor", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("New Level", GUILayout.Width(100)))
        {
            CreateNewLevel();
        }

        if (GUILayout.Button("Load Level", GUILayout.Width(100)))
        {
            LoadLevel();
        }

        if (currentLevel != null && GUILayout.Button("Save Level", GUILayout.Width(100)))
        {
            EditorUtility.SetDirty(currentLevel);
            AssetDatabase.SaveAssets();
            Debug.Log("Level saved: " + currentLevel.levelName);
        }

        GUILayout.EndHorizontal();
    }

    private void DrawLevelInfo()
    {
        GUILayout.Label("Level Info", EditorStyles.boldLabel);

        LevelGridData selectedAsset = EditorGUILayout.ObjectField("Current Level", currentLevel, typeof(LevelGridData), false) as LevelGridData;
        if (selectedAsset != currentLevel)
        {
            currentLevel = selectedAsset;
            EnsureGridVisualizer();
            Repaint();
        }

        if (currentLevel == null)
        {
            EditorGUILayout.HelpBox("Select or create a Level Grid Data asset to edit", MessageType.Info);
            return;
        }

        currentLevel.levelName = EditorGUILayout.TextField("Level Name", currentLevel.levelName);
        currentLevel.levelId = EditorGUILayout.TextField("Level ID", currentLevel.levelId);
        currentLevel.difficulty = EditorGUILayout.IntSlider("Difficulty", currentLevel.difficulty, 1, 5);
        currentLevel.hintText = EditorGUILayout.TextArea(currentLevel.hintText, GUILayout.Height(50));

        GUILayout.Space(10);
        GUILayout.Label("JSON Export/Import", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Export to JSON", GUILayout.Height(25)))
        {
            ExportLevelToJson();
        }

        // Import button disabled if no level selected
        GUI.backgroundColor = currentLevel != null ? Color.cyan : Color.gray;
        GUI.enabled = currentLevel != null;
        if (GUILayout.Button("Import from JSON", GUILayout.Height(25)))
        {
            ImportLevelFromJson();
        }
        GUI.enabled = true;

        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
    }

    private void DrawPalette()
    {
        if (paletteConfig == null)
            return;

        // Terrain Palette
        GUILayout.Label("Terrain Palette", EditorStyles.boldLabel);
        terrainScrollPosition = GUILayout.BeginScrollView(terrainScrollPosition, GUILayout.Height(80));
        GUILayout.BeginVertical();

        foreach (var terrain in paletteConfig.terrainTypes)
        {
            DrawTerrainButton(terrain.typeName, terrain.color);
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();

        GUILayout.Space(5);

        // Object Palette
        GUILayout.Label("Object Palette", EditorStyles.boldLabel);
        objectScrollPosition = GUILayout.BeginScrollView(objectScrollPosition, GUILayout.Height(100));
        GUILayout.BeginVertical();

        foreach (var obj in paletteConfig.objectTypes)
        {
            DrawObjectButton(obj.typeName, obj.isSpecial);
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();

        GUILayout.Space(5);

        // Placement mode selector
        GUILayout.Label("Placement Mode", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        GUI.backgroundColor = GridVisualizer.placeTerrainMode ? Color.yellow : Color.gray;
        if (GUILayout.Button("Terrain Mode", GUILayout.Height(25)))
        {
            GridVisualizer.placeTerrainMode = true;
        }

        GUI.backgroundColor = !GridVisualizer.placeTerrainMode ? Color.yellow : Color.gray;
        if (GUILayout.Button("Object Mode", GUILayout.Height(25)))
        {
            GridVisualizer.placeTerrainMode = false;
        }

        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUI.backgroundColor = GridVisualizer.isEditing ? Color.green : Color.gray;
        if (GUILayout.Button(GridVisualizer.isEditing ? "EDITING (Click to disable)" : "Enable Scene Editing", GUILayout.Height(25)))
        {
            GridVisualizer.isEditing = !GridVisualizer.isEditing;
            if (GridVisualizer.isEditing)
            {
                if (GridVisualizer.placeTerrainMode)
                {
                    Debug.Log("✓ Scene editing ENABLED. Mode: Terrain. Type: " + selectedTerrainType);
                }
                else
                {
                    Debug.Log("✓ Scene editing ENABLED. Mode: Object. Type: " + selectedObjectType);
                }
                Debug.Log("  → Left click on Scene View to place");
                Debug.Log("  → Right click to remove blocks");
            }
            else
            {
                Debug.Log("✗ Scene editing DISABLED");
            }
        }
        GUI.backgroundColor = Color.white;

        string modeStr = GridVisualizer.placeTerrainMode ? "Terrain" : "Object";
        string selectedStr = GridVisualizer.placeTerrainMode ? selectedTerrainType : selectedObjectType;
        GUILayout.Label("Mode: " + modeStr + " | Selected: " + selectedStr, EditorStyles.miniLabel);
    }

    private void DrawTerrainButton(string terrainType, Color color)
    {
        GUI.backgroundColor = selectedTerrainType == terrainType ? Color.yellow : color;

        if (GUILayout.Button(terrainType, GUILayout.Height(25)))
        {
            selectedTerrainType = terrainType;
            GridVisualizer.currentTerrainType = terrainType;
            GridVisualizer.placeTerrainMode = true;
        }

        GUI.backgroundColor = Color.white;
    }

    private void DrawObjectButton(string objectType, bool isSpecial)
    {
        Color bgColor = isSpecial ? Color.cyan : Color.gray;
        GUI.backgroundColor = selectedObjectType == objectType ? Color.yellow : bgColor;

        if (GUILayout.Button(objectType, GUILayout.Height(25)))
        {
            selectedObjectType = objectType;
            GridVisualizer.currentObjectType = objectType;
            GridVisualizer.placeTerrainMode = false;
        }

        GUI.backgroundColor = Color.white;
    }

    private void LoadPaletteConfig()
    {
        // Try to find existing palette config
        string[] guids = AssetDatabase.FindAssets("t:LevelEditorPaletteConfig");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            paletteConfig = AssetDatabase.LoadAssetAtPath<LevelEditorPaletteConfig>(path);
        }

        // Fallback: create default config if not found
        if (paletteConfig == null)
        {
            paletteConfig = ScriptableObject.CreateInstance<LevelEditorPaletteConfig>();
            Debug.LogWarning("No palette config found. Using default palette. Create one with: Assets → Create → CodeBlocks → Level Editor Palette Config");
        }
    }

    private void DrawGridSettings()
    {
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);

        if (currentLevel != null)
        {
            currentLevel.gridWidth = EditorGUILayout.IntField("Grid Width", currentLevel.gridWidth);
            currentLevel.gridHeight = EditorGUILayout.IntField("Grid Height", currentLevel.gridHeight);
            cellSize = EditorGUILayout.FloatField("Cell Size", cellSize);
        }
    }

    private void DrawMiniMap()
    {
        if (currentLevel == null)
            return;

        GUILayout.Label("Mini Map", EditorStyles.boldLabel);

        int mapSize = 10;
        int cellPixels = 20;

        GUILayout.Label("Mini Map (matches Scene View top-down)", EditorStyles.miniLabel);
        GUILayout.BeginVertical(GUILayout.Width(mapSize * cellPixels), GUILayout.Height(mapSize * cellPixels));

        // Draw in REVERSE Y order to match top-down view
        for (int y = currentLevel.gridHeight - 1; y >= 0 && y >= currentLevel.gridHeight - mapSize; y--)
        {
            GUILayout.BeginHorizontal();

            for (int x = 0; x < currentLevel.gridWidth && x < mapSize; x++)
            {
                Color cellColor = Color.white;
                var terrain = currentLevel.GetTerrainAt(x, y);

                if (terrain != null)
                {
                    cellColor = terrain.terrainType switch
                    {
                        "Ground" => groundColor,
                        "Road" => roadColor,
                        "Pit" => pitColor,
                        _ => Color.white
                    };
                }

                if (currentLevel.start != null && currentLevel.start.position == new Vector2Int(x, y))
                    cellColor = Color.blue;

                if (currentLevel.finish != null && currentLevel.finish.position == new Vector2Int(x, y))
                    cellColor = Color.cyan;

                GUI.backgroundColor = cellColor;
                GUILayout.Box("", GUILayout.Width(cellPixels), GUILayout.Height(cellPixels));
                GUI.backgroundColor = Color.white;
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    private void CreateNewLevel()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create New Level", "NewLevel", "asset", "");
        if (string.IsNullOrEmpty(path))
            return;

        var newLevel = ScriptableObject.CreateInstance<LevelGridData>();
        newLevel.levelId = System.IO.Path.GetFileNameWithoutExtension(path);
        newLevel.levelName = newLevel.levelId;
        newLevel.gridWidth = 8;
        newLevel.gridHeight = 8;

        AssetDatabase.CreateAsset(newLevel, path);
        AssetDatabase.SaveAssets();

        currentLevel = newLevel;
        EnsureGridVisualizer();
        Debug.Log("Created new level: " + path);
    }

    private void LoadLevel()
    {
        string path = EditorUtility.OpenFilePanel("Load Level", "Assets", "asset");
        if (string.IsNullOrEmpty(path))
            return;

        path = FileUtil.GetProjectRelativePath(path);
        var level = AssetDatabase.LoadAssetAtPath<LevelGridData>(path);

        if (level != null)
        {
            currentLevel = level;
            EnsureGridVisualizer();
            Debug.Log("Loaded level: " + level.levelName);
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Failed to load level", "OK");
        }
    }

    private void EnsureGridVisualizer()
    {
        if (currentLevel == null)
            return;

        var visualizer = FindObjectOfType<GridVisualizer>();
        if (visualizer != null && visualizer.levelData == currentLevel)
            return;

        // Clear visualization when switching to a different level
        if (visualizer != null && visualizer.levelData != currentLevel)
        {
            visualizer.ClearLevelVisualization();
        }

        if (visualizer == null)
        {
            var obj = new GameObject("LevelGridVisualizer");
            visualizer = obj.AddComponent<GridVisualizer>();
        }

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

        EditorGUIUtility.PingObject(visualizer.gameObject);
        Debug.Log("GridVisualizer configured for: " + currentLevel.levelName);
    }

    private void ExportLevelToJson()
    {
        if (currentLevel == null)
        {
            EditorUtility.DisplayDialog("Error", "No level selected", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanel("Export Level to JSON", "Assets", currentLevel.levelId, "json");
        if (string.IsNullOrEmpty(path))
            return;

        bool success = LevelJsonSerializer.ExportToJson(currentLevel, path);
        if (success)
        {
            EditorUtility.DisplayDialog("Success", $"Level exported to:\n{path}", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Failed to export level", "OK");
        }
    }

    private void ImportLevelFromJson()
    {
        if (currentLevel == null)
        {
            EditorUtility.DisplayDialog("Error", "No level selected. Create or load a level first.", "OK");
            return;
        }

        string path = EditorUtility.OpenFilePanel("Import Level from JSON", "Assets", "json");
        if (string.IsNullOrEmpty(path))
            return;

        LevelGridData importedLevel = LevelJsonSerializer.ImportFromJson(path);
        if (importedLevel == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to import level", "OK");
            return;
        }

        // Copy data from imported level to current level
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

        // Refresh visualization
        EnsureGridVisualizer();

        Debug.Log($"✓ Level imported from JSON: {path}");
        EditorUtility.DisplayDialog("Success", $"Level '{currentLevel.levelName}' loaded from:\n{path}", "OK");
    }
}
