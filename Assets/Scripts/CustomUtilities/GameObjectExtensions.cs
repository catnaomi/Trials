// By Acegikmo
// https://forum.unity.com/threads/accessing-prefab-children-in-editor-scripts.13167/#post-2316699


using UnityEngine;
using System.Collections.Generic;
namespace CustomUtilities
{
    public static class GameObjectExtensions
    {

        public static T[] GetComponentsInChildrenOfAsset<T>(this GameObject go) where T : Component
        {
            List<Transform> tfs = new List<Transform>();
            CollectChildren(tfs, go.transform);
            List<T> all = new List<T>();
            for (int i = 0; i < tfs.Count; i++)
                all.AddRange(tfs[i].gameObject.GetComponents<T>());
            return all.ToArray();
        }

        static void CollectChildren(List<Transform> transforms, Transform tf)
        {
            transforms.Add(tf);
            foreach (Transform child in tf)
            {
                CollectChildren(transforms, child);
            }
        }

    }
}