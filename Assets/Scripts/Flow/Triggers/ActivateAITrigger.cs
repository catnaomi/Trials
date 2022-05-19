using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateAITrigger : MonoBehaviour
{
    public NavigatingHumanoidActor[] actors;

    public bool shouldAttackImmediately = false;
    public InputAttack attack;
    public void OnTriggerEnter(Collider other)
    {
        if (PlayerActor.player == null) return;
        if (other.GetComponent<PlayerActor>() != null || other.transform.IsChildOf(PlayerActor.player.transform))
        {
            foreach (NavigatingHumanoidActor actor in actors)
            {
                if (shouldAttackImmediately && !actor.actionsEnabled)
                {
                    attack.ProcessHumanoidAttack(actor, actor.MoveOnEnd);
                }
                actor.actionsEnabled = true;
            }
        }
    }
}
