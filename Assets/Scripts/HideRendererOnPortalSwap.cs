using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HideRendererOnPortalSwap : MonoBehaviour
{
    private Renderer rend;
    public bool isWorld2 = false;
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
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
        rend.enabled = onWorld2 == isWorld2;
    }
}
