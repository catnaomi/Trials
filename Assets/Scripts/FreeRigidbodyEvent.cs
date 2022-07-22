using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeRigidbodyEvent : MonoBehaviour
{
    public Rigidbody target;
    public void FreeRigidbody()
    {
        target.GetComponent<Collider>().enabled = true;
        target.transform.parent.SetParent(null);
        target.isKinematic = false;
        target.Sleep();
    }
}
