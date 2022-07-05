using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTimeTravelHandler : ActorTimeTravelHandler
{
    public override bool ShouldApplyTimeVisualEffect()
    {
        return (actor is PlayerActor player && player.IsResurrecting());
    }
}
