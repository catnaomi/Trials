using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceTargetContinuousMecanim : StateMachineBehaviour
{
    public float speedPerSecond;
    public bool lerp;
    public string angleBetweenParameterName;
    public float maxAngle;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.gameObject.TryGetComponent<Actor>(out Actor actor))
        {
            if (!lerp)
            {
                actor.RotateTowardsTarget(speedPerSecond * Time.deltaTime);
            }
            else
            {
                float angle = animator.GetFloat(angleBetweenParameterName);
                float t = Mathf.Clamp01(Mathf.Abs(angle) / maxAngle);
                float speed = Mathf.LerpAngle(0f, speedPerSecond, t);
                actor.RotateTowardsTarget(speed * Time.deltaTime);
            }
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