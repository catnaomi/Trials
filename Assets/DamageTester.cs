using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTester : MonoBehaviour
{

    public Color color1;
    public Color color2;

    public bool toggle;

    Renderer rend;
    // Start is called before the first frame update
    void Start()
    {
        toggle = true;
        rend = GetComponent<Renderer>();
        GetComponent<HurtboxController>().OnHurt.AddListener(() => { toggle = !toggle; });
    }

    private void FixedUpdate()
    {
        rend.material.color = toggle ? color1 : color2;
    }
}
