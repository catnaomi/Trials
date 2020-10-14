using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprintStateHandler : StateMachineBehaviour
{

    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Sprinting", true);
    }
    

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        bool input = Input.GetButton("Dodge");

        animator.SetBool("Sprinting", input); // stop sprinting when button released

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state

    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.gameObject.TryGetComponent<HumanoidActor>(out HumanoidActor actor))
        {
            animator.SetBool("Sprinting", false);
        }
    }
    


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
