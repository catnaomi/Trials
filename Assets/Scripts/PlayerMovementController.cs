using Animancer;
using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(HumanoidPositionReference))]
public class PlayerMovementController : Actor
{
    CharacterController cc;

    public Vector2 move;
    Vector2 moveSmoothed;
    public Vector2 look;
    float defaultRadius;
    [HideInInspector]public HumanoidPositionReference positionReference;

    [Header("Inventory")]
    public HumanoidInventory inventory;
    [Header("Camera")]
    public CameraState camState = CameraState.None;
    CameraState prevCamState;
    [SerializeField]
    public VirtualCameras vcam;

    [Header("Movement")]
    public float walkSpeedMax = 5f;
    public AnimationCurve walkSpeedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public float sprintSpeed = 10f;
    public float sprintTurnSpeed = 90f;
    public float walkAccel = 25f;
    public float walkTurnSpeed = 1080f;
    [Space(5)]
    public float hardLandAccel = 2.5f;
    public float softLandAccel = 2.5f;
    public float skidAngle = 160f;
    public float skidDecel = 10f;
    Vector3 lastSprintForward;
    public float gravity = 9.81f;
    public float terminalVel = 70f;
    public float fallBufferTime = 0.25f;
    public float hardLandingTime = 2f;
    public float softLandingTime = 1f;
    [Space(5)]
    public float rollSpeed = 5f;
    public float jumpVel = 10f;
    [Space(5)]
    public float yVel;
    public bool isGrounded;
    public Vector3 xzVel;
    public float airTime = 0f;
    float lastAirTime;
    float landTime = 0f;
    float speed;
    bool dashed;
    bool jump;
    Vector3 targetDirection;
    [Space(5)]
    public float strafeSpeed = 2.5f;
   
    float walkAccelReal;
    public bool sprinting;
    public bool shouldDodge;
    [Header("Climbing Settings")]
    public ClimbDetector currentClimb;
    bool ledgeSnap;
    bool ledgeHanging;
    
    bool allowClimb = true;
    bool allowLadderFinish = true;
    public Collider hangCollider;
    public float climbSpeed = 1f; 
    [Header("Swim Settings")]
    public bool inWater;
    public Collider swimCollider;
    public float waterVerticalSpeed;
    public float swimSpeed;
    public float swimAccel;
    public float wadingHeight;
    float waterHeight;
    public float wadingSpeed = 3f;
    public bool wading;
    public float wadingPercent;
    [Header("Animancer")]
    public AnimancerComponent animancer;

    public MixerTransition2DAsset moveAnim;
    public AnimationClip idleAnim;
    public ClipTransition dashAnim;
    public ClipTransition sprintAnim;
    public ClipTransition skidAnim;
    public ClipTransition fallAnim;
    public AnimationClip landSoftAnim;
    public AnimationClip landHardAnim;
    public ClipTransition rollAnim;
    public ClipTransition standJumpAnim;
    public ClipTransition runJumpAnim;
    [Space(5)]
    public MixerTransition2D ledgeHang;
    public ClipTransition ledgeClimb;
    public ClipTransition ledgeStart;
    public ClipTransition ladderClimb;
    public ClipTransition ladderClimbUp;
    [Space(5)]
    public LinearMixerTransition swimAnim;
    public AnimationClip swimEnd;
    public ClipTransition swimStart;

    PlayerMovementController movementController;
    AnimState state;


    private System.Action _OnLandEnd;
    private System.Action _OnFinishClimb;
    [Header("Targeting")]
    public UnityEvent toggleTarget;
    public UnityEvent changeTarget;
    struct AnimState
    {
        public MixerState move;
        public AnimancerState dash;
        public AnimancerState sprint;
        public AnimancerState skid;
        public AnimancerState fall;
        public AnimancerState roll;
        public AnimancerState jump;
        public AnimancerState climb;
        public AnimancerState swim;
    }
    [Serializable]
    public struct VirtualCameras
    {
        public CinemachineVirtualCameraBase free;
        public CinemachineVirtualCameraBase target;
        public CinemachineVirtualCameraBase aim;
        public CinemachineVirtualCameraBase climb;
    }

    private void Awake()
    {
        positionReference = this.GetComponent<HumanoidPositionReference>();
        positionReference.LocateSlotsByName();
    }
    // Start is called before the first frame update
    void Start()
    {
        cc = this.GetComponent<CharacterController>();
        defaultRadius = cc.radius;
        movementController = this.GetComponent<PlayerMovementController>();
        state.move = (MixerState)animancer.States.GetOrCreate(moveAnim);
        animancer.Play(state.move);

        this.GetComponent<PlayerInput>().actions["Sprint"].started += (context) =>
        {
            SprintStart();
        };

        this.GetComponent<PlayerInput>().actions["Sprint"].canceled += (context) =>
        {
            SprintEnd();
        };

        skidAnim.Events.OnEnd += () => { state.sprint = animancer.Play(sprintAnim); };
        //landAnim.Events.OnEnd += () => { animancer.Play(state.move, 1f); };
        rollAnim.Events.OnEnd += () => { animancer.Play(state.move, 0.5f); };
        standJumpAnim.Events.OnEnd += () => { state.fall = animancer.Play(fallAnim); };
        runJumpAnim.Events.OnEnd += () => { state.fall = animancer.Play(fallAnim); };
        swimStart.Events.OnEnd += () => {
            state.swim = animancer.Play(swimAnim);
        };
        walkAccelReal = walkAccel;

        _OnLandEnd = () =>
        {
            state.move.ChildStates[0].Clip = idleAnim;
            walkAccelReal = walkAccel;
        };

        _OnFinishClimb = () =>
        {
            cc.enabled = true;
            airTime = 0f;
            yVel = 0f;
            xzVel = Vector3.zero;
            animancer.Play(state.move, 0.25f);
        };

        ledgeClimb.Events.OnEnd = _OnFinishClimb;

        ladderClimbUp.Events.OnEnd = _OnFinishClimb;

        ledgeStart.Events.OnEnd += () => { state.climb = (DirectionalMixerState)animancer.Play(ledgeHang); };
    }


    // Update is called once per frame
    void Update()
    {
        isGrounded = GetGrounded();
        moveSmoothed = Vector2.MoveTowards(moveSmoothed, move, Time.deltaTime);
        Vector3 camForward = Camera.main.transform.forward;
        camForward.Scale(new Vector3(1f, 0f, 1f));
        Vector3 camRight = Camera.main.transform.right;
        camRight.Scale(new Vector3(1f, 0f, 1f));
        Vector3 stickDirection = Vector3.zero;
        Vector3 lookDirection = this.transform.forward;
        Vector3 moveDirection = Vector3.zero;
        float grav = gravity;

        stickDirection = camForward * move.y + camRight * move.x;

        if (animancer.States.Current == state.move)
        {
            float speedMax = 0f;
            if (wading)
            {
                speedMax = Mathf.Lerp(walkSpeedMax, wadingSpeed, wadingPercent);
            }
            else if (camState == CameraState.Lock)
            {
                speedMax = strafeSpeed;
            }
            else
            {
                speedMax = walkSpeedMax;
            }
            speed = Mathf.MoveTowards(speed, walkSpeedCurve.Evaluate(move.magnitude) * speedMax, walkAccelReal * Time.deltaTime);
            if (camState == CameraState.Free)
            {
                if (stickDirection.sqrMagnitude > 0)
                {
                    lookDirection = Vector3.RotateTowards(lookDirection, stickDirection.normalized, Mathf.Deg2Rad * walkTurnSpeed * Time.deltaTime, 1f);
                }
                moveDirection = stickDirection;
            }
            else if (camState == CameraState.Lock)
            {
                if (GetCombatTarget() != null)
                {
                    targetDirection = this.transform.position - GetCombatTarget().transform.position;
                    targetDirection.Scale(new Vector3(-1f, 0f, -1f));
                    lookDirection = Vector3.RotateTowards(lookDirection, targetDirection, Mathf.Deg2Rad * walkTurnSpeed * Time.deltaTime, 1f);

                }
                else
                {
                    lookDirection = this.transform.forward;
                }
                moveDirection = stickDirection;
            }
            if (!GetGrounded() && lastAirTime > fallBufferTime)
            {
                state.fall = animancer.Play(fallAnim);
            }
            if (sprinting && move.magnitude >= 0.5f && landTime >= 1f)
            {
                if (!dashed)
                {
                    lookDirection = stickDirection.normalized;
                    state.dash = animancer.Play(dashAnim);
                }
                else
                {
                    state.sprint = animancer.Play(sprintAnim, 1f);
                }

            }
            if (shouldDodge)
            {
                shouldDodge = false;
                lookDirection = stickDirection.normalized;
                state.roll = animancer.Play(rollAnim);
            }
            if (jump)
            {
                jump = false;
                state.jump = animancer.Play((move.magnitude < 0.5f) ? standJumpAnim : runJumpAnim);
            }
            if (CheckWater())
            {
                state.swim = animancer.Play(swimStart, 0.25f);
                this.gameObject.SendMessage("SplashBig");
            }
        }
        else if (animancer.States.Current == state.dash)
        {
            dashed = true;
            speed = sprintSpeed;
            moveDirection = this.transform.forward;
            lastSprintForward = moveDirection;
            if (shouldDodge)
            {
                state.roll = animancer.Play(rollAnim);
            }
            else if (jump)
            {
                jump = false;
                state.jump = animancer.Play(runJumpAnim);
            }
            else if (state.dash.NormalizedTime >= 0.8f)
            {
                if (sprinting)
                {
                    state.sprint = animancer.Play(sprintAnim);
                }
                else
                {
                    animancer.Play(state.move, 0.5f);
                }
            }
            if (!GetGrounded() && lastAirTime > fallBufferTime)
            {
                state.fall = animancer.Play(fallAnim);
            }

        }
        else if (animancer.States.Current == state.sprint)
        {
            speed = sprintSpeed;
            if (shouldDodge)
            {
                state.roll = animancer.Play(rollAnim);
            }
            else if (jump)
            {
                jump = false;
                state.jump = animancer.Play(runJumpAnim);
            }
            else if (move.magnitude > 0.75f && Vector3.Angle(lookDirection, stickDirection) >= skidAngle)
            {
                
                state.skid = animancer.Play(skidAnim);
                lookDirection = -stickDirection;
                lastSprintForward = -stickDirection;

            }
            else if (move.magnitude <= 0f || !sprinting)
            {
                animancer.Play(state.move, 0.5f);
                Debug.Log("sprint release");
            }
            else
            {
                lookDirection = Vector3.RotateTowards(lookDirection, stickDirection.normalized, Mathf.Deg2Rad * sprintTurnSpeed * Time.deltaTime, 1f);
                moveDirection = lookDirection;
            }
            if (!GetGrounded() && lastAirTime > fallBufferTime)
            {
                state.fall = animancer.Play(fallAnim);
            }
            if (CheckWater())
            {
                state.swim = animancer.Play(swimAnim);
                this.gameObject.SendMessage("SplashBig");
            }
            
        }
        else if (animancer.States.Current == state.skid)
        {
            speed = Mathf.MoveTowards(speed, 0f, skidDecel * Time.deltaTime);
            moveDirection = lastSprintForward;
        }
        else if (animancer.States.Current == state.fall)
        {
            //speed = 0f;
            airTime += Time.deltaTime;
            if (GetGrounded() && yVel <= 0)
            {
                if (lastAirTime >= hardLandingTime)
                {

                    AnimancerState land = state.move.ChildStates[0];
                    land.Clip = landHardAnim;
                    walkAccelReal = hardLandAccel;
                    land.Events.OnEnd = _OnLandEnd;
                    speed = 0f;
                    animancer.Play(state.move, 0.25f);
                    sprinting = false;
                }
                else if (lastAirTime >= softLandingTime)
                {
                    AnimancerState land = state.move.ChildStates[0];
                    land.Clip = landSoftAnim;
                    walkAccelReal = softLandAccel;
                    land.Events.OnEnd = _OnLandEnd;
                    speed = 0f;
                    animancer.Play(state.move, 0.25f);
                }
                else
                {
                    this.gameObject.SendMessage("Thud");
                    animancer.Play(state.move, 0.1f);
                }

            }
            if (ledgeSnap)
            {
                if (currentClimb.TryGetComponent<Ledge>(out Ledge ledge))
                {
                    animancer.Play(ledgeStart);

                }
                else if (currentClimb.TryGetComponent<Ladder>(out Ladder ladder))
                {
                    state.climb = animancer.Play(ladderClimb);
                }
                SnapToLedge();
                StartCoroutine(DelayedSnapToLedge());
            }
            if (CheckWater())
            {
                state.swim = animancer.Play(swimAnim);
                this.gameObject.SendMessage("SplashBig");
            }
        }
        else if (animancer.States.Current == state.roll)
        {
            shouldDodge = false;
            speed = rollSpeed;
            moveDirection = this.transform.forward;
        }
        else if (animancer.States.Current == state.jump)
        {
            jump = false;
            //speed = 0f;
            //speed = sprintSpeed;
            moveDirection = this.transform.forward;
        }
        else if (animancer.States.Current == state.climb)
        {
            if (currentClimb.TryGetComponent<Ledge>(out Ledge ledge))
            {
                ((DirectionalMixerState)state.climb).ParameterX = move.x;
            }
            else if (currentClimb.TryGetComponent<Ladder>(out Ladder ladder))
            {
                state.climb.Speed = move.y * climbSpeed;
                if (ladder.snapPoint <= -0.9 && move.y > 0 && allowLadderFinish)
                {
                    SnapToLedge();
                    this.transform.position = ladder.endpoint.transform.position;
                    this.transform.rotation = Quaternion.LookRotation(currentClimb.collider.transform.forward);
                    ledgeSnap = false;
                    animancer.Play(ladderClimbUp);
                    StartCoroutine("ClimbLockout");

                }
            }
        }
        else if (animancer.States.Current == state.swim)
        {
            if (CheckWater())
            {
                speed = Mathf.MoveTowards(speed, walkSpeedCurve.Evaluate(move.magnitude) * swimSpeed, swimAccel * Time.deltaTime);
                if (stickDirection.sqrMagnitude > 0)
                {
                    lookDirection = Vector3.RotateTowards(lookDirection, stickDirection.normalized, Mathf.Deg2Rad * walkTurnSpeed * Time.deltaTime, 1f);
                }
                moveDirection = stickDirection;
            }
            else
            {
                AnimancerState land = state.move.ChildStates[0];
                land.Clip = swimEnd;
                walkAccelReal = hardLandAccel;
                land.Events.OnEnd = _OnLandEnd;
                speed = 0f;
                animancer.Play(state.move, 0.25f);
            }


        }

        if (GetGrounded())
        {
            if (yVel <= 0)
            {
                yVel = 0f;
            }
            else
            {
                yVel -= grav * Time.deltaTime;
            }
            airTime = 0f;
            landTime += Time.deltaTime;
            if (landTime > 1f)
            {
                landTime = 1f;
            }
        }
        else
        {
            yVel -= grav * Time.deltaTime;
            if (yVel < -terminalVel)
            {
                yVel = -terminalVel;
            }
            airTime += Time.deltaTime;
            lastAirTime = airTime;
        }

        this.transform.rotation = Quaternion.LookRotation(lookDirection);
        Vector3 downwardsVelocity = this.transform.up * yVel;
        if (state.move is CartesianMixerState cartesian)
        {
            cartesian.Parameter = movementController.GetMovementVector();
        }
        else if (state.move is DirectionalMixerState directional)
        {
            directional.Parameter = movementController.GetMovementVector();
        }
        if (state.swim is LinearMixerState linearSwim)
        {
            linearSwim.Parameter = speed / swimSpeed;
        }
        Vector3 finalMov = (moveDirection * speed + downwardsVelocity);

        if (cc.enabled)
        {
            if (animancer.States.Current != state.swim && (!GetGrounded() || yVel > 0 || animancer.States.Current == state.jump))
            {
                /*
                if (Physics.SphereCast(this.transform.position, cc.radius, Vector3.down, out RaycastHit sphereHit, 1f, LayerMask.GetMask("Terrain")))
                {
                    Vector3 dir = -(this.transform.position - sphereHit.point);
                    dir.Scale(new Vector3(1f, 0f, 1f));
                    cc.Move(dir.normalized * 10f);
                    Debug.Log("slide");
                }*/
                cc.Move((xzVel + downwardsVelocity) * Time.deltaTime);
                Debug.Log("fall!!!");
            }
            else if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.5f, LayerMask.GetMask("Terrain")) && yVel <= 0 && animancer.States.Current != state.swim)
            {
                Vector3 temp = Vector3.Cross(hit.normal, finalMov);
                cc.Move((Vector3.Cross(temp, hit.normal) + gravity * Vector3.down) * Time.deltaTime);
            }
            else
            {
                cc.Move(finalMov * Time.deltaTime);
            }
        }
        if (animancer.States.Current == state.move || animancer.States.Current == state.sprint || animancer.States.Current == state.dash)
        {
            xzVel = finalMov;
            xzVel.Scale(new Vector3(1f, 0f, 1f));
        }

        HandleCinemachine();
    }


    private void LateUpdate()
    {

    }

    #region CLIMBING
    public void SetLedge(Ledge ledge)
    {
        if (allowClimb)
        {
            ledgeSnap = true;
            currentClimb = ledge;
        }
    }

    public void SetLadder(Ladder ladder)
    {
        if (allowClimb)
        {
            ledgeSnap = true;
            currentClimb = ladder;
            allowLadderFinish = false;
            StartCoroutine(LadderFinishLockout());
        }
    }
    public void UnsnapLedge(ClimbDetector ledge)
    {
        ledgeSnap = false;
    }

    public void SnapToCurrentLedge()
    {
        SnapToLedge();
        //StartCoroutine(DelayedSnapToLedge());
    }

    void SnapToLedge()
    {
        if (ledgeSnap == true && currentClimb != null)
        {
            if (currentClimb.TryGetComponent<Ledge>(out Ledge ledge))
            {
                this.transform.rotation = Quaternion.LookRotation(currentClimb.collider.transform.forward);
                int sign = 0;
                if (Mathf.Abs(move.x) > 0.1f) sign = (int)Mathf.Sign(move.x);
                this.transform.position = ledge.GetSnapPointDot(cc.radius * 2f, this.transform.position, this, -sign);
                this.GetComponent<Collider>().enabled = false;
                ledgeHanging = true;
            }
            else if (currentClimb.TryGetComponent<Ladder>(out Ladder ladder))
            {
                //descendClamp = (ladder.canDescend) ? -1 : 0;
                //ascendClamp = (ladder.canAscend) ? 1 : 0;
                this.transform.rotation = Quaternion.LookRotation(currentClimb.collider.transform.forward);
                int sign = 0;
                if (Mathf.Abs(move.y) > 0.1f) sign = (int)Mathf.Sign(move.y);
                this.transform.position = ladder.GetSnapPointDot(cc.height/2f, this.transform.position, this, -sign);
                this.GetComponent<Collider>().enabled = false;
                ledgeHanging = true;
            }
            /*
            else
            {


                this.transform.rotation = Quaternion.LookRotation(currentClimb.collider.transform.forward);

                //Vector3 offset = currentClimb.collider.transform.position - this.hangCollider.bounds.center;
                Vector3 offset = currentClimb.collider.transform.position - this.transform.position;

                Vector3 verticalOffset = Vector3.up * offset.y;

                Vector3 horizontalOffset = Vector3.Project(offset, currentClimb.collider.transform.forward);

                this.GetComponent<Collider>().enabled = false;

                transform.position = this.transform.position + (verticalOffset + horizontalOffset);

                Debug.Log("movement! horiz" + horizontalOffset.magnitude);

                ledgeHanging = true;
            }
            */
        }
    }
    IEnumerator DelayedSnapToLedge()
    {
        while (ledgeSnap == true)
        {
            //yield return new WaitForEndOfFrame();
            SnapToLedge();
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator ClimbLockout()
    {
        allowClimb = false;
        yield return new WaitForSeconds(1f);
        allowClimb = true;
    }

    IEnumerator LadderFinishLockout()
    {
        allowLadderFinish = false;
        yield return new WaitForSeconds(1f);
        allowLadderFinish = true;
    }
    #endregion

    #region CAMERA
    void HandleCinemachine()
    {
        if (animancer.States.Current == state.climb)
        {
            camState = CameraState.Climb;
        }
        else if (GetCombatTarget() != null)
        {
            camState = CameraState.Lock;
        }
        else
        {
            camState = CameraState.Free;
        }
        if (camState == CameraState.Free)
        {
            if (prevCamState != CameraState.Free)
            {
                vcam.free.gameObject.SetActive(true);
                vcam.climb.gameObject.SetActive(false);
                vcam.target.gameObject.SetActive(false);
            }
        }
        else if (camState == CameraState.Lock)
        {
            if (prevCamState != CameraState.Lock)
            {
                vcam.free.gameObject.SetActive(false);
                vcam.target.gameObject.SetActive(true);
                vcam.climb.gameObject.SetActive(false);
            }
        }
        else if (camState == CameraState.Climb)
        {
            if (prevCamState != CameraState.Climb)
            {
                vcam.free.gameObject.SetActive(false);
                vcam.climb.gameObject.SetActive(true);
                vcam.target.gameObject.SetActive(false);
            }
        }
    }
    public enum CameraState
    {
        None,
        Free,
        Lock,
        Aim,
        Climb
    }

    #endregion

    #region INPUT
    public void OnDodge(InputValue value)
    {
        if (animancer.States.Current == state.climb)
        {
            ledgeSnap = false;
            state.fall = animancer.Play(fallAnim, 1f);
            cc.enabled = true;
            airTime = 0f;
            cc.Move(Vector3.down * 0.5f);
            yVel = 0f;
            xzVel = Vector3.zero;
            StartCoroutine("ClimbLockout");
        }
        else
        {
            shouldDodge = true;
        }
        
    }

    public void OnJump(InputValue value)
    {
        if (animancer.States.Current == state.climb)
        {
            ledgeSnap = false;
            animancer.Play(ledgeClimb);
            StartCoroutine("ClimbLockout");
        }
        else if (GetGrounded())
        {
            jump = true;
        }
        
    }

    public void ApplyJump()
    {
        yVel = jumpVel;
    }

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        look = value.Get<Vector2>();
    }

    void SprintStart()
    {
        sprinting = true;
        dashed = false;
    }

    void SprintEnd()
    {
        sprinting = false;
    }

    public void DashEnd()
    {
        Debug.Log("dash-end");
        animancer.Play(state.sprint);
    }

    public void OnTarget(InputValue value)
    {
        toggleTarget.Invoke();
    }

    public void OnChangeTarget(InputValue value)
    {
        if (value.Get<Vector2>().magnitude > 0.9f) changeTarget.Invoke();
    }

    public Vector2 GetMovementVector()
    {
        if (camState == CameraState.Free)
        {
            return new Vector2(0f, speed / walkSpeedMax);
            //return new Vector2(0f, move.magnitude);
        }
        else if (camState == CameraState.Lock)
        {
            return move;
        }
        return Vector2.zero;
    }

    #endregion

    #region SWIMMING

    bool CheckWater()
    {
        wading = false;
        inWater = Physics.Raycast(this.transform.position + Vector3.up * cc.height, Vector3.down, out RaycastHit waterHit, cc.height + 0.1f, LayerMask.GetMask("Water"), QueryTriggerInteraction.Collide);
        if (inWater)
        {
            swimCollider = waterHit.collider;
            waterHeight = swimCollider.bounds.center.y + swimCollider.bounds.extents.y;
            if (Physics.Raycast(this.transform.position + Vector3.up * cc.height, Vector3.down, out RaycastHit wadingHit, 10f, LayerMask.GetMask("Terrain")))
            {
                wading = (waterHeight - wadingHit.point.y) <= wadingHeight;
                if (wading)
                {
                    wadingPercent = Mathf.Clamp((waterHeight - wadingHit.point.y)/wadingHeight,0f,1f);
                    
                }
                else
                {
                    wadingPercent = 0f;
                }
                Physics.IgnoreCollision(this.GetComponent<Collider>(), swimCollider, wading);
            }
            if (!wading)
            {
                return true;
            }
            
        }
        return false;
    }
    #endregion

    public bool GetGrounded()
    {
        // return cc.isGrounded;
        Collider c = this.GetComponent<Collider>();
        Vector3 bottom = c.bounds.center + c.bounds.extents.y * Vector3.down;
        Debug.DrawLine(bottom, bottom + Vector3.down * 0.2f, Color.red);
        return Physics.Raycast(bottom, Vector3.down, 0.2f, LayerMask.GetMask("Terrain")) || cc.isGrounded;
    }
}
