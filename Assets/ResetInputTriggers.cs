using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetInputTriggers : StateMachineBehaviour
{
    public bool IncludingDodge = false;
    public bool IncludingAttackRecoil = false;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("Input-SlashDown");
        animator.ResetTrigger("Input-SlashUp");
        animator.ResetTrigger("Input-ThrustDown");
        animator.ResetTrigger("Input-ThrustUp");
        animator.ResetTrigger("Input-HeavyDown");
        animator.ResetTrigger("Input-HeavyUp");
        if (IncludingDodge)
        {
            animator.ResetTrigger("Input-DodgeDown");
        }
        if (IncludingAttackRecoil)
        {
            animator.ResetTrigger("AttackBlocked");
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
