using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventInteract : Interactable
{
    public UnityEvent OnInteract;
    
    public override void Interact(PlayerActor player)
    {
        OnInteract.Invoke();
    }
}
