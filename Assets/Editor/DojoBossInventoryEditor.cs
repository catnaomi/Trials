using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DojoBossInventoryTransformingController))]
public class DojoBossInventoryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DojoBossInventoryTransformingController inventory = (DojoBossInventoryTransformingController)target;
        DrawDefaultInspector();
        GUILayout.Space(25f);
        if (GUILayout.Button("Set"))
        {
            inventory.SetWeaponByIndex(inventory.setByIndex);
        }
    }
}