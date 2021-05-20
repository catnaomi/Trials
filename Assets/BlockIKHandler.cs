using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockIKHandler : StateMachineBehaviour
{
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
        if (animator.TryGetComponent<PlayerActor>(out PlayerActor actor))
        {
            if (animator.GetFloat("Style-Block") == (int)StanceHandler.BlockStyle.TwoHand && !animator.IsInTransition(animator.GetLayerIndex("Base Movement")))
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.75f);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, actor.positionReference.MainHand.transform.position + actor.positionReference.MainHand.transform.forward * -0.25f);
            }
        }
    }
}
