using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    public bool playerOnly;
    public UnityEvent OnEnter;
    public UnityEvent OnExit;

    private void OnTriggerEnter(Collider other)
    {
        if (playerOnly && !other.TryGetComponent<PlayerActor>(out PlayerActor player))
        {
            return;
        }
        OnEnter.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerOnly && !other.TryGetComponent<PlayerActor>(out PlayerActor player))
        {
            return;
        }
        OnExit.Invoke();
    }
}
