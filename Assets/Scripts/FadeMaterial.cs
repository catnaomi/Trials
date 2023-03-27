using System.Collections;
using UnityEngine;

public class FadeMaterial : MonoBehaviour
{
    public Material material;
    public bool startDisabled = true;
    public float fadeTime = 3f;
    [Range(0,1)]
    public float lifetime;
    MaterialPropertyBlock block;

    private void Start()
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
            ApplyBlockToRenderers();
        }
        this.gameObject.SetActive(false);
    }
    public void Set()
    {
        if (block != null)
        {
            block.SetFloat("_Lifetime", Time.time);
            ApplyBlockToRenderers();
        }
        lifetime = 1f;
    }

    private void Update()
    {
        block.SetFloat("_Lifetime", lifetime);
        if (fadeTime > 0)
        lifetime -= Time.deltaTime / fadeTime;
        lifetime = Mathf.Clamp01(lifetime);
        ApplyBlockToRenderers();
    }

    void ApplyBlockToRenderers()
    {
        if (block == null) return;
        foreach (Renderer renderer in this.GetComponentsInChildren<Renderer>())
        {
            renderer.SetPropertyBlock(block);
        }
    }
    // Update is called once per frame
    void OnEnable()
    {
        Set();
    }
}