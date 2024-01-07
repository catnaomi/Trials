using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation.Editor;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;

public class BoxColliderNegativeFix : Editor
{
    [MenuItem("Tools/Collision/Fix Negative Scale Boxes")]
    public static void FixNegativeScale()
    {
        var targets = FindObjectsOfType<BoxCollider>();
        int count = 0;
        foreach (var target in targets)
        {
            GameObject targetObject = target.gameObject;
            if (target.transform.lossyScale.x < 0 || target.transform.lossyScale.y < 0 || target.transform.lossyScale.z < 0)
            {
                Debug.Log("found negative scale box" + target.ToString(), targetObject);
                Undo.DestroyObjectImmediate(target);
                MeshCollider meshCollider = Undo.AddComponent<MeshCollider>(targetObject);
                meshCollider.convex = true;
                if (PrefabUtility.IsPartOfAnyPrefab(targetObject))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(meshCollider);
                }
                count++;
            }
        }
        Debug.Log("fixed " + count + " negative scale boxes");
    }
}