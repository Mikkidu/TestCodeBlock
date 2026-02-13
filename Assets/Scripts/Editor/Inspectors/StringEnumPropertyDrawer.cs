
using PU.SharedData;
using System.IO;
using PU.SharedData.Config;
using UnityEditor;
using UnityEngine;

namespace PU.Editor.Inspectors
{
    [CustomPropertyDrawer(typeof(StringEnumAttribute))]
    public class StringEnumPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            const float buttonWidth = 50;
            position.width -= buttonWidth;
            int indent = EditorGUI.indentLevel;
            StringEnumAttribute attr = attribute as StringEnumAttribute;
            EditorGUI.indentLevel = 0;
            string path = "Assets/CodeBlocks/Resources/Configs/Enums/" + attr.configPath + ".asset";
            StringEnumConfig config = AssetDatabase.LoadAssetAtPath<StringEnumConfig>(path);

            if(config != null && config.values != null && config.values.Length > 0)
            {
                int selectedIndex = System.Array.FindIndex(config.values, s => s == property.stringValue);

                if (selectedIndex == -1)
                {
                    selectedIndex = 0;
                    property.stringValue = config.values[0];
                }

                int newIndex = EditorGUI.Popup(position, selectedIndex, config.values);
                if(newIndex != selectedIndex)
                {
                    property.stringValue = config.values[newIndex];
                }
                if (GUI.Button(new Rect(position.x + position.width, position.y, buttonWidth, position.height), "Show"))
                {
                    EditorGUIUtility.PingObject(config);
                }
            }
            else
            {
                if (GUI.Button(new Rect(position.x + position.width, position.y, buttonWidth, position.height), "Create"))
                {
                    config = ScriptableObject.CreateInstance<StringEnumConfig>();
                    config.name = Path.GetFileName(attr.configPath.ToString());
                    string dir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    AssetDatabase.CreateAsset(config, path);
                    AssetDatabase.Refresh();
                    EditorGUIUtility.PingObject(config);
                }
                EditorGUI.HelpBox(position, $"Invalid config path: {path}", MessageType.Error);
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}
