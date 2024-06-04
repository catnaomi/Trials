using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class DirectorTimeTravelHandler : MonoBehaviour, IAffectedByTimeTravel
{
    PlayableDirector director;
    bool isFrozen;
    bool directorPlaying;
    bool directorPaused;
    bool registered;

    void Start()
    {
        director = this.GetComponent<PlayableDirector>();
        director.played += SetPlaying;
        director.stopped += SetStopped;
        director.paused += SetPaused;

        TimeTravelController.AttemptToRegisterAffectee(this);
    }

    void SetPlaying(PlayableDirector d)
    {
        directorPlaying = true;
        directorPaused = false;
    }

    void SetStopped(PlayableDirector d)
    {
        directorPaused = false;
        directorPlaying = false;
    }

    void SetPaused(PlayableDirector d)
    {
        directorPlaying = true;
        directorPaused = true;
    }

    public void StartFreeze()
    {
        if (directorPlaying)
        {
            isFrozen = true;
            //director.Pause(); // normal pause not used bc it resets positioning for some godforsaken reason

            if (director.playableGraph.IsValid())
            {
                director.playableGraph.GetRootPlayable(0).Pause();
            }
            
        }
    }


    public void StopFreeze()
    {
        if (isFrozen && director.playableGraph.IsValid() && director.playableGraph.GetRootPlayable(0).GetPlayState() == PlayState.Paused)
        {
            isFrozen = false;
            //director.Play();

            director.playableGraph.GetRootPlayable(0).Play();
        }
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

    public void SetRegistered()
    {
        registered = true;
    }

    public GameObject GetObject()
    {
        return this.gameObject;
    }


    public void ClearTimeData()
    {
        
    }

    
    public List<TimeTravelData> GetTimeStates()
    {
        return null;
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

    public bool ShouldApplyTimeVisualEffect()
    {
        return false;
    }

    public void StartRewind()
    {
        
    }

    public void StopRewind()
    {
        
    }

}
