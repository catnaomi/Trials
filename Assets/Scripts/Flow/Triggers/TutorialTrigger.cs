using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public Sprite icon1;
    public Sprite icon2;
    public Sprite icon3;
    public string text;
    public bool triggered = false;

    public void OnTriggerEnter(Collider other)
    {
        if (PlayerActor.player == null || triggered) return;
        if (other.GetComponent<PlayerActor>() != null || other.transform.IsChildOf(PlayerActor.player.transform))
        {
            TutorialHandler.ShowTutorialStatic(icon1, icon2, icon3, text);
            triggered = true;
        }
    }
}
