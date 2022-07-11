using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowRotate : MonoBehaviour
{
    public float degreesPerSecond = 360f;
    public Vector3 worldAxis = Vector3.up;
    public Vector3 localAxis = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 laxis = this.transform.forward * localAxis.z + this.transform.up * localAxis.y + this.transform.right * localAxis.x;

        Vector3 axis = (worldAxis + localAxis).normalized;
        this.transform.rotation *= Quaternion.AngleAxis(degreesPerSecond * Time.deltaTime, axis);
    }
}
