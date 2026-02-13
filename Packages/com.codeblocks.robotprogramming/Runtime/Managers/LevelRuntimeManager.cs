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

            // Create container
            levelContainer = new GameObject("LevelRuntime");
            levelContainer.transform.SetParent(transform);
            levelContainer.transform.position = levelOrigin;

            // Calculate level center for proper origin
            float centerX = (currentLevel.gridWidth - 1) * cellSize * 0.5f;
            float centerZ = (currentLevel.gridHeight - 1) * cellSize * 0.5f;
            levelOrigin = new Vector3(-centerX, 0, -centerZ); // Center level at (0,0,0)

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
                InstantiateObject(obj.position, obj.objectTypeId);
            }

            // Load start visual
            if (currentLevel.start != null)
            {
                InstantiateStartVisual(currentLevel.start.position, currentLevel.start.direction);
            }

            // Load finish visual
            if (currentLevel.finish != null)
            {
                InstantiateFinishVisual(currentLevel.finish.position);
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

        private void InstantiateObject(Vector2Int gridPos, string objectTypeId)
        {
            GameObject prefab = Resources.Load<GameObject>($"LevelEditor/Objects/{objectTypeId}");
            if (prefab == null)
            {
                Debug.LogWarning($"LevelRuntimeManager: Object prefab not found: {objectTypeId}");
                return;
            }

            GameObject instance = Instantiate(prefab, levelContainer.transform);
            instance.name = $"{objectTypeId}_{gridPos.x}_{gridPos.y}";

            Vector3 worldPos = GetWorldPosition(gridPos);
            worldPos.x += cellSize * 0.5f; // Center of cell
            worldPos.z += cellSize * 0.5f;
            instance.transform.position = worldPos;

            objectInstances[gridPos] = instance;
        }
        
        private void InstantiateStartVisual(Vector2Int gridPos, CardinalDirection direction)
        {
            // Try to load custom start marker prefab
            GameObject prefab = Resources.Load<GameObject>("LevelEditor/Markers/StartPoint");
            if (prefab == null)
            {
                // Fallback: Create simple arrow
                startVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                startVisual.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f);
                startVisual.GetComponent<Renderer>().material.color = Color.green;
            }
            else
            {
                startVisual = Instantiate(prefab, levelContainer.transform);
            }

            startVisual.name = "StartVisual";
            Vector3 worldPos = GetWorldPosition(gridPos);
            worldPos.x += cellSize * 0.5f;
            worldPos.z += cellSize * 0.5f;
            worldPos.y = 0.1f; // Slightly above ground
            startVisual.transform.position = worldPos;

            // Rotate based on direction
            float angle = direction switch
            {
                CardinalDirection.North => 0f,
                CardinalDirection.East => 90f,
                CardinalDirection.South => 180f,
                CardinalDirection.West => 270f,
                _ => 0f
            };
            startVisual.transform.rotation = Quaternion.Euler(0, angle, 0);
        }

        private void InstantiateFinishVisual(Vector2Int gridPos)
        {
            // Try to load custom finish marker prefab
            GameObject prefab = Resources.Load<GameObject>("LevelEditor/Markers/FinishPoint");
            if (prefab == null)
            {
                // Fallback: Create simple flag
                finishVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                finishVisual.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
                finishVisual.GetComponent<Renderer>().material.color = Color.yellow;
            }
            else
            {
                finishVisual = Instantiate(prefab, levelContainer.transform);
            }

            finishVisual.name = "FinishVisual";
            Vector3 worldPos = GetWorldPosition(gridPos);
            worldPos.x += cellSize * 0.5f;
            worldPos.z += cellSize * 0.5f;
            worldPos.y = 0.25f; // Half height above ground
            finishVisual.transform.position = worldPos;
        }

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

            // Draw grid bounds
            Gizmos.color = Color.cyan;
            float width = currentLevel.gridWidth * cellSize;
            float height = currentLevel.gridHeight * cellSize;
            Vector3 center = levelOrigin + new Vector3(width * 0.5f, 0, height * 0.5f);
            Gizmos.DrawWireCube(center, new Vector3(width, 0.1f, height));

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

            // Draw start point
            if (currentLevel.start != null)
            {
                Gizmos.color = Color.green;
                Vector3 startPos = GetWorldPosition(currentLevel.start.position);
                startPos.x += cellSize * 0.5f;
                startPos.z += cellSize * 0.5f;
                Gizmos.DrawWireSphere(startPos, 0.3f);
            }

            // Draw finish point
            if (currentLevel.finish != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 finishPos = GetWorldPosition(currentLevel.finish.position);
                finishPos.x += cellSize * 0.5f;
                finishPos.z += cellSize * 0.5f;
                Gizmos.DrawWireSphere(finishPos, 0.3f);
            }
        }
    }
}