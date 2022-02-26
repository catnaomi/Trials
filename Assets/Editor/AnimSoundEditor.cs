using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimationFXSounds))]

public class AnimSoundEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AnimationFXSounds animationSoundHandler = (AnimationFXSounds)target;
        if (GUILayout.Button("Populate Fields"))
        {
            animationSoundHandler.PopulateWithDefaults();
        }
    }
}