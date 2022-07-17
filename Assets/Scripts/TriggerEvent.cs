using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    public bool playerOnly;
    public UnityEvent OnEnter;
    public UnityEvent OnExit;
    public bool enterTriggersOnce = true;
    public bool exitTriggersOnce = true;
    bool enterTriggered;
    bool exitTriggered;
    private void OnTriggerEnter(Collider other)
    {
        if (enterTriggersOnce && enterTriggered) return;
        if (playerOnly && !other.TryGetComponent<PlayerActor>(out PlayerActor player))
        {
            return;
        }
        OnEnter.Invoke();
        enterTriggered = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (exitTriggersOnce && exitTriggered) return;
        if (playerOnly && !other.TryGetComponent<PlayerActor>(out PlayerActor player))
        {
            return;
        }
        OnExit.Invoke();
        exitTriggered = true;
    }
}
