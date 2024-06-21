using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(SceneTransferHallway))]
public class SceneTransferHallwayEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SceneTransferHallway script = (SceneTransferHallway)target;
        DrawDefaultInspector();
        GUILayout.Space(10f);
        if (GUILayout.Button("Warp"))
        {
            script.StartTransfer();
        }
    }
}