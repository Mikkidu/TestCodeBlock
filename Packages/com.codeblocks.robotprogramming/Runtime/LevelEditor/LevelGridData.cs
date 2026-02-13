using UnityEngine;

[CreateAssetMenu(fileName = "LevelGridData", menuName = "CodeBlocks/Level Grid Data")]
[System.Serializable]
public class LevelGridData : ScriptableObject
{
    public string levelId = "level_001";
    public string levelName = "First Steps";
    public int difficulty = 1;
    public string hintText = "";

    public int gridWidth = 8;
    public int gridHeight = 8;

    public TerrainCell[] terrain = new TerrainCell[0];
    public GridObject[] objects = new GridObject[0];

    // Legacy fields - deprecated in v1.0.8, kept for Migration Tool
    // Run Tools → CodeBlocks → Migrate Levels (Start-Finish) to convert to unified format
    // ⚠️ These fields will be REMOVED in v1.1.0 (breaking change)
    [System.Obsolete("Use GetStartPoint() instead. Run Migration Tool to convert. Will be removed in v1.1.0.")]
    public StartPoint start;

    [System.Obsolete("Use GetFinishPoint() instead. Run Migration Tool to convert. Will be removed in v1.1.0.")]
    public FinishPoint finish;

    public int visualLayerId = 1;

    public TerrainCell GetTerrainAt(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
            return null;

        foreach (var cell in terrain)
        {
            if (cell.position.x == x && cell.position.y == y)
                return cell;
        }
        return null;
    }

    public GridObject GetObjectAt(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
            return null;

        foreach (var obj in objects)
        {
            if (obj.position.x == x && obj.position.y == y)
                return obj;
        }
        return null;
    }

    public bool IsPassable(int x, int y)
    {
        var terrain = GetTerrainAt(x, y);
        if (terrain == null)
            return false;
        return terrain.IsPassable;
    }

    // =========================
    // NEW: Unified Start/Finish access
    // =========================

    /// <summary>
    /// Finds StartPoint in objects array. Falls back to legacy 'start' field if not migrated yet.
    /// </summary>
    public GridObject GetStartPoint()
    {
        // NEW: Search in objects[] (migrated levels)
        foreach (var obj in objects)
        {
            if (obj != null && obj.objectTypeId == "StartPoint")
                return obj;
        }

        // FALLBACK: Legacy start field (for backward compatibility before migration)
        #pragma warning disable CS0618 // Type or member is obsolete
        if (start != null)
        {
            // Convert to GridObject on-the-fly
            var legacy = new GridObject
            {
                position = start.position,
                objectTypeId = "StartPoint",
                objectInstanceId = "start_legacy"
            };
            legacy.AddParameter("direction", start.direction.ToString());
            return legacy;
        }
        #pragma warning restore CS0618

        return null;
    }

    /// <summary>
    /// Finds FinishPoint in objects array. Falls back to legacy 'finish' field if not migrated yet.
    /// </summary>
    public GridObject GetFinishPoint()
    {
        // NEW: Search in objects[] (migrated levels)
        foreach (var obj in objects)
        {
            if (obj != null && obj.objectTypeId == "FinishPoint")
                return obj;
        }

        // FALLBACK: Legacy finish field (for backward compatibility before migration)
        #pragma warning disable CS0618 // Type or member is obsolete
        if (finish != null)
        {
            var legacy = new GridObject
            {
                position = finish.position,
                objectTypeId = "FinishPoint",
                objectInstanceId = "finish_legacy"
            };
            return legacy;
        }
        #pragma warning restore CS0618

        return null;
    }

    /// <summary>
    /// Gets start direction from StartPoint parameters.
    /// </summary>
    public CardinalDirection GetStartDirection()
    {
        var startObj = GetStartPoint();
        if (startObj?.parameters != null &&
            startObj.parameters.TryGetValue("direction", out string dirStr))
        {
            if (System.Enum.TryParse<CardinalDirection>(dirStr, out var dir))
                return dir;
        }
        return CardinalDirection.North; // default
    }
}
