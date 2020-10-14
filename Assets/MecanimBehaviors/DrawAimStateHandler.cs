using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawAimStateHandler : StateMachineBehaviour
{
    float aimProgress;
    float drawTime = 1f;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        aimProgress = 0f;
        
        animator.SetFloat("NormalTime", 0f);

        if (animator.TryGetComponent<HumanoidActor>(out HumanoidActor humanoid))
        {
            //drawTime = humanoid.drawTime;
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        aimProgress = Mathf.MoveTowards(aimProgress, 1f, Time.deltaTime / drawTime);

        animator.SetFloat("NormalTime", aimProgress);

        if (animator.TryGetComponent<HumanoidActor>(out HumanoidActor humanoid))
        {
            //humanoid.drawProgress = aimProgress;
        }
        return;
        /*
        if (animator.TryGetComponent<HumanoidActor>(out HumanoidActor humanoid)) {
            aimProgress = Mathf.MoveTowards(aimProgress, humanoid.GetAimCharge(), aimChargeSpeed * Time.deltaTime);
            aimProgress = Mathf.Clamp(aimProgress, 0f, 1f);
        }

        animator.SetFloat("AimCharge", aimProgress);
        */
    }

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
