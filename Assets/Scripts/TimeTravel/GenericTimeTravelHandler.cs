using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericTimeTravelHandler : MonoBehaviour, IAffectedByTimeTravel
{
    public bool isFrozen;
    bool registered;

    public UnityEvent OnStartFreeze;
    public UnityEvent OnStopFreeze;
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
        OnStartFreeze.Invoke();
    }

    public void StartRewind()
    {

    }

    public void StopFreeze()
    {
        isFrozen = false;
        OnStopFreeze.Invoke();
    }

    public void StopRewind()
    {

    }

    void Start()
    {
        TimeTravelController.AttemptToRegisterAffectee(this);
    }
}
