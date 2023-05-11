using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMoveOverTime : MonoBehaviour
{
    public Vector3 world;
    public Vector3 local;
    public Vector3 camera;
    Vector3 velocity;
    // Update is called once per frame
    void Update()
    {
        velocity = world;
        velocity += this.transform.forward * local.z;
        velocity += this.transform.right * local.x;
        velocity += this.transform.up * local.y;
        velocity += Camera.main.transform.forward * camera.z;
        velocity += Camera.main.transform.right * camera.x;
        velocity += Camera.main.transform.up * camera.y;

        this.transform.Translate(velocity * Time.deltaTime);
    }
}
