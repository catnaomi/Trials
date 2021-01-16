using UnityEngine;
using System.Collections;

public class Ledge : ClimbDetector
{
    Rigidbody ledge;
    // Use this for initialization
    void Awake()
    {
        ledge = this.GetComponent<Rigidbody>();
        collider = ledge.GetComponent<Collider>();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.TryGetComponent<PlayerActor>(out PlayerActor player))
        {
            player.SetLedge(this);
            inUse = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(this.transform.position, this.transform.forward);
    }
}
