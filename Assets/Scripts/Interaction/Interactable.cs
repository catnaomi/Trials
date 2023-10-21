using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public Collider interactionNode;
    protected PlayerActor player;
    public bool canInteract;
    public GameObject interactIcon;
    public int priority = 1;
    [Header("Interact UI Settings")]
    public float interactIconHeight = 1f;
    public string prompt;
    public float maxDistance = -1;

    public UnityEvent OnInteract;

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
        GetMaxDistance();
    }


    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        player = other.GetComponentInParent<PlayerActor>();
        if (player != null && canInteract)
        {
            player.AddInteractable(this);
            //Debug.Log("Player enter!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerActor eplayer = other.GetComponentInParent<PlayerActor>();
        if (eplayer != null && eplayer == player)
        {
            player.RemoveInteractable(this);
            this.SetIconVisiblity(false);
            //Debug.Log("Player exit!");
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
            interactIcon.SetActive(false);
        }
        
    }

    void GetMaxDistance()
    {
        if (maxDistance < 0 && interactionNode != null)
        {
            if (interactionNode is SphereCollider sphere)
            {
                maxDistance = sphere.radius;
            }
            else
            {
                maxDistance = interactionNode.bounds.extents.magnitude;
            }
        }
    }

    public virtual void Interact(PlayerActor player)
    {
        OnInteract.Invoke();
        // do nothing
    }
}
