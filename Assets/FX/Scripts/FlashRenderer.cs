using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FlashRenderer : MonoBehaviour
{
    public Renderer[] renderers;
    MaterialPropertyBlock block;

    public float defaultDuration = 1f;
    public float duration = 1f;
    public AnimationCurve curve;
    public Color color = Color.white;
    public float alphaMult = 1f;
    public bool inspectorFlash;
    public Material material;
    Color lastColor;
    float t;
    // Start is called before the first frame update
    void Start()
    {
        block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);

        t = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (inspectorFlash)
        {
            inspectorFlash = false;
            Flash(color, duration);
        }
        if (t < 1f)
        {
            if (duration > 0)
            {
                t += Time.deltaTime / duration;
            }
        }
        color = GetColor();
        if (color != lastColor)
        {
            block.SetColor("_BaseColor", color);
            foreach (Renderer r in renderers)
            {
                r.SetPropertyBlock(block);
                r.enabled = color.a > 0.05;
            }
            
        }
        lastColor = color;
    }

    public Color GetColor()
    {
        return new Color(color.r, color.g, color.b, alphaMult * curve.Evaluate(t));
    }

    public void Flash(Color c)
    {
        Flash(c, defaultDuration);
    }
    public void Flash(Color c, float duration)
    {
        this.duration = duration;
        color = c;
        alphaMult = c.a;
        t = 0f;
    } 
}
