using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using CustomUtilities;
using Animancer;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(HumanoidPositionReference)), RequireComponent(typeof(NavMeshAgent))]
public class NavigatingHumanoidActor : Actor, INavigates
{
    [HideInInspector]
    public NavMeshAgent nav;
    public float angleSpeed = 180f;

    Rigidbody rigidbody;

    protected HumanoidPositionReference positionReference;
    protected CharacterController cc;
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
    [HideInInspector]public bool followingTarget;
    bool obstacleTransitioning;
    protected bool ignoreRoot;
    bool shouldFall;
    protected bool offMeshInProgress;
    public float airTime = 0f;
    protected float lastAirTime;
    protected float landTime = 0f;
    float idleTime = 0f;
    float strafeClock = 0f;
    public float strafeDelay = 2f;
    public int strafeDirection = 0;
    Vector3 lastPosition;
    Vector3 animatorVelocity;
    public float jumpAdjustSpeed = 3f;
    public float turningDelay = 1f;
    [Header("Animancer")]
    public NavAnims navAnims;
    NavAnims runtimeNavAnims;
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
        public DirectionalMixerState strafe;
        public AnimancerState land;
    }

    public bool actionsEnabled = true;

    [Header("Combat")]
    public UnityEvent OnHitboxActive;
    

    System.Action _FinishJump;
    System.Action _FinishDrop;
    public override void ActorStart()
    {
        base.ActorStart();
        runtimeNavAnims = ScriptableObject.Instantiate(navAnims);
        jumpHorizontal = runtimeNavAnims.jumpHorizontal;
        jumpDown = runtimeNavAnims.jumpDown;
        fallAnim = runtimeNavAnims.fallAnim;
        landAnim = runtimeNavAnims.landAnim;

        nav = GetComponent<NavMeshAgent>();
        animancer = GetComponent<AnimancerComponent>();
        cc = GetComponent<CharacterController>();
        nav.updatePosition = false;

        nav.updateRotation = false;

        nav.angularSpeed = angleSpeed;
        nav.autoTraverseOffMeshLink = false;

        positionReference = GetComponent<HumanoidPositionReference>();
        navstate.move = (DirectionalMixerState)animancer.States.GetOrCreate(moveAnim);
        navstate.move.Key = "move";
        navstate.idle = (LinearMixerState)animancer.States.GetOrCreate(idleAnim);
        navstate.strafe = (DirectionalMixerState)animancer.States.GetOrCreate(moveAnim);
        navstate.strafe.Key = "strafe";
        animancer.Layers[HumanoidAnimLayers.UpperBody].SetMask(positionReference.upperBodyMask);
        animancer.Layers[HumanoidAnimLayers.UpperBody].IsAdditive = true;
        animancer.Layers[HumanoidAnimLayers.UpperBody].Weight = 1f;


        if (!animancer.IsPlaying())
        {
            animancer.Play(navstate.idle);
        }
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
            cc.enabled = true;
        };

        landAnim.Events.OnEnd = () =>
        {
            animancer.Play(navstate.idle);
        };
        StartCoroutine("UpdateDestination");
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
        bool allowTurning = turningDelay <= 0f;
        if (!allowTurning)
        {
            turningDelay -= Time.deltaTime;
        }
        if (!actionsEnabled)
        {
            shouldNavigate = false;
        }
        if (followingTarget)
        {
            if (GetCombatTarget() == null)
            {
                followingTarget = false;
            }
            else if (!IsStrafing())
            {
                destination = CombatTarget.transform.position;
            }
        }
        if (animancer.States.Current == navstate.idle)
        {
            nav.enabled = true;
            ignoreRoot = false;
            nav.isStopped = true;
            if (destination != Vector3.zero)
            {
                lookDirection = (destination - this.transform.position).normalized;
                lookDirection.y = 0f;
                angle = Mathf.MoveTowards(angle, Vector3.SignedAngle(this.transform.forward, lookDirection, Vector3.up), nav.angularSpeed * Time.deltaTime);
            }
            else
            {
                angle = 0f;
            }
            if (shouldNavigate)
            {
                
                Debug.DrawLine(this.transform.position + this.transform.up, this.transform.position + this.transform.up + lookDirection);
                if (!inBufferRange)
                {
                    animancer.Play(navstate.move, 0.25f);
                    idleTime = 0f;
                }
                else if (idleTime > strafeDelay)
                {
                    strafeDirection = CheckStrafe();
                    if (strafeDirection != 0)
                    {
                        animancer.Play(navstate.strafe);
                    }
                    idleTime = 0f;
                }
            }
            
            if (allowTurning)
            {
                navstate.idle.Parameter = angle;
            }
            else
            {
                navstate.idle.Parameter = 0f;
            }
            
            idleTime += Time.deltaTime;
            
            
        }
        else if (animancer.States.Current == navstate.move)
        {
            nav.enabled = true;
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

                Quaternion targetRot = nav.desiredVelocity.magnitude > 0 ? Quaternion.LookRotation(NumberUtilities.FlattenVector(nav.desiredVelocity)) : Quaternion.identity;
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
                    else if (nav.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeManual)
                    {
                        HandleCustomOffMeshLink();
                    }
                }
            }
            else
            {
                animancer.Play(navstate.idle, 0.25f);
            }
            
        }
        else if (animancer.States.Current == navstate.strafe)
        {
            nav.enabled = true;
            ignoreRoot = false;
            nav.isStopped = false;

            if (idleTime > strafeDelay)
            {
                idleTime = 0f;
                strafeDirection = CheckStrafe();
                SetStrafeDestination(strafeDirection);
            }
            if (strafeClock > updateSpeed)
            {
                SetStrafeDestination(strafeDirection);
                strafeClock = 0f;
            }

            float xmov;// = Mathf.Sign(strafeDirection);
            float ymov;
            if (inBufferRange && strafeDirection != 0)
            {
                moveDirection = nav.desiredVelocity.normalized;//this.transform.right * xmov;
                Vector3 dir = (CombatTarget.transform.position - this.transform.position);
                dir.y = 0f;
                dir.Normalize();
                lookDirection = dir;

                this.transform.rotation = Quaternion.LookRotation(lookDirection);

                xmov = Vector3.Dot(moveDirection, this.transform.right);
                ymov = Vector3.Dot(moveDirection, this.transform.forward);

                navstate.strafe.ParameterX = xmov;
                navstate.strafe.ParameterY = ymov;
            }
            else
            {
                animancer.Play(navstate.move);
            }

            strafeClock += Time.deltaTime;
            idleTime += Time.deltaTime;
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
                navstate.land = animancer.Play(landAnim);
                navstate.land.Events.OnEnd = () =>
                {
                    animancer.Play(navstate.move);
                };
                ignoreRoot = false;
            }
        }
        else if (IsFalling()) // likely a hurt fall animation
        {
            ignoreRoot = true;
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
        else if ((animancer.States.Current == navstate.move || animancer.States.Current == navstate.idle) && airTime > 0.1f)
        {
            navstate.fall = animancer.Play(fallAnim);
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
        cc.enabled = false;
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
        //cc.enabled = true;
        jumpDown.Events.OnEnd = _FinishDrop;
        animancer.Play(jumpDown);
        yield return new WaitUntil(() => { return animancer.States.Current == navstate.fall; });
        while (animancer.States.Current == navstate.fall)
        {
            dir = data.endPos - this.transform.position;
            dir.y = 0f;
            dir = Vector3.ClampMagnitude(dir, (data.endPos - this.transform.position).magnitude * Time.deltaTime);
            xzVel = Vector3.Lerp(xzVel, dir, 0.5f);
            yield return null;
        }
    }

    public virtual void HandleCustomOffMeshLink()
    {
        OffMeshLinkData data = nav.currentOffMeshLinkData;
        this.transform.position = data.endPos;
        nav.nextPosition = data.endPos;
    }
    void FixedUpdate()
    {
        Vector3 velocity = Vector3.zero;
        if (GetGrounded(out RaycastHit hitCheck1))
        {
            if (yVel <= 0)
            {
                yVel = 0f;
            }
            else
            {
                yVel += Physics.gravity.y * Time.fixedDeltaTime;
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
            yVel += Physics.gravity.y * Time.fixedDeltaTime;
            if (yVel < -70f)
            {
                yVel = -70;
            }
            airTime += Time.fixedDeltaTime;
            lastAirTime = airTime;
            if (IsFalling()) velocity += xzVel;
        }
        velocity += yVel * Vector3.up * Time.fixedDeltaTime;
        this.GetComponent<CharacterController>().Move((velocity));
    }
    public bool GetGrounded(out RaycastHit hitInfo)
    {
        Collider c = this.GetComponent<Collider>();
        Vector3 bottom = c.bounds.center + c.bounds.extents.y * Vector3.down * 0.9f;
        Vector3 top = c.bounds.center + c.bounds.extents.y * Vector3.up;
        bool hit = Physics.Raycast(top, Vector3.down, out hitInfo, c.bounds.extents.y * 2f + 0.5f, MaskReference.Terrain);
        Debug.DrawLine(top, bottom + Vector3.down * 0.2f, hit ? Color.green : Color.red);
        return hit;
    }

    public bool GetGrounded()
    {
        return GetGrounded(out RaycastHit unused);
    }

    public override bool IsGrounded()
    {
        return GetGrounded();
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //CharacterController cc = this.GetComponent<CharacterController>();
        if (!GetGrounded() && cc.isGrounded && IsFalling())
        {
            Debug.DrawLine(this.transform.position, hit.point, Color.blue);
            Vector3 dir = this.transform.position - hit.point;
            dir.y = 0;
            this.transform.Translate(dir);
        }
    }

    public override void SetToIdle()
    {
        animancer.Play(navstate.idle);
    }
    
    void OnAnimatorMove()
    {
        // Update position based on animation movement using navigation surface height
        if (animancer == null || animancer.Animator == null || nav == null) return;
        Vector3 position = animancer.Animator.rootPosition + yVel * Vector3.up;
        if (!ignoreRoot) transform.rotation = animancer.Animator.rootRotation;
        position.y = nav.nextPosition.y;
        Vector3 dir = position - this.transform.position;
        if (!ignoreRoot && ((animancer.States.Current != navstate.idle && !IsFalling()) || !Physics.SphereCast(this.transform.position + (Vector3.up * positionReference.eyeHeight), 0.25f, dir, out RaycastHit hit, dir.magnitude, MaskReference.Terrain)))
        {
            cc.enabled = false;
            transform.position = position;
            cc.enabled = true;
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
        if (destination != Vector3.zero)
        {
            Vector3 dir = destination - this.transform.position;
            dir.y = 0f;
            this.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }

    public void RealignToTargetWithOffset(float angle)
    {
        if (destination != Vector3.zero)
        {
            Vector3 dir = destination - this.transform.position;
            dir.y = 0f;
            this.transform.rotation = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.AngleAxis(angle, Vector3.up);
        }
    }

    public float GetDistanceToTarget()
    {
        if (nav.enabled) {
            float dist = nav.remainingDistance;
            if (dist >= Mathf.Infinity)
            {
                nav.SetDestination(destination);
            }
            return dist;
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

    public virtual bool CanAct()
    {
        return (animancer.States.Current == navstate.move || animancer.States.Current == navstate.idle || animancer.States.Current == navstate.strafe) && actionsEnabled;
    }

    // determines which direction should be strafed in
    // -1 = left, 1 = right, 0 = shouldn't strafe
    public int CheckStrafe()
    {
        float MAX_DISTANCE = 3f;
        float MIN_DISTANCE = 1f;
        Vector3 pointLeft = this.transform.position + this.transform.right * -MAX_DISTANCE;
        Vector3 pointRight = this.transform.position + this.transform.right * MAX_DISTANCE;

        bool rayLeft = Physics.SphereCast(this.transform.position + Vector3.up, 0.25f, -this.transform.right, out RaycastHit hitLeft, MAX_DISTANCE, MaskReference.Terrain);
        bool rayRight = Physics.SphereCast(this.transform.position + Vector3.up, 0.25f, this.transform.right, out RaycastHit hitRight, MAX_DISTANCE, MaskReference.Terrain);

        if (rayLeft && !rayRight)
        {
            return -1;
        }
        else if (rayRight && !rayLeft)
        {
            return 1;
        }
        else if (rayLeft && rayRight)
        {
            if (hitLeft.distance < hitRight.distance && hitLeft.distance > MIN_DISTANCE)
            {
                return -1;
            }
            else if (hitRight.distance < hitLeft.distance && hitRight.distance > MIN_DISTANCE)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            bool hasNavLeft = NavMesh.SamplePosition(pointLeft, out NavMeshHit navHitLeft, MAX_DISTANCE, NavMesh.AllAreas);
            bool hasNavRight = NavMesh.SamplePosition(pointRight, out NavMeshHit navHitRight, MAX_DISTANCE, NavMesh.AllAreas);

            float navDistLeft = Vector3.Distance(this.transform.position, navHitLeft.position);
            float navDistRight = Vector3.Distance(this.transform.position, navHitRight.position);
            if (hasNavLeft && !hasNavRight)
            {
                return -1;
            }
            else if (hasNavRight && !hasNavLeft)
            {
                return 1;
            }
            else if (hasNavLeft && hasNavRight)
            {
                if (navHitLeft.distance < navHitRight.distance && navDistLeft > MIN_DISTANCE)
                {
                    return -1;
                }
                else if (navHitRight.distance < navHitLeft.distance && navDistRight > MIN_DISTANCE)
                {
                    return 1;
                }
                else if (navDistLeft > MIN_DISTANCE && navDistRight > MIN_DISTANCE)
                {
                    return (Random.value > 0.5f) ? 1 : -1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return (Random.value > 0.5f) ? 1 : -1;
            }
        }
    }

    public void SetStrafeDestination(int direction)
    {
        if (direction != 0)
        {
            Vector3 samplePosition = CombatTarget.transform.position + (Quaternion.Euler(0f, direction * 60f, 0f) * (this.transform.position - CombatTarget.transform.position));
            Debug.DrawLine(CombatTarget.transform.position, samplePosition, Color.green, 3f);
            if (NavMesh.SamplePosition(samplePosition, out NavMeshHit hit, 2f, 1))
            {
                SetDestination(hit.position);
            }
        }
    }
    public void MoveOnEnd()
    {
        if (animancer == null) return;
        animancer.Play(navstate.move);
    }
    public bool IsClearLineToTarget()
    {
        if (CombatTarget != null)
        {
            Vector3 targetPos = CombatTarget.transform.position;
            if (CombatTarget.TryGetComponent<HumanoidPositionReference>(out HumanoidPositionReference hpr))
            {
                targetPos = hpr.Spine.position;
            }
            if (Physics.Raycast(positionReference.Spine.transform.position, targetPos - positionReference.Spine.transform.position, out RaycastHit hit, Vector3.Distance(targetPos, positionReference.Spine.transform.position), ~LayerMask.GetMask("Limbs", "Hitboxes","InteractionNode"), QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.root == CombatTarget.transform.root)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void PlayIdle()
    {
        animancer.Play(navstate.idle);
    }


    void OnDisable()
    {
        //ScriptableObject.Destroy(runtimeNavAnims);
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
    public override bool IsFalling()
    {
        return animancer.States.Current == navstate.fall;
    }

    public virtual bool IsStrafing()
    {
        return animancer.States.Current == navstate.strafe;
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
