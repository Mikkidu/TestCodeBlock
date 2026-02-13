using UnityEngine;
using UnityEditor;
using System.IO;

public class PrefabGenerator
{
    [MenuItem("Tools/CodeBlocks/Generate Level Editor Prefabs")]
    public static void GeneratePrefabs()
    {
        // Create directories
        string terrainPath = "Assets/Resources/CodeBlocks/Terrain";
        string objectsPath = "Assets/Resources/CodeBlocks/Objects";

        Directory.CreateDirectory(terrainPath);
        Directory.CreateDirectory(objectsPath);

        // Generate terrain prefabs
        GenerateTerrainPrefab("Ground", new Color(0, 1, 0, 0.8f), terrainPath);
        GenerateTerrainPrefab("Road", new Color(0.5f, 0.5f, 0.5f, 0.8f), terrainPath);
        GenerateTerrainPrefab("Pit", new Color(1, 0, 0, 0.8f), terrainPath);

        // Generate object prefabs
        GenerateObjectPrefab("Wall", Color.black, objectsPath);
        GenerateObjectPrefab("Button", new Color(1, 0.5f, 0, 0.8f), objectsPath);
        GenerateObjectPrefab("Door", new Color(0.5f, 0.5f, 1, 0.8f), objectsPath);

        Debug.Log("✓ Level Editor prefabs generated successfully!");
        AssetDatabase.Refresh();
    }

    private static void GenerateTerrainPrefab(string name, Color color, string path)
    {
        string prefabPath = Path.Combine(path, name + ".prefab");

        // Check if prefab already exists
        if (File.Exists(prefabPath))
        {
            Debug.LogWarning($"Prefab already exists: {prefabPath}");
            return;
        }

        // Create game object
        GameObject prefab = new GameObject(name);

        // Add cube visual
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Visual";
        cube.transform.SetParent(prefab.transform);
        cube.transform.localPosition = Vector3.zero;
        cube.transform.position = new Vector3(1, 0.2f, 1);

        // Remove collider from the visual cube
        Collider collider = cube.GetComponent<Collider>();
        if (collider != null)
            Object.DestroyImmediate(collider);

        // Set material color
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
        }

        // Add TerrainBlockVisual component
        TerrainBlockVisual visual = prefab.AddComponent<TerrainBlockVisual>();

        // Save as prefab
        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        Object.DestroyImmediate(prefab);

        Debug.Log($"Created terrain prefab: {name}");
    }

    private static void GenerateObjectPrefab(string name, Color color, string path)
    {
        string prefabPath = Path.Combine(path, name + ".prefab");

        // Check if prefab already exists
        if (File.Exists(prefabPath))
        {
            Debug.LogWarning($"Prefab already exists: {prefabPath}");
            return;
        }

        // Create game object
        GameObject prefab = new GameObject(name);

        // Add cube visual (taller for objects)
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Visual";
        cube.transform.SetParent(prefab.transform);
        cube.transform.position = new Vector3(0, 0.5f, 0);
        cube.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

        // Remove collider from the visual cube
        Collider collider = cube.GetComponent<Collider>();
        if (collider != null)
            Object.DestroyImmediate(collider);

        // Set material color
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
        }

        // Add ObjectBlockVisual component
        ObjectBlockVisual visual = prefab.AddComponent<ObjectBlockVisual>();

        // Save as prefab
        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        Object.DestroyImmediate(prefab);

        Debug.Log($"Created object prefab: {name}");
    }
}
