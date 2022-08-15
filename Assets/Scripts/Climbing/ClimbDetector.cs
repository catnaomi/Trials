using UnityEngine;
using System.Collections;

public class ClimbDetector : MonoBehaviour
{
    public bool inUse;
    public Collider collider;

    public virtual Quaternion GetClimbRotation()
    {
        return Quaternion.LookRotation(this.transform.forward);
    }

    public virtual Vector3 GetClimbHeading()
    {
        return this.transform.forward;
    }
}
