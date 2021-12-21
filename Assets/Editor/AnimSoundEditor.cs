using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimationSoundHandler))]

public class AnimSoundEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AnimationSoundHandler animationSoundHandler = (AnimationSoundHandler)target;
        if (GUILayout.Button("Populate Fields"))
        {
            animationSoundHandler.PopulateWithDefaults();
        }
    }
}