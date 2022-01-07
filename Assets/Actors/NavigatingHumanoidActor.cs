using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using CustomUtilities;
using Animancer;

[RequireComponent(typeof(HumanoidPositionReference)), RequireComponent(typeof(NavMeshAgent))]
public class NavigatingHumanoidActor : Actor, INavigates
{
    [HideInInspector]
    public NavMeshAgent nav;
    public float angleSpeed = 180f;

    Rigidbody rigidbody;

    HumanoidPositionReference positionReference;
    [Header("Navigation Settings")]

    public float bufferRange = 2f;
    public float closeRange = 1f; // close range should be <= buffer range
    public bool shouldNavigate;
    public float currentDistance;
    public float updateSpeed = 0.25f;
    public float neg = -1;
    Vector3 additMove;
    public float angle;
    public Vector3 destination;
    bool followingTarget;
    bool obstacleTransitioning;
    bool ignoreRoot;
    bool shouldFall;
    bool offMeshInProgress;
    public float airTime = 0f;
    float lastAirTime;
    float landTime = 0f;
    Vector3 yVel = Vector3.zero;
    Vector3 xzVel = Vector3.zero;
    Vector3 lastPosition;
    Vector3 animatorVelocity;
    public float jumpAdjustSpeed = 3f;
    protected IInventory inventory;
    [Header("Animancer")]
    protected AnimancerComponent animancer;
    public NavAnims navAnims;
    public LinearMixerTransitionAsset idleAnim;
    public MixerTransition2DAsset moveAnim;
    protected ClipTransition jumpHorizontal;
    protected ClipTransition jumpDown;
    protected ClipTransition fallAnim;
    protected ClipTransition landAnim;

    protected NavAnimState navstate;
    protected struct NavAnimState {
        public LinearMixerState idle;
        public DirectionalMixerState move;
        public AnimancerState fall;
    }

    void OnEnable()
    {
        StartCoroutine("UpdateDestination");
        jumpHorizontal = navAnims.jumpHorizontal;
        jumpDown = navAnims.jumpDown;
        fallAnim = navAnims.fallAnim;
        landAnim = navAnims.landAnim;
    }

    System.Action _FinishJump;
    System.Action _FinishDrop;
    public override void ActorStart()
    {
        base.ActorStart();

        nav = GetComponent<NavMeshAgent>();
        animancer = GetComponent<AnimancerComponent>();

        nav.updatePosition = false;

        nav.updateRotation = false;

        nav.angularSpeed = angleSpeed;
        nav.autoTraverseOffMeshLink = false;

        positionReference = GetComponent<HumanoidPositionReference>();
        navstate.move = (DirectionalMixerState)animancer.States.GetOrCreate(moveAnim);
        navstate.idle = (LinearMixerState)animancer.Play(idleAnim);

        rigidbody = this.GetComponent<Rigidbody>();
        if (CombatTarget != null) SetDestination(CombatTarget);

        _FinishJump = () =>
        {
            nav.CompleteOffMeshLink();
            animancer.Play(navstate.move, 0.1f);
            ignoreRoot = false;
            offMeshInProgress = false;
        };

        _FinishDrop = () =>
        {
            Vector3 pos = animancer.Animator.rootPosition;
            navstate.fall = animancer.Play(fallAnim);
            this.transform.position = pos;
            nav.nextPosition = pos;
        };

        landAnim.Events.OnEnd = () =>
        {
            animancer.Play(navstate.idle);
        };
    }

    public void StartNavigationToTarget(GameObject target)
    {
        SetDestination(target);
    }

    public void ResumeNavigation()
    {
        shouldNavigate = true;
    }
    public void StopNavigation()
    {
        shouldNavigate = false;
    }
   
    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();

        Vector3 moveDirection = Vector3.zero;
        Vector3 lookDirection = this.transform.forward;
        currentDistance = GetDistanceToTarget();
        bool inBufferRange = currentDistance <= bufferRange;
        bool inCloseRange = currentDistance <= closeRange;
        if (followingTarget)
        {
            if (GetCombatTarget() == null)
            {
                followingTarget = false;
            }
            else
            {
                destination = CombatTarget.transform.position;
            }
        }
        if (animancer.States.Current == navstate.idle)
        {
            ignoreRoot = false;
            nav.isStopped = true;
            if (shouldNavigate)
            {
                lookDirection = (destination - this.transform.position).normalized;
                lookDirection.Scale(new Vector3(1f, 0f, 1f));
                Debug.DrawLine(this.transform.position + this.transform.up, this.transform.position + this.transform.up + lookDirection);
                if (!inBufferRange)
                {
                    animancer.Play(navstate.move, 0.25f);
                }
            }
            angle = Mathf.MoveTowards(angle, Vector3.SignedAngle(this.transform.forward, lookDirection, Vector3.up), nav.angularSpeed * Time.deltaTime);
            navstate.idle.Parameter = angle;
            
        }
        else if (animancer.States.Current == navstate.move)
        {
            nav.isStopped = false;
            float xmov = 0;
            float ymov = 0;
            if (shouldNavigate && !inCloseRange)
            {
                moveDirection = nav.desiredVelocity.normalized;
                xmov = Vector3.Dot(moveDirection, this.transform.right);
                ymov = Vector3.Dot(moveDirection, this.transform.forward);

                navstate.move.ParameterX = xmov;
                navstate.move.ParameterY = ymov;

                Quaternion targetRot = Quaternion.LookRotation(NumberUtilities.FlattenVector(nav.desiredVelocity));
                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, targetRot, nav.angularSpeed * Time.deltaTime);
                if (nav.isOnOffMeshLink && !offMeshInProgress)
                {
                    offMeshInProgress = true;
                    if (nav.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeJumpAcross)
                    {
                        Jump();
                    }
                    else if (nav.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeDropDown)
                    {
                        Drop();
                    }
                }
            }
            else
            {
                animancer.Play(navstate.idle, 0.25f);
            }
            
        }

        if (animancer.States.Current == navstate.fall)
        {
            ignoreRoot = true;

            if (GetGrounded())
            {
                if (offMeshInProgress)
                {
                    nav.CompleteOffMeshLink();
                    offMeshInProgress = false;
                }
                animancer.Play(landAnim);
                ignoreRoot = false;
            }
        }
        else
        {
            Vector3 worldDeltaPosition = nav.nextPosition - transform.position;
            if (worldDeltaPosition.magnitude > nav.radius) nav.nextPosition = transform.position + 0.9f * worldDeltaPosition;
        }

        if (GetGrounded())
        {
            if (lastPosition != Vector3.zero)
            {
                xzVel = (this.transform.position - lastPosition);
            }
            lastPosition = this.transform.position;
        }
    }

    public void Jump()
    {
        OffMeshLinkData data = nav.currentOffMeshLinkData;
        StartCoroutine(JumpRoutine(data));
    }

    IEnumerator JumpRoutine(OffMeshLinkData data)
    {
        ignoreRoot = true;
        Vector3 dir;
        while (Vector3.Distance(this.transform.position, data.startPos) > 0.1f)
        {
            dir = (data.endPos - this.transform.position).normalized;
            dir.Scale(new Vector3(1f, 0f, 1f));
            this.transform.position = Vector3.MoveTowards(this.transform.position, data.startPos, jumpAdjustSpeed * Time.fixedDeltaTime);
            this.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(this.transform.forward, dir, 360f * Mathf.Deg2Rad * Time.fixedDeltaTime, 10f));
            yield return new WaitForFixedUpdate();
        }

        ignoreRoot = false;
        dir = (data.endPos - this.transform.position).normalized;
        dir.Scale(new Vector3(1f, 0f, 1f));
        this.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        jumpHorizontal.Events.OnEnd = _FinishJump;
        animancer.Play(jumpHorizontal);
        yield return new WaitWhile(() => animancer.States.Current != navstate.idle);
        Vector3 pos = animancer.Animator.rootPosition;
        this.transform.position = pos;
        nav.nextPosition = pos;
    }

    public void Drop()
    {
        OffMeshLinkData data = nav.currentOffMeshLinkData;
        StartCoroutine(DropRoutine(data));
    }

    IEnumerator DropRoutine(OffMeshLinkData data)
    {
        ignoreRoot = true;
        Vector3 dir;
        while (Vector3.Distance(this.transform.position, data.startPos) > 0.1f)
        {
            dir = (data.endPos - this.transform.position).normalized;
            dir.Scale(new Vector3(1f, 0f, 1f));
            this.transform.position = Vector3.MoveTowards(this.transform.position, data.startPos, jumpAdjustSpeed * Time.fixedDeltaTime);
            this.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(this.transform.forward, dir, 360f * Mathf.Deg2Rad * Time.fixedDeltaTime, 10f));
            yield return new WaitForFixedUpdate();
        }
        ignoreRoot = false;
        dir = (data.endPos - this.transform.position).normalized;
        dir.Scale(new Vector3(1f, 0f, 1f));
        this.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        jumpDown.Events.OnEnd = _FinishDrop;
        animancer.Play(jumpDown);

    }

    void FixedUpdate()
    {
        Vector3 velocity = Vector3.zero;
        if (GetGrounded(out RaycastHit hitCheck1))
        {
            if (yVel.y <= 0)
            {
                yVel.y = 0f;
            }
            else
            {
                yVel.y += Physics.gravity.y * Time.fixedDeltaTime;
            }
            airTime = 0f;
            landTime += Time.fixedDeltaTime;
            if (landTime > 1f)
            {
                landTime = 1f;
            }
            Vector3 groundY = new Vector3(this.transform.position.x, hitCheck1.point.y, this.transform.position.z);
            this.transform.position = Vector3.MoveTowards(this.transform.position, groundY, 0.2f * Time.fixedDeltaTime);
        }
        else
        {
            yVel.y += Physics.gravity.y * Time.fixedDeltaTime;
            if (yVel.y < -70f)
            {
                yVel.y = -70;
            }
            airTime += Time.fixedDeltaTime;
            lastAirTime = airTime;
            if (animancer.States.Current == navstate.fall) velocity += xzVel;
        }
        velocity += yVel * Time.fixedDeltaTime;
        this.GetComponent<CharacterController>().Move((velocity));
    }
    public bool GetGrounded(out RaycastHit hitInfo)
    {
        Collider c = this.GetComponent<Collider>();
        Vector3 bottom = c.bounds.center + c.bounds.extents.y * Vector3.down * 0.9f;
        
        bool hit = Physics.Raycast(bottom, Vector3.down, out hitInfo, 0.2f, LayerMask.GetMask("Terrain"));
        Debug.DrawLine(bottom, bottom + Vector3.down * 0.2f, hit ? Color.green : Color.red);
        return hit;
    }

    public bool GetGrounded()
    {
        return GetGrounded(out RaycastHit unused);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        CharacterController cc = this.GetComponent<CharacterController>();
        if (!GetGrounded() && cc.isGrounded && animancer.States.Current == navstate.fall)
        {
            Debug.DrawLine(this.transform.position, hit.point, Color.blue);
            Vector3 dir = this.transform.position - hit.point;
            dir.y = 0;
            this.transform.Translate(dir);
        }
    }

    private void UpdateWithNav()
    {
        currentDistance = GetDistanceToTarget();

        Quaternion targetRot;
        if (CanMove() || ShouldFaceTarget() || IsBlocking())
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
            shouldNavigate = false;
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

    private void UpdateWithoutNav()
    {
        if (this.CanMove())
        {
            shouldNavigate = true;
        }

        Quaternion targetRot;
        if (CombatTarget != null && (ShouldFaceTarget() || IsBlocking()))
        {
            targetRot = Quaternion.LookRotation(NumberUtilities.FlattenVector(CombatTarget.transform.position - this.transform.position));
        }
        else
        {
            targetRot = this.transform.rotation;
        }
        this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, targetRot, nav.angularSpeed * Time.deltaTime);
    }

    void OnAnimatorMove()
    {
        // Update position based on animation movement using navigation surface height
        Vector3 position = animancer.Animator.rootPosition + yVel;
        if (!ignoreRoot) transform.rotation = animancer.Animator.rootRotation;
        position.y = nav.nextPosition.y;
        Vector3 dir = position - this.transform.position;
        if (!ignoreRoot && (animancer.States.Current == navstate.move || !Physics.SphereCast(this.transform.position + (Vector3.up * positionReference.eyeHeight), 0.25f, dir, out RaycastHit hit, dir.magnitude, LayerMask.GetMask("Terrain"))))
        {
            transform.position = position;
        }
        animatorVelocity = animancer.Animator.velocity;

    }

    IEnumerator UpdateDestination()
    {
        while (this.enabled)
        {
            yield return new WaitForSecondsRealtime(updateSpeed);
            if (nav.enabled) nav.SetDestination(destination);
        }
    }
    public void SetDestination(Vector3 position)
    {
        destination = position;
        followingTarget = false;
    }

    public void SetDestination(GameObject target)
    {
        SetCombatTarget(target);
        destination = target.transform.position;
        followingTarget = true;
    }
    public void RealignToTarget()
    {
        if (CombatTarget != null)
        {
            this.transform.rotation = Quaternion.LookRotation(NumberUtilities.FlattenVector(CombatTarget.transform.position - this.transform.position));
        }
    }

    public float GetDistanceToTarget()
    {
        if (nav.enabled) {
            return nav.remainingDistance;
        }
        if (destination == Vector3.zero)
        {
            return 0f;
        }
        else
        {
            return Vector3.Distance(this.transform.position, destination);
        }
    }

    public bool IsClearLineToTarget()
    {
        if (CombatTarget != null)
        {
            if (Physics.SphereCast(this.transform.position + Vector3.up, 0.25f, CombatTarget.transform.position - this.transform.position, out RaycastHit hit, Vector3.Distance(CombatTarget.transform.position, this.transform.position), ~LayerMask.GetMask("Limbs", "Hitboxes")))
            {
                if (hit.transform.root == CombatTarget.transform.root)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(this.transform.position + this.transform.up, this.transform.position + this.transform.forward + this.transform.up);
    }
    public bool ShouldFaceTarget()
    {
        string TAG = "FACING";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(animator.GetLayerIndex("Actions")));
    }
    public bool IsBlocking()
    {
        string TAG = "BLOCKING";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(animator.GetLayerIndex("Actions")));
    }

    public bool CanMove()
    {
        return true;
    }

    public Vector3 GetDestination()
    {
        return destination;
    }
}
