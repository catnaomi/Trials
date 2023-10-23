using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseTutorials : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (PlayerActor.player == null) return;
        if (other.GetComponent<PlayerActor>() != null || other.transform.IsChildOf(PlayerActor.player.transform))
        {
            Close();
        }
    }

    public void Close()
    {
        TutorialHandler.HideAllStatic();
    }
}
