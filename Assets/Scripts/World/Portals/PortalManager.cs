using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PortalManager : MonoBehaviour
{
    public static PortalManager instance;
    RenderTexture rt;

    public UnityEvent OnSwap;

    public LayerMask world1Mask;
    public LayerMask world2Mask;
    public LayerMask portalObjectMask;
    public LayerMask portalDepthMask;
    public bool inWorld2 = false;

    public bool inspectorSwap;
    private void Awake()
    {
        instance = this;
        rt = new RenderTexture(Screen.width, Screen.height, 24);
        Shader.SetGlobalTexture("_TimeCrackTexture", rt);
        Camera.main.cullingMask = !this.inWorld2 ? this.GetWorld1Mask() : this.GetWorld2Mask();
        Camera.main.cullingMask |= portalObjectMask;
        WindowManager.instance.ScreenSizeChangeEventDelayed += UpdateTextureSizeToScreenSize;
    }

    private void Update()
    {
        if (inspectorSwap)
        {
            inspectorSwap = false;
            Swap();
        }
    }
    public void Swap()
    {
        inWorld2 = !inWorld2;
        Camera.main.cullingMask = !this.inWorld2 ? this.GetWorld1Mask() : this.GetWorld2Mask();
        Camera.main.cullingMask |= portalObjectMask;
        OnSwap.Invoke();
    }

    public LayerMask GetWorld1Mask()
    {
        return world1Mask;
    }

    public LayerMask GetWorld2Mask()
    {
        return world2Mask;
    }

    public LayerMask GetPortalDepthMask()
    {
        return portalDepthMask;
    }
    public RenderTexture GetPortalTex()
    {
        return rt;
    }

    void UpdateTextureSizeToScreenSize(int width, int height)
    {
        RenderTexture rtOld = rt;
        Destroy(rtOld);
        rt = new RenderTexture(width, height, 24);
        Shader.SetGlobalTexture("_TimeCrackTexture", rt);
    }
}
