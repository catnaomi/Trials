using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShardPickup : Pickup
{
    public override void OnPickup()
    {
        base.OnPickup();
        TimeTravelController.time.RecoverCharge();
    }
}
