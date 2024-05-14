using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(Renderer))]
public class CausticsLightOverride : MonoBehaviour
{
    public float fade = 1f;
    float lastFade;
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
        if (fade != lastFade)
        {
            block.SetFloat("_Fade", fade);

            lastFade = fade;
        }
        block.SetFloat("_InputTime", (float)time);
        block.SetMatrix("_MainLightDirection", RenderSettings.sun.transform.localToWorldMatrix);
        renderer.SetPropertyBlock(block);
    }
    private void Init()
    {
        renderer = this.GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
        block.SetFloat("_Fade", fade);
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
