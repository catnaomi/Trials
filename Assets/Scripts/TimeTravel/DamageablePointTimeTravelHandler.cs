using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageablePointTimeTravelHandler : MonoBehaviour, IAffectedByTimeTravel
{
    DamageablePoint point;
    bool isFrozen;
    bool registered;

    void Start()
    {
        point = this.GetComponent<DamageablePoint>();
        TimeTravelController.AttemptToRegisterAffectee(this);
    }

    public void ClearTimeData()
    {
        
    }

    public GameObject GetObject()
    {
        return this.gameObject;
    }

    public List<TimeTravelData> GetTimeStates()
    {
        return null;
    }

    public bool IsFrozen()
    {
        return isFrozen;
    }

    public bool IsNull()
    {
        return this == null;
    }

    public bool IsRegistered()
    {
        return registered;
    }

    public bool IsRewinding()
    {
        return false;
    }

    public void LoadTimeState(TimeTravelData data)
    {
        
    }

    public TimeTravelData SaveTimeState()
    {
        return null;
    }

    public void SetRegistered()
    {
        registered = true;
    }

    public bool ShouldApplyTimeVisualEffect()
    {
        return false;
    }

    public void StartFreeze()
    {
        isFrozen = true;
        point.isInTimeState = true;
    }

    public void StartRewind()
    {
        
    }

    public void StopFreeze()
    {
        isFrozen = false;
        point.isInTimeState = false;
    }

    public void StopRewind()
    {
        
    }
}
