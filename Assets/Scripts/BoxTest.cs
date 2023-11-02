using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BoxTest : MonoBehaviour
{
    public Color color;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DrawCube.ForDebug(this.transform.position, this.transform.localScale, this.transform.rotation, color);
    }
}
