using UnityEngine;

public class ObjectBlockVisual : MonoBehaviour
{
    public Vector2Int gridPosition;
    public string objectTypeId;
    public string objectInstanceId;

    public void SetObject(Vector2Int pos, string typeId)
    {
        gridPosition = pos;
        objectTypeId = typeId;
        objectInstanceId = typeId + "_" + Random.Range(1000, 9999);
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // Update visuals based on object type
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color objectColor = GetObjectColor(objectTypeId);
            renderer.material.color = objectColor;
        }
    }

    private Color GetObjectColor(string typeId)
    {
        return typeId switch
        {
            "Wall" => Color.black,
            "Button" => new Color(1, 0.5f, 0, 0.8f),     // Orange
            "Door" => new Color(0.5f, 0.5f, 1, 0.8f),    // Light blue
            "DestructibleWall" => new Color(0.5f, 0, 0, 0.8f),  // Dark red
            _ => Color.gray
        };
    }
}
