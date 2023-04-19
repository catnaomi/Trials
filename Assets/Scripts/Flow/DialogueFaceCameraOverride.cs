using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn;
using Yarn.Unity;

public class DialogueFaceCameraOverride : MonoBehaviour
{
    DialogueRunner dialogue;
    bool talking;
    Vector3 lastDestination;
    GameObject lastDestinationTarget;
    bool wasNavigating;
    public void StartDialogue()
    {
        PlayerActor player = PlayerActor.player;
        if (talking) return;
        player.SheatheAll();
        player.LookAtCamera(true);
        player.StartDialogue();
        try
        {
            this.GetComponent<YarnPlayer>().Play();
        }
        catch (DialogueException ex)
        {
            Debug.LogError(ex);
            player.StopDialogue();
            return;
        }

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
        talking = true;
    }

    public void StopDialogue()
    {
        PlayerActor.player.StopDialogue();
        PlayerActor.player.LookAtCamera(false);
        if (dialogue != null) dialogue.onDialogueComplete.RemoveListener(StopDialogue);
        StartCoroutine(StopTalkingDelay());
    }

    IEnumerator StopTalkingDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
        talking = false;
    }
}
