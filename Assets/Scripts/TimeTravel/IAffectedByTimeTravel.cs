using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAffectedByTimeTravel
{
    public void StartRewind();
    public void StopRewind();

    public TimeTravelData SaveTimeState();

    public void LoadTimeState(TimeTravelData data);

    public List<TimeTravelData> GetTimeStates();

    public bool IsFrozen();

    public void StartFreeze();

    public void StopFreeze();

    public GameObject GetObject();
}