using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePointIceCrack : MonoBehaviour
{
    public Renderer renderer;
    public DamageablePoint point;
    MaterialPropertyBlock block;
    float lastHealth = -1;
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
    // Start is called before the first frame update
    void Start()
    {
        if (point == null)
        point = GetComponent<DamageablePoint>();
        if (!point.hasHealth)
        {
            this.enabled = false;
            return;
        }
        if (renderer == null)
        renderer = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
        block.SetFloat("_CrackedAmount", 0f);
        renderer.SetPropertyBlock(block);
        UpdateCrack();
    }

    // Update is called once per frame
    void Update()
    {
        if (lastHealth != point.health.current)
        {
            UpdateCrack();
            lastHealth = point.health.current;
        }
    }

    public void UpdateCrack()
    {
        if (point.health.max == 0) return;
        block.SetFloat("_CrackedAmount", curve.Evaluate(1f - Mathf.Clamp01(point.health.current / point.health.max)));
        renderer.SetPropertyBlock(block);
    }
}
