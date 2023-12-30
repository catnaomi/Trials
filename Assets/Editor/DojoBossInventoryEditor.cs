using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DojoBossInventoryTransformingController))]
public class DojoBossInventoryEditor : Editor
{
    string indexText;
    int index = 0;
    public override void OnInspectorGUI()
    {
        DojoBossInventoryTransformingController inventory = (DojoBossInventoryTransformingController)target;
        DrawDefaultInspector();
        GUILayout.Space(25f);
        //indexText = EditorGUILayout.TextField("Set Weapon By Index", indexText);
        if (GUILayout.Button("Set"))
        {
            inventory.SetWeaponByIndex(inventory.setByIndex);
        }
    }
}