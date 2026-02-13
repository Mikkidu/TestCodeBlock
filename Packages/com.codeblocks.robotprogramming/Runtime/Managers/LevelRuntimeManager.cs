using UnityEngine;
using System.Collections.Generic;

namespace CodeBlocks.Managers
{
    public class LevelRuntimeManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 levelOrigin = Vector3.zero;

        [Header("Visuals")]
        [SerializeField] private GameObject backgroundPrefab; // Optional

        private LevelGridData currentLevel;
        private GameObject levelContainer;
        private GameObject backgroundInstance;
        private Dictionary<Vector2Int, GameObject> terrainInstances = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, GameObject> objectInstances = new Dictionary<Vector2Int, GameObject>();
        private GameObject startVisual;
        private GameObject finishVisual;

        public LevelGridData CurrentLevel => currentLevel;
        public float CellSize => cellSize;
        public Vector3 LevelOrigin => levelOrigin;

        // Public API - TODO: implement methods
        
        public void LoadLevel(LevelGridData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError("LevelRuntimeManager: Cannot load null level data!");
                return;
            }

            ClearLevel();

            currentLevel = levelData;

            // Calculate level origin to center the grid at world origin (0,0,0)
            // For a grid of size W×H, the grid spans from levelOrigin to levelOrigin + (W*cellSize, H*cellSize)
            // To center it at (0,0): levelOrigin = (-W*cellSize/2, 0, -H*cellSize/2)
            float gridWidth = currentLevel.gridWidth * cellSize;
            float gridHeight = currentLevel.gridHeight * cellSize;
            levelOrigin = new Vector3(-gridWidth * 0.5f, 0, -gridHeight * 0.5f);

            // Create container at world origin - all objects will be positioned relative to (0,0,0)
            levelContainer = new GameObject("LevelRuntime");
            levelContainer.transform.position = Vector3.zero; // Always at world center!
            levelContainer.transform.localScale = Vector3.one;
            levelContainer.transform.SetParent(transform);

            // Load components in next steps...
            
            if (backgroundPrefab != null)
            {
                backgroundInstance = Instantiate(backgroundPrefab, transform);
                backgroundInstance.name = "LevelBackground";

                float width = currentLevel.gridWidth * cellSize;
                float height = currentLevel.gridHeight * cellSize;
                backgroundInstance.transform.localScale = new Vector3(width + 4, 1, height + 4);
                backgroundInstance.transform.position = new Vector3(0, -0.1f, 0); // Slightly below level
            }
            
            // Load terrain
            foreach (var cell in currentLevel.terrain)
            {
                InstantiateTerrain(cell.position, cell.terrainType);
            }

            // Load objects
            foreach (var obj in currentLevel.objects)
            {
                InstantiateObject(obj.position, obj.objectTypeId, obj);
            }

            // NEW: Load start/finish as unified objects
            var startObj = currentLevel.GetStartPoint();
            if (startObj != null)
            {
                InstantiateObject(startObj.position, startObj.objectTypeId, startObj);
            }

            var finishObj = currentLevel.GetFinishPoint();
            if (finishObj != null)
            {
                InstantiateObject(finishObj.position, finishObj.objectTypeId, finishObj);
            }

            Debug.Log($"LevelRuntimeManager: Level '{currentLevel.levelName}' loaded successfully!");
        }
        
        private void InstantiateTerrain(Vector2Int gridPos, string terrainType)
        {
            GameObject prefab = Resources.Load<GameObject>($"LevelEditor/Terrain/{terrainType}");
            if (prefab == null)
            {
                Debug.LogWarning($"LevelRuntimeManager: Terrain prefab not found: {terrainType}");
                return;
            }

            GameObject instance = Instantiate(prefab, levelContainer.transform);
            instance.name = $"{terrainType}_{gridPos.x}_{gridPos.y}";

            Vector3 worldPos = GetWorldPosition(gridPos);
            worldPos.x += cellSize * 0.5f; // Center of cell
            worldPos.z += cellSize * 0.5f;
            instance.transform.position = worldPos;

            terrainInstances[gridPos] = instance;
        }

        private void InstantiateObject(Vector2Int gridPos, string objectTypeId, GridObject gridObject = null)
        {
            GameObject prefab = Resources.Load<GameObject>($"LevelEditor/Objects/{objectTypeId}");
            GameObject instance = null;

            if (prefab == null)
            {
                // Fallback: Create primitive for StartPoint/FinishPoint if prefab missing
                if (objectTypeId == "StartPoint")
                {
                    instance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    instance.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f);
                    instance.GetComponent<Renderer>().material.color = Color.green;
                }
                else if (objectTypeId == "FinishPoint")
                {
                    instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    instance.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
                    instance.GetComponent<Renderer>().material.color = Color.yellow;
                }
                else
                {
                    Debug.LogWarning($"LevelRuntimeManager: Object prefab not found: {objectTypeId}");
                    return;
                }
            }
            else
            {
                instance = Instantiate(prefab);
            }

            // Always set parent (fixes marker duplication bug)
            instance.transform.SetParent(levelContainer.transform);
            instance.name = $"{objectTypeId}_{gridPos.x}_{gridPos.y}";

            Vector3 worldPos = GetWorldPosition(gridPos);
            worldPos.x += cellSize * 0.5f; // Center of cell
            worldPos.z += cellSize * 0.5f;

            // Special Y positioning for markers
            if (objectTypeId == "StartPoint")
                worldPos.y = 0.1f; // Slightly above ground
            else if (objectTypeId == "FinishPoint")
                worldPos.y = 0.25f; // Half height above ground

            instance.transform.position = worldPos;

            // Handle StartPoint direction rotation
            if (objectTypeId == "StartPoint" && gridObject?.parameters != null)
            {
                if (gridObject.parameters.TryGetValue("direction", out string dirStr))
                {
                    if (System.Enum.TryParse<CardinalDirection>(dirStr, out var dir))
                    {
                        float angle = dir switch
                        {
                            CardinalDirection.North => 0f,
                            CardinalDirection.East => 90f,
                            CardinalDirection.South => 180f,
                            CardinalDirection.West => 270f,
                            _ => 0f
                        };
                        instance.transform.rotation = Quaternion.Euler(0, angle, 0);
                    }
                }
            }

            objectInstances[gridPos] = instance;

            // Store references for Gizmos (backward compatibility)
            if (objectTypeId == "StartPoint")
                startVisual = instance;
            else if (objectTypeId == "FinishPoint")
                finishVisual = instance;
        }

        // Legacy methods removed in v1.1.0 - use unified InstantiateObject() instead

        public void ClearLevel()
        {
            if (levelContainer != null)
            {
                Destroy(levelContainer);
                levelContainer = null;
            }

            if (backgroundInstance != null)
            {
                Destroy(backgroundInstance);
                backgroundInstance = null;
            }

            terrainInstances.Clear();
            objectInstances.Clear();
            startVisual = null;
            finishVisual = null;
            currentLevel = null;
        }
        
        public Vector3 GetWorldPosition(Vector2Int gridPos)
        {
            return levelOrigin + new Vector3(
                gridPos.x * cellSize,
                0,
                gridPos.y * cellSize
            );
        }

        public Vector2Int GetGridPosition(Vector3 worldPos)
        {
            Vector3 localPos = worldPos - levelOrigin;
            return new Vector2Int(
                Mathf.FloorToInt(localPos.x / cellSize),
                Mathf.FloorToInt(localPos.z / cellSize)
            );
        }
        
        private void OnDrawGizmos()
        {
            if (currentLevel == null) return;

            float gridWidth = currentLevel.gridWidth * cellSize;
            float gridHeight = currentLevel.gridHeight * cellSize;

            // Draw grid bounds (centered at world origin)
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(gridWidth, 0.1f, gridHeight));

            // Draw grid lines
            Gizmos.color = new Color(1, 1, 1, 0.2f);
            for (int x = 0; x <= currentLevel.gridWidth; x++)
            {
                Vector3 start = GetWorldPosition(new Vector2Int(x, 0));
                Vector3 end = GetWorldPosition(new Vector2Int(x, currentLevel.gridHeight));
                Gizmos.DrawLine(start, end);
            }
            for (int y = 0; y <= currentLevel.gridHeight; y++)
            {
                Vector3 start = GetWorldPosition(new Vector2Int(0, y));
                Vector3 end = GetWorldPosition(new Vector2Int(currentLevel.gridWidth, y));
                Gizmos.DrawLine(start, end);
            }

            // NEW: Draw start point (green sphere + arrow pointing in direction)
            var startObj = currentLevel.GetStartPoint();
            if (startObj != null)
            {
                Gizmos.color = Color.green;
                Vector3 startPos = GetWorldPosition(startObj.position) +
                                   new Vector3(cellSize * 0.5f, 0, cellSize * 0.5f);
                Gizmos.DrawWireSphere(startPos, 0.3f);

                // Draw direction arrow
                CardinalDirection dir = currentLevel.GetStartDirection();
                Vector3 direction = dir switch
                {
                    CardinalDirection.North => Vector3.forward,
                    CardinalDirection.East => Vector3.right,
                    CardinalDirection.South => Vector3.back,
                    CardinalDirection.West => Vector3.left,
                    _ => Vector3.forward
                };
                Gizmos.DrawLine(startPos, startPos + direction * 0.5f);
            }

            // NEW: Draw finish point (yellow sphere)
            var finishObj = currentLevel.GetFinishPoint();
            if (finishObj != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 finishPos = GetWorldPosition(finishObj.position) +
                                    new Vector3(cellSize * 0.5f, 0, cellSize * 0.5f);
                Gizmos.DrawWireSphere(finishPos, 0.3f);
            }

            // Draw world origin (white cross)
            Gizmos.color = Color.white;
            Gizmos.DrawLine(Vector3.zero - Vector3.right * 0.3f, Vector3.zero + Vector3.right * 0.3f);
            Gizmos.DrawLine(Vector3.zero - Vector3.forward * 0.3f, Vector3.zero + Vector3.forward * 0.3f);
        }
    }
}