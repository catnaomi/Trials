using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Renderer))]
public class HideShadowsOnPortalSwap : MonoBehaviour
{
    private Renderer rend;
    public bool isWorld2 = false;
    ShadowCastingMode shadowCastingMode;
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        shadowCastingMode = rend.shadowCastingMode;
        if (PortalManager.instance != null)
        {
            PortalManager.instance.OnSwap.AddListener(OnSwap);
        }
    }

    public void OnSwap()
    {
        Swap(PortalManager.instance.inWorld2);
    }

    void Swap(bool onWorld2)
    {
        bool show = onWorld2 == isWorld2;
        rend.shadowCastingMode = show ? shadowCastingMode : ShadowCastingMode.Off;
    }
}
