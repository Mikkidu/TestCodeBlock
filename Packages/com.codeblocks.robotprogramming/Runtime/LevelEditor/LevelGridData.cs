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

    public StartPoint start;
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
}
