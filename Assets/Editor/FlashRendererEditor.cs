using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FlashRenderer))]
public class FlashRendererEditor : Editor
{

    public override void OnInspectorGUI()
    {
        FlashRenderer flash = (FlashRenderer)target;
        DrawDefaultInspector();
        GUILayout.Space(10f);
        if (GUILayout.Button("Get Renderers"))
        {
            GetRenderers(flash);
        }
        GUILayout.Space(5f);
        if (GUILayout.Button("Create Renderers"))
        {
            CreateRenderers(flash);
        }
    }

    void GetRenderers(FlashRenderer flash)
    { 
        flash.renderers = flash.GetComponentsInChildren<Renderer>();
    }

    void CreateRenderers(FlashRenderer flash)
    {
        foreach (Transform t in flash.transform)
        {
            GameObject.DestroyImmediate(t.gameObject);
        }
        var renderers = flash.transform.parent.GetComponentsInChildren<Renderer>()
            .Where(r => r.transform.parent == flash.transform.parent && r is not ParticleSystemRenderer && r is not SpriteRenderer)
            .ToList();

        List<Renderer> toAdd = new List<Renderer>();
        foreach (Renderer r in renderers)
        {
            GameObject rObj = GameObject.Instantiate(r.gameObject, flash.transform);
            rObj.name = "flash_"+r.name;

            Renderer rNew = rObj.GetComponent<Renderer>();
            int matCount = rNew.sharedMaterials.Length;
            for (int i = 0; i < matCount; i++)
            {
                rNew.sharedMaterials[i] = flash.material;
            }
            toAdd.Add(rObj.GetComponent<Renderer>());
        }
        flash.renderers = toAdd.ToArray();

    }
}
