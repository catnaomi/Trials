using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer)), ExecuteInEditMode]
public class PassLightMatrix : MonoBehaviour
{
    MaterialPropertyBlock block;
    Renderer renderer;
    // Start is called before the first frame update
    void Start()
    {
       Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (renderer == null || block == null) Init();
        block.SetMatrix("_MainLightDirection", RenderSettings.sun.transform.localToWorldMatrix);
        renderer.SetPropertyBlock(block);
    }

    void Init()
    {
        renderer = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
    }
}
