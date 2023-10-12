using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassTimeToShader : MonoBehaviour
{
    IAffectedByTimeTravel timeTravelHandler;
    double time;
    MaterialPropertyBlock block;
    Renderer renderer;
    public bool applyRandomness = false;
    [Tooltip("Should prevent visual bugs over long playtimes, but will stutter 1/day")]
    public bool precisionMod = true;
    public bool dontApplyBlock = false;
    float rand;
    // Start is called before the first frame update
    void Start()
    {
        timeTravelHandler = GetComponent<IAffectedByTimeTravel>();
        if (timeTravelHandler == null)
            timeTravelHandler = GetComponentInParent<IAffectedByTimeTravel>();
        time = 0f;
        renderer = this.GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
        block.SetFloat("_UseTime", 1f);
        block.SetFloat("_InputTime", (float)time);
        if (!dontApplyBlock) renderer.SetPropertyBlock(block);
        if (applyRandomness)
        {
            rand = Random.Range(0f, Mathf.PI * 2f);
        }
        else
        {
            rand = 0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (timeTravelHandler != null && !timeTravelHandler.IsFrozen())
        {
            time += Time.deltaTime;
            if (precisionMod) time %= 86400;
            block.SetFloat("_UseTime", 1f);
            block.SetFloat("_InputTime", (float)(time + rand));
            if (!dontApplyBlock) renderer.SetPropertyBlock(block);
        }
    }

    public MaterialPropertyBlock GetBlock()
    {
        return block;
    }
}
