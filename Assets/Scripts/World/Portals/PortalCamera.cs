using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    PortalManager manager;
    Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        manager = PortalManager.instance;
        camera = this.GetComponent<Camera>();
        manager.OnSwap.AddListener(Swap);
        Swap();
        camera.targetTexture = manager.GetPortalTex();
    }

    public void Swap()
    {
        camera.cullingMask = manager.inWorld2 ? manager.GetWorld1Mask() : manager.GetWorld2Mask();
        camera.cullingMask |= manager.GetPortalDepthMask();
    }
}
