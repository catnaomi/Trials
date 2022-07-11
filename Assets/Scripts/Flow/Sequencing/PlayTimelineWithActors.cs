using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class PlayTimelineWithActors : MonoBehaviour
{
    public PlayableDirector director;
    public bool playOnAwake;
    public bool destroyOnComplete;
    public bool hidePlayer;

    public Transform playerRefTransform;

    [SerializeField] BindingIndex[] bindingIndexMap;
    [Serializable]
    struct BindingIndex
    {
        public int index;
        public UnityEngine.Object obj;
        public bool isPlayer;
        public bool isCinemachineBrain;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Play()
    {
        SetBindings();
        director.Play();
        if (hidePlayer)
        {
            PlayerActor.player.gameObject.SetActive(false);
            director.stopped += (c) =>
            {
                PlayerActor.player.gameObject.SetActive(true);
            };
        }
    }

    void SetBindings()
    {
        var bindings = GetBindings();

        foreach (BindingIndex bindingIndex in bindingIndexMap)
        {
            int index = bindingIndex.index;
            var track = bindings[index].sourceObject;
            
            if (bindingIndex.isPlayer)
            {
                director.SetGenericBinding(track, PlayerActor.player.GetComponent<Animator>());
                playerRefTransform.position = PlayerActor.player.transform.position;
                playerRefTransform.rotation = PlayerActor.player.transform.rotation;
            }
            else if (bindingIndex.isCinemachineBrain)
            {
                director.SetGenericBinding(track, Camera.main.GetComponent<CinemachineBrain>());
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
