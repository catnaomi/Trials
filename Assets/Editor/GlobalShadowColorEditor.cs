using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(GlobalShadowColor))]
public class GlobalShadowColorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GlobalShadowColor script = (GlobalShadowColor)target;
        DrawDefaultInspector();
        GUILayout.Space(10f);
        if (GUILayout.Button("Set Shadow Color"))
        {
            script.SetShadowColor();
        }
    }
}