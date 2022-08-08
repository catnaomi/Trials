using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleTimeTravelHandler : MonoBehaviour, IAffectedByTimeTravel
{
    bool isFrozen;
    ParticleSystem particles;
    TimeTravelController timeTravelController;
    void Start()
    {
        particles = this.GetComponent<ParticleSystem>();
        timeTravelController = TimeTravelController.time;
        if (timeTravelController == null)
        {
            this.enabled = false;
            return;
        }
        TimeTravelController.time.RegisterAffectee(this);
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

    public bool IsRewinding()
    {
        return false;
    }

    public bool IsNull()
    {
        return this == null;
    }
    public void LoadTimeState(TimeTravelData data)
    {
        // do nothing
    }

    public TimeTravelData SaveTimeState()
    {
        // do nothing
        return null;
    }

    public bool ShouldApplyTimeVisualEffect()
    {
        return false;
    }

    public void StartFreeze()
    {
        isFrozen = true;
        if (particles.isPlaying)
        {
            particles.Pause();
        }
    }

    public void StartRewind()
    {
        // do nothing
    }

    public void StopFreeze()
    {
        isFrozen = false;
        if (particles.isPaused)
        {
            particles.Play();
        }
    }

    public void StopRewind()
    {
        // do nothing
    }

    void OnDestroy()
    {
        TimeTravelController.time.DeregisterAffectee(this);
    }
}
