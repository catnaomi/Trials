using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyFollowTransform : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public bool offsetForceX;
    public bool offsetForceY;
    public bool offsetForceZ;
    Rigidbody rigid;
    // Start is called before the first frame update
    void Start()
    {
        rigid = this.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (target == null) return;
        Vector3 position = target.position + offset;
        if (offsetForceX) position.x = offset.x;
        if (offsetForceY) position.y = offset.y;
        if (offsetForceZ) position.z = offset.z;
        rigid.MovePosition(position);
    }
}
