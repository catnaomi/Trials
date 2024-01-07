using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.AI.Navigation.Editor;
using UnityEditor;
using UnityEngine;

public class NavBakeShortcut : Editor
{
    [MenuItem("Tools/Navigation/Bake Nav Surface #n")]
    public static void BakeNav()
    {
        Debug.Log("attempting bake");
        var targets = FindObjectsOfType<NavMeshSurface>();
        foreach (var target in targets)
        {
            NavMeshAssetManager.instance.StartBakingSurfaces(targets);
        }
    }

    [MenuItem("Tools/Navigation/Delete Nav Mods on Empty Objects")]
    public static void DeleteNavFromEmptyObjects()
    {
        var targets = FindObjectsOfType<NavMeshModifier>();
        int destroyed = 0;
        for (int i = 0; i < targets.Length; i++)
        {
            NavMeshModifier target = targets[i];
            var components = target.GetComponents<Component>();
            if (components.Length <= 2) // only contains transform and navmeshmodifier
            {
                Undo.DestroyObjectImmediate(target);
                destroyed++;
            }

        }
        Debug.Log("destroyed " + destroyed + " empty navmeshmodifiers");
    }
}
