using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(SoundFXAssetManager))]
public class SoundFXAssetManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SoundFXAssetManager script = (SoundFXAssetManager)target;
        DrawDefaultInspector();
        GUILayout.Space(10f);
        if (GUILayout.Button("Reload"))
        {
            script.LoadSoundAssets();
        }
    }
}
