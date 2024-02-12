using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public float activationDelay = 2f;
    public float destructionDelay = 30f;
    public Collider collider;
    float clock;
    private void Start()
    {
        clock = 0f;
        if (activationDelay > 0f && collider != null)
        {
            collider.enabled = false;
        }
        OnStart();
        this.StartTimer(destructionDelay, Destroy);
    }

    private void Update()
    {
        if (clock < activationDelay)
        {
            clock += Time.deltaTime;
        }
        else
        {
            if (!collider.enabled && collider != null)
            {
                collider.enabled = true;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.TryGetComponent(out PlayerActor player))
        {
            OnPickup();
            Destroy(this.gameObject);
        }
    }

    public virtual void OnStart()
    {

    }
    public virtual void OnPickup()
    {

    }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }
}
