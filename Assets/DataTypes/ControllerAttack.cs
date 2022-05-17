using UnityEngine;
using System.Collections;
using Animancer;
using System;

[CreateAssetMenu(fileName = "phaseatk0000_name", menuName = "ScriptableObjects/Attacks/Controller Attack", order = 1)]
public class ControllerAttack : InputAttack
{
    [SerializeField] private ControllerTransition controller;
    public ControllerTransition GetController()
    {
        return controller;
    }


    public override AnimancerState ProcessHumanoidAttack(NavigatingHumanoidActor actor, Action endEvent)
    {

        AnimancerState state = actor.animancer.Play(this.GetController());
        actor.SetCurrentDamage(this.GetDamage());
        state.Events.OnEnd = endEvent;
        return state;
    }
}
