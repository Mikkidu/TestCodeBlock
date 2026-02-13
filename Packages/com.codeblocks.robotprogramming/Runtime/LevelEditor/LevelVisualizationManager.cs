using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelVisualizationManager : MonoBehaviour
{
    public bool usePrefabs = false;

    private Dictionary<Vector2Int, GameObject> terrainVisuals = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> objectVisuals = new Dictionary<Vector2Int, GameObject>();
    private GameObject visualsContainer;

    public void Initialize()
    {
        if (visualsContainer == null)
        {
            visualsContainer = new GameObject("LevelVisuals");
            visualsContainer.transform.SetParent(transform);
        }
    }

    public void RebuildVisualization(LevelGridData levelData)
    {
        ClearVisualization();

        if (!usePrefabs)
            return;

        Initialize();
        RebuildWithPrefabs(levelData);
    }

    private void RebuildWithPrefabs(LevelGridData levelData)
    {
        foreach (var cell in levelData.terrain)
        {
            PlaceTerrainVisual(cell.position, cell.terrainType);
        }

        foreach (var obj in levelData.objects)
        {
            PlaceObjectVisual(obj.position, obj.objectTypeId);
        }
    }

    public void PlaceTerrainVisual(Vector2Int pos, string terrainType)
    {
        if (!usePrefabs)
            return;

        Initialize();

        // Remove existing visual if present
        if (terrainVisuals.TryGetValue(pos, out var existingObj))
        {
#if UNITY_EDITOR
            DestroyImmediate(existingObj);
#else
            Destroy(existingObj);
#endif
        }

        GameObject prefab = GetTerrainPrefab(terrainType);
        if (prefab == null)
        {
            Debug.LogWarning($"Terrain prefab not found for type: {terrainType}");
            return;
        }

        GameObject visual = Instantiate(prefab, visualsContainer.transform);
        visual.name = $"{terrainType}_{pos}";

        // Position the visual at cell center
        GridVisualizer visualizer = GetComponent<GridVisualizer>();
        if (visualizer != null)
        {
            Vector3 worldPos = visualizer.GridToWorldPos(pos) + new Vector3(visualizer.cellSize * 0.5f, 0, visualizer.cellSize * 0.5f);
            visual.transform.position = worldPos;
        }

        // Set the terrain visual component if it exists
        TerrainBlockVisual terrainVisual = visual.GetComponent<TerrainBlockVisual>();
        if (terrainVisual != null)
        {
            terrainVisual.SetTerrain(pos, terrainType);
        }

        terrainVisuals[pos] = visual;
    }

    public void RemoveTerrainVisual(Vector2Int pos)
    {
        if (terrainVisuals.TryGetValue(pos, out var obj))
        {
#if UNITY_EDITOR
            DestroyImmediate(obj);
#else
            Destroy(obj);
#endif
            terrainVisuals.Remove(pos);
        }
    }

    public void PlaceObjectVisual(Vector2Int pos, string objectTypeId)
    {
        if (!usePrefabs)
            return;

        Initialize();

        // Remove existing visual if present
        if (objectVisuals.TryGetValue(pos, out var existingObj))
        {
#if UNITY_EDITOR
            DestroyImmediate(existingObj);
#else
            Destroy(existingObj);
#endif
        }

        GameObject prefab = GetObjectPrefab(objectTypeId);
        if (prefab == null)
        {
            Debug.LogWarning($"Object prefab not found for type: {objectTypeId}");
            return;
        }

        GameObject visual = Instantiate(prefab, visualsContainer.transform);
        visual.name = $"{objectTypeId}_{pos}";

        // Position the visual at cell center
        GridVisualizer visualizer = GetComponent<GridVisualizer>();
        if (visualizer != null)
        {
            Vector3 worldPos = visualizer.GridToWorldPos(pos) + new Vector3(visualizer.cellSize * 0.5f, 0, visualizer.cellSize * 0.5f);
            visual.transform.position = worldPos;
        }

        // Set the object visual component if it exists
        ObjectBlockVisual objectVisual = visual.GetComponent<ObjectBlockVisual>();
        if (objectVisual != null)
        {
            objectVisual.SetObject(pos, objectTypeId);
        }

        objectVisuals[pos] = visual;
    }

    public void RemoveObjectVisual(Vector2Int pos)
    {
        if (objectVisuals.TryGetValue(pos, out var obj))
        {
#if UNITY_EDITOR
            DestroyImmediate(obj);
#else
            Destroy(obj);
#endif
            objectVisuals.Remove(pos);
        }
    }

    public void ClearVisualization()
    {
        foreach (var obj in terrainVisuals.Values)
        {
#if UNITY_EDITOR
            DestroyImmediate(obj);
#else
            Destroy(obj);
#endif
        }
        terrainVisuals.Clear();

        foreach (var obj in objectVisuals.Values)
        {
#if UNITY_EDITOR
            DestroyImmediate(obj);
#else
            Destroy(obj);
#endif
        }
        objectVisuals.Clear();

        if (visualsContainer != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(visualsContainer);
#else
            Destroy(visualsContainer);
#endif
            visualsContainer = null;
        }
    }

    private GameObject GetTerrainPrefab(string terrainType)
    {
        return Resources.Load<GameObject>($"LevelEditor/Terrain/{terrainType}");
    }

    private GameObject GetObjectPrefab(string objectTypeId)
    {
        return Resources.Load<GameObject>($"LevelEditor/Objects/{objectTypeId}");
    }
}
