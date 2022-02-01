using System.Collections;
using UnityEngine;

public class PickupInteract : Interactable
{
    public Item item;
    public bool destroyOnInteract;
    public GameObject rootObject;
    public override void Interact(PlayerActor player)
    {
        player.inventory.Add(item);
        if (destroyOnInteract)
        {
            GameObject.Destroy(rootObject);
        }
    }

}