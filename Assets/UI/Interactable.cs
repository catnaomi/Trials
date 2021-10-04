using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public Collider interactionNode;
    protected PlayerActor player;
    public bool canInteract;
    public GameObject interactIcon;
    // Start is called before the first frame update
    void Start()
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
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        player = other.GetComponentInParent<PlayerActor>();
        if (player != null && canInteract)
        {
            player.AddInteractable(this);
            Debug.Log("Player enter!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerActor eplayer = other.GetComponentInParent<PlayerActor>();
        if (eplayer != null && eplayer == player)
        {
            player.RemoveInteractable(this);
            this.SetIconVisiblity(false);
            Debug.Log("Player exit!");
        }
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.RemoveInteractable(this);
        }
    }
    public void SetIconVisiblity(bool visible)
    {
        if (interactIcon != null)
        {
            interactIcon.SetActive(visible);
        }
        
    }

    public virtual void Interact(PlayerActor player)
    {
        // do nothing
    }
}
