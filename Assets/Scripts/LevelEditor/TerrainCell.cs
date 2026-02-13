using UnityEngine;

[System.Serializable]
public class TerrainCell
{
    public Vector2Int position;
    public string terrainType;

    public bool IsPassable => terrainType != "Pit";
}
