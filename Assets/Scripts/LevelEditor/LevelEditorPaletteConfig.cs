using UnityEngine;

[CreateAssetMenu(menuName = "CodeBlocks/Level Editor Palette Config")]
public class LevelEditorPaletteConfig : ScriptableObject
{
    [System.Serializable]
    public class TerrainType
    {
        public string typeName = "Ground";
        public Color color = Color.green;
    }

    [System.Serializable]
    public class ObjectType
    {
        public string typeName = "Wall";
        public bool isSpecial = false;  // true for Start/Finish
    }

    public TerrainType[] terrainTypes = new TerrainType[]
    {
        new TerrainType { typeName = "Ground", color = Color.green },
        new TerrainType { typeName = "Road", color = Color.gray },
        new TerrainType { typeName = "Pit", color = Color.red }
    };

    public ObjectType[] objectTypes = new ObjectType[]
    {
        new ObjectType { typeName = "Wall", isSpecial = false },
        new ObjectType { typeName = "Button", isSpecial = false },
        new ObjectType { typeName = "Door", isSpecial = false },
        new ObjectType { typeName = "Start", isSpecial = true },
        new ObjectType { typeName = "Finish", isSpecial = true }
    };
}
