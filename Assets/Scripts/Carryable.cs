using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Carryable : MonoBehaviour
{
    public PlayerActor player;

    public float yOffset;
    public bool isBeingCarried;

    Rigidbody rigidbody;

    public bool playClipOnPickup;
    public Animancer.ClipTransition pickupClip;

    public UnityEvent OnStopCarry;
    public UnityEvent OnStartCarry;
    // Start is called before the first frame update
    void Start()
    {

        rigidbody = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isBeingCarried)
        {
            if (rigidbody != null && !rigidbody.isKinematic)
            {
                rigidbody.isKinematic = true;
            }
        }
    }

    public void Carry(PlayerActor player)
    {
        this.player = player;
        if (playClipOnPickup)
        {
            player.CarryWithAnimation(this);
        }
        else
        {
            player.Carry(this);
            StartCarry();
        }
        
    }

    public void Throw(Vector3 force)
    {
        StopCarry();
        if (rigidbody != null)
        {
            rigidbody.isKinematic = false;
            rigidbody.Sleep();
            rigidbody.AddForce(force, ForceMode.Force);
        }
    }
    public void StartCarry()
    {
        isBeingCarried = true;
        OnStartCarry.Invoke();
    }
    public void StopCarry()
    {
        isBeingCarried = false;
        if (rigidbody != null)
        {
            rigidbody.isKinematic = false;
        }
        OnStopCarry.Invoke();
    }
    public void StopMovement()
    {
        if (rigidbody != null)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
    }
    public void SetCarryPosition(Vector3 position)
    {
        this.transform.position = position;
        this.transform.rotation = Quaternion.LookRotation(player.transform.forward);
    }

    public float GetMass()
    {
        if (rigidbody != null)
        {
            return rigidbody.mass;
        }
        else
        {
            return 1f;
        }
    }
}
