using System.Collections;
using UnityEngine;

public class FadeMaterial : MonoBehaviour
{
    public Material material;
    public bool startDisabled = true;
    MaterialPropertyBlock block;

    private void Start()
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
            foreach (Renderer renderer in this.GetComponentsInChildren<Renderer>())
            {
                renderer.SetPropertyBlock(block);
            }
        }
        this.gameObject.SetActive(false);
    }
    public void Set()
    {
        if (block != null)
        {
            block.SetFloat("_creationTime", Time.time);
            foreach (Renderer renderer in this.GetComponentsInChildren<Renderer>())
            {
                renderer.SetPropertyBlock(block);
            }
        }

    }

    // Update is called once per frame
    void OnEnable()
    {
        Set();
    }
}