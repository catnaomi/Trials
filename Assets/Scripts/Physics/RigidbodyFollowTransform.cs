using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody)), ExecuteInEditMode]
public class RigidbodyFollowTransform : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public bool offsetForceX;
    public bool offsetForceY;
    public bool offsetForceZ;
    public bool executeInEditor;
    Rigidbody rigid;
    // Start is called before the first frame update
    void Start()
    {
        rigid = this.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        UpdatePosition();
    }

    private void Update()
    {
        if (!Application.isPlaying && executeInEditor)
        {
            if (target == null) return;
            if (!Application.isPlaying && !executeInEditor) return;
            if (rigid == null) rigid = this.GetComponent<Rigidbody>();
            Vector3 position = target.position + offset;
            if (offsetForceX) position.x = offset.x;
            if (offsetForceY) position.y = offset.y;
            if (offsetForceZ) position.z = offset.z;
            this.transform.position = position;
        }
    }

    void UpdatePosition()
    {
        if (target == null) return;
        if (!Application.isPlaying && !executeInEditor) return;
        if (rigid == null) rigid = this.GetComponent<Rigidbody>();
        Vector3 position = target.position + offset;
        if (offsetForceX) position.x = offset.x;
        if (offsetForceY) position.y = offset.y;
        if (offsetForceZ) position.z = offset.z;
        this.transform.position = position;
        rigid.MovePosition(position);
    }
}