using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JSON-serializable level data structure (independent of Unity ScriptableObject)
/// </summary>
[System.Serializable]
public class LevelJsonData
{
    [System.Serializable]
    public class Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int() { }
        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public UnityEngine.Vector2Int ToUnityVector2Int()
        {
            return new UnityEngine.Vector2Int(x, y);
        }

        public static Vector2Int FromUnityVector2Int(UnityEngine.Vector2Int v)
        {
            return new Vector2Int(v.x, v.y);
        }
    }

    [System.Serializable]
    public class JsonTerrainCell
    {
        public Vector2Int position;
        public string terrainType;

        public JsonTerrainCell() { }
        public JsonTerrainCell(string terrainType, Vector2Int position)
        {
            this.terrainType = terrainType;
            this.position = position;
        }
    }

    [System.Serializable]
    public class JsonGridObject
    {
        public Vector2Int position;
        public string objectTypeId;
        public string objectInstanceId;

        public JsonGridObject() { }
        public JsonGridObject(string objectTypeId, Vector2Int position)
        {
            this.objectTypeId = objectTypeId;
            this.position = position;
            this.objectInstanceId = objectTypeId + "_" + Random.Range(1000, 9999);
        }
    }

    [System.Serializable]
    public class StartPoint
    {
        public Vector2Int position;
        public string direction = "North";

        public StartPoint() { }
        public StartPoint(Vector2Int position, string direction = "North")
        {
            this.position = position;
            this.direction = direction;
        }
    }

    [System.Serializable]
    public class FinishPoint
    {
        public Vector2Int position;

        public FinishPoint() { }
        public FinishPoint(Vector2Int position)
        {
            this.position = position;
        }
    }

    // Level metadata
    public string levelId = "level_001";
    public string levelName = "Untitled Level";
    public int difficulty = 1;
    public string hintText = "";

    // Grid configuration
    public int gridWidth = 8;
    public int gridHeight = 8;

    // Level content
    public List<JsonTerrainCell> terrain = new List<JsonTerrainCell>();
    public List<JsonGridObject> objects = new List<JsonGridObject>();
    public StartPoint start;
    public FinishPoint finish;

    // Metadata
    public string version = "1.0";
    public long savedTimestamp;

    public LevelJsonData() { }
    public LevelJsonData(string levelId, string levelName)
    {
        this.levelId = levelId;
        this.levelName = levelName;
    }

    /// <summary>
    /// Converts from Unity's LevelGridData to JSON-friendly LevelJsonData
    /// </summary>
    public static LevelJsonData FromLevelGridData(LevelGridData gridData)
    {
        var jsonData = new LevelJsonData
        {
            levelId = gridData.levelId,
            levelName = gridData.levelName,
            difficulty = gridData.difficulty,
            hintText = gridData.hintText,
            gridWidth = gridData.gridWidth,
            gridHeight = gridData.gridHeight,
            savedTimestamp = System.DateTime.Now.Ticks
        };

        // Convert terrain - use nested classes
        foreach (var terrainCell in gridData.terrain)
        {
            jsonData.terrain.Add(new JsonTerrainCell(
                terrainCell.terrainType,
                Vector2Int.FromUnityVector2Int(terrainCell.position)
            ));
        }

        // Convert objects - use nested classes
        foreach (var obj in gridData.objects)
        {
            jsonData.objects.Add(new JsonGridObject
            {
                position = Vector2Int.FromUnityVector2Int(obj.position),
                objectTypeId = obj.objectTypeId,
                objectInstanceId = obj.objectInstanceId
            });
        }

        // NEW: Convert start point from unified objects array
        var startObj = gridData.GetStartPoint();
        if (startObj != null)
        {
            string direction = gridData.GetStartDirection().ToString();
            jsonData.start = new StartPoint(
                Vector2Int.FromUnityVector2Int(startObj.position),
                direction
            );
        }

        // NEW: Convert finish point from unified objects array
        var finishObj = gridData.GetFinishPoint();
        if (finishObj != null)
        {
            jsonData.finish = new FinishPoint(
                Vector2Int.FromUnityVector2Int(finishObj.position)
            );
        }

        return jsonData;
    }

    /// <summary>
    /// Converts from LevelJsonData to Unity's LevelGridData
    /// </summary>
    public LevelGridData ToLevelGridData()
    {
        var gridData = ScriptableObject.CreateInstance<LevelGridData>();
        gridData.levelId = this.levelId;
        gridData.levelName = this.levelName;
        gridData.difficulty = this.difficulty;
        gridData.hintText = this.hintText;
        gridData.gridWidth = this.gridWidth;
        gridData.gridHeight = this.gridHeight;

        // Convert terrain - use global namespace TerrainCell (not LevelJsonData.TerrainCell)
        var terrainList = new System.Collections.Generic.List<global::TerrainCell>();
        foreach (var jsonTerrain in this.terrain)
        {
            terrainList.Add(new global::TerrainCell
            {
                terrainType = jsonTerrain.terrainType,
                position = jsonTerrain.position.ToUnityVector2Int()
            });
        }
        gridData.terrain = terrainList.ToArray();

        // Convert objects - use global namespace GridObject (not LevelJsonData.GridObject)
        var objectsList = new System.Collections.Generic.List<global::GridObject>();
        foreach (var jsonObj in this.objects)
        {
            objectsList.Add(new global::GridObject
            {
                objectTypeId = jsonObj.objectTypeId,
                position = jsonObj.position.ToUnityVector2Int(),
                objectInstanceId = jsonObj.objectInstanceId
            });
        }

        // NEW: Convert start point as unified GridObject
        if (this.start != null)
        {
            var startGridObj = new global::GridObject
            {
                objectTypeId = "StartPoint",
                position = this.start.position.ToUnityVector2Int(),
                objectInstanceId = $"start_{gridData.levelId}"
            };
            startGridObj.AddParameter("direction", this.start.direction);
            objectsList.Add(startGridObj);
        }

        // NEW: Convert finish point as unified GridObject
        if (this.finish != null)
        {
            var finishGridObj = new global::GridObject
            {
                objectTypeId = "FinishPoint",
                position = this.finish.position.ToUnityVector2Int(),
                objectInstanceId = $"finish_{gridData.levelId}"
            };
            objectsList.Add(finishGridObj);
        }

        gridData.objects = objectsList.ToArray();

        return gridData;
    }
}
