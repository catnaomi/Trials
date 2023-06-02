using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Yarn.Unity;

public class PlayTimelineWithActors : MonoBehaviour
{
    public PlayableDirector director;
    public bool playOnAwake;
    public bool destroyOnComplete;
    public bool hidePlayer;
    public bool disablePlayerMovement;
    bool playing;
    public Transform playerRefTransform;
    public bool refTransformFollowsPlayer = false;
    public Animator fakePlayer;
    public UnityEvent OnStart;
    public UnityEvent OnEnd;
    [SerializeField] BindingIndex[] bindingIndexMap;
    [Serializable]
    struct BindingIndex
    {
        public int index;
        public RuntimeBinding bindingType;
        public UnityEngine.Object obj;

        public enum RuntimeBinding
        {
            DefinedBelow,
            PlayerAnimator,
            PlayerGameObject,
            CinemachineBrain,
            DialogueRunner
        }

        public bool IsPlayerAnimator()
        {
            return bindingType == RuntimeBinding.PlayerAnimator;
        }

        public bool IsCinemachineBrain()
        {
            return bindingType == RuntimeBinding.CinemachineBrain;
        }

        public bool IsDialogueRunner()
        {
            return bindingType == RuntimeBinding.DialogueRunner;
        }

        public bool IsPlayerObject()
        {
            return bindingType == RuntimeBinding.PlayerGameObject;
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        if (playerRefTransform != null)
        playerRefTransform.gameObject.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!playing && playOnAwake)
        {
            Play();
            playOnAwake = false;
        }
        if (playing && fakePlayer != null)
        {
            PlayerActor.player.transform.position = fakePlayer.transform.position;
        }
        if (playing && playerRefTransform != null && refTransformFollowsPlayer)
        {
            playerRefTransform.position = PlayerActor.player.transform.position;
            playerRefTransform.rotation = PlayerActor.player.transform.rotation;
        }
    }


    public void Play()
    {
        if (playerRefTransform != null)
        {
            playerRefTransform.gameObject.SetActive(true);
        }
        
        SetBindings();
        director.Play();
        playing = true;
        OnStart.Invoke();
        if (hidePlayer)
        {
            PlayerActor.player.gameObject.SetActive(false);
            
        }
        else if (disablePlayerMovement)
        {
            PlayerActor.player.StartDialogue();
        }
        director.stopped += (context) =>
        {
            Stop(context);
        };
    }

    private void Stop(PlayableDirector d)
    {
        playing = false;
        if (PlayerActor.player != null)
        {
            if (hidePlayer)
            {
                PlayerActor.player.gameObject.SetActive(true);
            }
            else if (disablePlayerMovement)
            {
                PlayerActor.player.StopDialogue();
            }
            PlayerActor.player.JumpToNavMesh();
        }

        if (playerRefTransform != null) playerRefTransform.gameObject.SetActive(false);
        OnEnd.Invoke();
        if (destroyOnComplete)
        {
            Destroy(this.gameObject);
        }
    }
    void SetBindings()
    {
        var bindings = GetBindings();

        foreach (BindingIndex bindingIndex in bindingIndexMap)
        {
            int index = bindingIndex.index;
            var track = bindings[index].sourceObject;
            
            if (bindingIndex.IsPlayerAnimator())
            {
                director.SetGenericBinding(track, PlayerActor.player.GetComponent<Animator>());
                playerRefTransform.position = PlayerActor.player.transform.position;
                playerRefTransform.rotation = PlayerActor.player.transform.rotation;
            }
            else if (bindingIndex.IsCinemachineBrain())
            {
                director.SetGenericBinding(track, Camera.main.GetComponent<CinemachineBrain>());
            }
            else if (bindingIndex.IsDialogueRunner())
            {
                director.SetGenericBinding(track, GameObject.FindGameObjectWithTag("DialogueRunner").GetComponent<DialogueRunner>());
            }
            else if (bindingIndex.IsPlayerObject())
            {

                director.SetGenericBinding(track, PlayerActor.player.gameObject);
            }
            else
            {
                director.SetGenericBinding(track, bindingIndex.obj);
            }
        }
    }
    List<PlayableBinding> GetBindings()
    {
        TimelineAsset timeline = (TimelineAsset)director.playableAsset;
        List<PlayableBinding> bindings = new List<PlayableBinding>();
        foreach (var binding in timeline.outputs)
        {
            bindings.Add(binding);
        }
        return bindings;
    }
}
