using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ClimbDetector))]
public class ClimbTimeTravelHandler : MonoBehaviour, IAffectedByTimeTravel
{
    ClimbDetector climb;
    [SerializeField, ReadOnly] bool isFrozen;
    // Start is called before the first frame update
    void Start()
    {
        climb = this.GetComponent<ClimbDetector>();
        climb.isDisabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (TimeTravelController.time != null && TimeTravelController.time.IsFreezing() && !isFrozen)
        {
            StartFreeze();
        }
        else if (TimeTravelController.time != null && !TimeTravelController.time.IsFreezing() && isFrozen)
        {
            StopFreeze();
        }
    } 
    public void ClearTimeData()
    {
        // do nothing
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

    public bool ShouldApplyTimeVisualEffect()
    {
        return false;
    }

    public void StartFreeze()
    {
        isFrozen = true;
        climb.isDisabled = false;
    }

    public void StartRewind()
    {
        //climb.ForceDismount();
    }

    public void StopFreeze()
    {
        isFrozen = false;
        if (climb.inUse)
        {
            climb.ForceDismount();
        }
        climb.isDisabled = true;
    }

    public void StopRewind()
    {
        
    }

    public void SetRegistered()
    {
        throw new System.NotImplementedException();
    }

    public bool IsRegistered()
    {
        throw new System.NotImplementedException();
    }
}
