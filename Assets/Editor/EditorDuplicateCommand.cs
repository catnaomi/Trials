using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// https://forum.unity.com/threads/fixing-the-duplicate-command-that-duplicates-at-bottom-of-hierarchy.433179/
public static class EditorDuplicateCommand
{
    [MenuItem("Edit/Smart Duplicate %#d", false, 0)]
    private static void DuplicateSelection()
    {
        var newSelection = new List<Object>();
        if (Selection.gameObjects.Length > 0)
        {
            for (int ui = 0; ui < Selection.gameObjects.Length; ui++)
            {
                var go = Selection.gameObjects[ui];
                int siblingIndex = go.transform.GetSiblingIndex();
                var newGo = SmartInstantiate(go);
                if (newGo.transform is RectTransform)
                {
                    newGo.transform.SetParent(go.transform.parent);
                }
                else
                {
                    newGo.transform.parent = go.transform.parent;
                }
                newGo.transform.position = go.transform.position;
                newGo.transform.SetSiblingIndex(siblingIndex + 1);
                newGo.transform.localScale = go.transform.localScale;
                newGo.name = go.name;
                newSelection.Add(newGo);
                Undo.RegisterCreatedObjectUndo(newGo, $"Undo instantiate {newGo.name}");
            }
        }
        else
        {
            for (int ui = 0; ui < Selection.objects.Length; ui++)
            {
                var go = Selection.objects[ui];
                var newGo = DuplicateAsset(go);
                newSelection.Add(newGo);
                Undo.RegisterCreatedObjectUndo(newGo, $"Undo instantiate {newGo.name}");
            }
        }
        Selection.objects = newSelection.ToArray();
    }
    private static GameObject SmartInstantiate(GameObject go)
    {
        if (PrefabUtility.GetPrefabInstanceStatus(go) == PrefabInstanceStatus.Connected)
        {
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject;
            var newGo = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var mods = PrefabUtility.GetPropertyModifications(go);
            PrefabUtility.SetPropertyModifications(newGo, mods);
            return newGo;
        }
        if (PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab)
        {
            return DuplicateAsset(go) as GameObject;
        }
        return GameObject.Instantiate(go) as GameObject;
    }
    private static Object DuplicateAsset(Object targetAsset)
    {
        int index = 1;
        while (AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GetAssetPath(targetAsset).Replace(".", " " + index + ".")) != null)
        {
            index++;
            if (index > 100)
            {
                Debug.LogError("Massive Asset Duplicate Detect");
            }
        }
        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(targetAsset), AssetDatabase.GetAssetPath(targetAsset).Replace(".", " " + index + "."));
        return AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GetAssetPath(targetAsset).Replace(".", " " + index + "."));
    }
}



