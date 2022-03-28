using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

public class DoorPlayableInteract : Interactable
{
    public PlayableDirector director;
    public Transform refTransform;
    bool playing;
    bool playStart;

    public Collider doorCollider;
    UnityEngine.SceneManagement.Scene scene;

    public override void Interact(PlayerActor player)
    {
        base.Interact(player);


        StartTimeline();
    }

    public void StartTimeline()
    {
        player = PlayerActor.player;
        var bindings = GetBindings();

        var track = bindings[0].sourceObject;

        director.SetGenericBinding(track, player.GetComponent<Animator>());
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<Animancer.AnimancerComponent>().enabled = false;
        doorCollider.enabled = false;
        player.GetComponent<Collider>().enabled = false;
        director.Play();
        scene = player.gameObject.scene;
        player.transform.position = refTransform.position;
        player.transform.SetParent(director.transform, true);
        playing = true;
        playStart = true;
    }
    private void Update()
    {
        if (playing)
        {
            player.airTime = 0f;

            if (playStart)
            {
                player.transform.position = refTransform.position;
                playStart = false;
            }
            if (director.time >= director.duration)
            {
                director.Stop();
                player.transform.parent = null;
                SceneManager.MoveGameObjectToScene(player.gameObject, scene);
                
                player.GetComponent<CharacterController>().enabled = true;
                player.GetComponent<Animancer.AnimancerComponent>().enabled = true;
                doorCollider.enabled = true;
                player.GetComponent<Collider>().enabled = true;
                player.ResetOnMove();
                var bindings = GetBindings();

                var track = bindings[0].sourceObject;

                director.SetGenericBinding(track, refTransform.GetComponent<Animator>());
                playing = false;
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
