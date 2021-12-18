using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ForceWorldScale : MonoBehaviour
{
    public Vector3 targetScale = Vector3.one;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SetGlobalScale(targetScale);
    }
    public void SetGlobalScale(Vector3 globalScale)
    {
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
    }
}
