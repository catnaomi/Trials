using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class BillboardSprite : MonoBehaviour
{
    private void Update()
    {

        this.transform.forward = -Camera.main.transform.forward;
        
    }
}
