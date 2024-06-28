using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GlobalShadowColor : MonoBehaviour
{
    [ColorUsage(false,true)] public Color shadowColor;
    // Start is called before the first frame update
    void Start()
    {
        SetShadowColor();
    }

    public void SetShadowColor()
    {
        Shader.SetGlobalColor("_SceneShadowColor", shadowColor);
    }
}
