using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StayRelativeToParent : MonoBehaviour
{
    Vector3 offset;
    Rigidbody rigidbody;
    // Start is called before the first frame update
    void Start()
    {
        offset = this.transform.localPosition;
        rigidbody = this.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rigidbody.MovePosition(this.transform.parent.position + offset);
        rigidbody.MoveRotation(this.transform.parent.rotation);
    }
}
