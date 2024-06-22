using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyDisplayInspector : MonoBehaviour
{
    [SerializeField, ReadOnly] Quaternion rotation;
    [SerializeField, ReadOnly] Vector3 position;
    [SerializeField, ReadOnly] bool sleeping;
    [SerializeField, ReadOnly] Vector3 velocity;
    [SerializeField, ReadOnly] float speed;
    [SerializeField, ReadOnly] Vector3 angularVelocity;
    [SerializeField, ReadOnly] float angularSpeed;
    [SerializeField, ReadOnly] Vector3 centerOfMass;
    [SerializeField, ReadOnly] Vector3 inertiaTensor;
    [SerializeField, ReadOnly] Quaternion inertiaTensorRotation;
    
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rotation = rb.rotation;
        position = rb.position;
        sleeping = rb.IsSleeping();
        velocity = rb.velocity;
        speed = rb.velocity.magnitude;
        angularVelocity = rb.angularVelocity;
        angularSpeed = rb.angularVelocity.magnitude;
        centerOfMass = rb.centerOfMass;
        inertiaTensor = rb.inertiaTensor;
        inertiaTensorRotation = rb.inertiaTensorRotation;
    }
}
