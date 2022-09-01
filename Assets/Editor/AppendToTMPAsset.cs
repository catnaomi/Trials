using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
using TMPro.EditorUtilities;


// adds a menu item to append a string to all sprites in the asset
[CustomEditor(typeof(TMP_SpriteAsset))]
public class AppendToTMPAsset : TMP_SpriteAssetEditor
{
    string append = "";
    float bx = 0f;
    float by = 0f;
    float ad = 128;
    TMP_SpriteAsset asset;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(40f);

        asset = (TMP_SpriteAsset)target;
        append = EditorGUILayout.TextField("String to Append", append);

        if (GUILayout.Button("Append String to Sprite Names"))
        {
            ApplyAppendString();
        }
        GUILayout.Space(20f);
        bx = EditorGUILayout.FloatField("BX:", bx);
        by = EditorGUILayout.FloatField("BY:", by);
        ad = EditorGUILayout.FloatField("AD:", ad);
        if (GUILayout.Button("Set Sprite Offset Values"))
        {
            ApplyOffsetValues();
        }
    }

    public void ApplyAppendString()
    {
        foreach (TMP_SpriteCharacter sprite in asset.spriteCharacterTable)
        {
            sprite.name = append + sprite.name;
        }
    }

    public void ApplyOffsetValues()
    {
        foreach (TMP_SpriteCharacter sprite in asset.spriteCharacterTable)
        {
            
        }
    }
}
