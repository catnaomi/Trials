using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// note: Interfaces cannot be searched for with GetComponent, so class only has virtual methods instead.
public class Actionable : MonoBehaviour
{
    public virtual int GetBufferedAction()
    {
        return -1; // do nothing but also negative
    }

    public virtual void SetDodgeDirection(AxisUtilities.AxisDirection axis)
    {
        return; // do nothing
    }

    public virtual AxisUtilities.AxisDirection GetStickAxis()
    {
        return AxisUtilities.AxisDirection.Zero; // do nothing
    }
}
