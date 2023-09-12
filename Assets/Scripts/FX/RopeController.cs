using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways,RequireComponent(typeof(LineRenderer))]
public class RopeController : MonoBehaviour
{
    LineRenderer renderer;
    Vector3[] positions;
    public Transform target1;
    public Transform target2;
    // Start is called before the first frame update
    void Start()
    {
        renderer = this.GetComponent<LineRenderer>();
        positions = new Vector3[2];
    }

    private void LateUpdate()
    {
        if (target1 != null && target2 != null && renderer != null)
        {
            positions[0] = target1.position;
            positions[1] = target2.position;
            renderer.SetPositions(positions);
        }
    }
}
