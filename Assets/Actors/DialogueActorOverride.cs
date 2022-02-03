using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class DialogueActorOverride : Interactable
{
    public Actor actor;
    public GameObject lookAtTarget;
    public Transform dialogueMount;
    DialogueRunner dialogue;
    bool talking;
    Vector3 lastDestination;
    GameObject lastDestinationTarget;
    bool wasNavigating;
    public override void Interact(PlayerActor player)
    {
        if (talking) return;
        player.SheatheAll();
        player.SetCombatTarget(lookAtTarget);
        player.StartDialogue();
        this.GetComponent<YarnPlayer>().Play();
        
        /*try
        {
            
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            return;
        }*/
        
        dialogue = GameObject.FindGameObjectWithTag("DialogueRunner").GetComponent<DialogueRunner>();
        dialogue.onDialogueComplete.AddListener(StopDialogue);
        dialogue.GetComponent<LineActorPositioningHandler>().SetSpeaker(this.gameObject, dialogueMount);
        talking = true;
        if (actor is NavigatingHumanoidActor navactor)
        {
            lastDestination = navactor.destination;
            if (navactor.followingTarget)
            {
                lastDestinationTarget = navactor.GetCombatTarget();
            }
            else
            {
                lastDestinationTarget = null;
            }
            wasNavigating = navactor.shouldNavigate;
            navactor.StopNavigation();
            navactor.SetDestination(player.gameObject);
        }
    }

    public void StopDialogue()
    {
        PlayerActor.player.StopDialogue();
        if (actor is NavigatingHumanoidActor navactor)
        {
            if (wasNavigating)
            {
                if (lastDestinationTarget != null)
                {
                    navactor.SetDestination(lastDestinationTarget);
                }
                else
                {
                    navactor.SetDestination(lastDestination);
                }
                navactor.ResumeNavigation();
            }
        }
        if (dialogue != null) dialogue.onDialogueComplete.RemoveListener(StopDialogue);
        StartCoroutine(StopTalkingDelay());
    }

    IEnumerator StopTalkingDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
        talking = false;
    }
}
