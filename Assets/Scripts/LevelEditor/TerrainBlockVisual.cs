using UnityEngine;

public class TerrainBlockVisual : MonoBehaviour
{
    public Vector2Int gridPosition;
    public string terrainType;

    private Material visualMaterial;

    public void SetTerrain(Vector2Int pos, string type)
    {
        gridPosition = pos;
        terrainType = type;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // Update material based on terrain type
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && visualMaterial != null)
        {
            renderer.material = visualMaterial;

            // Set color based on terrain type
            Color terrainColor = GetTerrainColor(terrainType);
            renderer.material.color = terrainColor;
        }
    }

    private Color GetTerrainColor(string type)
    {
        return type switch
        {
            "Ground" => new Color(0, 1, 0, 0.8f),    // Green
            "Road" => new Color(0.5f, 0.5f, 0.5f, 0.8f),  // Gray
            "Pit" => new Color(1, 0, 0, 0.8f),      // Red
            _ => Color.white
        };
    }
}
