using Animancer;
using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

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
    public bool isMenuOpen;
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
    public bool isGrounded;
    public float airTime = 0f;
    float lastAirTime;
    float landTime = 0f;
    float speed;
    bool dashed;
    bool jump;
    bool blocking;
    bool aiming;
    Vector3 targetDirection;
    Vector3 headPoint;
    Vector3 smoothedHeadPoint;
    bool aimAtkLockout;
    public float headPointSpeed = 25f;
    public PhysicMaterial lastPhysicsMaterial;
    [Space(5)]
    public float strafeSpeed = 2.5f;
    [Space(5)]
    public float attackDecel = 25f;
    public float dashAttackDecel = 10f;
    float attackDecelReal;
    public float walkAccelReal;
    public bool sprinting;
    public bool shouldDodge;
    [Header("Carrying Settings")]
    public Carryable carryable;
    public bool isCarrying;
    public bool isDropping;
    public float throwForce = 500f;
    public float throwForceUp = 50f;
    public float throwMassMax = 20f;
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
    public float wadingHeightIn = 0.4f;
    public float wadingHeightOut = 0.6f;
    float waterHeight;
    public float wadingSpeed = 3f;
    public bool wading;
    public float wadingPercent;
    [Header("Combat")]
    public bool attack;
    bool slash;
    bool thrust;
    bool plunge;
    bool hold;
    ClipTransition plungeEnd;
    float cancelTime;
    public float blockSpeed = 2.5f;
    float aimTimer;
    bool isHitboxActive;
    bool isSheathing;
    string test1;
    bool dead;
    public float aimCancelTime = 2f;
    public float aimTime;
    public float aimStartTime = 0.25f;
    Vector2 camAimSpeeds;
    [Range(-1f,1f)]
    public float thrustIKValue;
    public float thrustIKWeight;
    public float thrustIKMultiplier = 1f;
    public float thrustIKAdjustSpeed;
    public float thrustInitialHeight;
    public float thrustIKHeightRange;
    [ReadOnly, SerializeField] private DamageKnockback currentDamage;
    [Header("Animancer")]
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
    [Space(5)]
    [ReadOnly] public BilayerMixer2DAsset bilayerMove;
    [Header("Damage Anims")]
    public DamageAnims damageAnim;
    HumanoidDamageHandler damageHandler;
    MixerTransition2D blockMove;
    ClipTransition blockAnimStart;
    ClipTransition blockAnim;
    ClipTransition blockStagger;
    PlayerActor movementController;
    AnimState state;
    public AimAttack.AimState astate;

    public static PlayerActor player;
    public UnityEvent OnHitboxActive;

    private System.Action _OnLandEnd;
    private System.Action _OnFinishClimb;
    private System.Action _MoveOnEnd;
    private System.Action _AttackEnd;
    private System.Action _StopUpperLayer;
    [Header("Carry Anims")]
    public ClipTransition pickUpAnim;
    public ClipTransition slowPickUpAnim;
    public ClipTransition carryAnim;
    public ClipTransition throwAnim;
    public ClipTransition dropAnim;
    [Header("Movesets")]
    public Moveset runtimeMoveset;
    public Moveset runtimeOffMoveset;
    [Header("Targeting")]
    public UnityEvent toggleTarget;
    public UnityEvent changeTarget;
    [Header("Controls")]
    public UnityEvent onControlsChanged;
    public UnityEvent onNewCurrentInteractable;
    struct AnimState
    {
        public MixerState move;
        public AnimancerState moveAdditLayer;
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
        public AnimancerState dialogue;
        public AnimancerState carry;
    }

    

    
    [Serializable]
    public struct VirtualCameras
    {
        public CinemachineVirtualCameraBase free;
        public CinemachineVirtualCameraBase target;
        public CinemachineVirtualCameraBase aim;
        public CinemachineVirtualCameraBase climb;
        public CinemachineVirtualCameraBase dialogue;
    }

    private void Awake()
    {
        positionReference = this.GetComponent<HumanoidPositionReference>();
        positionReference.LocateSlotsByName();
        player = this;
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

    }
    // Start is called before the first frame update
    public override void ActorStart()
    {
        base.ActorStart();
        cc = this.GetComponent<CharacterController>();
        defaultRadius = cc.radius;
        movementController = this.GetComponent<PlayerActor>();
        animancer = this.GetComponent<AnimancerComponent>();
        interactables = new List<Interactable>();
        state.move = (MixerState)animancer.States.GetOrCreate(moveAnim);
        state.attack = animancer.States.GetOrCreate(rollAnim);
        animancer.Play(state.move);

        SetupInputListeners();

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
            //attack = false;
            slash = false;
            thrust = false;
            if (mainWeaponAngle != 0f)
            {
                StartCoroutine("GradualResetMainRotation");
            }
            if (offWeaponAngle != 0f)
            {
                StartCoroutine("GradualResetOffRotation");
            }
        };

        _StopUpperLayer = () =>
        {
            animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
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
        animancer.Layers[HumanoidAnimLayers.UpperBody].SetMask(upperBodyMask);
        //animancer.Layers[HumanoidAnimLayers.UpperBody].IsAdditive = true;
        UpdateFromMoveset();

        inventory.OnChange.AddListener(() =>
        {
            if (inventory.CheckWeaponChanged())
            {
                UpdateFromMoveset();
            }
        });

        OnHurt.AddListener(() => { HitboxActive(0); });
        OnAttack.AddListener(RealignToTarget);

        onControlsChanged.AddListener(HandleCinemachine);
        
    }


    // Update is called once per frame
    public override void ActorPostUpdate()
    {
        if (dead) return;
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

        aiming = GetAiming();
        if (aiming)
        {
            aimTimer = aimCancelTime;
        }

        GetHeadPoint();

        if (!animancer.Layers[0].IsAnyStatePlaying())
        {
            animancer.Play(state.move);
        }
        if (isHitboxActive && animancer.States.Current != state.attack && animancer.States.Current != state.aim)
        {
            HitboxActive(0);
        }
        #region Animancer State Checks
        #region move
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
            if (shouldDodge && isCarrying)
            {
                StartDrop();
                shouldDodge = false;
            }
            else if (shouldDodge)
            {
                shouldDodge = false;
                lookDirection = stickDirection.normalized;
                state.roll = animancer.Play(rollAnim);
                animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
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
            if (isCarrying)
            {
                if (!animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying())
                {
                    animancer.Layers[HumanoidAnimLayers.UpperBody].Play(carryAnim);
                }
            }
            EquippableWeapon blockWeapon = inventory.GetBlockWeapon();
            if (blocking && blockWeapon != null)
            {
                //((MixerState)state.block).ChildStates[0].Clip = blockAnimStart.Clip;

                int itemSlot = inventory.GetItemEquipType(blockWeapon);
                if ((itemSlot == Inventory.MainType && !inventory.IsMainDrawn()) || (itemSlot == Inventory.OffType && !inventory.IsOffDrawn()))
                {
                    inventory.SetDrawn(inventory.GetItemEquipType(blockWeapon), true);
                    UpdateFromMoveset();
                }
                animancer.Play(state.block, 0.25f);
                /*
                animancer.Layers[HumanoidAnimLayers.UpperBody].Play(blockAnimStart, 0f);
                blockAnimStart.Events.OnEnd = () => {
                    animancer.Layers[HumanoidAnimLayers.UpperBody].Play(blockAnim);
                    ((MixerState)state.block).ChildStates[0].Clip = blockAnim.Clip;
                };
                */
            }
            if (aiming)
            {
                Aim();
            }
            if (bilayerMove != null && !animancer.Layers[HumanoidAnimLayers.BilayerBlend].IsPlayingClip(bilayerMove.transition2.Clip))
            {
                animancer.Layers[HumanoidAnimLayers.BilayerBlend].Play(bilayerMove.transition2);
            }
            else if (bilayerMove == null)
            {
                animancer.Layers[HumanoidAnimLayers.BilayerBlend].Stop();
            }
            if (attack && isCarrying)
            {
                StartThrow();
                attack = false;
                slash = false;
                thrust = false;
            }
            else if (attack && !animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying())
            {
                if (!inventory.IsMainDrawn())
                {
                    inventory.SetDrawn(true, true);
                    UpdateFromMoveset();
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
                //else if (inventory.IsMainEquipped() && !animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying())
                //{
                //    TriggerSheath(true, inventory.GetMainWeapon().MainHandEquipSlot, true);
                //}
                attack = false;
                slash = false;
                thrust = false;
            }
            else if (!animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying() && !blocking)
            {
                if (inventory.IsMainEquipped() && inventory.IsMainDrawn() && inventory.IsOffEquipped() && !inventory.IsOffDrawn())
                {
                    //TriggerSheath(true, inventory.GetOffWeapon().OffHandEquipSlot, false);
                }
                else if ((!inventory.IsMainEquipped() || !inventory.IsMainDrawn()) && inventory.IsOffEquipped() && inventory.IsOffDrawn())
                {
                    //TriggerSheath(false, inventory.GetOffWeapon().OffHandEquipSlot, false);
                }
            }
            /*
            if (attackResetTimer <= 0f)
            {
                attackIndex = 0;
            }
            else
            {
                attackResetTimer -= Time.deltaTime;
            }
            */
            animancer.Layers[0].ApplyAnimatorIK = true;
        }
        #endregion

        #region block
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
                if (animancer.Layers[HumanoidAnimLayers.UpperBody].IsPlayingClip(blockAnim.Clip) || animancer.Layers[HumanoidAnimLayers.UpperBody].IsPlayingClip(blockAnimStart.Clip))
                {
                    animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
                }
            }
            animancer.Layers[0].ApplyAnimatorIK = true;
        }
        #endregion

        #region dash
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
        #endregion

        #region sprint
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
            if (attack && !animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying())
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
        #endregion

        #region skid
        else if (animancer.States.Current == state.skid)
        {
            speed = Mathf.MoveTowards(speed, 0f, skidDecel * Time.deltaTime);
            moveDirection = lastSprintForward;
            animancer.Layers[0].ApplyAnimatorIK = false;
        }
        #endregion

        #region fall
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
            if (aiming)
            {
                Aim();
            }
            else if (attack && isCarrying)
            {
                StartThrow();
                attack = false;
                slash = false;
                thrust = false;
            }
            else if (attack && !animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying())
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
        #endregion

        #region roll
        else if (animancer.States.Current == state.roll)
        {
            shouldDodge = false;
            HitboxActive(0);
            speed = rollSpeed;
            moveDirection = this.transform.forward;
            if (attack && !animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying())
            {
                if (!inventory.IsMainDrawn())
                {
                    inventory.SetDrawn(true, true);
                    UpdateFromMoveset();
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
        #endregion

        #region jump
        else if (animancer.States.Current == state.jump)
        {
            jump = false;
            //speed = 0f;
            //speed = sprintSpeed;
            moveDirection = this.transform.forward;
            animancer.Layers[0].ApplyAnimatorIK = true;
        }
        #endregion

        #region climb
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
            yVel = 0f;
            animancer.Layers[0].ApplyAnimatorIK = false;
        }
        #endregion

        #region swim
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
        #endregion

        #region attack
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
            }
            if (plunge && GetGrounded())
            {
                state.attack = animancer.Play(plungeEnd);
                plunge = false;
                xzVel = Vector3.zero;
                speed = 0f;
            }
            else if (hold)
            {
                if ((currentDamage.isThrust && !IsThrustHeld()))
                {
                    HoldThrustRelease(false);
                }
                else if ((currentDamage.isSlash && !IsSlashHeld()))
                {
                    HoldSlashRelease(false);
                }
            }
            if (CheckWater())
            {
                state.swim = animancer.Play(swimAnim);
                this.gameObject.SendMessage("SplashBig");
            }
            animancer.Layers[0].ApplyAnimatorIK = true;
        }
        #endregion

        #region aim
        else if (animancer.States.Current == state.aim)
        {
            speed = Mathf.MoveTowards(speed, walkSpeedCurve.Evaluate(move.magnitude) * aimSpeed, walkAccelReal * Time.deltaTime);
            moveDirection = stickDirection;

            aimForwardVector = Quaternion.AngleAxis(look.x * horizontalAimSpeed * Time.deltaTime, Vector3.up) * aimForwardVector;

            bool turn = false;
            if (false)//GetGrounded())
            {
                
                if (move.magnitude > 0f)
                {
                    lookDirection = aimForwardVector;
                }
                else
                {
                    turn = true;
                    lookDirection = this.transform.forward;
                }
            }
            else
            {
                lookDirection = Camera.main.transform.forward;
                lookDirection.y = 0;
            }
            
            
            if (state.aim is DirectionalMixerState aimDir)
            {
                try
                {
                    aimDir.ParameterX = Vector3.Dot(moveDirection, this.transform.right) * (speed / walkSpeedMax);
                    aimDir.ParameterY = Vector3.Dot(moveDirection, this.transform.forward) * (speed / walkSpeedMax);
                    if (aimDir.ChildStates[0] is LinearMixerState aimTurn)
                    {
                        if (turn)
                        {
                            float angle = Vector3.SignedAngle(lookDirection, aimForwardVector, Vector3.up);
                            aimTurn.Parameter = Mathf.Clamp(angle, -10f, 10f);
                            if (Mathf.Abs(angle) > 10f)
                            {
                                lookDirection = Vector3.RotateTowards(lookDirection, aimForwardVector, (Mathf.Abs(angle) - 10f) * Mathf.Deg2Rad, 1f);
                            }
                        }
                        else
                        {
                            aimTurn.Parameter = 0f;
                        }
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Debug.LogError(ex);

                }
                
            }
            if (inventory.IsRangedEquipped())
            {
                RangedWeapon rwep = (RangedWeapon)inventory.GetRangedWeapon();
                bool anyPlaying = animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying();
                bool atk = false;
                bool begin = false;
                switch (rwep.GetFireMode())
                {
                    default:
                    case RangedWeapon.FireMode.TriggerRelease:
                        atk = !aiming;
                        begin = aiming;
                        break;
                    case RangedWeapon.FireMode.TriggerAndAttackRelease:
                        atk = !IsAttackHeld();
                        begin = IsAttackHeld();
                        break;
                    case RangedWeapon.FireMode.TriggerAndAttackPress:
                        atk = IsAttackHeld();
                        begin = aiming;
                        break;
                }

                rwep.moveset.aimAttack.ProcessAimAttack(this, aiming, slash || thrust, IsAttackHeld());
            }
            if (shouldDodge)
            {
                shouldDodge = false;
                attack = false;
                thrust = false;
                slash = false;
                if (stickDirection.magnitude > 0)
                {
                    lookDirection = stickDirection.normalized;
                }
                else
                {
                    lookDirection = transform.forward;
                }
                animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
                inventory.SetDrawn(Inventory.RangedType, false);
                state.roll = animancer.Play(rollAnim);
            }
            if (!aiming)
            {
                if (aimTimer <= 0f)
                {
                    animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
                    if (inventory.IsRangedEquipped())
                    {
                        //TriggerSheath(false, inventory.GetRangedWeapon().RangedEquipSlot, Inventory.RangedType);
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
        #endregion

        #region dialogue
        else if (animancer.States.Current == state.dialogue)
        {
            animancer.Layers[0].ApplyAnimatorIK = true;
        }
        #endregion
        #endregion

        if (TimeTravelController.time != null && TimeTravelController.time.IsSlowingTime())
        {
            animancer.Layers[HumanoidAnimLayers.Base].Speed = 1f / TimeTravelController.time.timeSlowAmount;
            animancer.Layers[HumanoidAnimLayers.UpperBody].Speed = 1f / TimeTravelController.time.timeSlowAmount;
        }
        else
        {
            animancer.Layers[HumanoidAnimLayers.Base].Speed = 1f;
            animancer.Layers[HumanoidAnimLayers.UpperBody].Speed = 1f;
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
        if (bilayerMove != null)
        {
            animancer.Layers[HumanoidAnimLayers.BilayerBlend].Weight = (IsMoving()) ? Mathf.Min(state.move.Weight, bilayerMove.weight) : 0f;
        }
        else
        {
            animancer.Layers[HumanoidAnimLayers.BilayerBlend].Weight = 0f;
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
            animancer.Layers[HumanoidAnimLayers.UpperBody].ApplyAnimatorIK = true;
            
        }
        else
        {
            animancer.Layers[HumanoidAnimLayers.UpperBody].ApplyAnimatorIK = false;
        }
        if (GetGrounded() && !IsFalling() && !IsClimbing())
        {
            UnsnapLedge();   
        }
        if (isCarrying)
        {
            if (animancer.States.Current != state.move && animancer.States.Current != state.jump && animancer.States.Current != state.fall && animancer.States.Current != state.carry)
            {
                StopCarrying();
            }
            else if (inventory.IsAnyWeaponDrawn())
            {
                StopCarrying();
            }
            else
            {
                //carryable.SetCarryPosition(this.transform.position + Vector3.up * (2f + carryable.yOffset));
                Vector3 dirVector = (isDropping) ? this.transform.forward : Vector3.up;
                Vector3 carryPos = ((positionReference.MainHand.transform.position + positionReference.OffHand.transform.position) / 2f) + (dirVector * carryable.yOffset);
                carryable.SetCarryPosition(carryPos);
            }
            if (!Physics.GetIgnoreCollision(carryable.GetComponent<Collider>(), this.GetComponent<Collider>()))
            {
                Physics.IgnoreCollision(carryable.GetComponent<Collider>(), this.GetComponent<Collider>());
            }
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

    #region GAME FLOW

    public void ProcessDeath()
    {
        SceneLoader.DelayReloadCurrentScene();
    }

    public override string GetCurrentGroundPhysicsMaterial()
    {
        if (CheckWater() || wading)
        {
            return "water";
        }
        else if (lastPhysicsMaterial != null)
        {
            return lastPhysicsMaterial.name;
        }
        else
        {
            return base.GetCurrentGroundPhysicsMaterial();
        }
    }
    #endregion

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
        cc.enabled = true;
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
            camState = (!IsInDialogue()) ? CameraState.Lock : CameraState.Dialogue;
        }
        else if (IsAiming() || aiming)
        {
            camState = CameraState.Aim;
        }
        else
        {
            camState = CameraState.Free;
        }
        if (true)
        {
            if (vcam.free != null) vcam.free.m_Priority = (camState == CameraState.Free) ? 10 : 1;//gameObject.SetActive(camState == CameraState.Free);
            if (vcam.climb != null) vcam.climb.m_Priority = (camState == CameraState.Climb) ? 10 : 1;//.gameObject.SetActive(camState == CameraState.Climb);
            if (vcam.target != null) vcam.target.m_Priority = (camState == CameraState.Lock) ? 10 : 1;//.gameObject.SetActive(camState == CameraState.Lock);
            if (vcam.aim != null) vcam.aim.m_Priority = (camState == CameraState.Aim) ? 10 : 1;//.SetActive(camState == CameraState.Aim);
            if (vcam.dialogue != null) vcam.dialogue.m_Priority = (camState == CameraState.Dialogue) ? 10 : 1;//.gameObject.SetActive(camState == CameraState.D);
        }
        prevCamState = camState;
    }

    public void InitializeAimCameras()
    {
        if (vcam.aim is CinemachineMixingCamera aimMix)
        {
            //((CinemachineMixingCamera)vcam.aim).
        }
    }
    public enum CameraState
    {
        None,
        Free,
        Lock,
        Aim,
        Climb,
        Dialogue,
    }
    #endregion

    #region INPUT

    void SetupInputListeners()
    {
        PlayerInput inputs = this.GetComponent<PlayerInput>();
        inputs.actions["Sprint"].performed += (context) =>
        {
            SprintStart();
        };

        inputs.actions["Sprint"].canceled += (context) =>
        {
            SprintEnd();
        };

        inputs.actions["Block"].started += (context) =>
        {
            BlockStart();
        };

        inputs.actions["Block"].canceled += (context) =>
        {
            BlockEnd();
        };

        inputs.actions["Atk_Slash"].performed += (context) =>
        {
            if (context.interaction is HoldInteraction)
            {
                hold = true;
            }
        };

        inputs.actions["Atk_Thrust"].performed += (context) =>
        {
            if (context.interaction is HoldInteraction)
            {
                hold = true;
            }
        };

        inputs.actions["QuickSlot - 0"].performed += (context) =>
        {
            if (context.interaction is HoldInteraction)
            {
                OnQuickSlotHold(0);
            }
            else
            {
                OnQuickSlot(0);
            }
        };

        inputs.actions["QuickSlot - 1"].performed += (context) =>
        {
            if (context.interaction is HoldInteraction)
            {
                OnQuickSlotHold(1);
            }
            else
            {
                OnQuickSlot(1);
            }
        };

        inputs.actions["QuickSlot - 2"].performed += (context) =>
        {
            if (context.interaction is HoldInteraction)
            {
                OnQuickSlotHold(2);
            }
            else
            {
                OnQuickSlot(2);
            }
        };
    }

    public void OnDodge(InputValue value)
    {
        if (!CanPlayerInput()) return;
        if (IsClimbing())
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
        if (!CanPlayerInput()) return;
        if (IsClimbing() && currentClimb != null && currentClimb is Ledge)
        {
            ledgeSnap = false;
            animancer.Play(ledgeClimb);
            StartCoroutine("ClimbLockout");
        }
        else if (!GetGrounded() && !allowClimb)
        {
            StopCoroutine("ClimbLockout");
            allowClimb = true;
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

    bool GetAiming()
    {
        return (CanPlayerInput() && GetComponent<PlayerInput>().actions["Aim"].IsPressed());
    }

    public bool IsAttackHeld()
    {
        if (!CanPlayerInput()) return false;
        bool s = this.GetComponent<PlayerInput>().actions["Atk_Slash"].IsPressed();
        bool t = this.GetComponent<PlayerInput>().actions["Atk_Thrust"].IsPressed();
        return s || t;
    }

    public bool IsSlashHeld()
    {
        if (!CanPlayerInput()) return false;
        return this.GetComponent<PlayerInput>().actions["Atk_Slash"].IsPressed();
    }

    public bool IsThrustHeld()
    {
        if (!CanPlayerInput()) return false;
        return this.GetComponent<PlayerInput>().actions["Atk_Thrust"].IsPressed();
    }

    void BlockStart()
    {
        if (!CanPlayerInput()) return;
        blocking = true;

    }

    void BlockEnd()
    {
        blocking = false;
    }
    void SprintStart()
    {
        if (!CanPlayerInput()) return;
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
        if (!CanPlayerInput()) return;
        toggleTarget.Invoke();
    }

    public void OnChangeTarget(InputValue value)
    {
        if (value.Get<Vector2>().magnitude > 0.9f) changeTarget.Invoke();
    }

    public void OnAtk_Slash(InputValue value)
    {
        if (!CanPlayerInput()) return;
        attack = true;
        slash = true;
    }

    public void OnAtk_Thrust(InputValue value)
    {
        if (!CanPlayerInput()) return;
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
        if (!CanPlayerInput()) return;
        if (inventory.IsMainEquipped() && !animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying())
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

    public void ResetInputs()
    {
        attack = false;
        slash = false;
        thrust = false;
    }
    void OnControlsChanged()
    {
        onControlsChanged.Invoke();
    }
    #region MENUS

    public void OnMenu(InputValue value)
    {
        if (!IsInDialogue())
        {
            ToggleMenu();
        }
    }
    // checks to see if player input is accepted. used for inventory menu
    public bool CanPlayerInput()
    {
        return !isMenuOpen;
    }
    public void ToggleMenu()
    {
        if (!isMenuOpen)
        {
            MenuController.menu.OpenMenu(MenuController.Inventory);
            //isMenuOpen = true;
        }
        else
        {
            MenuController.menu.HideMenu();
            //isMenuOpen = false;
        }
    }

    public void OnQuickSlot(int slot)
    {
        if ((IsMoving() && CanPlayerInput()) || (InventoryUI2.invUI != null && InventoryUI2.invUI.awaitingQuickSlotEquipInput))
        {
            inventory.InputOnSlot(slot);
            InventoryUI2.invUI.FlareSlot(slot);
        }
    }

    public void OnQuickSlotHold(int slot)
    {
        if (IsMoving() && CanPlayerInput())
        {
            inventory.UnequipOnSlot(slot);
            InventoryUI2.invUI.FlareSlot(slot);
        }
    }
    #endregion
    #endregion

    #region SWIMMING

    bool CheckWater()
    {
        wading = false;
        float wadingHeight = (IsSwimming() ? wadingHeightOut : wadingHeightIn);
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
            if (runtimeMoveset != null)
            {
                return runtimeMoveset;
            }
            return inventory.GetMainWeapon().moveset;
        }
        return null;
    }

    public Moveset GetMovesetOff()
    {
        if (inventory.IsOffDrawn())
        {
            if (runtimeOffMoveset != null)
            {
                return runtimeOffMoveset;
            }
            return inventory.GetOffWeapon().moveset;
        }
        else
        {
            return GetMoveset();
        }
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

        if (movementAnim is BilayerMixer2DAsset bilayerMovementAnim)
        {
            bilayerMove = bilayerMovementAnim;
            state.moveAdditLayer = animancer.Layers[HumanoidAnimLayers.BilayerBlend].GetOrCreateState(bilayerMove.transition2);
            animancer.Layers[HumanoidAnimLayers.BilayerBlend].SetMask(bilayerMove.mask);
            if (moving) animancer.Layers[HumanoidAnimLayers.BilayerBlend].Weight = bilayerMove.weight;
        }
        else
        {
            bilayerMove = null;
        }

        MixerTransition2DAsset blockingMoveAnim = moveAnim;
        if (inventory.IsOffEquipped() && inventory.GetOffWeapon().moveset.overridesBlock)
        {
            blockingMoveAnim = inventory.GetOffWeapon().moveset.blockMove;
            blockAnim = inventory.GetOffWeapon().moveset.blockAnim;
            blockAnimStart = inventory.GetOffWeapon().moveset.blockAnimStart;
            blockStagger = inventory.GetOffWeapon().moveset.blockStagger;
        }
        else if (inventory.IsMainEquipped() && inventory.GetMainWeapon().moveset.overridesBlock)
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
        if (inventory.IsMainEquipped())
        {
            if (runtimeMoveset != null && runtimeMoveset.isClone)
            {
                Destroy(runtimeMoveset);
            }
            runtimeMoveset = inventory.GetMainWeapon().moveset.Clone();
        }
        if (inventory.IsOffEquipped())
        {
            if (runtimeOffMoveset != null && runtimeOffMoveset.isClone)
            {
                Destroy(runtimeOffMoveset);
            }
            runtimeOffMoveset = inventory.GetOffWeapon().moveset.Clone();
        }
    }



    public AnimancerState TriggerSheath(bool draw, Inventory.EquipSlot slot, int targetSlot)
    {
        isSheathing = true;
        if ((targetSlot == Inventory.MainType) && inventory.IsMainEquipped())
        {
            AnimancerState drawState = animancer.Layers[HumanoidAnimLayers.UpperBody].Play((draw) ? inventory.GetMainWeapon().moveset.draw : inventory.GetMainWeapon().moveset.sheathe);
            drawState.Events.OnEnd = _StopUpperLayer;
            equipToType = targetSlot;
            return drawState;
        }
        else if ((targetSlot == Inventory.OffType) && inventory.IsOffEquipped())
        {
            AnimancerState drawState = animancer.Layers[HumanoidAnimLayers.UpperBody].Play((draw) ? inventory.GetOffWeapon().moveset.draw : inventory.GetOffWeapon().moveset.sheathe);
            drawState.Events.OnEnd = _StopUpperLayer;
            equipToType = targetSlot;
            return drawState;
        }
        else if ((targetSlot == Inventory.RangedType) && inventory.IsRangedEquipped())
        {
            AnimancerState drawState = animancer.Layers[HumanoidAnimLayers.UpperBody].Play((draw) ? inventory.GetRangedWeapon().moveset.draw : inventory.GetRangedWeapon().moveset.sheathe);
            drawState.Events.OnEnd = _StopUpperLayer;
            equipToType = targetSlot;
            return drawState;
        }
        return null;
    }

    public void TriggerSheath(bool draw, Inventory.EquipSlot slot, bool targetMain)
    {
        TriggerSheath(draw, slot, (targetMain) ? 0 : 1);
    }

    public void AnimDrawWeapon(int slot)
    {
        isSheathing = false;
        inventory.SetDrawn(equipToType, true);
        UpdateFromMoveset();
    }

    public void AnimSheathWeapon(int slot)
    {
        isSheathing = false;
        inventory.SetDrawn(equipToType, false);
        UpdateFromMoveset();
    }

    public void SetAimAtkLockout(bool lockout)
    {
        aimAtkLockout = lockout;
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
        StopCoroutine("GradualResetMainRotation");
        RotateMainWeapon(0f);
    }

    public void ResetOffRotation()
    {
        StopCoroutine("GradualResetOffRotation");
        RotateOffWeapon(0f);
    }
    
    IEnumerator GradualResetMainRotation()
    {
        float angle;
        while (mainWeaponAngle != 0f)
        {
            angle = Mathf.MoveTowards(mainWeaponAngle, 0f, 45f * Time.deltaTime);
            RotateMainWeapon(angle);
            yield return null;
        }
    }

    IEnumerator GradualResetOffRotation()
    {
        float angle;
        while (offWeaponAngle != 0f)
        {
            angle = Mathf.MoveTowards(offWeaponAngle, 0f, 45f * Time.deltaTime);
            RotateOffWeapon(angle);
            yield return null;
        }
    }
    #endregion

    #region CARRYING

    public void Carry(Carryable c)
    {
        carryable = c;
        isCarrying = true;
        inventory.SetDrawn(Inventory.MainType, false);
        inventory.SetDrawn(Inventory.OffType, false);
        inventory.SetDrawn(Inventory.RangedType, false);
        UpdateFromMoveset();
        Physics.IgnoreCollision(this.GetComponent<Collider>(), c.GetComponent<Collider>());
        isDropping = false;
        
        
    }

    public void CarryWithAnimation(Carryable c)
    {
        carryable = c;
        Physics.IgnoreCollision(this.GetComponent<Collider>(), c.GetComponent<Collider>());
        StartCoroutine("StartCarryRoutine");
        isDropping = false;
    }


    IEnumerator StartCarryRoutine()
    {
        SheatheAll();
        Vector3 dir = carryable.transform.position - this.transform.position;
        dir.Normalize();
        dir.y = 0;
        this.transform.rotation = Quaternion.LookRotation(dir);
        yield return new WaitWhile(inventory.IsAnyWeaponDrawn);
        state.carry = animancer.Play(pickUpAnim);
        if (carryable.GetMass() > 10f)
        {
            state.carry = animancer.Play(slowPickUpAnim);
        }
        else
        {
            state.carry = animancer.Play(pickUpAnim);
        }
        state.carry.Events.OnEnd = () =>
        {
            isCarrying = true;
            carryable.StartCarry();
            _MoveOnEnd();
        };
    }
    public void StopCarrying()
    {
        StartCoroutine(DelayAllowingCollision(carryable));
        isCarrying = false;
        isDropping = false;
        carryable.StopCarry();
        if (animancer.Layers[HumanoidAnimLayers.UpperBody].IsPlayingClip(carryAnim.Clip))
        {
            animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
        }
    }

    public void StartThrow()
    {
        animancer.Layers[HumanoidAnimLayers.UpperBody].Weight = 0f;
        state.carry = animancer.Play(throwAnim);
        state.carry.Events.OnEnd = _MoveOnEnd;
        /*if (animancer.States.Current != state.carry)
        {
            Throw();
            state.carry = animancer.Play(throwAnim);
            state.carry.Events.OnEnd = _MoveOnEnd;
        }*/
    }

    public void Throw()
    {
        if (isCarrying)
        {
            StopCarrying();
            carryable.Throw(this.transform.forward * Mathf.Clamp(carryable.GetMass(),1f,throwMassMax) * throwForce + Vector3.up * Mathf.Clamp(carryable.GetMass(), 1f, throwMassMax) * throwForceUp);
            
        }
    }

    public void StartDrop()
    {
        animancer.Layers[HumanoidAnimLayers.UpperBody].Weight = 0f;
        state.carry = animancer.Play(dropAnim);
        state.carry.Events.OnEnd = _MoveOnEnd;
        state.carry.Events.OnEnd += () =>
        {
            carryable.StopMovement();
        };
        isDropping = true;
    }

    public void Drop()
    {
        if (isCarrying)
        {
            StopCarrying();
            carryable.StopMovement();
            //carryable.StopCarry();
            isDropping = false;
        }
    }
    IEnumerator DelayAllowingCollision(Carryable carryable)
    {
        yield return new WaitForSeconds(0.5f);
        if (!((isCarrying || animancer.States.Current == state.carry) && this.carryable == carryable))
        {
            Physics.IgnoreCollision(this.GetComponent<Collider>(), carryable.GetComponent<Collider>(), false);
        }
        
    }
    #endregion

    #region COMBAT

    // attacks
    #region attacks
    public void MainSlash()
    {
        if (hold && GetMoveset().powerSlash != null)
        {
            PowerSlash();
            return;
        }
        System.Action endAction = (GetMoveset().quickSlash1h is ComboAttack) ? _MoveOnEnd : _AttackEnd;
        state.attack = GetMoveset().quickSlash1h.ProcessAttack(this, out cancelTime, endAction);
        attackDecelReal = attackDecel;
        OnAttack.Invoke();
        hold = false;
    }

    public void MainThrust()
    {
        if (hold && GetMoveset().powerThrust != null)
        {
            PowerThrust();
            return;
        }

        System.Action endAction = (GetMoveset().quickThrust1h is ComboAttack) ? _MoveOnEnd : _AttackEnd;
        state.attack = GetMoveset().quickThrust1h.ProcessAttack(this, out cancelTime, endAction);
        attackDecelReal = attackDecel;
        OnAttack.Invoke();
        hold = false;
    }

    public void PowerSlash()
    {
        InputAttack attack = GetMoveset().powerSlash;
        if (attack is HoldAttack holdAttack)
        {
            hold = true;
            state.attack = animancer.Play(holdAttack.GetStartClip());
            state.attack.Events.OnEnd = () => { HoldSlashRelease(true); };
            SetCurrentDamage(attack.GetDamage());
        }
        else
        {
            hold = false;
            state.attack = GetMoveset().powerSlash.ProcessAttack(this, out cancelTime, _AttackEnd);
        }
        OnAttack.Invoke();
    }

    public void PowerThrust()
    {
        InputAttack attack = GetMoveset().powerThrust;
        if (attack is HoldAttack holdAttack)
        {
            hold = true;
            state.attack = animancer.Play(holdAttack.GetStartClip());
            state.attack.Events.OnEnd = () => { HoldThrustRelease(true); };
            SetCurrentDamage(attack.GetDamage());
        }
        else
        {
            hold = false;
            state.attack = GetMoveset().powerSlash.ProcessAttack(this, out cancelTime, _AttackEnd);
        }
        OnAttack.Invoke();
    }

    public void HoldSlashRelease(bool wasFullyCharged)
    {
        hold = false;
        if (GetMoveset().powerSlash is not HoldAttack holdAttack)
        {
            _AttackEnd();
            return;
        }
        else {
            state.attack = animancer.Play(holdAttack.GetClip());
            if (wasFullyCharged)
            {
                SetCurrentDamage(holdAttack.GetDamage());
            }
            else
            {
                SetCurrentDamage(holdAttack.GetHeldDamage());
            }
            state.attack.Events.OnEnd = _AttackEnd;
        }
    }

    public void HoldThrustRelease(bool wasFullyCharged)
    {
        hold = false;
        if (GetMoveset().powerThrust is not HoldAttack holdAttack)
        {
            _AttackEnd();
            return;
        }
        else
        {
            state.attack = animancer.Play(holdAttack.GetClip());
            if (!wasFullyCharged)
            {
                SetCurrentDamage(holdAttack.GetDamage());
            }
            else
            {
                SetCurrentDamage(holdAttack.GetHeldDamage());
            }
            state.attack.Events.OnEnd = _AttackEnd;
        }
    }

    public void CancelSlash()
    {
        state.attack = GetMoveset().quickSlash1h.ProcessAttack(this, out cancelTime, _AttackEnd);
        OnAttack.Invoke();
    }

    public void CancelThrust()
    {
        state.attack = GetMoveset().quickThrust1h.ProcessAttack(this, out cancelTime, _AttackEnd);
        OnAttack.Invoke();
    }
    public void DashSlash()
    {
        state.attack = GetMoveset().dashSlash.ProcessAttack(this, out cancelTime, _AttackEnd);
        attackDecelReal = dashAttackDecel;
        dashed = false;
        OnAttack.Invoke();
    }

    public void DashThrust()
    {
        state.attack = GetMoveset().dashThrust.ProcessAttack(this, out cancelTime, _AttackEnd);
        attackDecelReal = dashAttackDecel;
        dashed = false;
        OnAttack.Invoke();
    }
    

    public void BlockSlash()
    {
        System.Action _BlockAttackEnd = () => {
            if (blocking)
            {
                animancer.Layers[HumanoidAnimLayers.UpperBody].Play(blockAnim, 0.25f);
                animancer.Play(state.block, 0.25f);
            }
            else
            {
                _MoveOnEnd();
            }
        };
        if (inventory.IsOffDrawn() && GetMovesetOff().stanceSlash != null)
        {
            state.attack = GetMovesetOff().stanceSlash.ProcessAttack(this, out cancelTime, _BlockAttackEnd);
        }
        else if (inventory.IsMainDrawn() && GetMoveset().stanceSlash != null)
        {
            state.attack = GetMoveset().stanceSlash.ProcessAttack(this, out cancelTime, _BlockAttackEnd);
        }
        else
        {
            MainSlash();
        }
        OnAttack.Invoke();
    }
    public void BlockThrust()
    {
        System.Action _BlockAttackEnd = () => {
            if (blocking)
            {
                animancer.Layers[HumanoidAnimLayers.UpperBody].Play(blockAnim, 0.25f);
                animancer.Play(state.block, 0.25f);
            }
            else
            {
                _MoveOnEnd();
            }
        };
        if (inventory.IsOffDrawn() && GetMovesetOff().stanceThrust != null)
        {
            state.attack = GetMovesetOff().stanceSlash.ProcessAttack(this, out cancelTime, _BlockAttackEnd);
        }
        else if (inventory.IsMainDrawn() && GetMoveset().stanceThrust != null)
        {
            state.attack = GetMovesetOff().stanceSlash.ProcessAttack(this, out cancelTime, _BlockAttackEnd);
        }
        else
        {
            MainThrust();
        }
        OnAttack.Invoke();
    }
    public void RollSlash()
    {
        state.attack = GetMoveset().rollSlash.ProcessAttack(this, out cancelTime, _MoveOnEnd);
        attackDecelReal = dashAttackDecel;
        rollAnim.Events.OnEnd = () => { animancer.Play(state.move, 0.5f); };   
        dashed = false;
        OnAttack.Invoke();
    }

    public void RollThrust()
    {
        state.attack = GetMoveset().rollThrust.ProcessAttack(this, out cancelTime, _MoveOnEnd);
        attackDecelReal = dashAttackDecel;
        rollAnim.Events.OnEnd = () => { animancer.Play(state.move, 0.5f); };
        dashed = false;
        OnAttack.Invoke();
    }

    public void PlungeSlash()
    { 
        if (GetMoveset().plungeSlash is PhaseAttack phase)
        {
            ClipTransition clip = GetMoveset().plungeSlash.GetClip();
            state.attack = animancer.Play(clip);
            state.attack.Events.OnEnd = () => { state.attack = animancer.Play(phase.GetLoopPhaseClip(), 0.1f); };
 
            attackDecelReal = 0f;
            plungeEnd = phase.GetEndPhaseClip();
            plungeEnd.Events.OnEnd = () => { animancer.Play(state.move, 0.5f); };
        }
        plunge = true;
        SetCurrentDamage(GetMoveset().plungeSlash.GetDamage());
        OnAttack.Invoke();
    }

    public void PlungeThrust()
    {
        if (GetMoveset().plungeThrust is PhaseAttack phase)
        {
            ClipTransition clip = GetMoveset().plungeThrust.GetClip();
            state.attack = animancer.Play(clip);
            state.attack.Events.OnEnd = () => { state.attack = animancer.Play(phase.GetLoopPhaseClip(), 0.1f); };
            
            attackDecelReal = 0f;
            plungeEnd = phase.GetEndPhaseClip();
            plungeEnd.Events.OnEnd = () => { animancer.Play(state.move, 0.5f); };
        }
        plunge = true;
        SetCurrentDamage(GetMoveset().plungeThrust.GetDamage());
        OnAttack.Invoke();
    }

    public void Aim()
    {
        state.aim = animancer.Play(aimAnim);
        aimForwardVector = this.transform.forward;
        aimTime = 0f;
    }

    public void RealignToTarget()
    {
        if (this.GetCombatTarget() != null)
        {
            Vector3 dir = this.GetCombatTarget().transform.position - this.transform.position;
            dir.y = 0f;
            this.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }

    public void SheatheAll()
    {
        StartCoroutine("SheatheAllRoutine");
    }

    IEnumerator SheatheAllRoutine()
    {
        while (inventory.IsMainDrawn() || inventory.IsOffDrawn() || inventory.IsRangedDrawn())
        {
            if (animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying())
            {
                yield return new WaitWhile(() => { return !animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying(); });
            }
            else if (inventory.IsRangedDrawn())
            {
                TriggerSheath(false, inventory.GetRangedWeapon().RangedEquipSlot, Inventory.RangedType);
            }
            else if (inventory.IsMainDrawn())
            {
                TriggerSheath(false, inventory.GetMainWeapon().MainHandEquipSlot, Inventory.MainType);
            }
            else if (inventory.IsOffDrawn())
            {
                TriggerSheath(false, inventory.GetOffWeapon().OffHandEquipSlot, Inventory.OffType);
            }
            yield return new WaitForSeconds(0.1f);
        }
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
        bool main = (mainWeapon != null && mainWeapon is IHitboxHandler);
        bool off = (offHandWeapon != null && offHandWeapon is IHitboxHandler);
        bool ranged = (rangedWeapon != null && rangedWeapon is IHitboxHandler);
        if (active == 0)
        {
            if (main)
            {
                ((IHitboxHandler)mainWeapon).HitboxActive(false);
            }
            if (off)
            {
                ((IHitboxHandler)offHandWeapon).HitboxActive(false);
            }
            if (ranged)
            {
                ((IHitboxHandler)rangedWeapon).HitboxActive(false);
            }
            isHitboxActive = false;
            //SetAimAtkLockout(false);
        }
        else if (active == 1)
        {
            if (main)
            {
                ((IHitboxHandler)mainWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }
        else if (active == 2)
        {
            if (off)
            {
                ((IHitboxHandler)offHandWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }
        else if (active == 3)
        {
            if (main)
            {
                ((IHitboxHandler)mainWeapon).HitboxActive(true);
            }
            if (off)
            {
                ((IHitboxHandler)offHandWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }
        else if (active == 4)
        {
            if (ranged)
            {
                 ((IHitboxHandler)rangedWeapon).HitboxActive(true);
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
        if (damageKnockback == null)
        {
            currentDamage = new DamageKnockback();
            currentDamage.source = this.gameObject;
            throw new NullReferenceException("Damage Knockback data is null");
        }
        currentDamage = new DamageKnockback(damageKnockback);
    }

    public DamageKnockback GetCurrentDamage()
    {
        return currentDamage;
    }

    public override List<DamageResistance> GetBlockResistance()
    {
        List<DamageResistance> dr = new List<DamageResistance>();
        dr.AddRange(inventory.GetBlockResistance());
        return dr;
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
        HitboxActive(0);
        damageHandler.TakeDamage(damage);
    }

    public override void Die()
    {
        base.Die();
        dead = true;
        ProcessDeath();
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

    public override Vector3 GetLaunchVector(Vector3 origin)
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

            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 100f, LayerMask.GetMask("Terrain", "Actors", "Default", "Wall")) && !hit.transform.IsChildOf(this.transform.root))
            {
                aimPos = hit.point;
                
            }
            Debug.DrawLine(origin, aimPos, Color.red);
            return (aimPos - origin).normalized;
        }
        else
        {
            return this.transform.forward;
        }
    }


    public override bool ShouldCalcFireStrength()
    {
        return true;
    }
    private void OnAnimatorIK(int layerIndex)
    {
        //Vector3 initialThrustPos = this.transform.position + this.transform.up * thrustInitialHeight;
        Vector3 initialThrustPos = positionReference.Spine.position;
        float y = 0f;
        float h = 0f;
        if (GetCombatTarget() != null)
        {
            
            y = (GetCombatTarget().transform.position - initialThrustPos).y;
            Vector3 diff = (GetCombatTarget().transform.position - initialThrustPos);
            diff.y = 0;
            float xz = diff.magnitude;

            h = (2 * y) / xz;
            h = Mathf.Clamp(h, -thrustIKHeightRange, thrustIKHeightRange);
        }
        //Vector3 ikThrustVector = initialThrustPos + this.transform.forward * 2f + (thrustIKValue * this.transform.up * thrustIKHeightRange);
        float forwardOffset = 2f;
        if (inventory.IsMainEquipped() && inventory.GetMainWeapon() is BladeWeapon mwep)
        {
            forwardOffset = mwep.GetLength();
        }
        Vector3 ikThrustVector = initialThrustPos + this.transform.forward * forwardOffset + (h * this.transform.up);
        Debug.DrawLine(initialThrustPos, ikThrustVector, Color.red);
        if (IsAttacking())
        {
            
            if (currentDamage != null && currentDamage.isThrust && animancer.States.Current == state.attack)
            {

                animancer.Animator.SetIKPosition(AvatarIKGoal.RightHand, ikThrustVector);
                animancer.Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, thrustIKWeight * thrustIKMultiplier);
            }
            else
            {
                animancer.Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
            }
            float weightTarget = (isHitboxActive) ? 1f : 0f;
            thrustIKWeight = Mathf.MoveTowards(thrustIKWeight, weightTarget, thrustIKAdjustSpeed * Time.deltaTime);
        }
        else if (IsAiming() && inventory.IsRangedDrawn())
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
            thrustIKWeight = 0f;
        }
        
    }

    public override void SetLastBlockpoint(Vector3 point)
    {
        EquippableWeapon weapon = inventory.GetBlockWeapon();
        if (weapon == null || weapon.GetModel() == null)
        {
            base.SetLastBlockpoint(point);
        }
        else
        {
            Bounds bounds = weapon.GetBlockBounds();
            if (bounds.extents.magnitude <= 0)
            {
                base.SetLastBlockpoint(point);
            }
            else
            {
                lastBlockPoint = bounds.ClosestPoint(point);
            }
        }
        
    }
    #endregion

    #region State Checks

    public override bool IsAlive()
    {
        return !dead;
    }
    public bool IsSwimming()
    {
        return animancer.States.Current == state.swim;
    }
    public bool IsMoving()
    {
        return animancer.States.Current == state.move;
    }
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

    public override bool ShouldDustOnStep()
    {
        return animancer.States.Current == state.sprint || animancer.States.Current == state.dash;
    }
    public override bool IsGrounded()
    {
        return GetGrounded();
    }
    public override bool IsDodging()
    {
        return animancer.States.Current == state.roll;
    }
    public override bool IsHitboxActive()
    {
        return isHitboxActive;
    }
    public bool IsTwoHanding()
    {
        return inventory.IsMainDrawn() && inventory.GetMainWeapon().TwoHandOnly();
    }

    public override bool IsAttacking()
    {
        return animancer.States.Current == state.attack;
    }
    public override bool IsBlocking()
    {
        return animancer.States.Current == state.block || animancer.States.Current == damageHandler.block;
    }
   
    public bool IsInDialogue()
    {
        return animancer.States.Current == state.dialogue;
    }
    public bool ShouldShowTargetIcon()
    {
        return this.GetCombatTarget() != null && !IsInDialogue();
    }

    public bool ShouldSlowTime()
    {
        //return this.IsAiming() && camState == CameraState.Aim && IsAttackHeld();
        return this.IsAiming() && !GetGrounded();
    }
    public override void SetToIdle()
    {
        animancer.Play(state.move);
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
        Interactable lastInteractable = highlightedInteractable;
        highlightedInteractable = null;
        float leadDist = Mathf.Infinity;

        foreach (Interactable interactable in interactables)
        {
            if (interactable == null || !interactable.canInteract) continue;
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
        if (highlightedInteractable != lastInteractable)
        {
            onNewCurrentInteractable.Invoke();
        }
        return highlightedInteractable;
    }

    private void OnInteract()
    {
        if (!CanPlayerInput()) return;
        Interactable interactable = GetHighlightedInteractable();
        if (interactable != null)
        {
            interactable.Interact(this);
        }

    }

    public void StartDialogue()
    {
        state.dialogue = animancer.Play(idleAnim);
    }

    public void StopDialogue()
    {
        this.SetCombatTarget(null);
        animancer.Play(state.move);
    }
    #endregion

    public bool GetGrounded()
    {
        // return cc.isGrounded;
        Collider c = this.GetComponent<Collider>();
        Vector3 bottom = c.bounds.center + c.bounds.extents.y * Vector3.down;
        Debug.DrawLine(bottom, bottom + Vector3.down * 0.2f, Color.red);
        bool didHit = Physics.Raycast(bottom, Vector3.down, out RaycastHit hit, 0.2f, LayerMask.GetMask("Terrain"));
        if (didHit)
        {
            lastPhysicsMaterial = hit.collider.sharedMaterial;
        }
        return didHit || cc.isGrounded;
    }

    public void HitWall()
    {
        Debug.Log("wall hit");
    }
}
