using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class QuickQuaterionCalculator : MonoBehaviour
{
    public float[] quaternionRepresentation;
    public Vector3 eulerRotate;
    public string output;
    public bool go;

    // Update is called once per frame
    void Update()
    {
        if (go)
        {
            go = false;
            Quaternion q = Quaternion.Euler(eulerRotate) * ArrayToQuaternion(quaternionRepresentation);
            output = QuaternionToString(q);
        }
    }

    Quaternion ArrayToQuaternion(float[] angles)
    {
        Quaternion q = Quaternion.identity;
        if (angles.Length >= 4)
        {
            q.Set(angles[0], angles[1], angles[2], angles[3]);
        }
        return q;
    }
    string QuaternionToString(Quaternion q)
    {
        return $"{q.x},{q.y},{q.z},{q.w}";
    }
}
