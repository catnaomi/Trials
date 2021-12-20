using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HumanoidPositionReference))]

public class HumanPositionReferenceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HumanoidPositionReference positionReference = (HumanoidPositionReference)target;
        if (GUILayout.Button("Populate Fields"))
        {
            positionReference.LocateSlotsByName();
        }
    }
}