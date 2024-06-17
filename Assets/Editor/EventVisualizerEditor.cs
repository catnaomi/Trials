using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EventVisualizer))]
public class EventVisualizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EventVisualizer viz = (EventVisualizer)target;
        DrawDefaultInspector();
        GUILayout.Space(10f);
        if (GUILayout.Button("Refresh"))
        {
            viz.Refresh();
        }
    }
}
