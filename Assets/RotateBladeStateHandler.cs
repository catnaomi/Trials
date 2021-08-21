using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBladeStateHandler : StateMachineBehaviour
{
    public float mainAngle;
    public bool rotateMain;
    public bool delayMain;
    public float mainTargetStartTime;
    public float mainTargetEndTime;
    public bool exitTransitionMain;
    public float exitMainStartTime;
    public float exitMainEndTime;
    [Space(5)]
    public float offAngle;
    public bool rotateOff;
    public bool delayOff;
    public float offTargetStartTime;
    public float offTargetEndTime;
    public bool exitTransitionOff;
    public float exitOffStartTime;
    public float exitOffEndTime;
    [Space(5)]
    public float rcAngle;
    public float lcAngle;
    public float timeDisplay;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.TryGetComponent<HumanoidActor>(out HumanoidActor actor))
        {
            if (rotateMain && !delayMain)
            {
                actor.RotateMainWeapon(mainAngle);
            }
            if (rotateOff && !delayOff)
            {
                actor.RotateOffWeapon(offAngle);
            }
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if (animator.TryGetComponent<HumanoidActor>(out HumanoidActor actor))
        {
            timeDisplay = stateInfo.normalizedTime;

            if (exitTransitionMain && stateInfo.normalizedTime >= exitMainStartTime)
            {
                float mainTargetTime = exitMainEndTime - exitMainStartTime;
                float mainCurrentTime = Mathf.Clamp(stateInfo.normalizedTime - exitMainStartTime, 0f, 1f);
                rcAngle = Mathf.Clamp(((mainTargetTime - mainCurrentTime) / mainTargetTime), 0f, 1f) * mainAngle;


                actor.RotateMainWeapon(rcAngle);
            }
            else if (rotateMain && delayMain) // entry rotation
            {
                float mainTargetTime = mainTargetEndTime - mainTargetStartTime;
                float mainCurrentTime = Mathf.Max(stateInfo.normalizedTime - mainTargetStartTime, 0f);
                rcAngle = Mathf.Min(1f - ((mainTargetTime - mainCurrentTime) / mainTargetTime), 1f) * mainAngle;


                actor.RotateMainWeapon(rcAngle);
            }

            if (exitTransitionOff && stateInfo.normalizedTime >= exitOffStartTime)
            {
                float offTargetTime = exitOffEndTime - exitOffStartTime;
                float offCurrentTime = Mathf.Clamp(stateInfo.normalizedTime - exitOffStartTime, 0f, 1f);
                lcAngle = Mathf.Clamp(((offTargetTime - offCurrentTime) / offTargetTime), 0f, 1f) * offAngle;


                actor.RotateOffWeapon(lcAngle);
            }
            else if (rotateOff && delayOff)
            {
                float offTargetTime = offTargetEndTime - offTargetStartTime;
                float offCurrentTime = Mathf.Max(stateInfo.normalizedTime - offTargetStartTime, 0f);

                lcAngle = Mathf.Min(1f - ((offTargetTime - offCurrentTime) / offTargetTime), 1f) * offAngle;


                actor.RotateOffWeapon(lcAngle);
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.TryGetComponent<HumanoidActor>(out HumanoidActor actor))
        {
            if (rotateMain)
            {
                actor.ResetMainRotation();
            }
            if (rotateOff)
            {
                actor.ResetOffRotation();
            }
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
