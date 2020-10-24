using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyAttackStateHandler : StateMachineBehaviour
{
    public bool enter;
    public bool exit;
    public bool update;
    public bool ik;
    public bool isSpecial = false;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enter && animator.TryGetComponent<HumanoidActor>(out HumanoidActor actor))
        {
            if (!isSpecial)
            {
                if (actor.stance != null && actor.stance.heavyAttack != null)
                {
                    actor.stance.heavyAttack.OnHeavyEnter();
                }
            }
            else
            {
                if (actor.stance != null && actor.stance.specialAttack != null)
                {
                    actor.stance.specialAttack.OnHeavyEnter();
                }
            }
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (update && animator.TryGetComponent<HumanoidActor>(out HumanoidActor actor))
        {
            if (!isSpecial)
            {
                if (actor.stance != null && actor.stance.heavyAttack != null)
                {
                    actor.stance.heavyAttack.OnHeavyUpdate();
                }
            }
            else
            {
                if (actor.stance != null && actor.stance.specialAttack != null)
                {
                    actor.stance.specialAttack.OnHeavyUpdate();
                }
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (exit && animator.TryGetComponent<HumanoidActor>(out HumanoidActor actor))
        {
            if (!isSpecial)
            {
                if (actor.stance != null && actor.stance.heavyAttack != null)
                {
                    actor.stance.heavyAttack.OnHeavyExit();
                }
            }
            else
            {
                if (actor.stance != null && actor.stance.specialAttack != null)
                {
                    actor.stance.specialAttack.OnHeavyExit();
                }
            }
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (ik && animator.TryGetComponent<HumanoidActor>(out HumanoidActor actor))
        {
            if (!isSpecial)
            {
                if (actor.stance != null && actor.stance.heavyAttack != null)
                {
                    actor.stance.heavyAttack.OnHeavyIK();
                }
            }
            else
            {
                if (actor.stance != null && actor.stance.specialAttack != null)
                {
                    actor.stance.specialAttack.OnHeavyIK();
                }
            }
        }
    }
}
