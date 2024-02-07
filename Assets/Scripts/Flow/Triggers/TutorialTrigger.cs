using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialTrigger : MonoBehaviour
{
    public UnityEngine.InputSystem.InputActionReference input;
    public string text;
    public bool onlyTriggersOnce;
    [ReadOnly] public bool triggered = false;
    public float expiryTime = -1;
    public bool ignoreTrigger = false;
    int interactionIndex = -1;


    
    public void OnTriggerEnter(Collider other)
    {
        if (ignoreTrigger || PlayerActor.player == null || (triggered && onlyTriggersOnce)) return;
        if (other.GetComponent<PlayerActor>() != null || other.transform.IsChildOf(PlayerActor.player.transform))
        {
            ShowTutorial();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (ignoreTrigger || other.GetComponent<PlayerActor>() != null || other.transform.IsChildOf(PlayerActor.player.transform))
        {
            HideTutorial();
        }
    }

    public void ShowTutorial()
    {
        if (triggered)
        {
            HideTutorial();
        }
        string inputString = TutorialHandler.GetInputString(input);
        interactionIndex = TutorialHandler.ShowTutorialStatic(text, inputString);
        triggered = true;
        if (expiryTime > 0)
        {
            StartCoroutine(ExpireAfterTimer());
        }
    }

    public void HideTutorial()
    {
        if (interactionIndex >= 0)
        {
            TutorialHandler.HideTutorialStatic(interactionIndex);
            interactionIndex = -1;
        }
    }

    IEnumerator ExpireAfterTimer()
    {
        yield return new WaitForSeconds(expiryTime);
        HideTutorial();
    }

    
}
