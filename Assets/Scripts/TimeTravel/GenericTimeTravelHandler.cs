using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericTimeTravelHandler : MonoBehaviour, IAffectedByTimeTravel
{
    public bool isFrozen;
    bool registered;

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
        return;
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
    }

    public void StartRewind()
    {
        
    }

    public void StopFreeze()
    {
        isFrozen = false;
    }

    public void StopRewind()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        TimeTravelController.AttemptToRegisterAffectee(this); 
    }
}
