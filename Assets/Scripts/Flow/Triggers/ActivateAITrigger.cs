using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ActivateAITrigger : MonoBehaviour, IEventVisualizable
{
    public Actor[] actors;

    public bool shouldAttackImmediately = false;
    public InputAttack attack;

    public UnityEvent OnTrigger;
    bool triggered;

    public GameObject[] GetEventTargets()
    {
        return actors.Where(actor => actor != null).Select(actor => actor.gameObject).ToArray();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (PlayerActor.player == null) return;
        if (other.GetComponent<PlayerActor>() != null || other.transform.IsChildOf(PlayerActor.player.transform))
        {
            foreach (Actor actor in actors)
            {
                if (actor == null || !actor.IsAlive() || !actor.gameObject.activeInHierarchy) continue;
                actor.gameObject.SendMessage("EnableActions");
            }
            if (!triggered)
            {
                triggered = true;
                OnTrigger.Invoke();
            }
        }
    }
}
