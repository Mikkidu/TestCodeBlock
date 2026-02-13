
using System.Reflection;
using PU.UnityFree.Helpers;
using UnityEditor;
using UnityEngine;

namespace PU.Editor.Inspectors
{
    [CustomPropertyDrawer(typeof(string), false)]
    public class StringPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnumAsStringAttribute enumAsStringAttribute = fieldInfo.GetCustomAttribute<EnumAsStringAttribute>();

            if (enumAsStringAttribute != null)
            {
                EditorGUI.BeginProperty(position, label, property);
                {
                    position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                    int indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;
                    {
                        string[] values = System.Enum.GetNames(enumAsStringAttribute.enumType);
                        int selectedIndex = System.Array.IndexOf(values, property.stringValue);
                        if (selectedIndex < 0)
                        {
                            ArrayUtility.Insert(ref values, 0, "null");
                            selectedIndex = 0;
                            GUI.color = Color.red;
                        }
                        property.stringValue = values[EditorGUI.Popup(position, selectedIndex, values)];
                        GUI.color = Color.white;
                    }
                    EditorGUI.indentLevel = indent;
                }
                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }
    }
}
