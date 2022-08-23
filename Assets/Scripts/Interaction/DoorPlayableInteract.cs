using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

public class DoorPlayableInteract : Interactable
{
    public AnimancerComponent doorAnimancer;
    public Transform refTransform;
    public ClipTransition openHumanAnim;
    public ClipTransition openDoorAnim;
    public ClipTransition closeHumanAnim;
    public ClipTransition closeDoorAnim;

    public bool isOpen;
    bool playing;
    bool playStart;

    public Collider doorCollider;
    public DoorPlayableInteract otherInteract;
    public float autoCloseTime = -1f;
    float clock;

    UnityEngine.SceneManagement.Scene scene;

    public override void Interact(PlayerActor player)
    {
        base.Interact(player);


        StartTimeline();
    }

    public void StartTimeline()
    {
        player = PlayerActor.player;
        /*
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
        */

        if (!isOpen)
        {
            player.GetComponent<CharacterController>().enabled = false;
            doorCollider.enabled = false;
            player.GetComponent<Collider>().enabled = false;
            AnimancerState state = player.GetComponent<Animancer.AnimancerComponent>().Play(openHumanAnim);
            player.airTime = 0f;
            //player.transform.position = refTransform.position;
            player.transform.rotation = refTransform.rotation;
            player.SetExternalSourceState(state);
            state.Events.OnEnd = () =>
            {
                player.GetComponent<CharacterController>().enabled = true;
                player.PlayMove();
            }; 
            OpenDoor();
        }
        else
        {
            AnimancerState state = player.GetComponent<Animancer.AnimancerComponent>().Play(openHumanAnim);
            player.airTime = 0f;
            //player.transform.position = refTransform.position;
            player.transform.rotation = refTransform.rotation;
            player.SetExternalSourceState(state);
            state.Events.OnEnd = player.PlayMove;
            CloseDoor();
        }
        
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            doorAnimancer.Play(openDoorAnim);
            doorCollider.enabled = false;
            isOpen = true;
            otherInteract.isOpen = true;
            otherInteract.clock = 0f;
            if (autoCloseTime > 0)
            {
                clock = autoCloseTime;
                
            }
        }
    }

    public void CloseDoor()
    {
        if (isOpen)
        {
            doorAnimancer.Play(closeDoorAnim);
            doorCollider.enabled = true;
            isOpen = false;
            otherInteract.isOpen = false;
            clock = 0f;
            otherInteract.clock = 0f;
        }
    }
    private void Update()
    {
        if (clock > 0f)
        {
            clock -= Time.deltaTime;
            if (clock <= 0f)
            {
                CloseDoor();
            }
        }
        /*
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
        */
    }
    /*
    List<PlayableBinding> GetBindings()
    {
        TimelineAsset timeline = (TimelineAsset)director.playableAsset;
        List<PlayableBinding> bindings = new List<PlayableBinding>();
        foreach (var binding in timeline.outputs)
        {
            bindings.Add(binding);
        }
        return bindings;
    }*/
}
