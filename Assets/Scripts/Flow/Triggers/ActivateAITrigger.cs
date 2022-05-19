using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateAITrigger : MonoBehaviour
{
    public NavigatingHumanoidActor[] actors;

    public void OnTriggerEnter(Collider other)
    {
        if (PlayerActor.player == null) return;
        if (other.GetComponent<PlayerActor>() != null || other.transform.IsChildOf(PlayerActor.player.transform))
        {
            foreach (NavigatingHumanoidActor actor in actors)
            {
                actor.actionsEnabled = true;
            }
        }
    }
}
