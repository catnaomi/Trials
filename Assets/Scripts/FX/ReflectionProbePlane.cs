using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionProbePlane : MonoBehaviour
{
    public ReflectionProbe probe;
    Plane plane;
    // Start is called before the first frame update
    void Start()
    {
        plane = new Plane(this.transform.up, this.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        probe.transform.position = plane.ClosestPointOnPlane(Camera.main.transform.position);
    }
}
