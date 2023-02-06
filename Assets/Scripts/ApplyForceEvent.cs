using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyForceEvent : MonoBehaviour
{
    public Rigidbody rigidbody;
    public bool atPosition;
    public bool usingForward;
    public Vector3 force = Vector3.forward;
    public float magnitude;
    public ForceMode forceMode;

    public void ApplyForce()
    {
        Vector3 calcForce = (usingForward) ?
            this.transform.forward * force.magnitude * magnitude :
            force * magnitude;

        if (!atPosition)
        {
            rigidbody.AddForce(calcForce, forceMode);
        }
        else
        {
            rigidbody.AddForceAtPosition(calcForce, this.transform.position, forceMode);
        }
    }
}
