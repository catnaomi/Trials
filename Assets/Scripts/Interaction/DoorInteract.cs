using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DoorInteract : Interactable
{
    public AnimancerComponent animancer;
    public ClipTransition open;
    public ClipTransition close;

    public bool isOpen;
    public Collider doorCollider;
    public bool shouldDisableCollider;
    public bool autoClose;
    public float autoCloseTime = 10f;
    float clock;
    public UnityEvent OnOpen;
    public UnityEvent OnClose;

    private void Start()
    {
        if (interactionNode == null)
        {
            interactionNode = this.GetComponent<Collider>();
        }
        if (!interactionNode.isTrigger)
        {
            Debug.LogWarning("Warning! Interaction Nodes should be triggers!");
        }
        if (interactionNode.gameObject.layer != LayerMask.NameToLayer("InteractionNode"))
        {
            Debug.LogWarning("Warning! Interaction Nodes must be in the layer 'InteractionNode' !");
        }
        SetIconVisiblity(false);
        animancer.Stop();
    }
    public override void Interact(PlayerActor player)
    {
        base.Interact(player);
        if (!isOpen)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    private void Update()
    {
        if (isOpen)
        {
            clock -= Time.deltaTime;
            if (clock < 0f)
            {
                Close();
            }
        }
    }
    public void Open()
    {
        if (!isOpen) animancer.Play(open);
        if (shouldDisableCollider && doorCollider != null)
        {
            doorCollider.enabled = false;
        }
        isOpen = true;
        OnOpen.Invoke();
        clock = autoCloseTime;
    }

    public void Close()
    {
        if (isOpen) animancer.Play(close);
        if (shouldDisableCollider && doorCollider != null)
        {
            doorCollider.enabled = true;
        }
        isOpen = false;
        OnClose.Invoke();
    }
}
