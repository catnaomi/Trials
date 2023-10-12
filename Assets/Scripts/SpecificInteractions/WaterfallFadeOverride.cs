using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(Renderer))]
public class WaterfallFadeOverride : MonoBehaviour
{
    public float LowerFadeTop = 0.5f;
    public float LowerFadeBottom = 1f;
    float lastTop;
    float lastBot;
    Renderer renderer;
    MaterialPropertyBlock block;
    double time;
    IAffectedByTimeTravel timeTravelHandler;
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (block == null || renderer == null) Init();
        UpdateTime();
        if (LowerFadeTop != lastTop || LowerFadeBottom != lastBot)
        {
            block.SetFloat("_LowerFadeTop", LowerFadeTop);
            block.SetFloat("_LowerFadeBottom", LowerFadeBottom);
            
            lastTop = LowerFadeTop;
            lastBot = LowerFadeBottom;
        }
        block.SetFloat("_InputTime", (float)time);
        renderer.SetPropertyBlock(block);
    }
    private void Init()
    {
        renderer = this.GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
        block.SetFloat("_LowerFadeTop", LowerFadeTop);
        block.SetFloat("_LowerFadeBottom", LowerFadeBottom);
        block.SetFloat("_InputTime", (float)time);
        renderer.SetPropertyBlock(block);
        timeTravelHandler = this.GetComponent<IAffectedByTimeTravel>();
        
    }

    void UpdateTime()
    {
        if (timeTravelHandler == null || !timeTravelHandler.IsFrozen())
        {
            time += Time.deltaTime;
            time %= 86400;
        }
    }
}
