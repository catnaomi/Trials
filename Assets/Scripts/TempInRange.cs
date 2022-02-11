using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TempInRange : MonoBehaviour
{
    public float x;
    public float min;
    public float max;
    public float result;
    public float percent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        result = (x - max) * (x - min);
        percent = ((x - min) / (max - min)) * Step(0,result);
    }

    int Step(float x, float y)
    {
        return (x >= y) ? 1 : 0;
    }
}
