using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartPickup : Pickup
{
    public float healAmount = 1f;

    public override void OnPickup()
    {
        base.OnPickup();
        PlayerActor.player.attributes.RecoverHealth(healAmount);
    }
}
