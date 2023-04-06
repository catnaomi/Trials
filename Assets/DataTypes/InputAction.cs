using UnityEngine;
using System.Collections;
using System;
using Animancer;

[Serializable]
public class InputAction : ScriptableObject
{
    public string desc;

    public string GetDescription()
    {
        return desc;
    }

    public virtual AnimancerState ProcessHumanoidAction(NavigatingHumanoidActor actor, System.Action endEvent)
    {
        return null;
    }

    public virtual AnimancerState ProcessGenericAction(Actor actor, Action endEvent)
    {
        return null;
    }
    public virtual AnimancerState ProcessPlayerAction(PlayerActor player, out float cancelTime, System.Action endEvent)
    {

        // do nothing
        cancelTime = -1f;
        return null;
    }
}
