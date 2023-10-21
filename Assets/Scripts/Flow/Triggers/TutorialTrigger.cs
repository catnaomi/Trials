using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public Sprite icon1;
    public Sprite icon2;
    public Sprite icon3;
    public string text;
    public bool onlyTriggersOnce;
    [ReadOnly] public bool triggered = false;
    public float expiryTime = -1;
    int interactionIndex = -1;


    
    public void OnTriggerEnter(Collider other)
    {
        if (PlayerActor.player == null || (triggered && onlyTriggersOnce)) return;
        if (other.GetComponent<PlayerActor>() != null || other.transform.IsChildOf(PlayerActor.player.transform))
        {
            ShowTutorial();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerActor>() != null || other.transform.IsChildOf(PlayerActor.player.transform))
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
        interactionIndex = TutorialHandler.ShowTutorialStatic(text);
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
