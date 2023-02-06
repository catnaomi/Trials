using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSetKinematic : MonoBehaviour
{
    public Rigidbody rigidbody;

    public void SetKinematic(bool shouldSetToKinematic)
    {
        rigidbody.isKinematic = shouldSetToKinematic;
        //rigidbody.Sleep();
    }
}
