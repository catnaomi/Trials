using Animancer;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "atk0000_name", menuName = "ScriptableObjects/Actions/Play Drink Action", order = 1)]
public class DrinkAction : InputAction
{
    public LinearMixerTransitionAsset walk;
    public ClipTransition drink;
    public float exitTime = 1f;
    public virtual LinearMixerTransition GetWalkClip()
    {
        return walk.Transition;
    }
    public virtual ClipTransition GetDrinkClip()
    {
        return drink;
    }

    public float GetExitTime()
    {
        return exitTime;
    }


    public override AnimancerState ProcessPlayerAction(PlayerActor player, out float cancelTime, System.Action endEvent)
    {

        var states = player.PlayDrinkClip(this.GetWalkClip(), this.GetDrinkClip());
        AnimancerState drink = states.Item2;
        AnimancerState walk = states.Item1;
        cancelTime = this.GetExitTime();
        drink.Events.OnEnd += endEvent;
        return walk;
    }
}