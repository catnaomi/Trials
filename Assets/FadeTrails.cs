using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeTrails : MonoBehaviour
{
    float lifetime;
    public float duration = 1f;

    ParticleSystem p;
    ParticleSystem.TrailModule t;

    bool fading;
    private void Start()
    {
        var p = this.GetComponent<ParticleSystem>();

        t = p.trails;

        lifetime = t.lifetimeMultiplier;
    }
    public void StartFade()
    {     
        fading = true;
    }

    private void Update()
    {
        if (fading)
        {
            t.lifetimeMultiplier = Mathf.MoveTowards(t.lifetimeMultiplier, 0f, 0.01f); //(lifetime / (Time.deltaTime * duration))
            if (t.lifetimeMultiplier <= 0f)
            {
                fading = false;
            }
        }
    }

    public void Reset()
    {
        t.lifetimeMultiplier = lifetime;
    }
}
