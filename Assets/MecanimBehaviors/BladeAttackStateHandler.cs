using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeAttackStateHandler : StateMachineBehaviour
{
    public BladeWeapon.AttackType attackType;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.gameObject.TryGetComponent<HumanoidActor>(out HumanoidActor actor))
        {
            actor.nextAttackType = attackType;
        }
    }
}
