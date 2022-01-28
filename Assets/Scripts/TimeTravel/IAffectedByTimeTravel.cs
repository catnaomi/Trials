using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAffectedByTimeTravel
{
    public void StartRewind();
    public void StopRewind();

    public void SaveTimeState();

    public void LoadTimeState(TimeTravelData data);

    public List<TimeTravelData> GetTimeStates();

    public GameObject GetAfterImagePrefab();
}