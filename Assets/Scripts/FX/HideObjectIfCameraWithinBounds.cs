using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideObjectIfCameraWithinBounds : MonoBehaviour
{
    public Renderer rendererToHide;
    Bounds bounds;
    // Start is called before the first frame update
    void Start()
    {
        bounds = this.GetComponent<Collider>().bounds;
    }

    // Update is called once per frame
    void Update()
    {
        if (bounds.Contains(Camera.main.transform.position) && rendererToHide.enabled)
        {
            rendererToHide.enabled = false;
        }
        else if (!bounds.Contains(Camera.main.transform.position) && !rendererToHide.enabled)
        {
            rendererToHide.enabled = true;
        }
    }
}
