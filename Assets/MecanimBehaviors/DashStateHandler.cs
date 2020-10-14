using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashStateHandler : StateMachineBehaviour
{
    public float speedPerSecond = 1f;
    public bool clampSpeed = false;
    public float maxSpeed = 1f;
    public float minSpeed = 0;
    public AxisUtilities.AxisDirection direction = AxisUtilities.AxisDirection.Forward;
    float speed;
    public float multPerFrame = 1f;
    public bool HasDuration = false;
    public float duration;
    float clock;
    public float delay = 0f;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        speed = speedPerSecond;
        clock = duration;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if ((!HasDuration || clock > 0f) && !(HasDuration && clock > duration - delay) && animator.TryGetComponent<CharacterController>(out CharacterController cc))
        {
            AxisUtilities.AxisDirection adjustedDir = direction;
            if (direction == AxisUtilities.AxisDirection.Zero)
            {
                adjustedDir = AxisUtilities.AxisDirection.Forward;
            }
            Vector3 dirVector = AxisUtilities.AxisDirectionToTransformDirection(cc.transform, adjustedDir);
            cc.Move(dirVector * speed * Time.deltaTime);
        }
        speed *= multPerFrame;
        if (clampSpeed)
        {
            speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        }
        if (HasDuration)
        {
            clock -= Time.deltaTime;
        }
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
