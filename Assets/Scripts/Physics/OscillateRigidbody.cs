using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscillateRigidbody : MonoBehaviour
{
    public Vector3 axis = Vector3.up;
    public float distance = 1f;
    public float speed = 1f;
    Rigidbody rigidbody;
    Vector3 initPos;
    public Vector3 targetPosition;
    bool usingTimeTravel;
    RigidbodyTimeTravelHandler timeTravelHandler;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = this.GetComponent<Rigidbody>();
        initPos = rigidbody.position;
        timeTravelHandler = rigidbody.GetComponent<RigidbodyTimeTravelHandler>();
        usingTimeTravel = timeTravelHandler != null;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        targetPosition = initPos + Mathf.Sin(GetFixedTime() * speed) * distance * axis;
        rigidbody.MovePosition(targetPosition);
    }

    float GetFixedTime()
    {
        return !usingTimeTravel ? Time.fixedTime : timeTravelHandler.GetFixedTime();
    }
}
