using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Ledge)), CanEditMultipleObjects]
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
            AutoconnectLedge(ledge);
            ledge.ValidateLinks();
        }
        GUILayout.Space(10f);
        if (GUILayout.Button("Validate Links (All)"))
        {
            Ledge.ValidateAllLedges();
        }

        GUILayout.Space(1f);
        if (GUILayout.Button("Auto Connect Links (All)"))
        {
            Ledge[] ledges = FindObjectsOfType<Ledge>();
            foreach (Ledge l in ledges)
            {
                AutoconnectLedge(l);
            }
            Ledge.ValidateAllLedges();
        }


    }

    void AutoconnectLedge(Ledge ledge)
    {
        var toConnect = ledge.GetLedgesToConnect();
        Undo.RecordObject(ledge, "Auto Connect Ledge" + ledge.name);
        if (toConnect.Item1 != null) Undo.RecordObject(toConnect.Item1, "Auto Connect Ledge" + ledge.name);
        if (toConnect.Item2 != null) Undo.RecordObject(toConnect.Item2, "Auto Connect Ledge" + ledge.name);
        ledge.AutoConnectLedge(true);
        PrefabUtility.RecordPrefabInstancePropertyModifications(ledge);
        if (toConnect.Item1 != null)
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(toConnect.Item1);
        }
        if (toConnect.Item2 != null)
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(toConnect.Item2);
        }
    }
}