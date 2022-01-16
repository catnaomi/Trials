using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIImpulseController : MonoBehaviour
{
    public Vector3 impulseVariation;
    Vector3 lastPosition;
    public Vector3 delta;
    public static UIImpulseController impulse;

    // Start is called before the first frame update

    private void Awake()
    {
        impulse = this;
    }
    void Update()
    {
        impulseVariation = this.transform.localPosition;
        if (impulseVariation.magnitude < 0.01)
        {
            impulseVariation = Vector3.zero;
        }
        delta = impulseVariation - lastPosition;
        lastPosition = impulseVariation;
    }
}
