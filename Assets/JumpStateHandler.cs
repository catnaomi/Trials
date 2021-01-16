using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpStateHandler : StateMachineBehaviour
{
    public float mult = 1f;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Input-Jump", false);
        if (animator.TryGetComponent<HumanoidActor>(out HumanoidActor actor))
        {
            actor.SetAdditionalMovement(Vector3.zero);
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.TryGetComponent<HumanoidActor>(out HumanoidActor actor) && animator.TryGetComponent<CharacterController>(out CharacterController cc))
        {
            Vector3 dirVector = Vector3.up;

            float mag = animator.GetFloat("AdditRoot");

            actor.SetAdditionalMovement(dirVector * mag * mult);
            //cc.Move((cc.transform.forward - Vector3.up) * mag * Time.deltaTime);
        }


    }

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
