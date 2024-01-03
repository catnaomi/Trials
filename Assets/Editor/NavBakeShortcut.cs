using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.AI.Navigation.Editor;
using UnityEditor;
using UnityEngine;

public class NavBakeShortcut : Editor
{
    [MenuItem("Tools/Bake Nav Surface #n")]
    public static void BakeNav()
    {
        Debug.Log("attempting bake");
        var targets = FindObjectsOfType<NavMeshSurface>();
        foreach (var target in targets)
        {
            NavMeshAssetManager.instance.StartBakingSurfaces(targets);
        }
    }
}
