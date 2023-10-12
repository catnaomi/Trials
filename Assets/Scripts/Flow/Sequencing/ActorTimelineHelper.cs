using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class ActorTimelineHelper : MonoBehaviour
{
    PlayableDirector director;
    [ReadOnly] public List<Actor> boundActors;
    // Start is called before the first frame update
    void Start()
    {
        director = this.GetComponent<PlayableDirector>();
        boundActors = new List<Actor>();

        director.played += DirectorPlay;
        director.paused += DirectorPause;
        director.stopped += DirectorStop;
    }

    void DirectorPlay(PlayableDirector d)
    {
        boundActors.Clear();
        foreach (var binding in director.GetTimelineBindings())
        {
            if (binding.sourceObject == null) continue;

            var target = director.GetGenericBinding(binding.sourceObject);

            if (target == null) continue;

            if (target is Animator animatorBinding)
            {
                if (animatorBinding.TryGetComponent<Actor>(out Actor actorBinding))
                {
                    boundActors.Add(actorBinding);
                    actorBinding.SetInTimeline(true);
                }
            }
        }
    }

    void DirectorStop(PlayableDirector d)
    {
        foreach (Actor actor in boundActors)
        {
            actor.SetInTimeline(false);
        }
    }

    void DirectorPause(PlayableDirector d)
    {

    }
}
