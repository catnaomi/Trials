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
                if (actor == null || !actor.IsAlive() || !actor.gameObject.activeInHierarchy) continue;
                if (shouldAttackImmediately && !actor.actionsEnabled)
                {
                    attack.ProcessHumanoidAction(actor, actor.MoveOnEnd);
                }
                actor.actionsEnabled = true;
            }
        }
    }
}
