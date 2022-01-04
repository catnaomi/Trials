using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeAttackStateHandler : StateMachineBehaviour
{
    public BladeWeapon.AttackType attackType;
    public bool isTwoHanded = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.gameObject.TryGetComponent<HumanoidActor>(out HumanoidActor actor))
        {
            actor.SetNextAttackType(attackType, true);
            /*
            if (actor is NavigatingHumanoidActor navActor)
            {
                navActor.RealignToTarget();
            }
            else if (actor is PlayerActor player)
            {
                //player.AnimSetTwoHand(isTwoHanded);
            }
            */
        }
    }
}
