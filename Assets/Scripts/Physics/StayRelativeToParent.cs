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
        Vector3 position = this.transform.parent.position + offset;
        Quaternion rotation = Quaternion.LookRotation(this.transform.parent.forward);

        rigidbody.Move(position, rotation);
    }
}
