using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turnaround : MonoBehaviour
{
    public float speed = 360f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
    }
}
