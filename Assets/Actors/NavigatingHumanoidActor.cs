using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using CustomUtilities;

public class NavigatingHumanoidActor : HumanoidActor
{
    private NavMeshAgent nav;

    [Header("Navigation Settings")]

    public float bufferRange = 2f;

    bool shouldNavigate;
    protected float currentDistance;
    float followDistance;

    Vector3 additMove;

    public override void ActorStart()
    {
        base.ActorStart();

        nav = GetComponent<NavMeshAgent>();

        nav.updatePosition = false;

        nav.updateRotation = false;
        
    }

    public void NavigateToTarget(GameObject target, float distance)
    {
        followDistance = distance;
        CombatTarget = target;
        shouldNavigate = true;
    }

    public void StopNavigation()
    {
        shouldNavigate = false;
    }

    public void SetAdditionalMovement(Vector3 move, bool relative)
    {
        if (relative)
        {
            additMove = transform.forward * move.z + transform.up * move.y + transform.right * move.x;
        }
        else
        {
            additMove = move;
        }
    }

    public bool InRangeOfTarget()
    {
        if (shouldNavigate)
        {
            return currentDistance < followDistance;
        }
        else
        {
            return false;
        }
    }

    public bool IsClearLineToTarget()
    {
        if (shouldNavigate)
        {
            if (Physics.Raycast(this.transform.position + Vector3.up, CombatTarget.transform.position - this.transform.position, out RaycastHit hit, followDistance * 2f))
            {
                if (hit.transform.root == CombatTarget.transform.root)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();

        if (humanoidState != HumanoidState.Actionable || !CanMove() || !shouldNavigate)
        {
            return;
        }

        currentDistance = Vector3.Distance(this.transform.position, CombatTarget.transform.position);
        if (currentDistance > (followDistance + bufferRange))
        {
            nav.isStopped = false;
            nav.SetDestination(CombatTarget.transform.position);
        }
        else if (currentDistance < followDistance)
        {
            nav.isStopped = true;
        }
        this.transform.rotation = Quaternion.LookRotation(NumberUtilities.FlattenVector(CombatTarget.transform.position - this.transform.position));
        

        if (!this.CanMove())
        {
            nav.isStopped = true;
        }
        Vector3 movement = (nav.desiredVelocity.normalized + additMove);

        float animVel = (movement.magnitude > 0 ? 1f : 0f);

        animator.SetFloat("ForwardVelocity", Mathf.MoveTowards(animator.GetFloat("ForwardVelocity"), animVel * Vector3.Project(movement, transform.forward).magnitude, Time.deltaTime / 0.25f));
        animator.SetFloat("StrafingVelocity", Mathf.MoveTowards(animator.GetFloat("StrafingVelocity"), animVel * Vector3.Project(movement, transform.right).magnitude, Time.deltaTime / 0.25f));
        //animator.SetFloat("StickVelocity", Mathf.MoveTowards(animator.GetFloat("StickVelocity"), animVel, Time.deltaTime / 0.25f));

        nav.nextPosition = transform.position + movement * GetCurrentSpeed() * Time.deltaTime;
        cc.Move(movement * GetCurrentSpeed() * Time.deltaTime);
    }
}
