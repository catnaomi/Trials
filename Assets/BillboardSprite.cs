using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BillboardSprite : MonoBehaviour
{
    private void Update()
    {

        if (Camera.main != null)
        {
            this.transform.forward = -Camera.main.transform.forward;
        }
        else if (Camera.current != null)
        {
            this.transform.forward = -Camera.current.transform.forward;
        }
        
    }
}
