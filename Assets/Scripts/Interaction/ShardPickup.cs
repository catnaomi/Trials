using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShardPickup : MonoBehaviour
{
    public float recoverAmount = 1f;
    public float activationDelay = 2f;
    public Collider collider;
    float clock;
    private void Start()
    {
        clock = 0f;
        if (activationDelay > 0f)
        {
            collider.enabled = false;
        }
    }

    private void Update()
    {
        if (clock < activationDelay)
        {
            clock += Time.deltaTime;
        }
        else
        {
            if (!collider.enabled)
            {
                collider.enabled = true;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.TryGetComponent(out PlayerActor player))
        {
            TimeTravelController.time.RecoverCharge();
            Destroy(this.gameObject);
        }
    }
}
