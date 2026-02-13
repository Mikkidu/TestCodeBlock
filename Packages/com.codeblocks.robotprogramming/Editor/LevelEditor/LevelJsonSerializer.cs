using UnityEngine;
using UnityEditor;
using System.IO;

public class LevelJsonSerializer
{
    /// <summary>
    /// Exports a level to JSON format
    /// </summary>
    public static bool ExportToJson(LevelGridData levelData, string filePath)
    {
        try
        {
            // Ensure directory exists
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Convert to JSON-serializable format
            LevelJsonData jsonData = LevelJsonData.FromLevelGridData(levelData);

            // Serialize to JSON with pretty printing
            string json = JsonUtility.ToJson(jsonData, true);

            // Write to file
            File.WriteAllText(filePath, json);

            Debug.Log($"✓ Level exported to JSON: {filePath}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"✗ Failed to export level: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Imports a level from JSON format
    /// </summary>
    public static LevelGridData ImportFromJson(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"✗ File not found: {filePath}");
                return null;
            }

            // Read JSON from file
            string json = File.ReadAllText(filePath);

            // Deserialize from JSON
            LevelJsonData jsonData = JsonUtility.FromJson<LevelJsonData>(json);

            if (jsonData == null)
            {
                Debug.LogError("✗ Failed to deserialize JSON");
                return null;
            }

            // Convert to Unity format
            LevelGridData gridData = jsonData.ToLevelGridData();

            Debug.Log($"✓ Level imported from JSON: {filePath}");
            return gridData;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"✗ Failed to import level: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Saves JSON to Assets as ScriptableObject for quick access
    /// </summary>
    public static LevelGridData SaveJsonAsAsset(string jsonFilePath, string assetPath)
    {
        LevelGridData levelData = ImportFromJson(jsonFilePath);
        if (levelData == null)
            return null;

        // Ensure directory exists
        string directory = Path.GetDirectoryName(assetPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Save as ScriptableObject asset
        AssetDatabase.CreateAsset(levelData, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✓ Level saved as asset: {assetPath}");
        return levelData;
    }
}
