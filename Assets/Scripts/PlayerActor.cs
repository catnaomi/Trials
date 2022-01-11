using Animancer;
using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(HumanoidPositionReference))]
public class PlayerActor : Actor, IAttacker, IDamageable
{
    CharacterController cc;
    public bool instatemove;
    public Vector2 move;
    Vector2 moveSmoothed;
    public Vector2 look;
    float defaultRadius;
    [HideInInspector]public HumanoidPositionReference positionReference;

    [Header("Inventory")]
    public PlayerInventory inventory;
    int equipToType;
    float mainWeaponAngle;
    float offWeaponAngle;
    [Header("Camera")]
    public CameraState camState = CameraState.None;
    CameraState prevCamState;
    [SerializeField]
    public VirtualCameras vcam;
    [Header("Interaction & Dialogue")]
    List<Interactable> interactables;
    public Interactable highlightedInteractable;
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
    bool blocking;
    bool aiming;
    bool aimAttack;
    Vector3 targetDirection;
    Vector3 headPoint;
    Vector3 smoothedHeadPoint;
    public float headPointSpeed = 25f;
    [Space(5)]
    public float strafeSpeed = 2.5f;
    [Space(5)]
    public float attackDecel = 25f;
    public float dashAttackDecel = 10f;
    float attackDecelReal;
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
    [Header("Combat")]
    public bool attack;
    bool slash;
    bool thrust;
    bool plunge;
    ClipTransition plungeEnd;
    float cancelTime;
    int attackIndex = 0;
    public float blockSpeed = 2.5f;
    float attackResetTimer = 0f;
    float aimTimer;
    bool isHitboxActive;
    public float aimCancelTime = 2f;
    public float aimTime;
    public float aimStartTime = 0.25f;
    [ReadOnly, SerializeField] private DamageKnockback currentDamage;
    [Header("Animancer")]
    AnimancerComponent animancer;
    public MixerTransition2DAsset moveAnim;
    public AnimationClip idleAnim;
    public ClipTransition dashAnim;
    public ClipTransition sprintAnim;
    ClipTransition currentSprintAnim;
    public ClipTransition skidAnim;
    public ClipTransition fallAnim;
    public ClipTransition landSoftAnim;
    public ClipTransition landHardAnim;
    public ClipTransition rollAnim;
    public ClipTransition standJumpAnim;
    public ClipTransition runJumpAnim;
    public ClipTransition backflipAnim;
    [Space(5)]
    public MixerTransition2D ledgeHang;
    public ClipTransition ledgeClimb;
    public ClipTransition ledgeStart;
    public ClipTransition ladderClimb;
    public ClipTransition ladderClimbUp;
    [Space(5)]
    public LinearMixerTransition swimAnim;
    public ClipTransition swimEnd;
    public ClipTransition swimStart;
    [Space(5)]
    public float horizontalAimSpeed = 90f;
    public float aimSpeed = 2.5f;
    Vector3 aimForwardVector;
    MixerTransition2DAsset aimAnim;
    [Space(5)]
    public AvatarMask upperBodyMask;
    [Header("Damage Anims")]
    public DamageAnims damageAnim;
    HumanoidDamageHandler damageHandler;
    MixerTransition2D blockMove;
    ClipTransition blockAnimStart;
    ClipTransition blockAnim;
    ClipTransition blockStagger;
    PlayerActor movementController;
    AnimState state;

    public static PlayerActor player;
    public UnityEvent OnHitboxActive;

    private System.Action _OnLandEnd;
    private System.Action _OnFinishClimb;
    private System.Action _MoveOnEnd;
    private System.Action _AttackEnd;
    private System.Action _StopUpperLayer;
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
        public AnimancerState attack;
        public AnimancerState block;
        public AnimancerState aim;
        public AnimancerState hurt;
    }

    enum AnimLayer
    {
       Base = 0,
       UpperBody = 1,
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
        player = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        cc = this.GetComponent<CharacterController>();
        defaultRadius = cc.radius;
        movementController = this.GetComponent<PlayerActor>();
        animancer = this.GetComponent<AnimancerComponent>();
        state.move = (MixerState)animancer.States.GetOrCreate(moveAnim);
        state.attack = animancer.States.GetOrCreate(rollAnim);
        animancer.Play(state.move);

        

        this.GetComponent<PlayerInput>().actions["Sprint"].performed += (context) =>
        {
            SprintStart();
        };

        this.GetComponent<PlayerInput>().actions["Sprint"].canceled += (context) =>
        {
            SprintEnd();
        };

        this.GetComponent<PlayerInput>().actions["Block"].started += (context) =>
        {
            BlockStart();
        };

        this.GetComponent<PlayerInput>().actions["Block"].canceled += (context) =>
        {
            BlockEnd();
        };

        this.GetComponent<PlayerInput>().actions["Aim"].started += (context) =>
        {
            AimStart();
        };

        this.GetComponent<PlayerInput>().actions["Aim"].canceled += (context) =>
        {
            AimEnd();
        };

        currentSprintAnim = sprintAnim;

        skidAnim.Events.OnEnd += () => { state.sprint = animancer.Play(sprintAnim); };
        //landAnim.Events.OnEnd += () => { animancer.Play(state.move, 1f); };
        rollAnim.Events.OnEnd += () => { animancer.Play(state.move, 0.5f); };
        standJumpAnim.Events.OnEnd += () => { state.fall = animancer.Play(fallAnim); };
        runJumpAnim.Events.OnEnd += () => { state.fall = animancer.Play(fallAnim); };
        backflipAnim.Events.OnEnd += () => { state.fall = animancer.Play(fallAnim); };
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

        _MoveOnEnd = () =>
        {
            animancer.Play(state.move, 0.1f);
        };

        damageHandler = new HumanoidDamageHandler(this, damageAnim, animancer);
        damageHandler.SetEndAction(_MoveOnEnd);
        damageHandler.SetBlockEndAction(() => { animancer.Play(state.block, 0.5f); });

        _AttackEnd = () =>
        {
            animancer.Play(state.move, 0.5f);
            attack = false;
            slash = false;
            thrust = false;
        };

        _StopUpperLayer = () =>
        {
            animancer.Layers[1].Stop();
        };

        ledgeClimb.Events.OnEnd = _OnFinishClimb;

        ladderClimbUp.Events.OnEnd = _OnFinishClimb;

        ledgeStart.Events.OnEnd += () => { state.climb = (DirectionalMixerState)animancer.Play(ledgeHang); };

        System.Action _MoveAndReset = () =>
        {
            animancer.Play(state.move, 0.1f);
            walkAccelReal = walkAccel;
        };
        landHardAnim.Events.OnEnd = _MoveAndReset;
        landSoftAnim.Events.OnEnd = _MoveAndReset;
        swimEnd.Events.OnEnd = _MoveAndReset;
        animancer.Layers[(int)AnimLayer.UpperBody].SetMask(upperBodyMask);
        //animancer.Layers[(int)AnimLayer.UpperBody].IsAdditive = true;
        UpdateFromMoveset();

        inventory.OnChange.AddListener(() =>
        {
            if (inventory.CheckWeaponChanged())
            {
                UpdateFromMoveset();
            }
        });

        OnHurt.AddListener(() => { HitboxActive(0); });
    }


    // Update is called once per frame
    void Update()
    {
        instatemove = (animancer.States.Current == state.move);
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
        GetHeadPoint();

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
                    state.sprint = animancer.Play(currentSprintAnim, 1f);
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
            if (blocking && inventory.IsMainDrawn())
            {
                //((MixerState)state.block).ChildStates[0].Clip = blockAnimStart.Clip;
                animancer.Play(state.block, 0.25f);
                /*
                animancer.Layers[1].Play(blockAnimStart, 0f);
                blockAnimStart.Events.OnEnd = () => {
                    animancer.Layers[1].Play(blockAnim);
                    ((MixerState)state.block).ChildStates[0].Clip = blockAnim.Clip;
                };
                */
            }
            if (aiming)
            {
                Aim();
            }
            if (attack && !animancer.Layers[1].IsAnyStatePlaying())
            {
                if (!inventory.IsMainDrawn())
                {
                    inventory.SetDrawn(true, true);
                }
                if (inventory.IsMainDrawn())
                {
                    if (slash)
                    {
                        MainSlash();

                    }
                    else if (thrust)
                    {
                        MainThrust();
                    }
                }
                //else if (inventory.IsMainEquipped() && !animancer.Layers[1].IsAnyStatePlaying())
                //{
                //    TriggerSheath(true, inventory.GetMainWeapon().MainHandEquipSlot, true);
                //}
                attack = false;
                slash = false;
                thrust = false;
            }
            else if (!animancer.Layers[1].IsAnyStatePlaying())
            {
                if (inventory.IsMainEquipped() && inventory.IsMainDrawn() && inventory.IsOffEquipped() && !inventory.IsOffDrawn())
                {
                    TriggerSheath(true, inventory.GetOffWeapon().OffHandEquipSlot, false);
                }
                else if (inventory.IsMainEquipped() && !inventory.IsMainDrawn() && inventory.IsOffEquipped() && inventory.IsOffDrawn())
                {
                    TriggerSheath(false, inventory.GetOffWeapon().OffHandEquipSlot, false);
                }
            }
            if (attackResetTimer <= 0f)
            {
                attackIndex = 0;
            }
            else
            {
                attackResetTimer -= Time.deltaTime;
            }
            animancer.Layers[0].ApplyAnimatorIK = true;
        }
        else if (animancer.States.Current == state.block)
        {
            bool stopBlock = false;
            speed = Mathf.MoveTowards(speed, walkSpeedCurve.Evaluate(move.magnitude) * blockSpeed, walkAccelReal * Time.deltaTime);
            if (camState == CameraState.Free)
            {
                lookDirection = this.transform.forward;
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
                stopBlock = true;
            }
            /*
            if (jump)
            {
                jump = false;
                state.jump = animancer.Play(backflipAnim);
                moveDirection = this.transform.forward;
                speed = -blockSpeed;
                stopBlock = true;
            }
            */
            if (attack)
            {
                if (slash)
                {
                    BlockSlash();
                    attackDecelReal = attackDecel;
                }
                else if (thrust)
                {
                    BlockThrust();
                    attackDecelReal = attackDecel;
                }
                attack = false;
                slash = false;
                thrust = false;
                stopBlock = true;
            }
            if (CheckWater())
            {
                state.swim = animancer.Play(swimStart, 0.25f);
                this.gameObject.SendMessage("SplashBig");
                stopBlock = true;
            }
            if (shouldDodge)
            {
                shouldDodge = false;
                if (stickDirection.magnitude > 0)
                {
                    lookDirection = stickDirection.normalized;
                }
                else
                {
                    lookDirection = transform.forward;
                }
                state.roll = animancer.Play(rollAnim);
                stopBlock = true;
            }
            if (!blocking)
            {
                stopBlock = true;
                animancer.Play(state.move, 0.25f);
            }
            if (stopBlock)
            {
                if (animancer.Layers[1].IsPlayingClip(blockAnim.Clip) || animancer.Layers[1].IsPlayingClip(blockAnimStart.Clip))
                {
                    animancer.Layers[1].Stop();
                }
            }
            animancer.Layers[0].ApplyAnimatorIK = true;
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
                    state.sprint = animancer.Play(currentSprintAnim);
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
            animancer.Layers[0].ApplyAnimatorIK = false;
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
            if (attack && !animancer.Layers[1].IsAnyStatePlaying())
            {
                if (!inventory.IsMainDrawn())
                {
                    inventory.SetDrawn(true, true);
                }
                if (inventory.IsMainDrawn())
                {
                    if (slash)
                    {
                        DashSlash();
                    }
                    else if (thrust)
                    {
                        DashThrust();
                    }
                }
                attack = false;
                slash = false;
            }
            animancer.Layers[0].ApplyAnimatorIK = false;
        }
        else if (animancer.States.Current == state.skid)
        {
            speed = Mathf.MoveTowards(speed, 0f, skidDecel * Time.deltaTime);
            moveDirection = lastSprintForward;
            animancer.Layers[0].ApplyAnimatorIK = false;
        }
        else if (animancer.States.Current == state.fall)
        {
            //speed = 0f;
            airTime += Time.deltaTime;
            if (GetGrounded() && yVel <= 0)
            {
                if (lastAirTime >= hardLandingTime)
                {

                    /*AnimancerState land = state.move.ChildStates[0];
                    land.Clip = landHardAnim;
                    
                    land.Events.OnEnd = _OnLandEnd;
                    speed = 0f;
                    animancer.Play(state.move, 0.25f);*/

                    AnimancerState land = animancer.Play(landHardAnim);
                    walkAccelReal = hardLandAccel;
                    speed = 0f;
                    sprinting = false;
                }
                else if (lastAirTime >= softLandingTime)
                {
                    AnimancerState land = animancer.Play(landSoftAnim);
                    //AnimancerState land = state.move.ChildStates[0];
                    //land.Clip = landSoftAnim;
                    walkAccelReal = softLandAccel;
                    //land.Events.OnEnd = _OnLandEnd;
                    speed = 0f;
                    animancer.Play(state.move, 0.25f);
                }
                else
                {
                    this.gameObject.SendMessage("Thud");
                    animancer.Play(state.move, 0.25f);
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
            if (attack && !animancer.Layers[1].IsAnyStatePlaying())
            {
                if (!inventory.IsMainDrawn())
                {
                    inventory.SetDrawn(true, true);
                }
                if (inventory.IsMainDrawn())
                {
                    if (slash)
                    {
                        PlungeSlash();
                    }
                    else if (thrust)
                    {
                        PlungeThrust();
                    }
                }
                attack = false;
                slash = false;
                thrust = false;
            }
        }
        else if (animancer.States.Current == state.roll)
        {
            shouldDodge = false;
            speed = rollSpeed;
            moveDirection = this.transform.forward;
            if (attack && !animancer.Layers[1].IsAnyStatePlaying())
            {
                if (!inventory.IsMainDrawn())
                {
                    inventory.SetDrawn(true, true);
                }
                if (inventory.IsMainDrawn())
                {
                    if (slash)
                    {
                        rollAnim.Events.OnEnd = () => { RollSlash(); };
                        
                    }
                    else if (thrust)
                    {
                        rollAnim.Events.OnEnd = () => { RollThrust(); };
                    }
                }
                attack = false;
                slash = false;
                thrust = false;
            }
            animancer.Layers[0].ApplyAnimatorIK = false;
        }
        else if (animancer.States.Current == state.jump)
        {
            jump = false;
            //speed = 0f;
            //speed = sprintSpeed;
            moveDirection = this.transform.forward;
            animancer.Layers[0].ApplyAnimatorIK = true;
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
            animancer.Layers[0].ApplyAnimatorIK = false;
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
                //AnimancerState land = state.move.ChildStates[0];
                AnimancerState land = animancer.Play(swimEnd);
                walkAccelReal = hardLandAccel;
                speed = 0f;
                sprinting = false;
            }

            animancer.Layers[0].ApplyAnimatorIK = (speed < 0.1f);
        }
        else if (animancer.States.Current == state.attack)
        {
            speed = Mathf.MoveTowards(speed, 0f, attackDecelReal * Time.deltaTime);
            moveDirection = this.transform.forward;

            if (cancelTime > 0 && animancer.States.Current.NormalizedTime >= cancelTime && !plunge)
            {
                if (attack && slash)
                {
                    attack = false;
                    slash = false;
                    cancelTime = -1f;
                    CancelSlash();
                    
                }
                else if (attack && thrust)
                {
                    attack = false;
                    thrust = false;
                    cancelTime = -1f;
                    CancelThrust();
                }
                attackResetTimer = 0.5f;
            }
            else
            {
                attackResetTimer = 0f;
            }
            if (plunge && GetGrounded())
            {
                state.attack = animancer.Play(plungeEnd);
                plunge = false;
                xzVel = Vector3.zero;
                speed = 0f;
            }
            if (CheckWater())
            {
                state.swim = animancer.Play(swimAnim);
                this.gameObject.SendMessage("SplashBig");
            }
            animancer.Layers[0].ApplyAnimatorIK = false;
        }
        else if (animancer.States.Current == state.aim)
        {
            speed = Mathf.MoveTowards(speed, walkSpeedCurve.Evaluate(move.magnitude) * aimSpeed, walkAccelReal * Time.deltaTime);
            moveDirection = stickDirection;

            aimForwardVector = Quaternion.AngleAxis(look.x * horizontalAimSpeed * Time.deltaTime, Vector3.up) * aimForwardVector;

            bool turn = false;
            if (move.magnitude > 0f)
            {
                lookDirection = aimForwardVector;
            }
            else
            {
                turn = true;
                lookDirection = this.transform.forward;
            }
            if (state.aim is DirectionalMixerState aimDir)
            {
                aimDir.ParameterX = Vector3.Dot(moveDirection, this.transform.right) * (speed / walkSpeedMax);
                aimDir.ParameterY = Vector3.Dot(moveDirection, this.transform.forward) * (speed / walkSpeedMax);
                if (aimDir.ChildStates[0] is LinearMixerState aimTurn)
                {
                    if (turn)
                    {
                        aimTurn.Parameter = Vector3.SignedAngle(lookDirection, aimForwardVector, Vector3.up);
                    }
                    else
                    {
                        aimTurn.Parameter = 0f;
                    }
                }
            }
            if (inventory.IsRangedEquipped())
            {
                EquippableWeapon rwep = inventory.GetRangedWeapon();
                bool anyPlaying = animancer.Layers[1].IsAnyStatePlaying();
                bool atk = IsAttackHeld();
                if (!inventory.IsRangedDrawn() && !anyPlaying)
                {
                    Debug.Log("ranged is equipped");
                    inventory.SetDrawn(Inventory.MainType, false);
                    inventory.SetDrawn(Inventory.OffType, false);
                    TriggerSheath(true, inventory.GetRangedWeapon().RangedEquipSlot, Inventory.RangedType);
                }
                else if (!aiming && aimAttack)//(!atk && aimAttack)
                {
                    ClipTransition clip = rwep.moveset.aimAttack.GetFireClip();
                    clip.Events.OnEnd = () => { animancer.Layers[1].Play(rwep.moveset.aimAttack.GetIdleClip()); };
                    animancer.Layers[1].Play(clip);
                    aimAttack = false;
                    attack = false;
                    slash = false;
                    thrust = false;
                }
                else
                {
                    if (aiming && !aimAttack && inventory.IsRangedDrawn())
                    {
                        ClipTransition clip = rwep.moveset.aimAttack.GetStartClip();
                        clip.Events.OnEnd = () => { animancer.Layers[1].Play(rwep.moveset.aimAttack.GetHoldClip()); };
                        animancer.Layers[1].Play(clip);
                        aimAttack = true;
                    }
                    else if (!anyPlaying)
                    {
                        animancer.Layers[1].Play(rwep.moveset.aimAttack.GetIdleClip());
                    }
                }
            }
            if (!aiming)
            {
                if (aimTimer <= 0f)
                {
                    if (inventory.IsRangedEquipped())
                    {
                        TriggerSheath(false, inventory.GetRangedWeapon().RangedEquipSlot, Inventory.RangedType);
                    }
                    
                    animancer.Play(state.move);
                }
                else
                {
                    aimTimer -= Time.deltaTime;
                }
            }
            animancer.Layers[0].ApplyAnimatorIK = true;
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
        if (blocking && state.block is DirectionalMixerState blockDirectional)
        {
            blockDirectional.ParameterX = Vector3.Dot(moveDirection, this.transform.right) * (speed / walkSpeedMax);
            blockDirectional.ParameterY = Vector3.Dot(moveDirection, this.transform.forward) * (speed / walkSpeedMax);
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
            xzVel.y = 0;
        }
        if (IsAiming() && inventory.IsRangedDrawn())
        {
            inventory.GetRangedWeapon().moveset.aimAttack.OnUpdate(this);
            animancer.Layers[1].ApplyAnimatorIK = true;
        }
        else
        {
            animancer.Layers[1].ApplyAnimatorIK = false;
        }
        if (GetGrounded() && !IsFalling() && !IsClimbing())
        {
            UnsnapLedge();   
        }
        HandleCinemachine();
    }

    private void LateUpdate()
    {
        if (IsAiming() && inventory.IsRangedDrawn())
        {
            inventory.GetRangedWeapon().moveset.aimAttack.OnUpdate(this);
        }
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
    public void UnsnapLedge()
    {
        ledgeSnap = false;
        if (currentClimb != null)
        {
            currentClimb.inUse = false;
        }
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
        else if (IsAiming())
        {
            camState = CameraState.Aim;
        }
        else
        {
            camState = CameraState.Free;
        }
        if (camState != prevCamState)
        {
            vcam.free.gameObject.SetActive(camState == CameraState.Free);
            vcam.climb.gameObject.SetActive(camState == CameraState.Climb);
            vcam.target.gameObject.SetActive(camState == CameraState.Lock);
            vcam.aim.gameObject.SetActive(camState == CameraState.Aim);
        }
        prevCamState = camState;
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

    void AimStart()
    {
        aiming = true;
        aimTimer = aimCancelTime;
    }

    void AimEnd()
    {
        aiming = false;
    }

    public bool IsAttackHeld()
    {
        bool s = this.GetComponent<PlayerInput>().actions["Atk_Slash"].IsPressed();
        bool t = this.GetComponent<PlayerInput>().actions["Atk_Thrust"].IsPressed();
        return s || t;
    }

    void BlockStart()
    {
        blocking = true;

    }

    void BlockEnd()
    {
        blocking = false;
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

    public void OnAtk_Slash(InputValue value)
    {
        attack = true;
        slash = true;
    }

    public void OnAtk_Thrust(InputValue value)
    {
        attack = true;
        thrust = true;
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
            return Vector2.ClampMagnitude(move,0.5f);
        }
        return Vector2.zero;
    }

    public void OnSheathe(InputValue value)
    {
        if (inventory.IsMainEquipped() && !animancer.Layers[1].IsAnyStatePlaying())
        {
            if (!inventory.IsMainDrawn())
            {
                TriggerSheath(true, inventory.GetMainWeapon().MainHandEquipSlot, true);
            }
            else
            {
                TriggerSheath(false, inventory.GetMainWeapon().MainHandEquipSlot, true);
            }
        }
        
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

    #region INVENTORY & ITEMS

    public Moveset GetMoveset()
    {
        if (inventory.IsMainDrawn())
        {
            return inventory.GetMainWeapon().moveset;
        }
        return null;
    }

    public Moveset GetMovesetOff()
    {
        if (inventory.IsOffDrawn())
        {
            return inventory.GetOffWeapon().moveset;
        }
        else if (inventory.IsMainDrawn())
        {
            return inventory.GetMainWeapon().moveset;
        }
        return null;
    }
    public void UpdateFromMoveset()
    {
        MixerTransition2DAsset movementAnim = moveAnim;

        if (inventory.IsMainDrawn())
        {
            Moveset moveset = inventory.GetMainWeapon().moveset;
            if (moveset.moveAnim != null)
            {
                movementAnim = moveset.moveAnim;
            }
        }
        bool moving = false;
        if (animancer.States.Current == state.move)
        {
            moving = true;
        }
        state.move = (MixerState)animancer.States.GetOrCreate(movementAnim);
        if (moving) animancer.Play(state.move, 0.25f);


        MixerTransition2DAsset blockingMoveAnim = moveAnim;
        if (inventory.IsOffDrawn() && inventory.GetOffWeapon().moveset.overridesBlock)
        {
            blockingMoveAnim = inventory.GetOffWeapon().moveset.blockMove;
            blockAnim = inventory.GetOffWeapon().moveset.blockAnim;
            blockAnimStart = inventory.GetOffWeapon().moveset.blockAnimStart;
            blockStagger = inventory.GetOffWeapon().moveset.blockStagger;
        }
        else if (inventory.IsMainDrawn() && inventory.GetMainWeapon().moveset.overridesBlock)
        {
            blockingMoveAnim = inventory.GetMainWeapon().moveset.blockMove;
            blockAnim = inventory.GetMainWeapon().moveset.blockAnim;
            blockAnimStart = inventory.GetMainWeapon().moveset.blockAnimStart;
            blockStagger = inventory.GetMainWeapon().moveset.blockStagger;
        }

        state.block = (MixerState)animancer.States.GetOrCreate(blockingMoveAnim);
        damageHandler.SetBlockClip(blockStagger);

        ClipTransition sprintingAnim = sprintAnim;
        if (inventory.IsMainDrawn() && inventory.GetMainWeapon().moveset.overridesSprint)
        {
            sprintingAnim = inventory.GetMainWeapon().moveset.sprintAnim;
        }

        currentSprintAnim = sprintingAnim;

        if (inventory.IsRangedEquipped())
        {
            aimAnim = inventory.GetRangedWeapon().moveset.aimAttack.GetMovement();
        }
        else
        {
            aimAnim = moveAnim;
        }
    }



    public void TriggerSheath(bool draw, Inventory.EquipSlot slot, int targetSlot)
    {
        if ((targetSlot == Inventory.MainType) && inventory.IsMainEquipped())
        {
            AnimancerState drawState = animancer.Layers[(int)AnimLayer.UpperBody].Play((draw) ? inventory.GetMainWeapon().moveset.draw : inventory.GetMainWeapon().moveset.sheathe);
            drawState.Events.OnEnd = _StopUpperLayer;
        }
        else if ((targetSlot == Inventory.OffType) && inventory.IsOffEquipped())
        {
            AnimancerState drawState = animancer.Layers[(int)AnimLayer.UpperBody].Play((draw) ? inventory.GetOffWeapon().moveset.draw : inventory.GetOffWeapon().moveset.sheathe);
            drawState.Events.OnEnd = _StopUpperLayer;
        }
        else if ((targetSlot == Inventory.RangedType) && inventory.IsRangedEquipped())
        {
            AnimancerState drawState = animancer.Layers[(int)AnimLayer.UpperBody].Play((draw) ? inventory.GetRangedWeapon().moveset.draw : inventory.GetRangedWeapon().moveset.sheathe);
            drawState.Events.OnEnd = _StopUpperLayer;
        }
        equipToType = targetSlot;
    }

    public void TriggerSheath(bool draw, Inventory.EquipSlot slot, bool targetMain)
    {
        TriggerSheath(draw, slot, (targetMain) ? 0 : 1);
    }

    public void AnimDrawWeapon(int slot)
    {
        inventory.SetDrawn(equipToType, true);
        UpdateFromMoveset();
    }

    public void AnimSheathWeapon(int slot)
    {
        inventory.SetDrawn(equipToType, false);
        UpdateFromMoveset();
    }


    public void RotateMainWeapon(float angle)
    {
        if (!inventory.IsMainDrawn())
        {
            return;
        }
        EquippableWeapon weapon = inventory.GetMainWeapon();
        GameObject weaponModel = weapon.model;
        GameObject mount = this.positionReference.MainHand;

        float angleDiff = mainWeaponAngle - angle;

        //Quaternion rotation = Quaternion.AngleAxis(angle, mount.transform.up);

        //weaponModel.transform.rotation = rotation;

        weaponModel.transform.RotateAround(mount.transform.position, mount.transform.up, angleDiff);

        if (weapon is BladeWeapon blade)
        {
            blade.GetHitboxes().root.transform.RotateAround(mount.transform.position, mount.transform.up, angleDiff);
        }
        mainWeaponAngle = angle;
        Debug.Log("rotating");
    }

    // rotates the off hand weapon model around the upwards axis of the off hand mount
    public void RotateOffWeapon(float angle)
    {
        if (!inventory.IsOffDrawn())
        {
            return;
        }
        EquippableWeapon weapon = inventory.GetOffWeapon();
        GameObject weaponModel = weapon.model;
        GameObject mount = this.positionReference.OffHand;

        float angleDiff = offWeaponAngle - angle;

        //Quaternion rotation = Quaternion.AngleAxis(angle, mount.transform.up);

        //weaponModel.transform.rotation = rotation;

        weaponModel.transform.RotateAround(mount.transform.position, mount.transform.up, angleDiff);

        if (weapon is BladeWeapon blade)
        {
            blade.GetHitboxes().root.transform.RotateAround(mount.transform.position, mount.transform.up, angleDiff);
        }
        offWeaponAngle = angle;
    }

    public void ResetMainRotation()
    {
        RotateMainWeapon(0f);
    }

    public void ResetOffRotation()
    {
        RotateOffWeapon(0f);
    }
    #endregion

    #region COMBAT

    // attacks
    #region attacks
    public void MainSlash()
    {
        if (GetMoveset().quickSlash1h is ComboAttack combo)
        {
            state.attack = animancer.Play(combo.GetClip(attackIndex));
            cancelTime = combo.GetExitTime(attackIndex);
            attackIndex++;
            attackResetTimer = 0.5f;
            SetCurrentDamage(combo.GetDamage(attackIndex));
        }
        else
        {
            state.attack = animancer.Play(GetMoveset().quickSlash1h.GetClip());
            cancelTime = -1f;
            SetCurrentDamage(GetMoveset().quickSlash1h.GetDamage());
        }
        
        
        state.attack.Events.OnEnd = _AttackEnd;
        attackDecelReal = attackDecel;
        SetCurrentDamage(GetMoveset().quickSlash1h.GetDamage());
    }

    public void MainThrust()
    {
        if (GetMoveset().quickThrust1h is ComboAttack combo && combo.HasNext(attackIndex))
        {
            state.attack = animancer.Play(combo.GetClip(attackIndex));
            cancelTime = combo.GetExitTime(attackIndex);
            attackIndex++;
            attackResetTimer = 0.5f;
            SetCurrentDamage(combo.GetDamage(attackIndex));
        }
        else
        {
            state.attack = animancer.Play(GetMoveset().quickThrust1h.GetClip());
            cancelTime = -1f;
            SetCurrentDamage(GetMoveset().quickThrust1h.GetDamage());
        }


        state.attack.Events.OnEnd = _AttackEnd;
        attackDecelReal = attackDecel;
        
    }

    public void CancelSlash()
    {
        if (GetMoveset().quickSlash1h is ComboAttack combo)
        {
            state.attack = animancer.Play(combo.GetClip(attackIndex));
            cancelTime = combo.GetExitTime(attackIndex);
            attackResetTimer = 0.5f;
            attackIndex++;
            SetCurrentDamage(combo.GetDamage(attackIndex));
        }
        else
        {
            cancelTime = -1f;
            SetCurrentDamage(GetMoveset().quickSlash1h.GetDamage());
        }
        state.attack.Events.OnEnd = _AttackEnd;
        
    }

    public void CancelThrust()
    {
        if (GetMoveset().quickThrust1h is ComboAttack combo)
        {
            state.attack = animancer.Play(combo.GetClip(attackIndex));
            cancelTime = combo.GetExitTime(attackIndex);
            attackResetTimer = 0.5f;
            attackIndex++;
            SetCurrentDamage(combo.GetDamage(attackIndex));
        }
        else
        {
            cancelTime = -1f;
            SetCurrentDamage(GetMoveset().quickThrust1h.GetDamage());
        }
        state.attack.Events.OnEnd = _AttackEnd;
    }
    public void DashSlash()
    {
        state.attack = animancer.Play(GetMoveset().dashSlash.GetClip());
        attackDecelReal = dashAttackDecel;
        state.attack.Events.OnEnd = _MoveOnEnd;
        dashed = false;
        attackIndex = 0;
        SetCurrentDamage(GetMoveset().dashSlash.GetDamage());
    }

    public void DashThrust()
    {
        state.attack = animancer.Play(GetMoveset().dashThrust.GetClip());
        attackDecelReal = dashAttackDecel;
        state.attack.Events.OnEnd = _MoveOnEnd;
        dashed = false;
        attackIndex = 0;
        SetCurrentDamage(GetMoveset().dashThrust.GetDamage());
    }
    

    public void BlockSlash()
    {
        System.Action _BlockAttackEnd = () => {
            if (blocking)
            {
                animancer.Layers[1].Play(blockAnim, 0.25f);
                animancer.Play(state.block, 0.25f);
            }
            else
            {
                _MoveOnEnd();
            }
        };
        if (inventory.IsOffDrawn() && inventory.GetOffWeapon().moveset.stanceSlash != null)
        {
            state.attack = animancer.Play(inventory.GetOffWeapon().moveset.stanceSlash.GetClip());
            state.attack.Events.OnEnd = _BlockAttackEnd;
            attackIndex = 0;
            SetCurrentDamage(inventory.GetOffWeapon().moveset.stanceSlash.GetDamage());
        }
        else if (inventory.IsMainDrawn() && inventory.GetMainWeapon().moveset.stanceSlash != null)
        {
            state.attack = animancer.Play(inventory.GetMainWeapon().moveset.stanceSlash.GetClip());
            state.attack.Events.OnEnd = _BlockAttackEnd;
            attackIndex = 0;
            SetCurrentDamage(inventory.GetMainWeapon().moveset.stanceSlash.GetDamage());
        }
        else
        {
            MainSlash();
        }
    }
    public void BlockThrust()
    {
        System.Action _BlockAttackEnd = () => {
            if (blocking)
            {
                animancer.Layers[1].Play(blockAnim, 0.25f);
                animancer.Play(state.block, 0.25f);
            }
            else
            {
                _MoveOnEnd();
            }
        };
        if (inventory.IsOffDrawn() && inventory.GetOffWeapon().moveset.stanceThrust != null)
        {
            state.attack = animancer.Play(inventory.GetOffWeapon().moveset.stanceThrust.GetClip());
            state.attack.Events.OnEnd = _BlockAttackEnd;
            attackIndex = 0;
            SetCurrentDamage(inventory.GetOffWeapon().moveset.stanceThrust.GetDamage());
        }
        else if (inventory.IsMainDrawn() && inventory.GetMainWeapon().moveset.stanceThrust != null)
        {
            state.attack = animancer.Play(inventory.GetMainWeapon().moveset.stanceThrust.GetClip());
            state.attack.Events.OnEnd = _BlockAttackEnd;
            attackIndex = 0;
            SetCurrentDamage(inventory.GetMainWeapon().moveset.stanceThrust.GetDamage());
        }
        else
        {
            MainThrust();
        }
    }
    public void RollSlash()
    {
        state.attack = animancer.Play(GetMoveset().rollSlash.GetClip());
        attackDecelReal = dashAttackDecel;
        state.attack.Events.OnEnd = _MoveOnEnd;
        rollAnim.Events.OnEnd = () => { animancer.Play(state.move, 0.5f); };
        dashed = false;
        attackIndex = 0;
        SetCurrentDamage(GetMoveset().rollSlash.GetDamage());
    }

    public void RollThrust()
    {
        state.attack = animancer.Play(GetMoveset().rollThrust.GetClip());
        attackDecelReal = dashAttackDecel;
        state.attack.Events.OnEnd = _MoveOnEnd;
        rollAnim.Events.OnEnd = () => { animancer.Play(state.move, 0.5f); };
        dashed = false;
        attackIndex = 0;
        SetCurrentDamage(GetMoveset().rollThrust.GetDamage());
    }

    public void PlungeSlash()
    { 
        if (GetMoveset().plungeSlash is PhaseAttack phase)
        {
            ClipTransition clip = GetMoveset().plungeSlash.GetClip();
            state.attack.Events.OnEnd = () => { state.attack = animancer.Play(phase.GetLoopPhaseClip(), 0.1f); };
            state.attack = animancer.Play(GetMoveset().plungeSlash.GetClip());
            attackDecelReal = 0f;
            plungeEnd = phase.GetEndPhaseClip();
            plungeEnd.Events.OnEnd = () => { animancer.Play(state.move, 0.5f); };
        }
        plunge = true;
        attackIndex = 0;
        SetCurrentDamage(GetMoveset().plungeSlash.GetDamage());
    }

    public void PlungeThrust()
    {
        if (GetMoveset().plungeThrust is PhaseAttack phase)
        {
            ClipTransition clip = GetMoveset().plungeThrust.GetClip();
            state.attack.Events.OnEnd = () => { state.attack = animancer.Play(phase.GetLoopPhaseClip(), 0.1f); };
            state.attack = animancer.Play(GetMoveset().plungeThrust.GetClip());
            attackDecelReal = 0f;
            plungeEnd = phase.GetEndPhaseClip();
            plungeEnd.Events.OnEnd = () => { animancer.Play(state.move, 0.5f); };
        }
        plunge = true;
        attackIndex = 0;
        SetCurrentDamage(GetMoveset().plungeThrust.GetDamage());
    }

    public void Aim()
    {
        state.aim = animancer.Play(aimAnim);
        aimForwardVector = this.transform.forward;
        aimTime = 0f;
    }
    #endregion
    /*
    * triggered by animation:
    * 0 = deactivate hitboxes
    * 1 = main weapon
    * 2 = off weapon, if applicable
    * 3 = both, if applicable
    * 4 = ranged weapon
    */
    public void HitboxActive(int active)
    {
        EquippableWeapon mainWeapon = inventory.GetMainWeapon();
        EquippableWeapon offHandWeapon = inventory.GetOffWeapon();
        EquippableWeapon rangedWeapon = inventory.GetRangedWeapon();
        bool main = (mainWeapon != null && mainWeapon is HitboxHandler);
        bool off = (offHandWeapon != null && offHandWeapon is HitboxHandler);
        bool ranged = (rangedWeapon != null && rangedWeapon is HitboxHandler);
        if (active == 0)
        {
            if (main)
            {
                ((HitboxHandler)mainWeapon).HitboxActive(false);
            }
            if (off)
            {
                ((HitboxHandler)offHandWeapon).HitboxActive(false);
            }
            isHitboxActive = false;
        }
        else if (active == 1)
        {
            if (main)
            {
                ((HitboxHandler)mainWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }
        else if (active == 2)
        {
            if (off)
            {
                ((HitboxHandler)offHandWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }
        else if (active == 3)
        {
            if (main)
            {
                ((HitboxHandler)mainWeapon).HitboxActive(true);
            }
            if (off)
            {
                ((HitboxHandler)offHandWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }
        else if (active == 4)
        {
            if (ranged)
            {
                 ((HitboxHandler)rangedWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }

    }

    public void Shockwave(int active)
    {
        if (currentDamage == null) return;
        currentDamage.source = this.gameObject;
        float SHOCKWAVE_RADIUS = 2f;

        bool main = (inventory.IsMainDrawn());
        bool off = (inventory.IsOffDrawn());

        Vector3 origin = this.transform.position;
        if (active == 1 && main)
        {
            origin = inventory.GetMainWeapon().GetModel().transform.position;
        }
        else if (active == 2 && off)
        {
            origin = inventory.GetOffWeapon().GetModel().transform.position;
        }

        Collider[] colliders = Physics.OverlapSphere(this.transform.position, SHOCKWAVE_RADIUS, LayerMask.GetMask("Actors"));
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<IDamageable>(out IDamageable damageable) && (collider.transform.root != this.transform.root || currentDamage.canDamageSelf))
            {
                damageable.TakeDamage(currentDamage);
            }
        }
    }
    public void SetCurrentDamage(DamageKnockback damageKnockback)
    {
        currentDamage = new DamageKnockback(damageKnockback);
        currentDamage.source = this.gameObject;
    }

    public DamageKnockback GetCurrentDamage()
    {
        return currentDamage;
    }


    public override void ProcessDamageKnockback(DamageKnockback damageKnockback)
    {
        HitboxActive(0);
        TakeDamage(damageKnockback);
    }
    public void Recoil()
    {
        HitboxActive(0);
        damageHandler.Recoil();
    }

    public void TakeDamage(DamageKnockback damage)
    {
        damageHandler.TakeDamage(damage);
    }
    #endregion

    #region IK

    void GetHeadPoint()
    {
        float dist = 10f;
        Vector3 point = Vector3.zero;
        if (this.GetCombatTarget() != null)
        {
            point = this.GetCombatTarget().transform.position;
        }
        else if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, dist, LayerMask.GetMask("Terrain")))
        {
            point = hit.point;
        }
        else
        {
            point = Camera.main.transform.position + Camera.main.transform.forward * 10f;
        }
        if (headPoint == Vector3.zero)
        {
            headPoint = point;
        }
        else
        {
            headPoint = Vector3.MoveTowards(headPoint, point, headPointSpeed * Time.deltaTime);
        }
    }

    public Vector3 GetLaunchVector(Vector3 origin)
    {

        GameObject target = this.GetCombatTarget();
        if (target != null)
        {
            if (Vector3.Distance(target.transform.position, origin) > 2)
            {
                return (target.transform.position - origin).normalized;
            }
            else
            {
                return this.transform.forward;
            }
        }
        else if (IsAiming())
        {
            Vector3 aimPos = Camera.main.transform.position + Camera.main.transform.forward * 100f;

            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 100f) && !hit.transform.IsChildOf(this.transform.root))
            {
                aimPos = hit.point;
            }
            return (aimPos - origin).normalized;
        }
        else
        {
            return this.transform.forward;
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (IsAiming() && inventory.IsRangedDrawn())
        {
            inventory.GetRangedWeapon().moveset.aimAttack.OnIK(animancer.Animator);
        }
        else
        {
            Vector3 point = Vector3.zero;
            if (this.GetCombatTarget() != null || IsAiming())
            {
                point = headPoint;
            }
            else
            {
                float headY = this.transform.position.y + positionReference.eyeHeight;
                Vector3 headDir = headPoint - this.transform.position;
                headDir.Scale(new Vector3(1f, 0f, 1f));
                if (Vector3.Angle(headDir.normalized, this.transform.forward) < 45f)
                {
                    point = new Vector3(headPoint.x, headY, headPoint.z);
                    animancer.Animator.SetLookAtPosition(point);
                }
                else
                {
                    point = this.transform.position + this.transform.forward * headDir.magnitude + this.transform.up * positionReference.eyeHeight;
                }
            }
            smoothedHeadPoint = Vector3.MoveTowards(smoothedHeadPoint, point, headPointSpeed * Time.deltaTime);
            animancer.Animator.SetLookAtPosition(smoothedHeadPoint);
            animancer.Animator.SetLookAtWeight(1f, 0.1f, 1f, 0f, 0.7f);
        }
        
    }
    #endregion

    #region State Checks

    public bool IsAiming()
    {
        return animancer.States.Current == state.aim;
    }

    public bool IsFalling()
    {
        return animancer.States.Current == state.fall;
    }

    public bool IsClimbing()
    {
        return animancer.States.Current == state.climb;
    }

    public bool IsTwoHanding()
    {
        return inventory.IsMainDrawn() && inventory.GetMainWeapon().TwoHandOnly();
    }

    public override bool IsBlocking()
    {
        return animancer.States.Current == state.block || animancer.States.Current == damageHandler.block;
    }
    #endregion

    #region INTERACTION
    public void AddInteractable(Interactable interactable)
    {
        if (!interactables.Contains(interactable))
        {
            interactables.Add(interactable);
            GetHighlightedInteractable();
        }
    }

    public void RemoveInteractable(Interactable interactable)
    {
        interactables.Remove(interactable);
        GetHighlightedInteractable();
    }

    public void ClearInteractables()
    {
        interactables.Clear();
        GetHighlightedInteractable();
    }

    public Interactable GetHighlightedInteractable()
    {
        highlightedInteractable = null;
        float leadDist = Mathf.Infinity;

        foreach (Interactable interactable in interactables)
        {
            if (interactable == null) continue;
            float dist = Vector3.Distance(this.transform.position, interactable.transform.position);
            if (dist < leadDist)
            {
                leadDist = dist;
                highlightedInteractable = interactable;
            }
            interactable.SetIconVisiblity(false);
        }
        if (highlightedInteractable != null)
        {
            highlightedInteractable.SetIconVisiblity(true);
        }
        return highlightedInteractable;
    }

    private void Interact()
    {
        Interactable interactable = GetHighlightedInteractable();
        if (interactable != null)
        {
            interactable.Interact(this);
        }

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

    public void HitWall()
    {
        Debug.Log("wall hit");
    }
}
