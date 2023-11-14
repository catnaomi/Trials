using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class DirectorEventHelper : MonoBehaviour
{
    public UnityEvent OnPlay;
    public UnityEvent OnStop;
    public UnityEvent OnPause;
    // Start is called before the first frame update
    void Start()
    {
        PlayableDirector director = this.GetComponent<PlayableDirector>();
        director.played += PlayEvent;
        director.stopped += StopEvent;
        director.paused += PauseEvent;
    }


    void PlayEvent(PlayableDirector d)
    {
        OnPlay.Invoke();
    }

    void StopEvent(PlayableDirector d)
    {
        OnStop.Invoke();
    }

    void PauseEvent(PlayableDirector d)
    {
        OnPause.Invoke();
    }
}
