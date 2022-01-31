using Animancer;
using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class ActorTimeTravelData : TimeTravelData
{
    public float time;
    public AnimancerState animancerState;
    public bool isMixer;
    public float[] animancerMixerParameters;
    public AnimationClip animationClip;
    public System.Action animancerEndEvent;
    public float animancerSpeed;
    public float animancerNormalizedTime;
    public Vector3 velocity;

    public float health;
}