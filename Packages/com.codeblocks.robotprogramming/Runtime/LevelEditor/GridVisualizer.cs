using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class GridVisualizer : MonoBehaviour
{
    public LevelGridData levelData;
    public float cellSize = 1f;
    public bool usePrefabs = true;

    public Color gridLineColor = Color.white;
    public Color groundColor = new Color(0, 1, 0, 0.3f);
    public Color roadColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    public Color pitColor = new Color(1, 0, 0, 0.3f);
    public Color wallColor = new Color(0, 0, 0, 0.5f);

    public static string currentTerrainType = "Ground";
    public static string currentObjectType = "Wall";
    public static bool isEditing = false;
    public static GridVisualizer instance;

    // Placement mode: true = Terrain, false = Object
    public static bool placeTerrainMode = true;

    private LevelVisualizationManager visualizationManager;
    private bool lastUsePrefabsState = false;
    private LevelGridData lastLoadedLevelData = null;

#if UNITY_EDITOR
    private static Vector2 rightClickStartPos = Vector2.zero;
    private static bool isRightClickDrag = false;

    private void OnEnable()
    {
        instance = this;
        SceneView.duringSceneGui += OnSceneViewGUI;
        EnsureVisualizationManager();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneViewGUI;
    }

    private void EnsureVisualizationManager()
    {
        if (visualizationManager == null)
        {
            visualizationManager = GetComponent<LevelVisualizationManager>();
            if (visualizationManager == null)
            {
                visualizationManager = gameObject.AddComponent<LevelVisualizationManager>();
            }
        }
        visualizationManager.usePrefabs = usePrefabs;
    }

    private static void OnSceneViewGUI(SceneView sceneView)
    {
        if (instance == null)
            return;

        if (instance.levelData == null)
            return;

        if (!isEditing)
            return;

        // Check if Level Editor window is open
        var levelEditorType = System.Type.GetType("CodeBlocksLevelEditorWindow");
        if (levelEditorType != null)
        {
            var isWindowOpenField = levelEditorType.GetField("isWindowOpen", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (isWindowOpenField != null && !(bool)isWindowOpenField.GetValue(null))
            {
                return;
            }
        }

        HandleSceneViewClick(sceneView);
    }

    private static void HandleSceneViewClick(SceneView sceneView)
    {
        Event evt = Event.current;

        // Ignore middle mouse button and modifiers
        if ((evt.button == 2) || evt.alt || evt.control || evt.shift)
            return;

        // Left click: place terrain or object (works on MouseDown)
        if (evt.button == 0 && evt.type == EventType.MouseDown && evt.clickCount == 1)
        {
            if (sceneView.camera == null)
            {
                Debug.LogWarning("SceneView camera is null!");
                return;
            }

            Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
            Vector2Int gridPos = instance.GetGridPositionFromRay(ray);

            Debug.Log($"✓ Left Click: gridPos={gridPos}, mousePos={evt.mousePosition}, isValid={instance.IsValidGridPosition(gridPos)}");

            if (instance.IsValidGridPosition(gridPos))
            {
                if (placeTerrainMode)
                {
                    Debug.Log($"Placing Terrain {currentTerrainType} at {gridPos}");
                    instance.PlaceTerrain(gridPos, currentTerrainType);
                }
                else
                {
                    Debug.Log($"Placing Object {currentObjectType} at {gridPos}");
                    instance.PlaceObject(gridPos, currentObjectType);
                }
                evt.Use();
                sceneView.Repaint();
            }
        }

        // Right click: track start position
        if (evt.button == 1 && evt.type == EventType.MouseDown)
        {
            rightClickStartPos = evt.mousePosition;
            isRightClickDrag = false;
        }

        // Right click: detect drag movement
        if (evt.button == 1 && evt.type == EventType.MouseDrag)
        {
            if (Vector2.Distance(evt.mousePosition, rightClickStartPos) > 5f)
            {
                isRightClickDrag = true;
            }
        }

        // Right click: delete on release if there was no drag
        if (evt.button == 1 && evt.type == EventType.MouseUp && !isRightClickDrag)
        {
            if (sceneView.camera == null)
            {
                Debug.LogWarning("SceneView camera is null!");
                return;
            }

            Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
            Vector2Int gridPos = instance.GetGridPositionFromRay(ray);

            Debug.Log($"✓ Right Click (Release): gridPos={gridPos}, mousePos={evt.mousePosition}, isValid={instance.IsValidGridPosition(gridPos)}");

            if (instance.IsValidGridPosition(gridPos))
            {
                Debug.Log($"Removing at {gridPos}");
                instance.RemoveTerrain(gridPos);
                evt.Use();
                sceneView.Repaint();
            }
        }

        // Reset drag flag on mouse up
        if (evt.button == 1 && evt.type == EventType.MouseUp)
        {
            isRightClickDrag = false;
        }
    }

    public Vector2Int GetGridPositionFromRay(Ray ray)
    {
        Plane gridPlane = new Plane(Vector3.up, Vector3.zero);
        if (gridPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.origin + ray.direction * enter;
            Vector2Int gridPos = WorldToGridPos(hitPoint);

            // Debug: checking accuracy
            Vector3 gridWorld = GridToWorldPos(gridPos) + new Vector3(cellSize * 0.5f, 0, cellSize * 0.5f);
            Debug.Log($"RayHit: {hitPoint}, GridPos: {gridPos}, GridWorld: {gridWorld}, Dist: {Vector3.Distance(hitPoint, gridWorld):F2}");

            return gridPos;
        }
        return new Vector2Int(-1, -1);
    }
#endif

    private void OnDrawGizmos()
    {
        if (levelData == null)
            return;

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

        // Check if usePrefabs state changed and rebuild visualization if needed
        if (usePrefabs != lastUsePrefabsState)
        {
            lastUsePrefabsState = usePrefabs;
            EnsureVisualizationManager(); // Make sure manager is initialized

            if (visualizationManager != null)
            {
                // Update visualization manager state FIRST
                visualizationManager.usePrefabs = usePrefabs;

                if (usePrefabs)
                {
                    // Rebuild prefab visualization with existing blocks
                    visualizationManager.RebuildVisualization(levelData);
                    Debug.Log("✓ Prefabs visualization rebuilt for existing blocks");
                }
                else
                {
                    // Clear prefab visualization
                    visualizationManager.ClearVisualization();
                    Debug.Log("✓ Prefabs visualization cleared");
                }
            }
        }

        DrawGrid();
        DrawTerrainCells();
        DrawObjects();
        DrawPoints();
    }

    private void DrawGrid()
    {
        Gizmos.color = gridLineColor;

        for (int x = 0; x <= levelData.gridWidth; x++)
        {
            Vector3 start = GridToWorldPos(new Vector2Int(x, 0));
            Vector3 end = GridToWorldPos(new Vector2Int(x, levelData.gridHeight));
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= levelData.gridHeight; y++)
        {
            Vector3 start = GridToWorldPos(new Vector2Int(0, y));
            Vector3 end = GridToWorldPos(new Vector2Int(levelData.gridWidth, y));
            Gizmos.DrawLine(start, end);
        }
    }

    private void DrawTerrainCells()
    {
        foreach (var cell in levelData.terrain)
        {
            DrawCell(cell.position, GetTerrainColor(cell.terrainType));
        }
    }

    private void DrawObjects()
    {
        foreach (var obj in levelData.objects)
        {
            DrawCell(obj.position, wallColor);
        }
    }

    private void DrawPoints()
    {
        // NEW: Draw start point from unified objects array
        var startObj = levelData.GetStartPoint();
        if (startObj != null)
        {
            Gizmos.color = Color.blue;
            Vector3 pos = GridToWorldPos(startObj.position) + new Vector3(cellSize * 0.5f, 0.5f, cellSize * 0.5f);
            Gizmos.DrawWireCube(pos, Vector3.one * cellSize * 0.7f);
        }

        // NEW: Draw finish point from unified objects array
        var finishObj = levelData.GetFinishPoint();
        if (finishObj != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 pos = GridToWorldPos(finishObj.position) + new Vector3(cellSize * 0.5f, 0.5f, cellSize * 0.5f);
            Gizmos.DrawWireCube(pos, Vector3.one * cellSize * 0.7f);
        }
    }

    private void DrawCell(Vector2Int gridPos, Color color)
    {
        Vector3 center = GridToWorldPos(gridPos) + new Vector3(cellSize * 0.5f, 0.5f, cellSize * 0.5f);
        Gizmos.color = color;
        Gizmos.DrawCube(center, new Vector3(cellSize, 1f, cellSize));
    }

    private Color GetTerrainColor(string terrainType)
    {
        return terrainType switch
        {
            "Ground" => groundColor,
            "Road" => roadColor,
            "Pit" => pitColor,
            _ => Color.white
        };
    }

    public Vector3 GridToWorldPos(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, 0, gridPos.y * cellSize);
    }

    public Vector2Int WorldToGridPos(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / cellSize),
            Mathf.FloorToInt(worldPos.z / cellSize)
        );
    }

    public bool IsValidGridPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < levelData.gridWidth &&
               pos.y >= 0 && pos.y < levelData.gridHeight;
    }

    public void PlaceTerrain(Vector2Int position, string terrainType)
    {
        var existing = levelData.GetTerrainAt(position.x, position.y);
        if (existing != null)
        {
            existing.terrainType = terrainType;
        }
        else
        {
            var newCell = new TerrainCell
            {
                position = position,
                terrainType = terrainType
            };
            ArrayUtility.Add(ref levelData.terrain, newCell);
        }

        EditorUtility.SetDirty(levelData);

        // Update visualization
        if (visualizationManager != null && visualizationManager.usePrefabs)
        {
            visualizationManager.PlaceTerrainVisual(position, terrainType);
        }
    }

    public void RemoveTerrain(Vector2Int position)
    {
        // NEW: Check if it's Start point (in objects array)
        var startObj = levelData.GetStartPoint();
        if (startObj != null && startObj.position == position)
        {
            ArrayUtility.Remove(ref levelData.objects, startObj);
            EditorUtility.SetDirty(levelData);
            Debug.Log($"✓ Removed Start point at {position}");
            return;
        }

        // NEW: Check if it's Finish point (in objects array)
        var finishObj = levelData.GetFinishPoint();
        if (finishObj != null && finishObj.position == position)
        {
            ArrayUtility.Remove(ref levelData.objects, finishObj);
            EditorUtility.SetDirty(levelData);
            Debug.Log($"✓ Removed Finish point at {position}");
            return;
        }

        // Normal terrain removal
        var existing = levelData.GetTerrainAt(position.x, position.y);
        if (existing != null)
        {
            ArrayUtility.Remove(ref levelData.terrain, existing);
            EditorUtility.SetDirty(levelData);

            // Update visualization
            if (visualizationManager != null && visualizationManager.usePrefabs)
            {
                visualizationManager.RemoveTerrainVisual(position);
            }
        }

        // Also check for objects
        var obj = levelData.GetObjectAt(position.x, position.y);
        if (obj != null)
        {
            ArrayUtility.Remove(ref levelData.objects, obj);
            EditorUtility.SetDirty(levelData);

            // Update visualization
            if (visualizationManager != null && visualizationManager.usePrefabs)
            {
                visualizationManager.RemoveObjectVisual(position);
            }
        }
    }

    public void PlaceObject(Vector2Int position, string objectTypeId)
    {
        // NEW: Unified handling for Start point
        if (objectTypeId == "Start")
        {
            // Remove existing StartPoint if any
            var existingStart = levelData.GetStartPoint();
            if (existingStart != null)
            {
                ArrayUtility.Remove(ref levelData.objects, existingStart);
            }

            // Create new StartPoint as GridObject
            var startObj = new GridObject
            {
                position = position,
                objectTypeId = "StartPoint",
                objectInstanceId = $"start_{levelData.levelId}"
            };
            startObj.AddParameter("direction", CardinalDirection.North.ToString());

            ArrayUtility.Add(ref levelData.objects, startObj);
            EditorUtility.SetDirty(levelData);
            Debug.Log($"✓ Set Start point at {position}");
            return;
        }

        // NEW: Unified handling for Finish point
        if (objectTypeId == "Finish")
        {
            // Remove existing FinishPoint if any
            var existingFinish = levelData.GetFinishPoint();
            if (existingFinish != null)
            {
                ArrayUtility.Remove(ref levelData.objects, existingFinish);
            }

            // Create new FinishPoint as GridObject
            var finishObj = new GridObject
            {
                position = position,
                objectTypeId = "FinishPoint",
                objectInstanceId = $"finish_{levelData.levelId}"
            };

            ArrayUtility.Add(ref levelData.objects, finishObj);
            EditorUtility.SetDirty(levelData);
            Debug.Log($"✓ Set Finish point at {position}");
            return;
        }

        // Normal object placement
        if (!levelData.IsPassable(position.x, position.y))
        {
            Debug.LogWarning("Cannot place object on Pit!");
            return;
        }

        var existing = levelData.GetObjectAt(position.x, position.y);
        if (existing != null)
        {
            ArrayUtility.Remove(ref levelData.objects, existing);
        }

        var newObj = new GridObject
        {
            position = position,
            objectTypeId = objectTypeId,
            objectInstanceId = objectTypeId + "_" + Random.Range(1000, 9999)
        };
        ArrayUtility.Add(ref levelData.objects, newObj);
        EditorUtility.SetDirty(levelData);

        // Update visualization
        if (visualizationManager != null && visualizationManager.usePrefabs)
        {
            visualizationManager.PlaceObjectVisual(position, objectTypeId);
        }
    }

    public void RemoveObject(Vector2Int position)
    {
        var obj = levelData.GetObjectAt(position.x, position.y);
        if (obj != null)
        {
            ArrayUtility.Remove(ref levelData.objects, obj);
            EditorUtility.SetDirty(levelData);

            // Update visualization
            if (visualizationManager != null && visualizationManager.usePrefabs)
            {
                visualizationManager.RemoveObjectVisual(position);
            }
        }
    }

    public void ClearLevelVisualization()
    {
        if (visualizationManager != null)
        {
            visualizationManager.ClearVisualization();
            Debug.Log("✓ Level visualization cleared");
        }
    }
}
