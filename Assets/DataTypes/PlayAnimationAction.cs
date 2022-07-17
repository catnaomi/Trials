using Animancer;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "atk0000_name", menuName = "ScriptableObjects/Actions/Play Animation Action", order = 1)]
public class PlayAnimationAction : InputAction
{
    public ClipTransition animation;
    public float exitTime = 1f;
    public virtual ClipTransition GetClip()
    {
        return animation;
    }

    public float GetExitTime()
    {
        return exitTime;
    }


    public override AnimancerState ProcessPlayerAction(PlayerActor player, out float cancelTime, System.Action endEvent)
    {

        AnimancerState state = player.animancer.Play(this.GetClip());
        cancelTime = this.GetExitTime();
        state.Events.OnEnd = endEvent;
        return state;
    }
}