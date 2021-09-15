using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKHandsThrustStateHandler : StateMachineBehaviour
{
    public float rWeight;
    public float lWeight;
    public float rTargetTime;
    public float lTargetTime;
    public float ufWeight;
    public float ufTargetTime;
    public bool ikRight = true;
    public bool ikLeft = true;
    public bool ikUpForward = false;
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
        float rcWeight = Mathf.Min(1f - ((rTargetTime - stateInfo.normalizedTime) / rTargetTime), 1f) * rWeight;
        float lcWeight = Mathf.Min(1f - ((lTargetTime - stateInfo.normalizedTime) / lTargetTime), 1f) * lWeight;
        float ufcWeight = (stateInfo.normalizedTime >= ufTargetTime) ? 1f : 0f;// Mathf.Min(1f - ((ufTargetTime - stateInfo.normalizedTime) / ufTargetTime), 1f) * ufWeight;
        //Debug.Log(cWeight);
        if (animator.TryGetComponent<HumanoidActor>(out HumanoidActor actor))
        {
            if (ikLeft)
            {
                if (actor is PlayerActor player && player.offGrip != null)
                {
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, player.offGrip.position);
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, lcWeight);
                }
                else
                {
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, actor.positionReference.MainHand.transform.position);
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, lcWeight);
                }
            }
            if (ikRight) {
                animator.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.LookRotation(actor.transform.right));
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rcWeight);    
            }
            if (ikUpForward)
            {
                animator.SetIKPosition(AvatarIKGoal.RightHand, actor.transform.position + actor.transform.forward + Vector3.up * actor.positionReference.eyeHeight);
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ufcWeight);
            }

        }
    }
}
