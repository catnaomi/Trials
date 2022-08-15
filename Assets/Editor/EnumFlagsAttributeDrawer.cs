using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
public class EnumFlagsAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position,
                                    SerializedProperty property,
                                    GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        uint a = (uint)(EditorGUI.MaskField(position, label, property.intValue, property.enumNames));
        if (EditorGUI.EndChangeCheck())
        {
            property.intValue = (int)a;
        }
    }
}