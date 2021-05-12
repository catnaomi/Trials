using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdditMotionStateHandler : StateMachineBehaviour
{
    public AxisUtilities.AxisDirection direction = AxisUtilities.AxisDirection.Forward;
    public float mult = 1f;
    public bool direct = false;
    public bool dodge = false;
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
            AxisUtilities.AxisDirection adjustedDir = direction;
            if (direction == AxisUtilities.AxisDirection.Zero)
            {
                adjustedDir = AxisUtilities.AxisDirection.Forward;
            }
            Vector3 dirVector = AxisUtilities.AxisDirectionToTransformDirection(cc.transform, adjustedDir).normalized;

            if (dodge)
            {
                dirVector = cc.transform.forward * animator.GetFloat("DodgeForward") + cc.transform.right * animator.GetFloat("DodgeStrafe");
            }

            float mag = animator.GetFloat("AdditRoot");

            if (!direct)
            {
                actor.SetAdditionalMovement(dirVector * mag * mult);
            }
            else
            {
                cc.Move(dirVector * mag * mult * Time.deltaTime);
            }
            //cc.Move((cc.transform.forward - Vector3.up) * mag * Time.deltaTime);
        }


    }

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
