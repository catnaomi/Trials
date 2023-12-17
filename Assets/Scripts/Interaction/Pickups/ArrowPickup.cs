using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowPickup : Pickup
{
    public Item arrowItem;
    public int amount;

    public override void OnPickup()
    {
        base.OnPickup();
        PlayerActor.player.inventory.AddMany(arrowItem, amount);
    }
}
