using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateRigidbody : MonoBehaviour
{
    public Vector3 axis = Vector3.up;
    public float speed = 360f;
    public Rigidbody rigidbody;
    bool usingTimeTravel;
    // Start is called before the first frame update
    void Start()
    {
        if (rigidbody == null) rigidbody = this.GetComponent<Rigidbody>();
        usingTimeTravel = (rigidbody.GetComponent<RigidbodyTimeTravelHandler>() != null);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rigidbody.MoveRotation(rigidbody.rotation * Quaternion.AngleAxis(speed * GetFixedDeltaTime(), axis));
    }

    float GetFixedDeltaTime()
    {
        return !usingTimeTravel ? Time.fixedDeltaTime : TimeTravelController.GetTimeAffectedFixedDeltaTime();
    }
}
