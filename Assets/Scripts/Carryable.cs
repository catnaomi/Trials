using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Carryable : MonoBehaviour
{
    public PlayerActor player;

    public float yOffset;
    public bool isBeingCarried;
    public Vector3 eulerCarryRotationOffset;
    Rigidbody rigidbody;

    public bool playClipOnPickup;
    public Animancer.ClipTransition pickupClip;
    NavMeshObstacle obstacle;

    public float maximumVelocityForObstacle = 0f;
    public UnityEvent OnStopCarry;
    public UnityEvent OnStartCarry;
    public UnityEvent OnThrow;
    // Start is called before the first frame update
    void Start()
    {

        rigidbody = this.GetComponent<Rigidbody>();
        obstacle = this.GetComponent<NavMeshObstacle>();
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
            if (obstacle != null)
            {
                obstacle.enabled = false;
            }
        }
        else
        {
            if (obstacle != null)
            {
                obstacle.enabled = rigidbody == null || rigidbody.velocity.magnitude < maximumVelocityForObstacle;
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
        OnThrow.Invoke();
        if (rigidbody != null)
        {
            rigidbody.isKinematic = false;
            rigidbody.Sleep();
            rigidbody.AddForce(force, ForceMode.Force);
        }
    }
    public void StartCarry()
    {
        if (isBeingCarried) return;
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
    public virtual void SetCarryPosition(Vector3 position)
    {
        this.transform.position = position;
        this.transform.rotation = Quaternion.LookRotation(player.transform.forward) * Quaternion.Euler(eulerCarryRotationOffset);
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
