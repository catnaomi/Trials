using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpaqueWhenApproachedShaderHelper : MonoBehaviour
{
    Renderer renderer;
    MaterialPropertyBlock block;
    // Start is called before the first frame update
    void Start()
    {
        block = new MaterialPropertyBlock();
        renderer = this.GetComponent<Renderer>();
        renderer.SetPropertyBlock(block);
        block.SetVector("_PlayerPosition", Vector3.zero);
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerActor.player != null)
        {
            block.SetVector("_PlayerPosition", PlayerActor.player.transform.position);
            renderer.SetPropertyBlock(block);
        }
    }

    private void OnDisable()
    {
        if (block != null && renderer != null)
        {
            block.SetVector("_PlayerPosition", Vector3.zero); ;
            renderer.SetPropertyBlock(block);
        }
    }
}
