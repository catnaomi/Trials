using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActivateAITrigger : MonoBehaviour
{
    public NavigatingHumanoidActor[] actors;

    public bool shouldAttackImmediately = false;
    public InputAttack attack;

    public UnityEvent OnTrigger;
    bool triggered;
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
                if (!triggered)
                {
                    triggered = true;
                    OnTrigger.Invoke();
                }
            }
        }
    }
}
