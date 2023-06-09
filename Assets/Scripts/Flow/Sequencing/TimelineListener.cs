using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineListener : MonoBehaviour
{

    public static TimelineListener instance;
    [ReadOnly,SerializeField] List<PlayableDirector> playingDirectors;


    private void Awake()
    {
        instance = this;
        playingDirectors = new List<PlayableDirector>();
    }

    public static void Register(PlayableDirector director)
    {
        if (instance != null)
        {
            instance.RegisterLocal(director);
        }
    }

    void RegisterLocal(PlayableDirector director)
    {
        director.played += OnDirectorPlay;
        director.stopped += OnDirectorStop;
    }

    public static void Deregister(PlayableDirector director)
    {
        if (instance != null)
        {
            instance.DeregisterLocal(director);
        }
    }

    void DeregisterLocal(PlayableDirector director)
    {
        director.played -= OnDirectorPlay;
        director.stopped -= OnDirectorStop;
    }

    public void OnDirectorPlay(PlayableDirector director)
    {
        playingDirectors.Add(director);
    }

    public void OnDirectorStop(PlayableDirector director)
    {
        playingDirectors.Remove(director);
    }

    public static bool IsAnyDirectorPlaying()
    {
        if (instance != null)
        {
            return instance.IsAnyDirectorPlayingLocal();
        }
        return false;
    }

    bool IsAnyDirectorPlayingLocal()
    {
        playingDirectors.RemoveAll(d => d == null);
        return playingDirectors.Count > 0;
    }
}
