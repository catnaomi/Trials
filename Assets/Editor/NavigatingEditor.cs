using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavigatingHumanoidActor))]
public class NavigatingEditor : Editor
{
    Vector3 destination = Vector3.zero;
    GameObject obj = null;

    public override void OnInspectorGUI()
    {
        INavigates nav = (INavigates)target;
        DrawDefaultInspector();
        GUILayout.Space(40f);
        CustomStyles.HorizontalLine(Color.gray);
        GUILayout.Space(25f);

        destination = EditorGUILayout.Vector3Field("vector destination", destination);

        if (GUILayout.Button("Set Destination (Vector)"))
        {
            nav.SetDestination(destination);
        }
        GUILayout.Space(10f);
        obj = (GameObject)EditorGUILayout.ObjectField("object destination", obj, typeof(GameObject), true);

        if (GUILayout.Button("Set Destination (Object)"))
        {
            nav.SetDestination(obj);
        }
        GUILayout.Space(25f);
        if (GUILayout.Button("Start Navigation"))
        {
            nav.ResumeNavigation();
        }
        GUILayout.Space(10f);
        if (GUILayout.Button("Stop Navigation"))
        {
            nav.StopNavigation();
        }

    }
}