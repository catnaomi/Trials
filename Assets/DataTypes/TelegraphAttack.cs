using UnityEngine;
using System.Collections;
using Animancer;
using System;

[CreateAssetMenu(fileName = "phaseatk0000_name", menuName = "ScriptableObjects/Attacks/Telegraph Attack", order = 1)]
public class TelegraphAttack : InputAttack
{
    [SerializeField] private InputAttack attack;
    public float telegraphLength = 1f;
    public float transitionTime = 1f;

    public override AnimancerState ProcessHumanoidAction(NavigatingHumanoidActor actor, Action endEvent)
    {
        AnimancerState telegraphState = actor.animancer.Layers[0].Play(GetClip());
        //telegraphState.Key = "telegraph";
        //telegraphState.Events.Clear();
        actor.StartCoroutine(TelegraphCoroutine(actor, telegraphState, endEvent));
        return telegraphState;    
    }

    IEnumerator TelegraphCoroutine(NavigatingHumanoidActor actor, AnimancerState telegraphState, Action endEvent)
    {
        yield return new WaitForSeconds(telegraphLength);
        if (actor.animancer.States.Current == telegraphState)
        {
            AnimancerState attackState = attack.ProcessHumanoidAction(actor, endEvent);
            //actor.animancer.Play(attackState, transitionTime);
        }
    }
}
