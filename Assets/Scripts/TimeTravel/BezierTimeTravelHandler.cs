using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Projectile))]
public class BezierTimeTravelHandler : MonoBehaviour, IAffectedByTimeTravel
{
    public TimeTravelController timeTravelController;
    bool isRewinding;
    bool isFrozen;
    bool frozenOnCreate;
    BezierProjectileController projectile;
    ProjectileTimeTravelData lastData;

    void Start()
    {
        timeTravelController = TimeTravelController.time;
        if (timeTravelController == null)
        {
            this.enabled = false;
            return;
        }
        projectile = this.GetComponent<BezierProjectileController>();
        timeTravelController.RegisterAffectee(this);
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
    public bool IsRewinding()
    {
        return isRewinding;
    }

    public bool IsNull()
    {
        return this == null;
    }

    public void LoadTimeState(TimeTravelData data)
    {
        return;
    }

    public TimeTravelData SaveTimeState()
    {
        return null;
    }

    public void StartFreeze()
    {
        isFrozen = true;
        projectile.SetHitbox(false);
        //lastData = (ProjectileTimeTravelData)SaveTimeState();

    }

    public void StartRewind()
    {
        isRewinding = true;
    }

    public void StopFreeze()
    {
        isFrozen = false;
        projectile.SetHitbox(true);
    }

    public void StopRewind()
    {
        isRewinding = false;
        
    }

    void OnDestroy()
    {
        timeTravelController.DeregisterAffectee(this);
    }

    public bool ShouldApplyTimeVisualEffect()
    {
        return false;
    }

    public void ClearTimeData()
    {
        return;
    }

    public float GetFixedDeltaTime()
    {
        if (IsFrozen())
        {
            return 0f;
        }
        else
        {
            return Time.fixedDeltaTime;
        }
    }
}