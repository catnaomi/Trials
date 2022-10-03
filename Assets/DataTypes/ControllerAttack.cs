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


    public override AnimancerState ProcessHumanoidAction(NavigatingHumanoidActor actor, Action endEvent)
    {
        ControllerState state = (ControllerState)actor.animancer.States.GetOrCreate(this.GetController());
        state.Stop();
        state.ApplyActionsOnStop();
        state.Play(0);
        actor.animancer.Play(state);
        actor.SetCurrentDamage(this.GetDamage());
        //state.Events.OnEnd = endEvent;
        return state;
    }
}
