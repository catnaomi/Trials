using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using CustomUtilities;

public class NavigatingHumanoidActor : HumanoidActor
{
    [HideInInspector]
    public NavMeshAgent nav;
    public float angleSpeed = 180f;
    [Header("Navigation Settings")]

    public float bufferRange = 2f;

    bool shouldNavigate;
    protected float currentDistance;

    Vector3 additMove;

    public override void ActorStart()
    {
        base.ActorStart();

        nav = GetComponent<NavMeshAgent>();

        nav.updatePosition = false;

        nav.updateRotation = false;

        nav.angularSpeed = angleSpeed;
    }

    public void StartNavigationToTarget(GameObject target)
    {
        CombatTarget = target;
        shouldNavigate = true;
    }

    public void StopNavigation()
    {
        shouldNavigate = false;
    }

    /*
    public void SetAdditionalMovement(Vector3 move)
    {
        Debug.Log("additmovenav");
        bool relative = false;
        if (relative)
        {
            additMove = transform.forward * move.z + transform.up * move.y + transform.right * move.x;
        }
        else
        {
            additMove = move;
        }
    }
    */

    public float GetDistanceToTarget()
    {
        if (CombatTarget != null)
        {
            Vector3 targetAnimPos = CombatTarget.transform.position;
            /*
            if (CombatTarget.TryGetComponent<Animator>(out Animator targetAnim))
            {
                targetAnimPos = targetAnim.bodyPosition;
            }*/
            Vector3 thisPos = this.transform.position;//animator.bodyPosition;
            thisPos.y = 0;
            targetAnimPos.y = 0;
            return Vector3.Distance(thisPos, targetAnimPos);
        }
        else
        {
            return 0;
        }
    }

    public bool IsClearLineToTarget()
    {
        if (shouldNavigate)
        {
            if (Physics.Raycast(this.transform.position + Vector3.up, CombatTarget.transform.position - this.transform.position, out RaycastHit hit, 2f))
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

        if (humanoidState != HumanoidState.Actionable || !CanMove())
        {
            return;
        }

        if (shouldNavigate)
        {
            nav.isStopped = false;
            nav.SetDestination(CombatTarget.transform.position);
        }
        else
        {
            nav.isStopped = true;
            return;
        }

        currentDistance = GetDistanceToTarget();

        Quaternion targetRot;
        if (ShouldFaceTarget()) {
            bool b = false;
        }
        if (CanMove() || ShouldFaceTarget())
        {
            targetRot = Quaternion.LookRotation(NumberUtilities.FlattenVector(nav.desiredVelocity));
        }
        else
        {
            targetRot = this.transform.rotation;
        }
        this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, targetRot, nav.angularSpeed * Time.deltaTime);

        if (!this.CanMove())
        {
            //nav.isStopped = true;
        }
        moveDirection = (currentDistance > 1.5) ? (nav.desiredVelocity.normalized) : Vector3.zero;
        Vector3 movement = moveDirection;

        float animVel = (movement.magnitude > 0 ? 1f : 0f);

        animator.SetFloat("ForwardVelocity", Mathf.MoveTowards(animator.GetFloat("ForwardVelocity"), animVel * Vector3.Project(movement, transform.forward).magnitude, Time.deltaTime / 0.25f));
        animator.SetFloat("StrafingVelocity", Mathf.MoveTowards(animator.GetFloat("StrafingVelocity"), animVel * Vector3.Project(movement, transform.right).magnitude, Time.deltaTime / 0.25f));
        //animator.SetFloat("StickVelocity", Mathf.MoveTowards(animator.GetFloat("StickVelocity"), animVel, Time.deltaTime / 0.25f));

        if (moveAdditional != Vector3.zero)
        {
            //Debug.Log(moveAdditional);
        }
        //nav.nextPosition = transform.position + (movement * GetCurrentSpeed()) + moveAdditional * Time.deltaTime;
        //cc.Move(((movement * GetCurrentSpeed()) + moveAdditional) * Time.deltaTime);
        Vector3 worldDeltaPosition = nav.nextPosition - transform.position;

        if (worldDeltaPosition.magnitude > nav.radius)
            nav.nextPosition = transform.position + 0.9f * worldDeltaPosition;


    }

    void OnAnimatorMove()
    {
        // Update position based on animation movement using navigation surface height
        Vector3 position = animator.rootPosition;
        position.y = nav.nextPosition.y;
        transform.position = position;
    }

    public void RealignToTarget()
    {
        if (CombatTarget != null)
        {
            this.transform.rotation = Quaternion.LookRotation(NumberUtilities.FlattenVector(CombatTarget.transform.position - this.transform.position));
        }
    }
    public bool ShouldFaceTarget()
    {
        string TAG = "FACE_TARGET";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(animator.GetLayerIndex("Actions")));
    }
    public override bool IsBlocking()
    {
        string TAG = "BLOCKING";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(animator.GetLayerIndex("Actions")));
    }
}
