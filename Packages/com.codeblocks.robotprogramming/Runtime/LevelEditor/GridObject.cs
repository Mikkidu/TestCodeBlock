using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GridObject
{
    public Vector2Int position;
    public string objectTypeId;
    public string objectInstanceId;
    public Dictionary<string, string> parameters = new Dictionary<string, string>();
}
