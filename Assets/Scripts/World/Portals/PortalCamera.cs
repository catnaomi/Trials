using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    PortalManager manager;
    Camera camera;
    bool updateTex;
    // Start is called before the first frame update
    void Start()
    {
        manager = PortalManager.instance;
        camera = this.GetComponent<Camera>();
        manager.OnSwap.AddListener(Swap);
        Swap();
        camera.targetTexture = manager.GetPortalTex();
        WindowManager.instance.ScreenSizeChangeEventDelayed += FlagTextureNeedsUpdate;
    }

    public void Swap()
    {
        camera.cullingMask = manager.inWorld2 ? manager.GetWorld1Mask() : manager.GetWorld2Mask();
        camera.cullingMask |= manager.GetPortalDepthMask();
    }

    public void Update()
    {
        if (updateTex)
        {
            camera.targetTexture = manager.GetPortalTex();
            updateTex = false;
            Debug.Log("updated portal camera texture");
        }
    }
    void FlagTextureNeedsUpdate(int width, int height)
    {
        updateTex = true;
    }
}
