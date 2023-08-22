using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActivateAITrigger : MonoBehaviour
{
    public Actor[] actors;

    public bool shouldAttackImmediately = false;
    public InputAttack attack;

    public UnityEvent OnTrigger;
    bool triggered;
    public void OnTriggerEnter(Collider other)
    {
        if (PlayerActor.player == null) return;
        if (other.GetComponent<PlayerActor>() != null || other.transform.IsChildOf(PlayerActor.player.transform))
        {
            foreach (Actor actor in actors)
            {
                if (actor == null || !actor.IsAlive() || !actor.gameObject.activeInHierarchy) continue;
                if (actor is NavigatingHumanoidActor navActor)
                {
                    if (shouldAttackImmediately && !navActor.actionsEnabled)
                    {
                        attack.ProcessHumanoidAction(navActor, navActor.MoveOnEnd);
                    }
                    navActor.EnableActions();
                }
                else if (actor is INavigates navigates)
                {
                    navigates.EnableActions();
                }

            }
            if (!triggered)
            {
                triggered = true;
                OnTrigger.Invoke();
            }
        }
    }
}
