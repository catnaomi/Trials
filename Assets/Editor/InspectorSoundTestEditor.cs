using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InspectorPlaySound))]
public class InspectorSoundTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        InspectorPlaySound play = (InspectorPlaySound)target;
        DrawDefaultInspector();
        GUILayout.Space(10f);
        if (GUILayout.Button("Play"))
        {
            play.Play();
        }
    }
}