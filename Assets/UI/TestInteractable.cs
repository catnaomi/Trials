using UnityEngine;
using System.Collections;

public class TestInteractable : Interactable
{

    public override void Interact(PlayerActor player)
    {
        Debug.Log(this.name + " was interacted with by " + player.name + "!!!!");
    }
}
