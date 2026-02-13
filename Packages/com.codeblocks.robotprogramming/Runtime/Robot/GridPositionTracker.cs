using UnityEngine;
using System;
using CodeBlocks.Managers;

namespace CodeBlocks.Robot
{
    /// <summary>
    /// Tracks robot's position on the level grid.
    /// Fires events when robot moves to a new cell.
    /// </summary>
    [RequireComponent(typeof(RobotController))]
    public class GridPositionTracker : MonoBehaviour
    {
        public event Action OnReachedFinish;
        
        private LevelRuntimeManager levelManager;
        private LevelGridData currentLevel;
        private RobotController robotController;

        private Vector2Int currentGridPosition;
        private Vector2Int lastGridPosition;
        private bool isInitialized = false;

        // Public properties
        public Vector2Int CurrentGridPosition => currentGridPosition;
        public Vector2Int LastGridPosition => lastGridPosition;
        public bool IsInitialized => isInitialized;
        
        private bool hasReachedFinish = false;

        // Events
        public event Action<Vector2Int, Vector2Int> OnGridPositionChanged; // (newPos, oldPos)
        public event Action<Vector2Int> OnMovedToImpassableTerrain;


        private void Awake()
        {
            robotController = GetComponent<RobotController>();
            if (robotController == null)
            {
                Debug.LogError("GridPositionTracker: RobotController component not found!");
            }
        }
        
        public void Initialize(LevelRuntimeManager manager, LevelGridData level)
        {
            if (manager == null)
            {
                Debug.LogError("GridPositionTracker: LevelRuntimeManager is null!");
                return;
            }

            if (level == null)
            {
                Debug.LogError("GridPositionTracker: LevelGridData is null!");
                return;
            }

            levelManager = manager;
            currentLevel = level;
            isInitialized = true;
            
            hasReachedFinish = false;

            // Calculate initial position
            UpdateGridPosition();

            Debug.Log($"GridPositionTracker: Initialized at grid position {currentGridPosition}");
        }
        
        private void LateUpdate()
        {
            if (!isInitialized) return;

            // Only update position when robot is not executing movement
            // This prevents multiple triggers during lerp animation
            if (robotController && !robotController.IsExecuting)
            {
                UpdateGridPosition();
            }
        }
        
        private void UpdateGridPosition()
        {
            if (!isInitialized || !levelManager) return;

            // Get current grid position from world position
            Vector2Int newGridPos = levelManager.GetGridPosition(transform.position);

            // Check if position changed
            if (newGridPos != currentGridPosition)
            {
                lastGridPosition = currentGridPosition;
                currentGridPosition = newGridPos;

                // Fire position changed event
                OnGridPositionChanged?.Invoke(currentGridPosition, lastGridPosition);

                Debug.Log($"GridPositionTracker: Moved from {lastGridPosition} to {currentGridPosition}");

                // Check if reached finish point (NEW: unified access)
                if (currentLevel != null && !hasReachedFinish)
                {
                    var finishObj = currentLevel.GetFinishPoint();
                    if (finishObj != null && finishObj.position == currentGridPosition)
                    {
                        hasReachedFinish = true;
                        OnReachedFinish?.Invoke();
                        Debug.Log($"GridPositionTracker: 🎉 Reached finish at {currentGridPosition}!");
                    }
                }
                
                // Check terrain passability (for future trap/pit detection)
                if (currentLevel && !currentLevel.IsPassable(currentGridPosition.x, currentGridPosition.y))
                {
                    OnMovedToImpassableTerrain?.Invoke(currentGridPosition);
                    Debug.LogWarning($"GridPositionTracker: Robot moved to impassable terrain at {currentGridPosition}!");
                }
            }
        }
        
        /// <summary>
        /// Checks if robot is precisely positioned on the grid (within tolerance).
        /// </summary>
        /// <returns>True if robot is within 0.1 units of cell center</returns>
        public bool IsOnGrid()
        {
            if (!isInitialized || levelManager == null) return false;

            float distance = GetDistanceFromGrid();
            return distance < 0.1f; // 10cm tolerance
        }

        /// <summary>
        /// Calculates distance from robot to the center of current grid cell.
        /// </summary>
        /// <returns>Distance in world units</returns>
        public float GetDistanceFromGrid()
        {
            if (!isInitialized || levelManager == null) return float.MaxValue;

            // Get expected world position (center of cell)
            Vector3 expectedWorldPos = levelManager.GetWorldPosition(currentGridPosition);
            expectedWorldPos.x += levelManager.CellSize * 0.5f;
            expectedWorldPos.z += levelManager.CellSize * 0.5f;
            expectedWorldPos.y = transform.position.y; // Ignore height

            // Calculate distance
            return Vector3.Distance(transform.position, expectedWorldPos);
        }
        
        public void ResetPosition()
        {
            if (!isInitialized) return;

            lastGridPosition = currentGridPosition;
            hasReachedFinish = false;
            UpdateGridPosition();

            Debug.Log($"GridPositionTracker: Position reset to {currentGridPosition}");
        }
        
        private void OnDrawGizmos()
        {
            if (!isInitialized || levelManager == null) return;

            // Draw current grid cell
            Gizmos.color = new Color(0, 1, 0, 0.3f); // Green transparent
            Vector3 cellCenter = levelManager.GetWorldPosition(currentGridPosition);
            cellCenter.x += levelManager.CellSize * 0.5f;
            cellCenter.z += levelManager.CellSize * 0.5f;
            cellCenter.y = 0.01f; // Slightly above ground
            Gizmos.DrawCube(cellCenter, new Vector3(levelManager.CellSize * 0.9f, 0.02f, levelManager.CellSize * 0.9f));

            // Draw arrow from last to current position (if moved)
            if (lastGridPosition != currentGridPosition)
            {
                Gizmos.color = Color.blue;
                Vector3 lastCellCenter = levelManager.GetWorldPosition(lastGridPosition);
                lastCellCenter.x += levelManager.CellSize * 0.5f;
                lastCellCenter.z += levelManager.CellSize * 0.5f;
                lastCellCenter.y = 0.5f;

                Vector3 currCellCenter = cellCenter;
                currCellCenter.y = 0.5f;

                Gizmos.DrawLine(lastCellCenter, currCellCenter);
                // Draw arrowhead
                Vector3 direction = (currCellCenter - lastCellCenter).normalized;
                Vector3 right = Vector3.Cross(direction, Vector3.up) * 0.2f;
                Gizmos.DrawLine(currCellCenter, currCellCenter - direction * 0.3f + right);
                Gizmos.DrawLine(currCellCenter, currCellCenter - direction * 0.3f - right);
            }

            // Draw distance to grid center (if not on grid)
            if (!IsOnGrid())
            {
                Gizmos.color = Color.red;
                Vector3 robotPos = transform.position;
                robotPos.y = 0.5f;
                cellCenter.y = 0.5f;
                Gizmos.DrawLine(robotPos, cellCenter);
            }

            // Draw position labels in Editor
#if UNITY_EDITOR
            UnityEditor.Handles.Label(cellCenter + Vector3.up * 0.5f, $"Grid: {currentGridPosition}\nDist: {GetDistanceFromGrid():F3}");
#endif
        }
    }
}