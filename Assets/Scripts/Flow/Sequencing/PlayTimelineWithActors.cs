using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Yarn.Unity;

public class PlayTimelineWithActors : MonoBehaviour
{
    public PlayableDirector director;
    public bool playOnAwake;
    public bool destroyOnComplete;
    public bool hidePlayer;
    bool playing;
    public Transform playerRefTransform;
    public Animator fakePlayer;

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
        playerRefTransform.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (playing)
        {
            PlayerActor.player.transform.position = fakePlayer.transform.position;
        }
    }


    public void Play()
    {
        playerRefTransform.gameObject.SetActive(true);
        SetBindings();
        director.Play();
        playing = true;
        if (hidePlayer)
        {
            PlayerActor.player.gameObject.SetActive(false);
            
        }
        director.stopped += (context) =>
        {
            Stop(context);
        };
    }

    private void Stop(PlayableDirector d)
    {
        playing = false;
        if (hidePlayer)
        {
            PlayerActor.player.gameObject.SetActive(true);
        }
        PlayerActor.player.JumpToNavMesh();
        playerRefTransform.gameObject.SetActive(false);
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
