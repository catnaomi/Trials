using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.Events;

public abstract class ClimbDetector : MonoBehaviour
{
    public bool inUse;
    public bool isDisabled;
    public Collider collider;

    public UnityEvent OnStartClimb;
    public UnityEvent OnStopClimb;

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
        if (inUse)
        {
            PlayerActor.player.UnsnapLedge();
            PlayerActor.player.StartClimbLockout();
            inUse = false;
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
        isDisabled = true;
    }

    public void EnableClimb()
    {
        isDisabled = false;
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
        OnStartClimb.Invoke();
    }

    public void StopClimb()
    {
        OnStopClimb.Invoke();
    }
}
