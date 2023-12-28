using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePointIceCrack : MonoBehaviour
{
    public Renderer renderer;
    public GameObject breakableObject;
    IHasHealthAttribute point;
    MaterialPropertyBlock block;
    float lastHealth = -1;
    float currentHealth;
    public float crackSpeed = 1f;
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
    // Start is called before the first frame update
    void Start()
    {
        if (breakableObject == null)
        {
            breakableObject = this.gameObject;
        }
        point = breakableObject.GetComponent<IHasHealthAttribute>();

        if (point.GetHealth().max <= 0)
        {
            this.enabled = false;
            return;
        }
        if (renderer == null)
        renderer = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
        block.SetFloat("_CrackedAmount", 0f);
        renderer.SetPropertyBlock(block);
        lastHealth = point.GetHealth().current;
        UpdateCrack();
    }

    // Update is called once per frame
    void Update()
    {
        if (lastHealth != point.GetHealth().current)
        {
            UpdateCrack();
            //lastHealth = point.GetHealth().current;
            lastHealth = Mathf.MoveTowards(lastHealth, point.GetHealth().current, crackSpeed * point.GetHealth().max * Time.deltaTime);
        }
    }

    public void UpdateCrack()
    {
        if (point.GetHealth().max == 0) return;
        block.SetFloat("_CrackedAmount", curve.Evaluate(1f - Mathf.Clamp01(lastHealth / point.GetHealth().max)));
        renderer.SetPropertyBlock(block);
    }
}
