using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace CustomUtilities
{
    public class MeshUtilities
    {
        public static Mesh GetMergedMesh(GameObject model)
        {
            MeshFilter[] meshFilters = model.GetComponentsInChildren<MeshFilter>(false);
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            Matrix4x4 pTransform = model.transform.worldToLocalMatrix;

            for (int i = 0; i < meshFilters.Length; i++)
            {
                GameObject obj = meshFilters[i].gameObject;
                if (
                    !obj.activeSelf ||
                    obj.layer == LayerMask.NameToLayer("InteractionNode") ||
                    (obj.TryGetComponent<MeshRenderer>(out MeshRenderer renderer) && !renderer.enabled)
                    )
                {
                    continue;
                }
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = pTransform * meshFilters[i].transform.localToWorldMatrix;
            }

            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine);
            mesh.name = "mesh_" + model.name;
            return mesh;
        }
    }

}