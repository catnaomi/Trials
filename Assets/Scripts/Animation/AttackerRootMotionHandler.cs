using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(IAdjustRootMotion))]
public class AttackerRootMotionHandler : MonoBehaviour
{
    public float minimumDistanceToTarget = 2f;
    public float maxRootMotionBackwardsAdjust = 0.3f;
    NavMeshAgent nav;
    Animator animator;
    Vector3 rootDelta;
    Actor actor;
    IAdjustRootMotion adjustActor;
    CharacterController cc;
    Collider collider;

    private void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        adjustActor = GetComponent<IAdjustRootMotion>();
        actor = GetComponent<Actor>();
        cc = GetComponent<CharacterController>();
        collider = GetComponent<CapsuleCollider>();
    }

    void OnAnimatorMove()
    {
        Vector3 diff = animator.rootPosition - this.transform.position;
        if (adjustActor.ShouldAdjustRootMotion())
        {
            Vector3 target = adjustActor.GetAdjustmentRelativePosition();
            Vector3 dirToTarget = (target - this.transform.position);
            dirToTarget.y = 0f;
            dirToTarget.Normalize();


            if (Vector3.Dot(diff, dirToTarget) > 0)
            {
                diff = Vector3.Project(diff, dirToTarget);
                //Debug.DrawRay(this.transform.position + Vector3.up, diff.normalized, Color.cyan, 1f / 60f);
                float startingMagnitude = diff.magnitude * Mathf.Sign(Vector3.Dot(dirToTarget, diff));
                float distanceAfterMovement = Vector3.Distance(this.transform.position + diff, target);
                float minimumDistance = minimumDistanceToTarget;//Mathf.Max(inventory.GetCurrentLength(), 2f);
                if (distanceAfterMovement < minimumDistance)
                {
                    diff = diff.normalized * (distanceAfterMovement - minimumDistance) * Time.deltaTime;
                    //diff = Vector3.ClampMagnitude(diff, minimumDistance - distanceAfterMovement);


                }
                float endMagnitude = diff.magnitude * Mathf.Sign(Vector3.Dot(dirToTarget, diff));
                if (endMagnitude < (maxRootMotionBackwardsAdjust * Time.deltaTime))
                {
                    //diff = Vector3.ClampMagnitude(diff, Mathf.Abs(maxRootMotionBackwardsAdjust) * Time.deltaTime);
                }
                //Debug.Log($"adjusted root motion movement: {startingMagnitude} vs {endMagnitude}");
            }
            else
            {
                //Debug.DrawRay(this.transform.position + Vector3.up, diff.normalized, Color.red, 1f / 60f);
            }
            
        }
        rootDelta = diff;
    }

    void FixedUpdate()
    {
        if (nav != null && nav.enabled)
        {
            Vector3 position = this.transform.position;
            position.y = nav.nextPosition.y;
            this.transform.position = position;
        }

        if (rootDelta.magnitude > 0)
        {
            cc.enabled = false;
            this.transform.position = this.transform.position + rootDelta;
            cc.enabled = true;
            SetVelocity(rootDelta / Time.fixedDeltaTime);
        }
        else if (actor != null && !actor.IsGrounded())
        {
            cc.enabled = true;
            cc.Move((actor.xzVel + Vector3.up * actor.yVel) * Time.fixedDeltaTime);
            actor.yVel -= Physics.gravity.magnitude;
        }
        if (PlayerActor.player != null)
        {
            Physics.IgnoreCollision(PlayerActor.player.cc, collider, !PlayerActor.player.isGrounded);
            Physics.IgnoreCollision(PlayerActor.player.cc, cc, !PlayerActor.player.isGrounded);
        }
    }

    void SetVelocity(Vector3 velocity)
    {
        if (actor != null)
        {
            actor.xzVel = velocity;
            actor.xzVel.y = 0f;
            actor.yVel = velocity.y;
        }
    }
}
