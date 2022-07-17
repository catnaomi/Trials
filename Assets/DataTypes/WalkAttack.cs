using UnityEngine;
using System.Collections;
using Animancer;
using System;

[CreateAssetMenu(fileName = "phaseatk0000_name", menuName = "ScriptableObjects/Attacks/Walk Attack", order = 1)]
public class WalkAttack : InputAttack
{
    [SerializeField] private ClipTransition walk;
    [SerializeField] private float transitionTime;
    public ClipTransition GetWalkClip()
    {
        return walk;
    }


    public override AnimancerState ProcessHumanoidAction(NavigatingHumanoidActor actor, Action endEvent)
    {
        
        AnimancerState walkState = actor.animancer.Play(this.GetWalkClip());
        float walkLength = walkState.Length;
        actor.SetCurrentDamage(this.GetDamage());
        AnimancerState attackState = actor.animancer.Layers[HumanoidAnimLayers.UpperBody].Play(this.GetClip());
        float attackLength = attackState.Length;
        attackState.Speed = walkLength / attackLength;
        attackState.Events.OnEnd = () =>
        {
            actor.animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
            endEvent();
        };
        return walkState;
    }
}
