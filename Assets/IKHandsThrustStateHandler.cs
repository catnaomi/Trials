using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKHandsThrustStateHandler : StateMachineBehaviour
{
    public float weight;
    public float targetTime;
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
    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float cWeight = Mathf.Min(((stateInfo.normalizedTime - targetTime) / targetTime), 1f) * weight;
        Debug.Log(cWeight);
        if (animator.TryGetComponent<HumanoidActor>(out HumanoidActor actor))
        {
            animator.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.LookRotation(actor.transform.right));
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, cWeight);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, actor.positionReference.MainHand.transform.position);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.75f);

        }
    }
}
