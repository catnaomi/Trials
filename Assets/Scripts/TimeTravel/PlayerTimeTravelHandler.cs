using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTimeTravelHandler : ActorTimeTravelHandler
{
    public override bool ShouldApplyTimeVisualEffect()
    {
        return (actor is PlayerActor player && player.IsResurrecting());
    }

    public override TimeTravelData SaveTimeState()
    {
        PlayerTimeTravelData data = (PlayerTimeTravelData)base.SaveTimeState();

        data.inWorld2 = PortalManager.instance.inWorld2;
        return data;
    }

    public override void LoadTimeState(TimeTravelData data, float speed)
    {
        PlayerActor player = actor as PlayerActor;
        CharacterController cc = player.GetComponent<CharacterController>();
        bool ccWasEnabled = cc.enabled;

        player.GetComponent<CharacterController>();

        base.LoadTimeState(data, speed);

        cc.enabled = ccWasEnabled;

        if (PortalManager.instance != null && ((PlayerTimeTravelData)data).inWorld2 != PortalManager.instance.inWorld2)
        {
            PortalManager.instance.Swap();
        }
    }
}
