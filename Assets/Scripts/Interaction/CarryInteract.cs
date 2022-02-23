using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarryInteract : Interactable
{
    public Carryable carryable;
    // Start is called before the first frame update
    void Start()
    {
        carryable.OnStopCarry.AddListener(() => { canInteract = true; });
    }

    public override void Interact(PlayerActor player)
    {
        carryable.Carry(player);
        canInteract = false;
    }
}
