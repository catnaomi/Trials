using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimationFXHandler))]

public class AnimSoundEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AnimationFXHandler animationSoundHandler = (AnimationFXHandler)target;
        if (GUILayout.Button("Populate Fields"))
        {
            animationSoundHandler.PopulateWithDefaults();
        }
    }
}