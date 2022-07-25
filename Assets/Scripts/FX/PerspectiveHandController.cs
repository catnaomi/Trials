using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveHandController : MonoBehaviour
{
    AnimancerComponent animancer;
    public Camera perspectiveCamera;

    public ClipTransition spellCastForwards;
    public Transform spellCastForwardsTransform;

    public ClipTransition spellResurrect;
    public Transform spellResurrectTransform;

    public ClipTransition spellStop;
    public Transform spellStopTransform;


    public bool playSpellCastForwards;
    bool playingSpellCastForwards;
    public bool playSpellResurrect;
    bool playingSpellResurrect;
    public bool playSpellStop;
    bool playingSpellStop;
    private void Start()
    {
        animancer = this.GetComponent<AnimancerComponent>();
    }

    private void Update()
    {
        if (playSpellCastForwards)
        {
            playSpellCastForwards = false;
            PlaySpellCastForwards();
        }
        if (playSpellResurrect)
        {
            playSpellResurrect = false;
            PlaySpellResurrect();
        }
        if (playSpellStop)
        {
            playSpellStop = false;
            PlaySpellStop();
        }
        if (playingSpellCastForwards)
        {
            perspectiveCamera.transform.position = spellCastForwardsTransform.position;
            perspectiveCamera.transform.rotation = spellCastForwardsTransform.rotation;
        }
        else if (playingSpellResurrect)
        {
            perspectiveCamera.transform.position = spellResurrectTransform.position;
            perspectiveCamera.transform.rotation = spellResurrectTransform.rotation;
        }
        else if (playingSpellStop)
        {
            perspectiveCamera.transform.position = spellStopTransform.position;
            perspectiveCamera.transform.rotation = spellStopTransform.rotation;
        }
    }

    public void PlaySpellCastForwards()
    {
        playingSpellCastForwards = true;
        AnimancerState state = animancer.Play(spellCastForwards);
        perspectiveCamera.transform.position = spellCastForwardsTransform.position;
        perspectiveCamera.transform.rotation = spellCastForwardsTransform.rotation;

        state.Events.OnEnd = () =>
        {
            playingSpellCastForwards = false;
        };
    }

    public void PlaySpellResurrect()
    {
        playingSpellResurrect = true;
        AnimancerState state = animancer.Play(spellResurrect);
        perspectiveCamera.transform.position = spellResurrectTransform.position;
        perspectiveCamera.transform.rotation = spellResurrectTransform.rotation;

        state.Events.OnEnd = () =>
        {
            playingSpellResurrect = false;
        };
    }

    public void PlaySpellStop()
    {
        playingSpellStop = true;
        AnimancerState state = animancer.Play(spellStop);
        perspectiveCamera.transform.position = spellStopTransform.position;
        perspectiveCamera.transform.rotation = spellStopTransform.rotation;

        state.Events.OnEnd = () =>
        {
            playingSpellStop = false;
        };
    }
}
