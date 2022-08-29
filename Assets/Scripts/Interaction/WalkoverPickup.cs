using System.Collections;
using UnityEngine;

public class WalkoverPickup : MonoBehaviour
{
    public Item item;
    Collider interactionNode;
    public GameObject rootObject;
    // Use this for initialization
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
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerActor player = other.GetComponentInParent<PlayerActor>();
        if (player != null)
        {
            player.GetInventory().Add(item);
            Destroy(rootObject);
            //Debug.Log("Player enter!");
        }
    }
}