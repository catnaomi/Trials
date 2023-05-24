using Animancer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
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

    [Space(40)]

    public HandAnim[] anims;

    [Space(10)]
    public int index;
    public bool playHandAnim;
    bool playingHandAnim;
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
        if (playHandAnim)
        {
            playHandAnim = false;
            PlayHandIndex();
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
        else if (playingHandAnim)
        {
            Transform handTransform = anims[index].transform;
            perspectiveCamera.transform.position = handTransform.position;
            perspectiveCamera.transform.rotation = handTransform.rotation;
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

    public void PlayHandIndex()
    {
        playingHandAnim = true;
        AnimancerState state = animancer.Play(anims[index].clip);
        perspectiveCamera.transform.position = anims[index].transform.position;
        perspectiveCamera.transform.rotation = anims[index].transform.rotation;

        state.Events.OnEnd = () =>
        {
            playingHandAnim = false;
        };
    }

    [Serializable]
    public struct HandAnim
    {
        public string name;
        public ClipTransition clip;
        public Transform transform;
    }
}
