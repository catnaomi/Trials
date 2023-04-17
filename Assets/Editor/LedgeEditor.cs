using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Ledge))]
public class LedgeEditor : Editor
{
    Vector3 destination = Vector3.zero;
    GameObject obj = null;

    public override void OnInspectorGUI()
    {
        Ledge ledge = (Ledge)target;
        DrawDefaultInspector();
        GUILayout.Space(10f);
        if (GUILayout.Button("Validate Links"))
        {
            ledge.ValidateLinks();
        }
        GUILayout.Space(1f);
        if (GUILayout.Button("Auto Connect Links"))
        {
            ledge.ValidateThenAutoConnectLedge();
        }
        GUILayout.Space(10f);
        if (GUILayout.Button("Validate Links (All)"))
        {
            Ledge.ValidateAllLedges();
        }
        GUILayout.Space(1f);
        if (GUILayout.Button("Auto Connect Links (All)"))
        {
            Ledge.AutoConnectLedges();
        }


    }
}