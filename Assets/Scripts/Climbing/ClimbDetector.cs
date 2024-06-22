using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.Events;

public abstract class ClimbDetector : MonoBehaviour
{
    [field: SerializeField, ReadOnly, Header("Base Class")]
    public bool InUse { get; set; }
    [field: SerializeField]
    public bool IsDisabled { get; set; }
    public Collider collider;

    public UnityEvent OnStartClimb;
    public UnityEvent OnStopClimb;

    virtual protected void Awake()
    {
        collider = GetComponent<Collider>();
    }

    public virtual Quaternion GetClimbRotation()
    {
        return Quaternion.LookRotation(this.transform.forward);
    }

    public virtual Vector3 GetClimbHeading()
    {
        return this.transform.forward;
    }

    public virtual Vector3 GetClimbTangent()
    {
        return this.transform.up;
    }
    public void ForceDismount()
    {
        if (InUse)
        {
            PlayerActor.player.UnsnapLedge();
            PlayerActor.player.StartClimbLockout();
            InUse = false;
        }
    }

    public virtual bool AllowAttacks()
    {
        return false;
    }

    public virtual bool AllowJumps()
    {
        return false;
    }
    public void DisableClimb()
    {
        IsDisabled = true;
    }

    public void EnableClimb()
    {
        IsDisabled = false;
    }


    public bool IsBeingClimbed()
    {
        return InUse &&
            PlayerActor.player != null &&
            PlayerActor.player.IsClimbing() && 
            PlayerActor.player.currentClimb == this;
    }

    public virtual bool CheckPlayerCollision()
    {
        if (PlayerActor.player == null || collider == null)
        {
            return false;
        }
        return Physics.OverlapBox(collider.bounds.center, collider.bounds.extents)
                .ToList()
                .Contains(PlayerActor.player.GetComponent<Collider>());
    }

    public abstract void SetClimb();
    public void CheckLedgeAfter(float delay)
    {
        StartCoroutine(CheckLedgeAfterRoutine(delay));
    }

    IEnumerator CheckLedgeAfterRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (CheckPlayerCollision())
        {
            SetClimb();
        }
    }

    public void StartClimb()
    {
        InUse = true;
        OnStartClimb.Invoke();
    }

    public void StopClimb()
    {
        InUse = false;
        OnStopClimb.Invoke();
    }
}
