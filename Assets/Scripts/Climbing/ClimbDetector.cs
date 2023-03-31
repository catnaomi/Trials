using UnityEngine;
using System.Collections;

public class ClimbDetector : MonoBehaviour
{
    public bool inUse;
    public bool isDisabled;
    public Collider collider;
    

    public virtual Quaternion GetClimbRotation()
    {
        return Quaternion.LookRotation(this.transform.forward);
    }

    public virtual Vector3 GetClimbHeading()
    {
        return this.transform.forward;
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

    public void DisableClimb()
    {
        isDisabled = true;
    }

    public void EnableClimb()
    {
        isDisabled = false;
    }
}
