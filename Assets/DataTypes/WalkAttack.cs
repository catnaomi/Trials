﻿using UnityEngine;
using System.Collections;
using Animancer;
using System;

[CreateAssetMenu(fileName = "phaseatk0000_name", menuName = "ScriptableObjects/Attacks/Walk Attack", order = 1)]
public class WalkAttack : InputAttack
{
    [SerializeField] private ClipTransition walk;
    [SerializeField] private ClipTransition upperBodyPose;
    [SerializeField] private float transitionTime;
    public ClipTransition GetWalkClip()
    {
        return walk;
    }

    public ClipTransition GetUpperBodyClip()
    {
        return upperBodyPose;
    }

    public override AnimancerState ProcessHumanoidAction(NavigatingHumanoidActor actor, Action endEvent)
    {
        
        AnimancerState walkState = actor.animancer.Play(this.GetWalkClip());
        actor.animancer.Layers[HumanoidAnimLayers.UpperBody].Play(this.GetUpperBodyClip());
        float walkLength = walkState.Length;
        walkState.Events.OnEnd = () =>
        {
            actor.SetCurrentDamage(this.GetDamage());
            actor.animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
            AnimancerState attackState = actor.animancer.Play(this.GetClip(), transitionTime);
            attackState.Events.OnEnd = () =>
            {
                endEvent();
            };
        };
        return walkState;
    }
}
