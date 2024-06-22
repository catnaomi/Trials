using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WalkableIfSleep : MonoBehaviour
{
    Rigidbody rb;
    Collider collider;
    [SerializeField, ReadOnly] bool walkable;
    // Start is called before the first frame update
    void Start()
    {
        collider = this.GetComponent<Collider>();
        rb = this.GetComponentInParent<Rigidbody>();

        if (collider == null)
        {
            Debug.LogError("Collider not found on object", this);
            this.enabled = false;
            return;
        }
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on parent object", this);
            this.enabled = false;
            return;
        }
    }

    private void FixedUpdate()
    {
        if (rb.IsSleeping() && !walkable)
        {
            collider.enabled = true;
            walkable = true;
        }
        else if (!rb.IsSleeping() && walkable)
        {
            collider.enabled = false;
            walkable = false;
        }
    }
}
