using UnityEngine;
using CodeBlocks.Managers;

public class LevelRuntimeManagerTest : MonoBehaviour
{
    [SerializeField] private LevelRuntimeManager levelManager;
    [SerializeField] private LevelGridData testLevel;

    private void Start()
    {
        if (levelManager != null && testLevel != null)
        {
            levelManager.LoadLevel(testLevel);
            TestCoordinateConversion();
        }
    }

    private void TestCoordinateConversion()
    {
        // Test Grid → World → Grid
        Vector2Int originalGrid = new Vector2Int(3, 4);
        Vector3 worldPos = levelManager.GetWorldPosition(originalGrid);
        Vector2Int convertedGrid = levelManager.GetGridPosition(worldPos);

        Debug.Log($"Test: Grid {originalGrid} → World {worldPos} → Grid {convertedGrid}");
        Debug.Assert(originalGrid == convertedGrid, "Coordinate conversion failed!");

        // Test World → Grid → World
        Vector3 originalWorld = new Vector3(2.5f, 0, 3.5f);
        Vector2Int gridPos = levelManager.GetGridPosition(originalWorld);
        Vector3 convertedWorld = levelManager.GetWorldPosition(gridPos);

        Debug.Log($"Test: World {originalWorld} → Grid {gridPos} → World {convertedWorld}");
        Debug.Log($"Expected grid: (2, 3), Actual: {gridPos}");
    }
}