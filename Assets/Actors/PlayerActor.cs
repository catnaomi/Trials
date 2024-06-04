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
    [HideInInspector]public CharacterController cc;
    public bool inStateMove;
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
    CinemachineFreeLook free_freeLook;
    GameObject consumableModel;
    public bool shouldLookAtCamera;
    public float[] headPointWeights = { 1f, 0.1f, 1f, 0f, 0.7f };
    Vector3 lastCameraForward = Vector3.forward;
    Vector3 camForward;
    Vector3 lastCameraRight = Vector3.right;
    Vector3 camRight;
    CinemachineBrain cinemachineBrain;
    [Header("Interaction & Dialogue")]
    public bool isMenuOpen;
    List<Interactable> interactables;
    public Interactable highlightedInteractable;
    float externalSourceClock;
    public float maxExternalSourceTime = 30000f;
    [Header("Respawning")]
    public Vector3 lastSafePoint;
    public float safePointClock;
    public float rewindDistanceThreshold = 2f;
    public float rewingTimeoutDuration = 3f;
    bool rewindingToSafePoint;
    bool spawned;
    [Header("Movement")]
    public float walkSpeedMax = 5f;
    public AnimationCurve walkSpeedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public float sprintSpeed = 10f;
    public float sprintTurnSpeed = 90f;
    public float walkAccel = 25f;
    public float blockHitAccel = 5f;
    public float walkTurnSpeed = 1080f;
    [Space(5)]
    public float strafeSpeed = 2.5f;
    public float weaponsDrawnSpeed = 5f;
    public float weaponDashSpeed = 2f;
    public float weaponDashMinDist = 1f;
    public float weaponDashMaxDist = 5f;
    [Space(5)]
    public float drinkSpeed = 2f;
    [Space(5)]
    public float airAccel = 1f;
    public float airTurnSpeed = 45f;
    public float maxAirSpeed = 8f;
    public float hardLandAccel = 2.5f;
    public float softLandAccel = 2.5f;
    public float skidAngle = 160f;
    public float skidDecel = 10f;
    public float slideOffSpeed = 1f;
    public float slideSpeed = 4f;
    public float slideAccel = 10f;
    Vector3 lastSprintForward;
    public float gravity = 9.81f;
    public float terminalVel = 70f;
    public float fallBufferTime = 0.25f;
    public float hardLandingTime = 2f;
    public float softLandingTime = 1f;
    public float softLandingTransitionTime = 0.05f;
    public float friction = 1f;
    public float groundFriction = 1f;
    public float waterFriction = 1f;
    public float slideFriction = 0.5f;
    [Space(5)]
    public float rollSpeed = 5f;
    public float dodgeJumpVel = 5f;
    public float dodgeJumpSpeed = 5f;
    public float jumpVel = 10f;
    public float attackJumpVel = 10f;
    public float backflipSpeed = 5f;
    [Space(5)]
    [ReadOnly] public bool isGrounded;
    [ReadOnly] public float airTime = 0f;
    public float groundBias = 0f;
    bool isGroundedLockout;
    bool isCaughtOnEdge;
    bool withinBias;
    float biasHeight;
    public float jumpBuffer = 1f;
    public float minAirTimeToAct = 0.5f;
    bool didJump;
    bool didAirJump;
    float lastAirTime;
    float landTime = 0f;
    [SerializeField, ReadOnly] float speed;
    bool dashed;
    bool jump;
    bool blocking;
    bool targeting;
    bool aiming;
    float parryTime;
    public bool sliding;
    bool wasSlidingIK;
    bool wasSlidingUpdate;
    [ReadOnly]public float slopeAngle;
    public float maxSlideAngle = 60f;
    public float minSlideAngle = 30f;

    [ReadOnly] public Vector3 groundNormal;
    [ReadOnly] public Vector3 groundPoint;
    RaycastHit rayHit;
    ControllerColliderHit ccHit;
    ControllerColliderHit lastCCHit;
    Vector3 targetDirection;
    Vector3 headPoint;
    Vector3 smoothedHeadPoint;
    Vector3 ccHitNormal;
    Vector3 warpDelta;

    Vector3 animatorVelocity;
    Vector3 animatorDelta;
    bool aimAtkLockout;
    public float headPointSpeed = 25f;
    public PhysicMaterial lastPhysicsMaterial;
    public bool lastGroundWasStatic;
    [Space(5)]
    public float attackDecel = 25f;
    public float dashAttackDecel = 10f;
    float attackDecelReal;
    [ReadOnly] public float walkAccelReal;
    [ReadOnly] public bool sprinting;
    [ReadOnly] public bool shouldDodge;
    [ReadOnly] public bool secondary;
    public Vector2 dodgeDirection;
    [Space(10)]
    public bool disablePhysics;
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

    Vector3 climbSnapPoint;
    bool allowClimb = true;
    bool allowLadderFinish = true;
    bool railSnap;
    public Collider hangCollider;
    public float climbSpeed = 1f;
    public float climbSnapSpeed = 5f;
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
    public float waterDismountSpeed = 5f;
    public bool wading;
    public float wadingPercent;
    [Header("Combat")]
    public bool attack;
    bool slash;
    bool thrust;
    bool plunge;
    bool hold;
    int invSlot = -1;
    bool hasTypedBlocks;
    bool wasBlockingLastFrame;
    int lastTypedBlockParam;
    public float blockShiftSpeed = 4f;
    public float blockShiftBufferWindow = 1f;
    public float blockShiftUpperWeight = 0.1f;
    public UnityEvent OnBlockTypeChange;
    ClipTransition plungeEnd;
    float cancelTime;
    [SerializeField, ReadOnly] AttackCancelAction cancelAction;
    public float blockSpeed = 2.5f;
    float aimTimer;
    bool isHitboxActive;
    bool isSheathing;
    bool resurrecting;
    [Space(5)]
    public float horizontalAimSpeed = 90f;
    public float aimSpeed = 2.5f;
    Vector3 aimForwardVector;
    public float aimCancelTime = 2f;
    public float aimTime;
    public float aimStartTime = 0.25f;
    Vector3 lastLaunchVector;
    public Vector3 smoothLaunchVector = Vector3.forward;
    public float launchVectorSmoothSpeed = 120f;

    [Range(-1f,1f)]
    public float thrustIKValue;
    public float thrustIKWeight;
    public float thrustIKMultiplier = 1f;
    public float thrustIKAdjustSpeed;
    public float thrustInitialHeight;
    public float thrustIKHeightRange;

    float holdAttackClock;
    float holdAttackMin;
    [Space(20)]
    public float standingColliderRadius;
    public float attackingColliderRadius;
    [Space(20)]
    public UnityEvent OnParryStart;
    public UnityEvent OnParryThrustStart;
    public UnityEvent OnParrySlashStart;
    public UnityEvent OnTypedBlockSuccess;
    public UnityEvent OnHitWeakness;
    public UnityEvent OnJumpStart;
    [Header("Control Vectors")]
    [ReadOnly] public Vector3 moveDirection;
    [ReadOnly] public Vector3 lookDirection;
    [ReadOnly] public Vector3 stickDirection;
    [Header("Animancer")]
    [Header("Stance & Movement")]
    public MixerTransition2DAsset unarmedStance;
    public MixerTransition2DAsset armedStance;
    public MixerTransition2DAsset blockingStance;
    public MixerTransition2DAsset bowWalkStance;
    public MixerTransition2DAsset bowAimStance;
    [Header("Default Anims")]
    public AnimationClip idleAnim;
    public MixerTransition2DAsset aimAnimDefault;
    public MixerTransition2DAsset strafeAnimDefault;
    [Space(10)]
    public ClipTransition dashAnim;
    public ClipTransition sprintAnim;
    ClipTransition currentSprintAnim;
    public ClipTransition skidAnim;
    public ClipTransition fallAnim;
    public ClipTransition landSoftAnim;
    public ClipTransition landHardAnim;
    [Space(10)]
    public ClipTransition rollAnim;
    public ClipTransition dodgeJumpForward;
    public ClipTransition dodgeJumpBack;
    public ClipTransition dodgeJumpLeft;
    public ClipTransition dodgeJumpRight;
    [Space(5)]
    public ClipTransition standJumpAnim;
    public ClipTransition runJumpAnim;
    public ClipTransition backflipAnim;
    [Space(5)]
    public MixerTransition2D ledgeHang;
    public ClipTransition ledgeClimb;
    public ClipTransition ledgeStart;
    public ClipTransition ladderClimb;
    public ClipTransition ladderClimbUp;
    public MixerTransition2D railWalk;
    [Space(5)]
    public LinearMixerTransition swimAnim;
    public ClipTransition swimEnd;
    public ClipTransition swimStart;
    public ClipTransition swimDive;
    public ClipTransition swimDiveHurt;
    public ClipTransition swimDiveRise;
    [Space(5)]
    public ClipTransition resurrectFaceUp;
    public ClipTransition resurrectFaceDown;
    [Space(5)]
    public AnimatedFloat bowBend;
    MixerTransition2DAsset aimAnim;
    [Space(5)]
    [ReadOnly] public BilayerMixer2DAsset bilayerMove;
    [Space(10)]
    public StanceHandler primaryStance;
    [Space(5)]
    public StanceHandler secondaryStance;
    [Header("Damage Anims")]
    public DamageAnims damageAnim;
    HumanoidDamageHandler damageHandler;
    MixerTransition2D blockMove;
    ClipTransition blockAnimStart;
    ClipTransition blockAnim;
    ClipTransition blockAnimSlash;
    ClipTransition blockAnimThrust;
    ClipTransition blockStagger;
    AnimState state;
    public AimAttack.AimState astate;

    public static PlayerActor player;
    public UnityEvent OnHitboxActive;
    [Header("Carry Anims")]
    public ClipTransition pickUpAnim;
    public ClipTransition slowPickUpAnim;
    public ClipTransition carryAnim;
    public ClipTransition throwAnim;
    public ClipTransition dropAnim;
    [Header("Movesets")]
    public Moveset runtimeMoveset;
    public Moveset runtimeOffMoveset;
    [Header("Controls")]
    public float inputBufferTimeoutTime;
    [SerializeField]InputBuffer buffer;
    public UnityEvent onControlsChanged;
    public UnityEvent onNewCurrentInteractable;
    [Header("Debug")]
    public bool isGoddess; // god mode
    public bool isBird; // fly mode
    public float flyFastVelocity;
    public float flySlowVelocity;

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
        public AnimancerState backflip;
        public AnimancerState climb;
        public AnimancerState swim;
        public AnimancerState attack;
        public AnimancerState parry;
        public AnimancerState holdAttack;
        public AnimancerState block;
        public AnimancerState aim;
        public AnimancerState dialogue;
        public AnimancerState externalSource;
        public AnimancerState carry;
        public AnimancerState resurrect;
        public MixerState<Vector2> primaryStance;
        public MixerState<Vector2> secondaryStance;
        public MixerState<float> upperBlock;

        public AnimancerState consume;
        public MixerState<float> drink;
        public AnimancerState drinkUpper;
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
    }

    public override void ActorStart()
    {
        base.ActorStart();
        cc = this.GetComponent<CharacterController>();
        defaultRadius = cc.radius;
        animancer = this.GetComponent<AnimancerComponent>();
        interactables = new List<Interactable>();

        state = new AnimState();
        state.move = (MixerState)animancer.States.GetOrCreate(unarmedStance);
        state.attack = animancer.States.GetOrCreate(rollAnim);
        state.block = animancer.States.GetOrCreate(strafeAnimDefault);
        state.fall = animancer.States.GetOrCreate(fallAnim);
        animancer.Play(state.move);

        SetupInputListeners();

        currentSprintAnim = sprintAnim;

        skidAnim.Events.OnEnd += () => { state.sprint = animancer.Play(sprintAnim); };
        //landAnim.Events.OnEnd += () => { animancer.Play(state.move, 1f); };
        rollAnim.Events.OnEnd += () => {
            animancer.Play(state.move, 0.5f);
            EnableCloth();
        };
        standJumpAnim.Events.OnEnd += () => { state.fall = animancer.Play(fallAnim); };
        runJumpAnim.Events.OnEnd += () => { state.fall = animancer.Play(fallAnim); };
        backflipAnim.Events.OnEnd += () => { state.fall = animancer.Play(fallAnim); };
        swimStart.Events.OnEnd += () => {
            state.swim = animancer.Play(swimAnim);
        };
        walkAccelReal = walkAccel;

        damageHandler = new HumanoidDamageHandler(this, damageAnim, animancer);
        damageHandler.SetEndAction(_OnHurtEnd);
        damageHandler.SetBlockEndAction(() => { animancer.Play(state.block, 0.5f); });

        buffer = new InputBuffer();

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
        animancer.Layers[HumanoidAnimLayers.UpperBody].SetMask(positionReference.upperBodyMask);
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
        OnHitboxActive.AddListener(RealignToTarget);
        onControlsChanged.AddListener(HandleCinemachine);
        OnAttack.AddListener(ProcessWeaponDash);
        OnAttack.AddListener(ResetAttackCancelAction);

        if (SceneLoader.IsSceneLoaderActive())
        {
            SceneLoader.GetOnActiveSceneChange().AddListener(SetNewSafePoint);
        }

        bowBend = new AnimatedFloat(animancer, "_BowBend");
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
    }


    public override void ActorPostUpdate()
    {
        if (dead) return;
        if (!spawned)
        {
            TryFindSpawnPoint();
        }
        inStateMove = (animancer.States.Current == state.move);
        //isGrounded = GetGrounded(out rayHit, out sphereHit, out nonterrainHit);
        if (isGrounded)
        {
            didJump = false;
            if (IsMoving())
            {
                didAirJump = false;
            }
        }
        moveSmoothed = Vector2.MoveTowards(moveSmoothed, move, Time.deltaTime);
        if (!cinemachineBrain.IsBlending)
        {
            lastCameraForward = Camera.main.transform.forward;
            lastCameraRight = Camera.main.transform.right;
        }
        camForward = lastCameraForward;
        camForward.y = 0f;
        camRight = lastCameraRight;
        camRight.y = 0f;
        stickDirection = Vector3.zero;
        lookDirection = this.transform.forward;
        moveDirection = Vector3.zero;
        bool applyMove = false;
        bool allowEndBuffer = false;
        slopeAngle = -1f;
        groundNormal = Vector3.up;
        if (rayHit.collider != null)
        {
            slopeAngle = Vector3.Angle(Vector3.up, rayHit.normal);
            groundNormal = rayHit.normal;
            groundPoint = rayHit.point;
        }
        else if (ccHit != null && ccHit.collider != null)
        {
            slopeAngle = Vector3.Angle(Vector3.up, ccHit.normal);
            groundNormal = ccHit.normal;
            groundPoint =  ccHit.point;
        }
        isCaughtOnEdge = (rayHit.collider == null && ccHit?.collider != null);
        stickDirection = camForward * move.y + camRight * move.x;

        PollInputs();
        ProcessBlock();
        ProcessRanged();
        //aiming = IsAimHeld();
        if (aiming)
        {
            aimTimer = aimCancelTime;
        }

        if (lastPhysicsMaterial != null)
        {
            groundFriction = lastPhysicsMaterial.dynamicFriction;
        }
        else
        {
            groundFriction = 1f;
        }
        friction = groundFriction;

        //GetHeadPoint();

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
            float speedMax = walkSpeedMax;
            if (inventory.IsAnyWeaponDrawn())
            {
                speedMax = Mathf.Min(speedMax, weaponsDrawnSpeed);
            }
            if (camState == CameraState.Lock)
            {
                speedMax = Mathf.Min(speedMax, strafeSpeed);
            }
            if (wading)
            {
                speedMax = Mathf.Lerp(speedMax, wadingSpeed, wadingPercent);
            }
            speed = Mathf.MoveTowards(speed, walkSpeedCurve.Evaluate(move.magnitude) * speedMax, walkAccelReal * Time.deltaTime);
            if (camState == CameraState.Free)
            {
                if (targeting && look.magnitude > 0.01f)
                {
                    lookDirection = Camera.main.transform.forward;
                    lookDirection.y = 0f;
                }
                else if (!targeting && stickDirection.magnitude > 0)
                {
                    lookDirection = Vector3.RotateTowards(lookDirection, stickDirection.normalized, Mathf.Deg2Rad * walkTurnSpeed * Time.deltaTime, 1f);
                }
                else
                {
                    lookDirection = this.transform.forward;
                }
                moveDirection = stickDirection;
            }
            else if (camState == CameraState.Aim)
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
            if (jump)
            {
                StandingJump();
            }
            else if (!isGrounded && lastAirTime > fallBufferTime)
            {
                state.fall = animancer.Play(fallAnim);
            }
            else if (sprinting && move.magnitude >= 0.5f && landTime >= 1f)
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
            else if (shouldDodge && isCarrying)
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
            else if (CheckWater())
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
            if (blocking && !isCarrying)
            {
                //((MixerState)state.block).ChildStates[0].Clip = blockAnimStart.Clip;


                if (blockWeapon != null)
                {
                    int itemSlot = inventory.GetItemEquipType(blockWeapon);
                    if ((itemSlot == Inventory.MainType && !inventory.IsMainDrawn()) || (itemSlot == Inventory.OffType && !inventory.IsOffDrawn()))
                    {
                        inventory.SetDrawn(inventory.GetItemEquipType(blockWeapon), true);
                        UpdateFromMoveset();
                    }
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
                attack = false;
                if (slash)
                {
                    slash = false;
                    StartThrow();
                    buffer.ClearInput(InputBuffer.Inputs.Slash);
                }
                if (thrust)
                {
                    thrust = false;
                    StartDrop();
                    buffer.ClearInput(InputBuffer.Inputs.Thrust);
                }
                slash = false;
                thrust = false;
            }
            else if (attack && !animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying())
            {
                BasicAttack();
            }
            else if (invSlot > -1) // pressed a button to use consumable
            {
                inventory.InputOnSlot(invSlot);
                invSlot = -1;
            }
            /*
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
            }*/
            animancer.Layers[0].ApplyAnimatorIK = true;
            applyMove = true;
            plunge = false;
        }
        #endregion
        #region drink
        else if (animancer.States.Current == state.drink)
        {
            float speedMax = drinkSpeed;
            bool exitEarly = false;
            if (stickDirection.magnitude > 0)
            {
                lookDirection = Vector3.RotateTowards(lookDirection, stickDirection.normalized, Mathf.Deg2Rad * walkTurnSpeed * Time.deltaTime, 1f);
            }
            moveDirection = stickDirection;
            speed = Mathf.MoveTowards(speed, walkSpeedCurve.Evaluate(move.magnitude) * speedMax, walkAccelReal * Time.deltaTime);
            if (!isGrounded && lastAirTime > fallBufferTime)
            {
                state.fall = animancer.Play(fallAnim);
                exitEarly = true;
            }
            else if (CheckWater())
            {
                state.swim = animancer.Play(swimStart, 0.25f);
                this.gameObject.SendMessage("SplashBig");
                exitEarly = true;
            }
            if (exitEarly)
            {
                animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
            }

            applyMove = true;
        }
        #endregion
        #region block
        else if (animancer.States.Current == state.block)
        {
            bool stopBlock = false;
            speed = Mathf.MoveTowards(speed, walkSpeedCurve.Evaluate(move.magnitude) * blockSpeed, walkAccelReal * Time.deltaTime);
            if (camState == CameraState.Free)
            {

                moveDirection = stickDirection;
                if (targeting && look.magnitude > 0.01f)
                {
                    lookDirection = Camera.main.transform.forward;
                    lookDirection.y = 0f;
                }
                else if (!targeting && stickDirection.magnitude > 0)
                {
                    lookDirection = stickDirection;
                }
                else
                {
                    lookDirection = this.transform.forward;
                }

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

            if (hasTypedBlocks)
            {
                if (animancer.Layers[HumanoidAnimLayers.BlockBlend].CurrentState != state.upperBlock)
                {
                    animancer.Layers[HumanoidAnimLayers.BlockBlend].Play(state.upperBlock);
                }

                int param = 0 + (IsSlashHeld() ? DamageKnockback.SLASH_INT : 0) + (IsThrustHeld() ? DamageKnockback.THRUST_INT : 0);
                if (param != lastTypedBlockParam)
                {
                    lastTypedBlockParam = param;
                    OnBlockTypeChange.Invoke();
                }
                state.upperBlock.Parameter = Mathf.MoveTowards(state.upperBlock.Parameter, (float)param, blockShiftSpeed * Time.deltaTime);

                animancer.Layers[HumanoidAnimLayers.BlockBlend].SetMask((stickDirection.magnitude > 0) ? positionReference.upperBodyMask : positionReference.fullBodyMask);
                float blendWeight = animancer.Layers[HumanoidAnimLayers.BlockBlend].Weight;
                float targetWeight = 0f;
                if (stickDirection.magnitude > 0)
                {
                    if (param == 0)
                    {
                        targetWeight = 0;
                    }
                    else
                    {
                        targetWeight = blockShiftUpperWeight;
                    }
                }
                else
                {
                    targetWeight = 1;
                }
                animancer.Layers[HumanoidAnimLayers.BlockBlend].SetWeight(Mathf.MoveTowards(blendWeight, targetWeight, Time.deltaTime * 10));
            }

            if (!isGrounded && lastAirTime > fallBufferTime)
            {
                state.fall = animancer.Play(fallAnim);
                stopBlock = true;
            }

            if (jump)
            {
                /*
                float y = Vector3.Dot(stickDirection, this.transform.forward);
                float x = Vector3.Dot(stickDirection, this.transform.right);
                bool north = y > 0.25f && Mathf.Abs(y) > Mathf.Abs(x);
                bool south = false;// y < 0.25f && Mathf.Abs(y) > Mathf.Abs(x);
                bool east = false;// x > 0.25f && Mathf.Abs(x) > Mathf.Abs(y);
                bool west = false;// x < 0.25f && Mathf.Abs(x) > Mathf.Abs(y);

                jump = false;

                if (north)
                {
                    state.jump = animancer.Play(runJumpAnim);
                    //dodgeDirection = this.transform.forward;
                    moveDirection = this.transform.forward;
                }
                else if (south)
                {
                    state.roll = animancer.Play(dodgeJumpBack);
                    state.roll.Events.OnEnd = _OnDodgeEnd;
                    lookDirection = Quaternion.FromToRotation(-this.transform.forward, stickDirection) * this.transform.forward;
                    moveDirection = stickDirection;
                    dodgeDirection = new Vector2(0, -1);
                    ApplyDodgeJump();
                }
                else if (east)
                {
                    state.roll = animancer.Play(dodgeJumpRight);
                    state.roll.Events.OnEnd = _OnDodgeEnd;
                    lookDirection = Quaternion.FromToRotation(this.transform.right, stickDirection) * this.transform.forward;
                    moveDirection = stickDirection;
                    dodgeDirection = new Vector2(1 , 0);
                    ApplyDodgeJump();
                }
                else if (west)
                {
                    state.roll = animancer.Play(dodgeJumpLeft);
                    state.roll.Events.OnEnd = _OnDodgeEnd;
                    lookDirection = Quaternion.FromToRotation(-this.transform.right, stickDirection) * this.transform.forward;
                    moveDirection = stickDirection;
                    dodgeDirection = new Vector2(-1, 0);
                    ApplyDodgeJump();
                }
                else
                {
                    state.jump = animancer.Play(standJumpAnim);
                }

                //state.jump = animancer.Play(backflipAnim);
                //moveDirection = this.transform.forward;
                //speed = -blockSpeed;
                */
                StandingJump();
                stopBlock = true;
            }

            if (attack)
            {
                if (!hasTypedBlocks)
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
                            BlockSlash();
                            attackDecelReal = attackDecel;
                        }
                        else if (thrust)
                        {
                            BlockThrust();
                            attackDecelReal = attackDecel;
                        }
                    }
                    stopBlock = true;
                }
                attack = false;
                slash = false;
                thrust = false;
            }
            else if (aiming)
            {
                Aim();
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
            if (stopBlock && blockAnim != null)
            {
                if (animancer.Layers[HumanoidAnimLayers.BlockBlend].IsPlayingClip(blockAnim.Clip) || animancer.Layers[HumanoidAnimLayers.BlockBlend].IsPlayingClip(blockAnimStart.Clip) || animancer.Layers[HumanoidAnimLayers.BlockBlend].CurrentState == state.upperBlock)
                {
                    animancer.Layers[HumanoidAnimLayers.BlockBlend].Stop();
                }
            }
            animancer.Layers[0].ApplyAnimatorIK = true;
            applyMove = true;
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
                lookDirection = stickDirection.normalized;
                state.roll = animancer.Play(rollAnim);
            }
            else if (jump)
            {
                jump = false;
                if (stickDirection.magnitude > 0)
                {
                    lookDirection = stickDirection.normalized;
                    moveDirection = stickDirection.normalized;
                    //xzVel = xzVel.magnitude * stickDirection.normalized;
                }
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
            else if (stickDirection.magnitude > 0)
            {
                lookDirection = Vector3.RotateTowards(lookDirection, stickDirection.normalized, Mathf.Deg2Rad * sprintTurnSpeed * Time.deltaTime, 1f);
                moveDirection = lookDirection;
            }
            else
            {
                lookDirection = moveDirection = this.transform.forward;
            }
            if (!isGrounded && lastAirTime > fallBufferTime)
            {
                state.fall = animancer.Play(fallAnim);
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
            applyMove = true;
        }
        #endregion

        #region sprint
        else if (animancer.States.Current == state.sprint)
        {
            speed = sprintSpeed;
            if (shouldDodge)
            {
                state.roll = animancer.Play(rollAnim);
                lookDirection = stickDirection.normalized;
            }
            else if (jump)
            {
                jump = false;
                if (stickDirection.magnitude > 0)
                {
                    lookDirection = stickDirection.normalized;
                    moveDirection = stickDirection.normalized;
                    //xzVel = xzVel.magnitude * stickDirection.normalized;
                }

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
            if (!isGrounded && lastAirTime > fallBufferTime)
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
            applyMove = true;
        }
        #endregion

        #region skid
        else if (animancer.States.Current == state.skid)
        {
            speed = Mathf.MoveTowards(speed, 0f, skidDecel * Time.deltaTime);
            moveDirection = lastSprintForward;
            if (jump)
            {
                jump = false;
                if (stickDirection.magnitude > 0)
                {
                    lookDirection = stickDirection.normalized;
                    moveDirection = stickDirection.normalized;
                    //xzVel = xzVel.magnitude * stickDirection.normalized;
                }
                state.jump = animancer.Play(runJumpAnim);
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
            applyMove = true;
        }
        #endregion

        #region fall
        else if (animancer.States.Current == state.fall)
        {
            //speed = 0f;
            sliding = false;
            //airTime += Time.deltaTime;
            if (isGrounded && yVel <= 0)
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
                    VerifyAccelerationAfterDelay();
                }
                else if (lastAirTime >= softLandingTime)
                {
                    AnimancerState land = animancer.Play(landSoftAnim);
                    //AnimancerState land = state.move.ChildStates[0];
                    //land.Clip = landSoftAnim;

                    //animancer.Layers[HumanoidAnimLayers.Flinch].Play(landSoftAnim)
                    //    .Events.OnEnd = () => { animancer.Layers[HumanoidAnimLayers.Flinch].Stop(); };
                    walkAccelReal = softLandAccel;
                    //land.Events.OnEnd = _OnLandEnd;
                    speed = 0f;

                    land.Events.OnEnd = () => { animancer.Play(state.move, softLandingTransitionTime); };
                    //animancer.Play(state.move, softLandingTransitionTime);
                    VerifyAccelerationAfterDelay();
                }
                else
                {
                    this.gameObject.SendMessage("Thud");
                    animancer.Play(state.move, 0.05f);
                }

            }
            else
            {
                if (!isGrounded && isCaughtOnEdge)
                {
                    Vector3 planarNormal = Vector3.ProjectOnPlane(groundNormal, Vector3.up).normalized;
                    //Physics.Raycast(bottom, Vector3.down, out groundRayHit, 0.2f, LayerMask.GetMask("Terrain"));
                    Vector3 dir = Vector3.forward;
                    if (false)//slopeAngle > (!wasSlidingUpdate ? cc.slopeLimit : minSlideAngle) && slopeAngle < maxSlideAngle) {
                    {
                        sliding = true;
                        Vector3 horizTangent = Vector3.Cross(groundNormal, Vector3.down);
                        Vector3 downSlope = Vector3.Cross(horizTangent, groundNormal);
                        dir = downSlope;
                    }
                    else if (slopeAngle > maxSlideAngle && slopeAngle < 90f)
                    {
                        dir = planarNormal;
                    }
                    else
                    {
                        dir = this.transform.position - ccHit.point;
                    }

                    if (sliding)
                    {
                        xzVel = Vector3.MoveTowards(xzVel, dir.normalized * slideSpeed, slideAccel * Time.deltaTime);
                        xzVel.Scale(new Vector3(1f, 0f, 1f));
                        yVel = Mathf.MoveTowards(yVel, dir.normalized.y * slideSpeed, slideAccel * Time.deltaTime);
                    }
                    else
                    {
                        dir.y = 0f;
                        if (planarNormal != Vector3.zero && Vector3.Dot(xzVel, planarNormal) < 0)
                        {
                            Vector3 remove = Vector3.Project(xzVel, -planarNormal);
                            xzVel -= remove;
                        }
                        Vector3 stickAddition = Vector3.zero;
                        if (stickDirection.sqrMagnitude > 0)
                        {
                            Vector3 horizTangent = Vector3.Cross(groundNormal, Vector3.down);
                            stickAddition = Vector3.Project(stickDirection, horizTangent);
                            Debug.DrawRay(this.transform.position, stickAddition * 5f, Color.green);
                        }
                        xzVel = Vector3.MoveTowards(xzVel, (dir.normalized * slideOffSpeed) + (stickAddition * maxAirSpeed), slideAccel * Time.deltaTime);
                    }


                    Debug.DrawRay(this.transform.position, dir * 5f, Color.magenta);
                }

                if (stickDirection.sqrMagnitude > 0)
                {
                    if (!sliding)
                    {
                        if (xzVel.magnitude > walkSpeedMax)
                        {
                            Vector3 accelVector = airAccel * stickDirection;
                            if (Vector3.Dot(accelVector, xzVel.normalized) >= 0)
                            {
                                Vector3 onNormalPortion = Vector3.Project(accelVector, xzVel.normalized);
                                Vector3 offNormal = accelVector - onNormalPortion;

                                xzVel += offNormal * Time.deltaTime;
                            }
                            else
                            {
                                xzVel += airAccel * Time.deltaTime * stickDirection;
                            }
                        }
                        else
                        {
                            xzVel += airAccel * Time.deltaTime * stickDirection;
                        }
                        xzVel = Vector3.ClampMagnitude(xzVel, maxAirSpeed);
                        lookDirection = Vector3.RotateTowards(lookDirection, stickDirection.normalized, Mathf.Deg2Rad * airTurnSpeed * Time.deltaTime, 1f).normalized;
                    }
                    else if (!isCaughtOnEdge)
                    {
                        Vector3 horizTangent = Vector3.Cross(groundNormal, Vector3.down);
                        xzVel += airAccel * Time.deltaTime * Vector3.Project(stickDirection,horizTangent);
                    }
                }



            }
            if (jump)
            {
                jump = false;
                if (airTime < jumpBuffer && !didJump)
                {
                    StandingJump();
                }

            }
            if (ledgeSnap)
            {
                if (currentClimb is Ledge ledge)
                {
                    //animancer.Play(ledgeStart);
                    state.climb = (DirectionalMixerState)animancer.Play(ledgeHang);
                }
                else if (currentClimb is Ladder ladder)
                {
                    state.climb = animancer.Play(ladderClimb);

                }
                else if (currentClimb is Rail rail)
                {
                    state.climb = animancer.Play(railWalk);
                }
                SnapToLedge();
                StartCoroutine(DelayedSnapToLedge());
            }
            if (CheckWater())
            {
                if (lastAirTime > softLandingTime)
                {
                    //xzVel = Vector3.zero;
                    AnimancerState astate = animancer.Play(swimDive);
                    StartCoroutine(DecelXZVel(1f));
                    speed = 0f;

                    astate.Events.OnEnd = () =>
                    {
                        state.swim = animancer.Play(swimAnim, 0.5f);
                    };
                    //walkAccelReal = hardLandAccel;
                }
                else
                {
                    //walkAccelReal = swimAccel;
                    state.swim = animancer.Play(swimAnim);
                    this.gameObject.SendMessage("SplashBig");
                }
            }
            HandleAirAttacks();
            animancer.Layers[0].ApplyAnimatorIK = sliding;
        }
        #endregion

        #region roll
        else if (animancer.States.Current == state.roll)
        {
            shouldDodge = false;
            HitboxActive(0);
            speed = Mathf.Abs(dodgeDirection.x) * rollSpeed + Mathf.Abs(dodgeDirection.y) * dodgeJumpSpeed;
            if (GetCombatTarget() != null)
            {
                targetDirection = GetCombatTarget().transform.position - this.transform.position;
                targetDirection.y = 0f;
                lookDirection = targetDirection.normalized;
            }
            else
            {
                lookDirection = this.transform.forward;
            }
            moveDirection = this.transform.forward * dodgeDirection.y + this.transform.right * dodgeDirection.x;
            //DisableCloth();
            /*
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
                        rollAnim.Events.OnEnd = () => {
                            RollSlash();
                            EnableCloth();
                        };

                    }
                    else if (thrust)
                    {
                        rollAnim.Events.OnEnd = () => {
                            RollThrust();
                            EnableCloth();
                        };
                    }
                }
                attack = false;
                slash = false;
                thrust = false;
            }
            */
            applyMove = true;
            animancer.Layers[0].ApplyAnimatorIK = false;
        }
        #endregion

        #region aim
        else if (animancer.States.Current == state.aim)
        {
            // TODO: Add Jumping while Aiming
            speed = Mathf.MoveTowards(speed, walkSpeedCurve.Evaluate(move.magnitude) * aimSpeed, walkAccelReal * Time.deltaTime);
            moveDirection = stickDirection;

            aimForwardVector = Quaternion.AngleAxis(look.x * horizontalAimSpeed * Time.deltaTime, Vector3.up) * aimForwardVector;

            bool turn = false;
            bool endAim = false;

            if (camState == CameraState.Free)
            {
                if (move.magnitude > 0f)
                {
                    lookDirection = stickDirection;
                }
                else
                {
                    lookDirection = this.transform.forward;
                }
            }
            else if (camState == CameraState.Lock)
            {
                if (GetCombatTarget() != null)
                {
                    lookDirection = GetCombatTarget().transform.position - this.transform.position;
                    lookDirection.y = 0f;

                }
                else
                {
                    lookDirection = this.transform.forward;
                }
            }
            else if (camState == CameraState.Aim)
            {
                lookDirection = Camera.main.transform.forward;
                lookDirection.y = 0;
            }

            try
            {
                if (state.aim is DirectionalMixerState aimDir)
                {
                    if (camState == CameraState.Free)
                    {
                        aimDir.ParameterX = 0f;
                        aimDir.ParameterY = move.magnitude * (speed / walkSpeedMax);
                    }
                    else if (camState == CameraState.Lock)
                    {
                        aimDir.ParameterX = Vector3.Dot(moveDirection, this.transform.right) * (speed / walkSpeedMax);
                        aimDir.ParameterY = Vector3.Dot(moveDirection, this.transform.forward) * (speed / walkSpeedMax);
                    }
                    else if (camState == CameraState.Aim)
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
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.LogError(ex);
            }
            if (inventory.IsRangedEquipped())
            {
                IRangedWeapon rwep = (IRangedWeapon)inventory.GetRangedWeapon();
                bool anyPlaying = animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying();

                inventory.GetRangedWeapon().moveset.aimAttack.ProcessAimAttack(this, aiming, slash || thrust, IsAttackHeld());
            }
            if (jump && animancer.States.Current != astate.jump)
            {
                jump = false;

                if (IsGrounded())
                {
                    AnimancerState jumpState;
                    if (inventory.IsRangedEquipped())
                    {
                        jumpState = animancer.Play(inventory.GetRangedWeapon().moveset.aimAttack.GetJumpClip());
                    }
                    else
                    {
                        jumpState = animancer.Play(standJumpAnim);
                    }
                    jumpState.NormalizedTime = 0f;
                    jumpState.Events.OnEnd = () =>
                    {
                        Aim();
                    };
                    state.aim = astate.jump = jumpState;
                    moveDirection = this.transform.forward;
                }

            }
            else if (IsGrounded() && animancer.States.Current == astate.jump && airTime > 0.5f)
            {
                Aim();
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
                //animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
                inventory.SetDrawn(Inventory.RangedType, false);
                state.roll = animancer.Play(rollAnim);
                endAim = true;
            }
            if (attack)
            {

                BasicAttack();
                if (inventory.IsMainEquipped()) endAim = true;
            }
            if (!aiming)
            {
                if (aimTimer <= 0f)
                {
                    endAim = true;
                    animancer.Play(state.move);
                }
                else if (stickDirection.magnitude > 0)
                {
                    aimTimer -= Time.deltaTime;
                }
            }
            if (endAim)
            {
                animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
            }
            animancer.Layers[0].ApplyAnimatorIK = true;
            applyMove = true;
        }
        #endregion

        #region jump
        else if (animancer.States.Current == state.jump)
        {
            jump = false;
            didJump = true;
            //speed = 0f;
            //speed = sprintSpeed;
            moveDirection = this.transform.forward;

            if (stickDirection.sqrMagnitude > 0 && animancer.States.Current != state.backflip)
            {
                xzVel += stickDirection * airAccel * Time.deltaTime;
                xzVel = Vector3.ClampMagnitude(xzVel, sprintSpeed);
                lookDirection = Vector3.RotateTowards(lookDirection, stickDirection.normalized, Mathf.Deg2Rad * airTurnSpeed * Time.deltaTime, 1f).normalized;
            }

            // only allow actions out of jump if enough time has passed
            if (airTime > minAirTimeToAct)
            {
                HandleAirAttacks();
            }
            /*
            if (ledgeSnap && currentClimb is Ledge && currentClimb.transform.position.y > this.transform.position.y)
            {
                if (currentClimb.TryGetComponent<Ledge>(out Ledge ledge))
                {
                    animancer.Play(ledgeStart);

                }
                SnapToLedge();
                StartCoroutine(DelayedSnapToLedge());
            }
            */
            animancer.Layers[0].ApplyAnimatorIK = true;
        }
        #endregion

        #region climb
        else if (animancer.States.Current == state.climb)
        {
            lookDirection = currentClimb.GetClimbHeading();
            if (currentClimb == null || ledgeSnap == false)
            {
                StopClimbing();
            }
            else if (currentClimb is Ledge ledge)
            {
                //((DirectionalMixerState)state.climb).ParameterX = move.x;
                float dot = Mathf.Clamp(Vector3.Dot(stickDirection, ledge.GetClimbTangent()), -1f, 1f);
                ((DirectionalMixerState)state.climb).ParameterX = dot;
                state.climb.Speed = Mathf.Abs(dot) * climbSpeed;
            }
            else if (currentClimb is Ladder ladder)
            {
                state.climb.Speed = move.y * climbSpeed;
                if (ladder.snapPoint <= -ladder.GetDismountPoint(cc.height) && move.y > 0 && allowLadderFinish)
                {
                    SnapToLedge();
                    this.transform.position = ladder.endpoint.transform.position;
                    this.transform.rotation = Quaternion.LookRotation(currentClimb.collider.transform.forward);
                    ledgeSnap = false;
                    animancer.Play(ladderClimbUp);
                    StartClimbLockout();
                    ladder.StopClimb();
                }
            }
            else if (currentClimb is Rail rail)
            {
                float dot = Mathf.Clamp(Vector3.Dot(stickDirection, rail.GetClimbTangent()), -1f, 1f);
                ((DirectionalMixerState)state.climb).ParameterX = dot;
                state.climb.Speed = Mathf.Abs(dot) * climbSpeed;
            }

            if (currentClimb.AllowAttacks() && attack && !animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying())
            {
                BasicAttack();
            }
            if (jump && currentClimb.AllowJumps())
            {
                jump = false;
                buffer.ClearInput(InputBuffer.Inputs.Jump);
                if (stickDirection.magnitude > 0)
                {
                    lookDirection = stickDirection.normalized;
                    moveDirection = stickDirection.normalized;
                    //xzVel = xzVel.magnitude * stickDirection.normalized;
                }
                //StopClimbing();
                ledgeSnap = false;
                cc.enabled = true;
                airTime = 0f;
                xzVel = Vector3.zero;
                PlayJump();
                currentClimb.CheckLedgeAfter(0.5f);
                //StartClimbLockout();
                /*
                ClimbDetector climbRef = currentClimb;

                state.jump.Events.OnEnd = () =>
                {
                    if (climbRef.CheckPlayerCollision())
                    {
                        ledgeSnap = true;
                        currentClimb = climbRef;
                        SnapToLedge();
                    }
                    state.jump.Events.Clear();
                };
                */
            }
            else
            {
                yVel = 0f;
            }

            animancer.Layers[0].ApplyAnimatorIK = false;
        }
        #endregion

        #region swim
        else if (animancer.States.Current == state.swim)
        {
            Debug.DrawRay(this.transform.position, Vector3.down * wadingHeightIn, Color.cyan, 0.1f);
            applyMove = true;
            if (CheckWater())
            {
                speed = Mathf.MoveTowards(speed, walkSpeedCurve.Evaluate(move.magnitude) * swimSpeed, swimAccel * Time.deltaTime);
                if (stickDirection.sqrMagnitude > 0)
                {
                    lookDirection = Vector3.RotateTowards(lookDirection, stickDirection.normalized, Mathf.Deg2Rad * walkTurnSpeed * Time.deltaTime, 1f);
                }
                moveDirection = stickDirection;
            }
            else if (Physics.Raycast(this.transform.position, Vector3.down, out RaycastHit wadingHit, wadingHeightIn, MaskReference.Terrain))
            {
                AnimancerState land = animancer.Play(swimEnd);
                walkAccelReal = hardLandAccel;
                speed = 0f;
                sprinting = false;
            }
            else
            {
                //AnimancerState land = state.move.ChildStates[0];
                animancer.Play(state.fall);
                xzVel = lookDirection * waterDismountSpeed;
                applyMove = false;
                SetYVel(0f);
            }

            animancer.Layers[0].ApplyAnimatorIK = (speed < 0.1f);
            friction = waterFriction;
        }
        #endregion

        #region attack
        else if (animancer.States.Current == state.attack)
        {
            speed = Mathf.MoveTowards(speed, 0f, attackDecelReal * Time.deltaTime);
            moveDirection = this.transform.forward;

            if (cancelTime > 0 && !plunge)
            {
                if (jump)
                {
                    jump = false;
                    cancelAction = AttackCancelAction.Jump;
                }
                else if (attack && slash)
                {
                    attack = false;
                    slash = false;
                    cancelAction = AttackCancelAction.Slash;
                }
                else if (attack && thrust)
                {
                    attack = false;
                    thrust = false;
                    cancelAction = AttackCancelAction.Thrust;
                }
                else if (invSlot > -1)
                {
                    cancelAction = (AttackCancelAction)((int)AttackCancelAction.InventorySlot0) + invSlot;
                    invSlot = -1;
                }

                if (animancer.States.Current.NormalizedTime >= cancelTime)
                {
                    if (cancelAction != AttackCancelAction.None)
                    {
                        var currentCancelAction = cancelAction;
                        cancelTime = -1f;
                        cancelAction = AttackCancelAction.None;

                        switch (currentCancelAction)
                        {
                            case AttackCancelAction.Jump:
                            {
                                StandingJump();
                            }
                            break;
                            case AttackCancelAction.Slash:
                            {
                                CancelSlash();
                            }
                            break;
                            case AttackCancelAction.Thrust:
                            {
                                CancelThrust();
                            }
                            break;
                            case AttackCancelAction.InventorySlot0:
                            {
                                inventory.InputOnSlot(0);
                            }
                            break;
                            case AttackCancelAction.InventorySlot1:
                            {
                                inventory.InputOnSlot(1);
                            }
                            break;
                            case AttackCancelAction.InventorySlot2:
                            {
                                inventory.InputOnSlot(2);
                            }
                            break;
                            case AttackCancelAction.InventorySlot3:
                            {
                                inventory.InputOnSlot(3);
                            }
                            break;
                        }
                    }
                }
            }

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
                if (jump)
                {
                    StandingJump();
                }
            }
            if (plunge)
            {
                bool grounded = isGrounded;
                grounded |= rayHit.collider != null || (ccHit != null && ccHit.collider != null && MaskReference.IsTerrain(ccHit.collider));
                if (yVel > 0)
                {
                    // do nothing, don't stop until you start going down
                }
                else if (grounded)
                {
                    // end plunge
                    state.attack = animancer.Play(plungeEnd);
                    plunge = false;
                    xzVel = Vector3.zero;
                    speed = 0f;
                }
                else if (ccHit != null && ccHit.collider != null)
                {
                    // slide off
                    Vector3 dir = Vector3.ProjectOnPlane(ccHit.normal, Vector3.up);

                    if (dir.magnitude <= 0f || Vector3.Dot(ccHit.normal,Vector3.up) == 1)
                    {
                        dir = stickDirection.magnitude > 0 ? stickDirection.normalized : this.transform.forward;
                    }
                    xzVel = dir.normalized * slideOffSpeed;
                }
            }
            else if (hold)
            {
                if (camState != CameraState.Lock || GetCombatTarget() == null)
                {
                    if (stickDirection.magnitude > 0)
                    {
                        lookDirection = stickDirection;
                    }
                }
                else
                {
                    targetDirection = GetCombatTarget().transform.position - this.transform.position;
                    targetDirection.y = 0f;
                    lookDirection = targetDirection.normalized;
                }

                if ((currentDamage.isThrust && !IsThrustHeld()) && holdAttackClock < holdAttackMin)
                {
                    HoldThrustRelease(holdAttackClock <= 0);
                }
                else if ((currentDamage.isSlash && !IsSlashHeld()) && holdAttackClock < holdAttackMin)
                {
                    HoldSlashRelease(holdAttackClock <= 0);
                }
                else if (holdAttackClock > 0f)
                {
                    holdAttackClock -= Time.deltaTime;
                    if (holdAttackClock <= 0f)
                    {
                        FlashWarning(1);
                    }
                }
            }
            if (CheckWater())
            {
                state.swim = animancer.Play(swimAnim);
                this.gameObject.SendMessage("SplashBig");
            }
            animancer.Layers[0].ApplyAnimatorIK = true;
            applyMove = isGrounded;
        }
        #endregion

        #region dialogue
        else if (animancer.States.Current == state.dialogue)
        {
            animancer.Layers[0].ApplyAnimatorIK = true;
        }
        #endregion

        #region hurt fall
        else if (animancer.States.Current == damageHandler.fall)
        {
            if (CheckWater())
            {
                AnimancerState astate = animancer.Play(swimDiveHurt);
                StartCoroutine(DecelXZVel(1f));
                speed = 0f;

                astate.Events.OnEnd = () =>
                {
                    AnimancerState rstate = animancer.Play(swimDiveRise);
                    rstate.Events.OnEnd = () =>
                    {
                        state.swim = animancer.Play(swimAnim, 1f);
                    };
                };

                if (!attributes.HasHealthRemaining())
                {
                    Resurrect(astate);
                }
            }
            else if (!isGrounded && (rayHit.collider != null || (ccHit != null && ccHit.collider != null)))
            {
                //Physics.Raycast(bottom, Vector3.down, out groundRayHit, 0.2f, LayerMask.GetMask("Terrain"));
                Vector3 dir = Vector3.forward;
                if (slopeAngle > maxSlideAngle && slopeAngle < 90f)
                {
                    dir = Vector3.ProjectOnPlane(groundNormal, Vector3.up).normalized;
                }
                else if (rayHit.collider == null && ccHit.collider != null)
                {
                    dir = this.transform.position - ccHit.point;
                }

                dir.y = 0f;
                xzVel = Vector3.MoveTowards(xzVel, dir.normalized * 1f, slideOffSpeed * Time.deltaTime);


                Debug.DrawRay(this.transform.position, dir * 5f, Color.magenta);
            }
            applyMove = isGrounded;
            friction = Mathf.Min(groundFriction, slideFriction);
        }
        #endregion
        #region ALL OTHERS
        else
        {
            applyMove = isGrounded;
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

        this.transform.rotation = Quaternion.LookRotation(lookDirection);
        Vector3 downwardsVelocity = this.transform.up * yVel;

        ((MixerState<Vector2>)state.move).Parameter = GetMovementVector();
        if (state.drink != null)
        {
            state.drink.Parameter = speed / drinkSpeed;
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
        if (blocking)
        {
            float x = Vector3.Dot(moveDirection, this.transform.right) * (speed / blockSpeed);
            float y = Vector3.Dot(moveDirection, this.transform.forward) * (speed / blockSpeed);
            ((MixerState<Vector2>)state.block).Parameter = new Vector2(x, y);
        }
        Vector3 finalMov = (moveDirection * speed + downwardsVelocity);

        /*
        if (animancer.States.Current == state.move || animancer.States.Current == state.sprint || animancer.States.Current == state.dash)
        {
            //xzVel = finalMov;
            //xzVel.y = 0;

        }
        */
        if (animancer.States.Current == damageHandler.block)
        {
            if (hasTypedBlocks)
            {
                int param = 0 + (IsSlashHeld() ? DamageKnockback.SLASH_INT : 0) + (IsThrustHeld() ? DamageKnockback.THRUST_INT : 0);
                if (param != lastTypedBlockParam)
                {
                    lastTypedBlockParam = param;
                    OnBlockTypeChange.Invoke();
                }
            }

        }

        if (applyMove)
        {
            xzVel = Vector3.Lerp(xzVel, moveDirection * speed, friction);
        }

        if (IsAiming() && inventory.IsRangedDrawn() && camState == CameraState.Aim)
        {
            inventory.GetRangedWeapon().moveset.aimAttack.OnUpdate(this);
            animancer.Layers[HumanoidAnimLayers.UpperBody].ApplyAnimatorIK = true;
        }
        else
        {
            animancer.Layers[HumanoidAnimLayers.UpperBody].ApplyAnimatorIK = false;
        }

        if (isGrounded && !IsFalling() && !IsClimbing())
        {
            //UnsnapLedge();
        }
        if (IsHurt() && IsFalling())
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

        if (animancer.States.Current != state.block && wasBlockingLastFrame)
        {
            if (animancer.Layers[HumanoidAnimLayers.BlockBlend].IsAnyStatePlaying())
            {
                animancer.Layers[HumanoidAnimLayers.BlockBlend].Stop();
            }
        }
        wasBlockingLastFrame = (animancer.States.Current == state.block);

        if (sliding && !wasSlidingUpdate)
        {
            SendMessage("StartContinuousSlide");
        }
        else if (!sliding && wasSlidingUpdate)
        {
            SendMessage("StopContinuousSlide");
        }
        wasSlidingUpdate = sliding;

        if (animancer.States.Current == state.externalSource)
        {
            externalSourceClock += Time.deltaTime;
            if (externalSourceClock >= maxExternalSourceTime)
            {
                Debug.Log("External Source animation timed out!");
                animancer.Play(state.move);
            }
        }
        else if (externalSourceClock >= 0f)
        {
            externalSourceClock = 0f;
        }

        smoothLaunchVector = new Vector3(lastLaunchVector.x, 0, lastLaunchVector.z);
        smoothLaunchVector = Vector3.RotateTowards(smoothLaunchVector, lastLaunchVector, launchVectorSmoothSpeed * Mathf.Deg2Rad * Time.deltaTime, 1f);
        HandleCinemachine();

        if (lastSafePoint == Vector3.zero || safePointClock <= 0f)
        {
            if (!lastGroundWasStatic || dead || resurrecting || (!IsMoving() && animancer.States.Current != state.sprint) || this.GetComponent<ActorTimeTravelHandler>().IsRewinding())
            {
                safePointClock = 0.25f;
            }
            else if (!isGrounded)
            {
                safePointClock = 0f;
            }
            else
            {
                SetNewSafePoint();
                safePointClock = 1f;
            }
        }
        if (safePointClock > 0f)
        {
            safePointClock -= Time.deltaTime;
        }
        //yield return new WaitForSecondsRealtime(1f);

        if (consumableModel != null && animancer.States.Current != state.consume)
        {
            Destroy(consumableModel);
        }

        //HandleIdleBlends();
    }

    private void LateUpdate()
    {
        if (IsAiming() && inventory.IsRangedDrawn() && camState == CameraState.Aim)
        {
            inventory.GetRangedWeapon().moveset.aimAttack.OnUpdate(this);
        }
        if (warpDelta.magnitude > 0.01f)
        {
            if (vcam.free != null)
                vcam.free.OnTargetObjectWarped(vcam.free.LookAt, warpDelta);
            /*if (vcam.target != null)
                vcam.target.OnTargetObjectWarped(vcam.target.Follow, delta);*/
            if (vcam.aim != null)
                vcam.aim.OnTargetObjectWarped(vcam.aim.LookAt, warpDelta);
            if (vcam.climb != null)
                vcam.climb.OnTargetObjectWarped(vcam.climb.LookAt, warpDelta);
            if (vcam.dialogue != null)
                vcam.dialogue.OnTargetObjectWarped(vcam.dialogue.LookAt, warpDelta);

            warpDelta = Vector3.zero;
        }
    }

    void OnAnimatorMove()
    {
        animatorDelta = animator.rootPosition - this.transform.position;
        if (IsAttacking())
        {
            // remove sideways root motion during attacks
            animatorDelta = Vector3.Project(animatorDelta, this.transform.forward);
            //this.transform.position = this.transform.position + animatorDelta;
            this.transform.rotation = animator.rootRotation;
        }
        else if (cc.enabled)
        {
            //this.transform.position = animator.rootPosition;
            this.transform.rotation = animator.rootRotation;
        }
        else
        {
            this.transform.position = animator.rootPosition;
            this.transform.rotation = animator.rootRotation;
        }

        animatorVelocity = animator.velocity;//animator.rootPosition - this.transform.position;
    }

    void FixedUpdate()
    {
        if (disablePhysics) return;
        if (isBird)
        {
            var flyVelocity = IsBlockHeld() ? flyFastVelocity : (IsJumpHeld() ? flySlowVelocity : 0.0f);
            transform.position += Camera.main.transform.forward * flyVelocity * Time.fixedDeltaTime;
            return;
        }

        isGrounded = GetGrounded(out rayHit);
        if (isGrounded)
        {
            if (yVel <= 0)
            {
                yVel = 0f;
            }
            else
            {
                yVel -= gravity * Time.fixedDeltaTime;
            }
            airTime = 0f;
            landTime += Time.fixedDeltaTime;
            if (landTime > 1f)
            {
                landTime = 1f;
            }
        }
        else
        {
            if (!sliding) yVel -= gravity * Time.fixedDeltaTime;
            if (yVel < -terminalVel)
            {
                yVel = -terminalVel;
            }
            airTime += Time.fixedDeltaTime;
            lastAirTime = airTime;
        }

        Vector3 velocity = xzVel;
        velocity.y = yVel;

        if ((IsAttacking() || IsBlocking()) && isGrounded)
        {
            cc.radius = attackingColliderRadius;
        }
        else
        {
            cc.radius = standingColliderRadius;
        }

        if (withinBias)
        {
            cc.Move(Vector3.up * (biasHeight - this.transform.position.y));
        }

        if (IsClimbing())
        {
            climbSnapPoint += animatorVelocity * Time.fixedDeltaTime;
            this.transform.position = Vector3.MoveTowards(this.transform.position, climbSnapPoint, climbSnapSpeed * Time.fixedDeltaTime);
        }
        else if (cc.enabled)
        {
            if (animancer.States.Current != state.swim && (!isGrounded || yVel > 0 || animancer.States.Current == state.jump))
            {
                /*
                if (Physics.SphereCast(this.transform.position, cc.radius, Vector3.down, out RaycastHit sphereHit, 1f, LayerMask.GetMask("Terrain")))
                {
                    Vector3 dir = -(this.transform.position - sphereHit.point);
                    dir.Scale(new Vector3(1f, 0f, 1f));
                    cc.Move(dir.normalized * 10f);
                    Debug.Log("slide");
                }*/
                cc.Move((velocity * Time.fixedDeltaTime) + animatorDelta);
            }
            else if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.5f, MaskReference.Terrain) && yVel <= 0 && animancer.States.Current != state.swim)
            {
                Vector3 temp = Vector3.Cross(hit.normal,velocity);
                cc.Move(((Vector3.Cross(temp, hit.normal) + gravity * Vector3.down) * Time.fixedDeltaTime) + animatorDelta);
            }
            else
            {
                cc.Move((velocity * Time.fixedDeltaTime) + animatorDelta);
            }
        }
    }

    public void DisablePhysics()
    {
        DisablePhysics(true);
    }

    public void EnablePhysics()
    {
        DisablePhysics(false);
    }
    public void DisablePhysics(bool disabled)
    {
        disablePhysics = disabled;
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

    public void WarpTo(Vector3 position)
    {
        CharacterController cc = this.GetComponent<CharacterController>();
        //warpDelta = position - this.transform.position;
        cc.enabled = false;
        this.transform.position = position;
        cc.enabled = true;
        ResetOnMove();
    }

    public void JumpToNavMesh(float range)
    {
        if (UnityEngine.AI.NavMesh.SamplePosition(this.transform.position, out var hit, range, 1))
        {
            WarpTo(hit.position);
        }
    }

    public void JumpToNavMesh()
    {
        JumpToNavMesh(10f);
    }
    public void ResetOnMove()
    {
        airTime = 0f;
        lastAirTime = 0f;
        xzVel = Vector3.zero;
        yVel = 0f;
        plunge = false;
        headPoint = Vector3.zero;
        GetHeadPoint();
        smoothedHeadPoint = headPoint;
        player.walkAccelReal = walkAccel;
        if (mainWeaponAngle != 0f)
        {
            StartCoroutine("GradualResetMainRotation");
        }
        if (offWeaponAngle != 0f)
        {
            StartCoroutine("GradualResetOffRotation");
        }
        Cloth cloth = player.GetComponentInChildren<Cloth>();
        if (cloth != null)
        cloth.ClearTransformMotion();
    }

    public override void OnFallOffMap()
    {

        if (attributes.health.current > 1f || attributes.lives > 0f)
        {
            ResetToSafePoint();
        }
        else
        {
            this.attributes.ReduceHealth(1f);
            Die();
        }
    }
    public void ResetToSafePoint()
    {
        //WarpTo(lastSafePoint);
        if (!rewindingToSafePoint) StartCoroutine("RewindToSafePoint");
    }

    IEnumerator RewindToSafePoint()
    {
        rewindingToSafePoint = true;
        Debug.Log("starting safe point rewind");
        float rewindTimeout = rewingTimeoutDuration;
        float health = attributes.health.current;
        ActorTimeTravelHandler timeTravelHandler = this.GetComponent<ActorTimeTravelHandler>();
        TimeTravelController.time.IgnoreLimits();
        TimeTravelController.time.StartRewindSelective(timeTravelHandler);
        float closestDistance = Mathf.Infinity;
        do
        {
            //Debug.Log("distance: " + Vector3.Distance(this.transform.position, lastSafePoint));
            yield return null;
            rewindTimeout -= Time.deltaTime;
            float dist = Vector3.Distance(this.transform.position, lastSafePoint);
            if (dist < closestDistance)
            {
                closestDistance = dist;
            }
        }
        while (timeTravelHandler.IsRewinding() && Vector3.Distance(this.transform.position, lastSafePoint) > rewindDistanceThreshold && rewindTimeout > 0);
        TimeTravelController.time.CancelRewind();
        if (rewindTimeout <= 0)
        {
            Debug.Log("rewind timed out! closest distance: " + closestDistance);
        }
        else
        {
            Debug.Log("rewind completed with " + rewindTimeout + " second(s) remaining");
        }
        yield return new WaitWhile(() => { return timeTravelHandler.IsRewinding(); });
        this.attributes.health.current = health;
        this.attributes.ReduceHealth(1f);
        WarpTo(lastSafePoint);
        state.resurrect = animancer.Play(resurrectFaceUp);
        state.resurrect.Events.OnEnd = _MoveOnEnd;
        if (attributes.health.current <= 0f && attributes.lives > 0)
        {
            state.resurrect.Speed = 0f;
            damageHandler.isFacingUp = true;
            Die();
        }
        rewindingToSafePoint = false;
    }


    IEnumerator DecelXZVel(float time)
    {
        float clock = 0f;
        float t;
        Vector3 startingVel = xzVel;
        while (clock <= time)
        {
            yield return null;
            clock += Time.deltaTime;
            t = Mathf.Clamp01(clock / time);
            xzVel = Vector3.Lerp(startingVel, Vector3.zero, t);
        }
        xzVel = Vector3.zero;
    }
    public void SetNewSafePoint()
    {
        lastSafePoint = this.transform.position;
        if (UnityEngine.AI.NavMesh.SamplePosition(lastSafePoint, out var hit, 10f, 1))
        {
            lastSafePoint = hit.position;
        }
    }

    public void TryFindSpawnPoint()
    {
        foreach (PlayerPositioner spawnPoint in GameObject.FindObjectsOfType<PlayerPositioner>())
        {
            if (spawnPoint.gameObject.activeInHierarchy && spawnPoint.gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene())
            {
                spawnPoint.SpawnPlayer(this);
                spawned = true;
                return;
            }
        }
    }

    public bool HasBeenSpawned()
    {
        return spawned;
    }

    public void SetSpawned()
    {
        spawned = true;
    }

    public void ResetAnim()
    {
        _MoveOnEnd();
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

    public void UnsetClimb(ClimbDetector climb)
    {
        if (currentClimb == climb)
        {
            ledgeSnap = false;
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

    public void SetRail(Rail rail)
    {
        if (allowClimb)
        {
            currentClimb = rail;
            ledgeSnap = true;
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

    public void StartLedge()
    {
        state.climb = (DirectionalMixerState)animancer.Play(ledgeHang);
    }


    public void StopClimbing()
    {
        ledgeSnap = false;
        state.fall = animancer.Play(fallAnim, 1f);
        cc.enabled = true;
        airTime = 0f;
        cc.Move(Vector3.down * 0.5f);
        yVel = 0f;
        xzVel = Vector3.zero;
        StartClimbLockout();
        currentClimb.StopClimb();
    }

    public void SnapToCurrentLedge()
    {
        SnapToLedge();
        //StartCoroutine(DelayedSnapToLedge());
    }

    public void ClimbUpLedge()
    {
        ledgeSnap = false;
        this.transform.position = climbSnapPoint;
        animancer.Play(ledgeClimb);
        currentClimb.StopClimb();
        StartClimbLockout();
    }

    void SnapToLedge()
    {
        if (ledgeSnap == true && currentClimb != null)
        {
            if (currentClimb is Ledge ledge)
            {
                this.transform.rotation = Quaternion.LookRotation(currentClimb.collider.transform.forward);
                int sign = 0;
                if (Mathf.Abs(move.x) > 0.1f) sign = (int)Mathf.Sign(move.x);
                climbSnapPoint = ledge.GetSnapPointDot(cc.radius * 2f, this.transform.position, this, -sign);
                cc.enabled = false;
            }
            else if (currentClimb is Ladder ladder)
            {
                //descendClamp = (ladder.canDescend) ? -1 : 0;
                //ascendClamp = (ladder.canAscend) ? 1 : 0;
                this.transform.rotation = Quaternion.LookRotation(currentClimb.collider.transform.forward);
                int sign = 0;
                if (Mathf.Abs(move.y) > 0.1f) sign = (int)Mathf.Sign(move.y);
                climbSnapPoint = ladder.GetSnapPointDot(cc.height/2f, this.transform.position, this, -sign);
                cc.enabled = false;
            }
            else if (currentClimb is Rail rail)
            {
                this.transform.rotation = Quaternion.LookRotation(currentClimb.collider.transform.right);
                int sign = 0;
                if (Mathf.Abs(move.x) > 0.1f) sign = (int)Mathf.Sign(move.x);
                climbSnapPoint = rail.GetSnapPointDot(cc.radius * 2f, this.transform.position, this, -sign);
                cc.enabled = false;
            }
            currentClimb.StartClimb();
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

    public void StartClimbLockout()
    {
        StartCoroutine(ClimbLockout());
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
        xzVel = Vector3.zero;

    }
    #endregion

    #region CAMERA
    void HandleCinemachine()
    {
        if (animancer.States.Current == state.climb && currentClimb is not Rail)
        {
            camState = CameraState.Free;//CameraState.Climb;
        }
        else if (IsInDialogue() && GetCombatTarget() != null)
        {
            camState = CameraState.Dialogue;
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

        inputs.actions["UseSecondary"].started += (context) =>
        {
            RangedStart();
        };

        inputs.actions["UseSecondary"].canceled += (context) =>
        {
            RangedEnd();
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

        inputs.actions["QuickSlot - 3"].performed += (context) =>
        {
            if (context.interaction is HoldInteraction)
            {
                OnQuickSlotHold(3);
            }
            else
            {
                OnQuickSlot(3);
            }
        };

        inputs.actions["Pause"].performed += (context) =>
        {
            MenuView.OnPressPause();
        };
    }

    [Serializable]
    struct InputBuffer
    {
        public enum Inputs
        {
            None,
            Jump,
            Dodge,
            SprintStart,
            BlockStart,
            Thrust,
            ThrustHold,
            Slash,
            SlashHold,
            RangedStart,
            InventorySlot0,
            InventorySlot1,
            InventorySlot2,
            InventorySlot3
        }

        public Inputs lastInput;
        public float lastInputTime;

        public void SetInput(Inputs input, float time)
        {
            lastInput = input;
            lastInputTime = time;
        }

        public void ClearInput(Inputs input)
        {
            if (lastInput == input)
            {
                lastInput = Inputs.None;
            }
        }

        public void ClearAll()
        {
            lastInput = Inputs.None;
        }
        public bool IsAttack(Inputs input)
        {
            return input == Inputs.Thrust || input == Inputs.ThrustHold || input == Inputs.Slash || input == Inputs.SlashHold;
        }

        public Inputs PollInput(float bufferLength)
        {
            Inputs input = Inputs.None;
            if (lastInputTime > Time.time - bufferLength)
            {
                input = lastInput;
                //ClearAll();
            }

            return input;
        }
    }

    // shouldDodge
    // jump
    // attack
    // slash
    // blocking
    // sprinting

    void PollInputs()
    {
        shouldDodge = false;
        jump = false;
        attack = false;
        slash = false;
        thrust = false;
        invSlot = -1;
        blocking = blocking && IsBlockHeld();
        sprinting = sprinting && IsSprintHeld();
        secondary = secondary && IsRangedHeld();
        targeting = IsTargetHeld();
        //aiming = IsAimHeld();
        switch (buffer.PollInput(inputBufferTimeoutTime))
        {
            case InputBuffer.Inputs.Dodge:
                shouldDodge = true;
                break;
            case InputBuffer.Inputs.Jump:
                jump = true;
                break;
            case InputBuffer.Inputs.Slash:
                attack = true;
                slash = true;
                break;
            case InputBuffer.Inputs.SlashHold:
                attack = true;
                slash = true;
                hold = true;
                break;
            case InputBuffer.Inputs.Thrust:
                attack = true;
                thrust = true;
                break;
            case InputBuffer.Inputs.ThrustHold:
                attack = true;
                thrust = true;
                hold = true;
                break;
            case InputBuffer.Inputs.SprintStart:
                sprinting = true;
                dashed = false;
                break;
            case InputBuffer.Inputs.BlockStart:
                blocking = true;
                break;
            case InputBuffer.Inputs.RangedStart:
                secondary = true;
                break;
            case InputBuffer.Inputs.InventorySlot0:
                invSlot = 0;
                break;
            case InputBuffer.Inputs.InventorySlot1:
                invSlot = 1;
                break;
            case InputBuffer.Inputs.InventorySlot2:
                invSlot = 2;
                break;
            case InputBuffer.Inputs.InventorySlot3:
                invSlot = 3;
                break;
        }
    }

    public float GetBlockBufferTime()
    {
        InputBuffer.Inputs inputButton = buffer.PollInput(999f);
        if (inputButton == InputBuffer.Inputs.Thrust || inputButton == InputBuffer.Inputs.Slash)
        {
            return Time.time - buffer.lastInputTime;
        }
        else
        {
            return -1;
        }
    }


    public void OnDodge(InputValue value)
    {
        return;
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
            StartClimbLockout();
        }
        else
        {
            //shouldDodge = true;
            buffer.SetInput(InputBuffer.Inputs.Dodge, Time.time);
        }

    }

    public void OnJump(InputValue value)
    {
        if (!CanPlayerInput()) return;
        InputJump();

    }

    public void InputJump()
    {
        if (IsClimbing() && currentClimb != null && currentClimb is Ledge)
        {
            ClimbUpLedge();
        }
        else if (!isGrounded && !allowClimb && (currentClimb == null && !currentClimb.AllowJumps()))
        {
            // what was this supposed to do...?
            StartClimbLockout();
            allowClimb = true;
        }
        else if (isGrounded || airTime < jumpBuffer || (IsClimbing() && currentClimb.AllowJumps()))
        {
            //jump = true;
            buffer.SetInput(InputBuffer.Inputs.Jump, Time.time);
        }
    }
    public void ApplyJump()
    {
        yVel = jumpVel;
        OnJumpStart.Invoke();
    }

    public void ApplyDodgeJump()
    {
        yVel = dodgeJumpVel;
        DisableCloth();
    }

    public void ApplyAttackJump()
    {
        yVel = attackJumpVel;
    }

    public void ApplyBackflip()
    {
        speed = -backflipSpeed;
        xzVel = moveDirection * -backflipSpeed;
        yVel = dodgeJumpVel;
    }

    public void IncreaseJumpDrag()
    {
        StartCoroutine(DecelXZVel(0.25f));
    }

    public void SetYVel(float velocity)
    {
        yVel = velocity;
    }

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        look = value.Get<Vector2>();
    }

    bool IsAimHeld()
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

    public bool IsTargetHeld()
    {
        return this.GetComponent<PlayerInput>().actions["Target"].IsPressed();
    }

    void BlockStart()
    {
        if (!CanPlayerInput()) return;
        //blocking = true;
        buffer.SetInput(InputBuffer.Inputs.BlockStart, Time.time);
    }

    void BlockEnd()
    {
        blocking = false;
    }

    public bool CheckStartBlockAnim()
    {
        EquippableWeapon blockWeapon = inventory.GetBlockWeapon();
        if (blocking)
        {
            if (blockWeapon != null)
            {
                int itemSlot = inventory.GetItemEquipType(blockWeapon);
                if ((itemSlot == Inventory.MainType && !inventory.IsMainDrawn()) || (itemSlot == Inventory.OffType && !inventory.IsOffDrawn()))
                {
                    inventory.SetDrawn(inventory.GetItemEquipType(blockWeapon), true);
                    UpdateFromMoveset();
                }
            }

            animancer.Play(state.block, 0.01f);
            return true;
        }
        return false;

    }
    public bool IsBlockHeld()
    {
        return this.GetComponent<PlayerInput>().actions["Block"].IsPressed();
    }
    void SprintStart()
    {
        if (!CanPlayerInput()) return;
        //sprinting = true;
        buffer.SetInput(InputBuffer.Inputs.SprintStart, Time.time);
        //dashed = false;
    }

    void SprintEnd()
    {
        sprinting = false;
    }

    public bool IsSprintHeld()
    {
        return this.GetComponent<PlayerInput>().actions["Sprint"].IsPressed();
    }

    public bool IsJumpHeld()
    {
        return GetComponent<PlayerInput>().actions["Jump"].IsPressed();
    }

    void RangedStart()
    {
        if (!CanPlayerInput()) return;
        //sprinting = true;
        buffer.SetInput(InputBuffer.Inputs.RangedStart, Time.time);
    }

    void RangedEnd()
    {
        if (!CanPlayerInput()) return;
        //sprinting = true;
        secondary = false;
    }

    public bool IsRangedHeld()
    {
        return this.GetComponent<PlayerInput>().actions["UseSecondary"].IsPressed();
    }
    public void DashEnd()
    {
        Debug.Log("dash-end");
        animancer.Play(state.sprint);
    }

    public void OnTarget(InputValue value)
    {
        if (!CanPlayerInput()) return;
        //toggleTarget.Invoke();
    }

    /*
    public void OnChangeTarget(InputValue value)
    {
        if (value.Get<Vector2>().magnitude > 0.9f) changeTarget.Invoke();
    }
    */
    public void OnAtk_Slash(InputValue value)
    {
        if (!CanPlayerInput()) return;
        if (IsClimbing() && (currentClimb == null || !currentClimb.AllowAttacks()))
        {
            StopClimbing();
        }
        else
        {
            buffer.SetInput(InputBuffer.Inputs.Slash, Time.time);
        }

        //attack = true;
        //slash = true;
    }

    public void OnAtk_Thrust(InputValue value)
    {
        if (!CanPlayerInput()) return;
        if (IsClimbing() && (currentClimb == null || !currentClimb.AllowAttacks()))
        {
            StopClimbing();
        }
        else
        {
            buffer.SetInput(InputBuffer.Inputs.Thrust, Time.time);
        }
        //attack = true;
        //thrust = true;
    }
    public Vector2 GetMovementVector()
    {
        if (camState == CameraState.Lock || targeting)
        {
            float x = Vector3.Dot(moveDirection.normalized, this.transform.right) * (speed / strafeSpeed);
            float y = Vector3.Dot(moveDirection.normalized, this.transform.forward) * (speed / strafeSpeed);
            return new Vector2(x, y);
        }
        else if (camState == CameraState.Free || camState == CameraState.Aim)
        {
            return new Vector2(0f, speed / walkSpeedMax);
            //return new Vector2(0f, move.magnitude);
        }
        return Vector2.zero;
    }
    public void OnSheathe(InputValue value)
    {
        if (!CanPlayerInput()) return;
        InputSheathe();

    }

    public void InputSheathe()
    {
        /*
        if (!animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying())
        {
            if (inventory.IsOffDrawn() && inventory.IsOffEquipped())
            {
                TriggerSheath(false, inventory.GetOffWeapon().MainHandEquipSlot, true);
            }
            else if (inventory.IsMainDrawn() && inventory.IsMainEquipped())
            {
                TriggerSheath(false, inventory.GetMainWeapon().MainHandEquipSlot, true);
            }
            else if (!inventory.IsMainDrawn() && inventory.IsMainEquipped())
            {
                TriggerSheath(true, inventory.GetMainWeapon().MainHandEquipSlot, true);
            }
        }*/
        SheatheAll();
    }

    public void ResetInputs()
    {
        attack = false;
        slash = false;
        thrust = false;
        buffer.ClearAll();
    }

    public void ProcessBlock()
    {
        blocking = (IsBlockHeld());
    }

    public void ProcessRanged()
    {
        aiming = (IsRangedHeld());
    }
    void OnControlsChanged()
    {
        onControlsChanged.Invoke();
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public void SetWalkAccel(float accel)
    {
        this.walkAccelReal = accel;
    }

    public void ResetWalkAccel()
    {
        this.walkAccelReal = walkAccel;
    }
    public void VerifyAccelerationAfterDelay(float delay = 5f)
    {
        StartCoroutine(CheckAccelCoroutine(delay));
    }

    IEnumerator CheckAccelCoroutine(float delay)
    {
        float currentAccel = walkAccelReal;
        yield return new WaitForSeconds(delay);
        if (walkAccelReal == currentAccel && walkAccelReal < walkAccel)
        {
            walkAccelReal = walkAccel;
        }
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
        return !isMenuOpen && !TimeScaleController.instance.paused;
    }

    public void ToggleMenu()
    {
        MenuController.menu.TryToggleInventory();
    }

    public void OnQuickSlot(int slot)
    {
        if (InventoryUI2.invUI != null && InventoryUI2.invUI.awaitingQuickSlotEquipInput)
        {
            inventory.InputOnSlot(slot);
            InventoryUI2.invUI.FlareSlot(slot);
        }
        else if (CanPlayerInput())
        {
            buffer.SetInput((InputBuffer.Inputs)(((int)InputBuffer.Inputs.InventorySlot0) + slot), Time.time);
            InventoryUI2.invUI.FlareSlot(slot);
        }
        else if (!isMenuOpen)
        {
            InventoryUI2.invUI.FlareSlot(slot);
        }

    }

    public void OnQuickSlotHold(int slot)
    {
        /*
        if (IsMoving() && CanPlayerInput())
        {
            //inventory.UnequipOnSlot(slot);

        }
        InventoryUI2.invUI.FlareSlot(slot);
        */
    }
    #endregion
    #endregion

    #region SWIMMING

    bool CheckWater()
    {
        wading = false;
        float wadingHeight = (IsSwimming() ? wadingHeightOut : wadingHeightIn);
        int mask = UnityEngine.LayerMask.GetMask("Water", "Water2");
        inWater = Physics.Raycast(this.transform.position + Vector3.up * cc.height, Vector3.down, out RaycastHit waterHit, cc.height + 0.2f, mask);
        Debug.DrawRay(this.transform.position + Vector3.up * cc.height, Vector3.down * (cc.height + 0.2f), inWater ? Color.yellow : Color.cyan);
        if (inWater)
        {
            swimCollider = waterHit.collider;
            waterHeight = swimCollider.bounds.center.y + swimCollider.bounds.extents.y;
            bool didWadingHit = Physics.Raycast(this.transform.position + Vector3.up * cc.height, Vector3.down, out RaycastHit wadingHit, 10f, MaskReference.Terrain);
            float slopeAngle = -1f;
            if (didWadingHit)
            {
                slopeAngle = Vector3.Angle(Vector3.up, wadingHit.normal);
            }
            bool slopeOK = slopeAngle <= cc.slopeLimit;
            if (didWadingHit && slopeOK)
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

    public override IInventory GetInventory()
    {
        return inventory;
    }
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
        MixerTransition2DAsset movementAnim = unarmedStance;

        if (inventory.IsMainDrawn() || inventory.IsOffDrawn())
        {
            movementAnim = armedStance;
        }
        else if (inventory.IsRangedDrawn())
        {
            movementAnim = bowWalkStance;
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

        MixerTransition2DAsset blockingMoveAnim = strafeAnimDefault;
        ClipTransition guardBreak = null;
        EquippableWeapon blockWeapon = inventory.GetBlockWeapon();
        if (blockWeapon != null)
        {
            blockingMoveAnim = blockingStance;
            blockAnim = blockWeapon.moveset.blockAnim;
            blockAnimStart = blockWeapon.moveset.blockAnimStart;
            blockStagger = blockWeapon.moveset.blockStagger;

            guardBreak = blockWeapon.moveset.guardBreak;

            if (blockWeapon.moveset.hasTypedBlocks && blockWeapon.moveset.blockAnimsTyped != null)
            {
                state.upperBlock = (MixerState<float>)animancer.States.GetOrCreate(blockWeapon.moveset.blockAnimsTyped);
                hasTypedBlocks = true;
            }
            else
            {
                hasTypedBlocks = false;
            }

        }

        state.block = (MixerState)animancer.States.GetOrCreate(blockingMoveAnim);
        //damageHandler.SetBlockClip(blockStagger);
        damageHandler.SetGuardBreakClip(guardBreak);

        //UpdateStances();


        ClipTransition sprintingAnim = sprintAnim;
        if (inventory.IsMainDrawn() && inventory.GetMainWeapon().moveset.overridesSprint)
        {
            sprintingAnim = inventory.GetMainWeapon().moveset.sprintAnim;
        }

        currentSprintAnim = sprintingAnim;

        if (inventory.IsRangedEquipped())
        {
            aimAnim = bowAimStance;
        }
        else
        {
            aimAnim = aimAnimDefault;
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

    /*
    public void UpdateStances()
    {
        return;
        primaryStance = (inventory.TryGetRightHandedWeapon(out EquippableWeapon rweapon)) ? rweapon.primaryStance : null;
        secondaryStance = (inventory.TryGetLeftHandedWeapon(out EquippableWeapon lweapon)) ? lweapon.secondaryStance : null;
        ApplyIdleBlends();
    }
    */

    public AnimancerState TriggerSheath(bool draw, Inventory.EquipSlot slot, int targetSlot)
    {
        void _OnSheathEnd()
        {
            StartCoroutine("SleepCloth");
            _StopUpperLayer();
        }
        isSheathing = true;
        if ((targetSlot == Inventory.MainType) && inventory.IsMainEquipped())
        {
            AnimancerState drawState = animancer.Layers[HumanoidAnimLayers.UpperBody].Play((draw) ? inventory.GetMainWeapon().moveset.draw : inventory.GetMainWeapon().moveset.sheathe);
            drawState.Events.OnEnd = _OnSheathEnd;
            equipToType = targetSlot;
            return drawState;
        }
        else if ((targetSlot == Inventory.OffType) && inventory.IsOffEquipped())
        {
            AnimancerState drawState = animancer.Layers[HumanoidAnimLayers.UpperBody].Play((draw) ? inventory.GetOffWeapon().moveset.draw : inventory.GetOffWeapon().moveset.sheathe);
            drawState.Events.OnEnd = _OnSheathEnd;
            equipToType = targetSlot;
            return drawState;
        }
        else if ((targetSlot == Inventory.RangedType) && inventory.IsRangedEquipped())
        {
            AnimancerState drawState = animancer.Layers[HumanoidAnimLayers.UpperBody].Play((draw) ? inventory.GetRangedWeapon().moveset.draw : inventory.GetRangedWeapon().moveset.sheathe);
            drawState.Events.OnEnd = _OnSheathEnd;
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

        if (weaponModel == null) return;
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

        if (weaponModel == null) return;
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
        while (mainWeaponAngle != 0f && !IsAttacking())
        {
            angle = Mathf.MoveTowards(mainWeaponAngle, 0f, 360f * Time.deltaTime);
            RotateMainWeapon(angle);
            yield return null;
        }
    }

    IEnumerator GradualResetOffRotation()
    {
        float angle;
        while (offWeaponAngle != 0f && !IsAttacking())
        {
            angle = Mathf.MoveTowards(offWeaponAngle, 0f, 360f * Time.deltaTime);
            RotateOffWeapon(angle);
            yield return null;
        }
    }

    public void StartUsingConsumable(Consumable consumable)
    {
        if (consumable.CanBeUsed())
        {
            SetCurrentConsumable(consumable);
            if (consumable.generateModelOnUse)
            {
                consumableModel = consumable.GenerateModel();
                Transform parent;
                if (consumable.parentMain)
                {
                    parent = positionReference.MainHand.transform;
                }
                else if (consumable.parentOff)
                {
                    parent = positionReference.OffHand.transform;
                }
                else
                {
                    parent = positionReference.GetPositionRefSlot(consumable.parentSlot).transform;

                }
                if (consumableModel != null && parent != null)
                {
                    consumableModel.transform.SetParent(parent);
                    consumableModel.transform.localPosition = Vector3.zero;
                    consumableModel.transform.localRotation = Quaternion.identity;
                }

            }
            if (consumable.sheatheMainOnUse)
            {
                inventory.SetDrawn(true, false);
            }
            if (consumable.sheatheOffOnUse)
            {
                inventory.SetDrawn(false, false);
            }
            state.consume = consumable.GetAction().ProcessPlayerAction(this, out cancelTime,() =>
            {
                if (consumable.generateModelOnUse && consumableModel != null)
                {
                    Destroy(consumableModel);
                }
                animancer.Play(state.move, 0.5f);
                DestroyEndEvents(ref state.consume);
            });

        }
    }

    public bool CanBlock()
    {
        return inventory.IsOffEquipped() && inventory.GetOffWeapon() is OffHandShield;
    }

    public bool CanRanged()
    {
        return inventory.IsRangedEquipped() && inventory.GetRangedWeapon() is RangedWeapon;
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
        Collider carryCollider = c.GetComponent<Collider>();
        Collider[] colliders = GetColliders();
        foreach (Collider collider in colliders)
        {
            Physics.IgnoreCollision(carryCollider, collider);
        }
        //Physics.IgnoreCollision(this.GetComponent<Collider>(), carryCollider);
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
        Collider[] colliders = GetColliders();
        yield return new WaitForSeconds(0.5f);
        if (!((isCarrying || animancer.States.Current == state.carry) && carryable != null && this.carryable == carryable))
        {
            Collider carryCollider = carryable.GetComponent<Collider>();
            foreach (Collider collider in colliders)
            {
                Physics.IgnoreCollision(carryCollider, collider, false);
            }
        }

    }

    Collider[] GetColliders()
    {
        List<Collider> colliders = new List<Collider>();
        colliders.Add(cc);
        colliders.Add(this.GetComponent<Collider>());
        colliders.AddRange(this.GetComponentsInChildren<Collider>());
        return colliders.ToArray();
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
        System.Action endAction = (GetMoveset().quickSlash1h != null && GetMoveset().quickSlash1h is ComboAttack) ? _MoveOnEnd : _AttackEnd;
        state.attack = GetMoveset().quickSlash1h.ProcessPlayerAction(this, out cancelTime, endAction);
        attackDecelReal = attackDecel;
        ClearSlashInput();
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
        state.attack = GetMoveset().quickThrust1h.ProcessPlayerAction(this, out cancelTime, endAction);
        attackDecelReal = attackDecel;
        ClearThrustInput();
        OnAttack.Invoke();
        hold = false;
    }

    public void PowerSlash()
    {
        InputAttack attack = GetMoveset().powerSlash;
        if (attack is HoldAttack holdAttack)
        {
            hold = true;
            AnimancerState atkState = animancer.Play(holdAttack.GetStartClip());
            atkState.Events.OnEnd = () => { state.holdAttack = state.attack = animancer.Play(holdAttack.GetLoopClip()); };
            state.holdAttack = state.attack = atkState;
            holdAttackClock = holdAttack.chargeTime;
            holdAttackMin = holdAttack.chargeTime - holdAttack.minDuration;
            cancelTime = -1f;
            //state.attack.Events.OnEnd = () => { HoldSlashRelease(true); };
            SetCurrentDamage(attack.GetDamage());
        }
        else
        {
            hold = false;
            state.attack = GetMoveset().powerSlash.ProcessPlayerAction(this, out cancelTime, _AttackEnd);
        }
        ClearSlashInput();
        OnAttack.Invoke();
        attackDecelReal = attackDecel;
    }

    public void PowerThrust()
    {
        InputAttack attack = GetMoveset().powerThrust;
        if (attack is HoldAttack holdAttack)
        {
            hold = true;
            AnimancerState atkState = animancer.Play(holdAttack.GetStartClip());
            atkState.Events.OnEnd = () => {
                state.holdAttack = state.attack = animancer.Play(holdAttack.GetLoopClip());
            };
            state.holdAttack = state.attack = atkState;
            holdAttackClock = holdAttack.chargeTime;
            holdAttackMin = holdAttack.chargeTime - holdAttack.minDuration;
            cancelTime = -1f;
            //state.attack.Events.OnEnd = () => { HoldThrustRelease(true); };
            SetCurrentDamage(attack.GetDamage());
        }
        else
        {
            hold = false;
            state.attack = GetMoveset().powerSlash.ProcessPlayerAction(this, out cancelTime, _AttackEnd);
        }
        ClearThrustInput();
        OnAttack.Invoke();
        attackDecelReal = attackDecel;
    }

    public void HoldSlashRelease(bool wasFullyCharged)
    {
        hold = false;
        holdAttackClock = 10f;
        if (GetMoveset().powerSlash is not HoldAttack holdAttack)
        {
            _AttackEnd();
            return;
        }
        else {
            //state.attack = animancer.Play(holdAttack.GetClip());
            if (wasFullyCharged)
            {
                state.attack = holdAttack.chargedAttack.ProcessPlayerAction(this, out cancelTime, _AttackEnd);
                //SetCurrentDamage(holdAttack.GetDamage());
            }
            else
            {
                state.attack = holdAttack.unchargedAttack.ProcessPlayerAction(this, out cancelTime, _AttackEnd);
                //SetCurrentDamage(holdAttack.GetHeldDamage());
            }
            //state.attack.Events.OnEnd = _AttackEnd;
        }
        ClearSlashInput();
    }

    public void HoldThrustRelease(bool wasFullyCharged)
    {
        hold = false;
        holdAttackClock = 10f;
        if (GetMoveset().powerThrust is not HoldAttack holdAttack)
        {
            _AttackEnd();
            return;
        }
        else
        {
            //state.attack = animancer.Play(holdAttack.GetClip());
            if (wasFullyCharged)
            {
                state.attack = holdAttack.chargedAttack.ProcessPlayerAction(this, out cancelTime, _AttackEnd);
                //SetCurrentDamage(holdAttack.GetDamage());
            }
            else
            {
                state.attack = holdAttack.unchargedAttack.ProcessPlayerAction(this, out cancelTime, _AttackEnd);
                //SetCurrentDamage(holdAttack.GetHeldDamage());
            }
            //state.attack.Events.OnEnd = _AttackEnd;
        }
        ClearThrustInput();
    }

    public void CancelSlash()
    {
        state.attack = GetMoveset().quickSlash1h.ProcessPlayerAction(this, out cancelTime, _AttackEnd);
        ClearSlashInput();
        OnAttack.Invoke();
    }

    public void CancelThrust()
    {
        state.attack = GetMoveset().quickThrust1h.ProcessPlayerAction(this, out cancelTime, _AttackEnd);
        ClearThrustInput();
        OnAttack.Invoke();
    }
    public void DashSlash()
    {
        state.attack = GetMoveset().dashSlash.ProcessPlayerAction(this, out cancelTime, _AttackEnd);
        attackDecelReal = dashAttackDecel;
        dashed = false;
        ClearSlashInput();
        OnAttack.Invoke();
    }

    public void DashThrust()
    {
        state.attack = GetMoveset().dashThrust.ProcessPlayerAction(this, out cancelTime, _AttackEnd);
        attackDecelReal = dashAttackDecel;
        dashed = false;
        ClearThrustInput();
        OnAttack.Invoke();
    }


    public void BlockSlash()
    {
        System.Action _BlockAttackEnd = () => {
            if (blocking)
            {
                //animancer.Layers[HumanoidAnimLayers.UpperBody].Play(blockAnim, 0.25f);
                animancer.Play(state.block, 0.25f);
            }
            else
            {
                _MoveOnEnd();
            }
        };
        if (inventory.IsOffDrawn() && GetMovesetOff().stanceSlash != null)
        {
            state.attack = GetMovesetOff().stanceSlash.ProcessPlayerAction(this, out cancelTime, _BlockAttackEnd);
        }
        else if (inventory.IsMainDrawn() && GetMoveset().stanceSlash != null)
        {
            state.attack = GetMoveset().stanceSlash.ProcessPlayerAction(this, out cancelTime, _BlockAttackEnd);
        }
        else
        {
            MainSlash();
        }
        ClearSlashInput();
        OnAttack.Invoke();
    }
    public void BlockThrust()
    {
        System.Action _BlockAttackEnd = () => {
            if (blocking)
            {
                //animancer.Layers[HumanoidAnimLayers.UpperBody].Play(blockAnim, 0.25f);
                animancer.Play(state.block, 0.25f);
            }
            else
            {
                _MoveOnEnd();
            }
        };
        if (inventory.IsOffDrawn() && GetMovesetOff().stanceThrust != null)
        {
            state.attack = GetMovesetOff().stanceThrust.ProcessPlayerAction(this, out cancelTime, _BlockAttackEnd);
        }
        else if (inventory.IsMainDrawn() && GetMoveset().stanceThrust != null)
        {
            state.attack = GetMoveset().stanceThrust.ProcessPlayerAction(this, out cancelTime, _BlockAttackEnd);
        }
        else
        {
            MainThrust();
        }
        ClearThrustInput();
        OnAttack.Invoke();
    }
    public void RollSlash()
    {
        state.attack = GetMoveset().rollSlash.ProcessPlayerAction(this, out cancelTime, _MoveOnEnd);
        attackDecelReal = dashAttackDecel;
        rollAnim.Events.OnEnd = () => { animancer.Play(state.move, 0.5f); };
        dashed = false;
        ClearSlashInput();
        OnAttack.Invoke();
    }

    public void RollThrust()
    {
        state.attack = GetMoveset().rollThrust.ProcessPlayerAction(this, out cancelTime, _MoveOnEnd);
        attackDecelReal = dashAttackDecel;
        rollAnim.Events.OnEnd = () => { animancer.Play(state.move, 0.5f); };
        dashed = false;
        ClearThrustInput();
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
            plunge = true;
        }
        else
        {
            if (didAirJump) return;
            state.jump = state.attack = GetMoveset().plungeSlash.ProcessPlayerAction(this, out cancelTime, () =>
            {
                if (IsGrounded())
                {
                    _AttackEnd();
                }
                else
                {
                    animancer.Play(state.fall, 0.5f);
                }
            });
            didAirJump = true;
        }
        //plunge = true;
        SetCurrentDamage(GetMoveset().plungeSlash.GetDamage());
        ClearSlashInput();
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
            plungeEnd.Events.OnEnd = _AttackEnd;// = () => { animancer.Play(state.move, 0.5f); };
        }
        plunge = true;
        SetCurrentDamage(GetMoveset().plungeThrust.GetDamage());
        ClearThrustInput();
        OnAttack.Invoke();
    }

    void HandleAirAttacks()
    {
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

    public void ClearAttackInput()
    {
        buffer.ClearInput(InputBuffer.Inputs.Slash);
        buffer.ClearInput(InputBuffer.Inputs.Thrust);
        buffer.ClearInput(InputBuffer.Inputs.SlashHold);
        buffer.ClearInput(InputBuffer.Inputs.ThrustHold);
    }

    public void ClearSlashInput()
    {
        buffer.ClearInput(InputBuffer.Inputs.Slash);
        buffer.ClearInput(InputBuffer.Inputs.SlashHold);
    }

    public void ClearThrustInput()
    {
        buffer.ClearInput(InputBuffer.Inputs.Thrust);
        buffer.ClearInput(InputBuffer.Inputs.ThrustHold);
    }
    public void Aim()
    {
        state.aim = animancer.Play(aimAnim);
        aimForwardVector = this.transform.forward;
        aimTime = 0f;
    }

    public override void RealignToTarget()
    {
        Quaternion targetRot;
        Vector3 dir = transform.forward;
        if (this.GetCombatTarget() != null)
        {
            dir = this.GetCombatTarget().transform.position - this.transform.position;
            dir.y = 0f;
        }
        else
        {
            dir = Camera.main.transform.forward * move.y + Camera.main.transform.right * move.x;
            dir.y = 0f;
        }
        float maxRotation = 180f;
        if (sprinting)
        {
            maxRotation = 90f;
        }
        if (dir.magnitude > 0)
        {
            targetRot = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(dir, Vector3.up), maxRotation);
            StartCoroutine(RealignCoroutine(targetRot, 0.05f));
        }
        //this.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        //animancer.Animator.rootRotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    IEnumerator RealignCoroutine(Quaternion target, float timeToRealign)
    {
        float time = 0f;
        float t;
        Quaternion initRot = this.transform.rotation;
        while (time < timeToRealign)
        {
            time += Time.deltaTime;
            t = Mathf.Clamp01(time / timeToRealign);
            this.transform.rotation = Quaternion.Lerp(initRot, target, t);
            animancer.Animator.rootRotation = Quaternion.Lerp(initRot, target, t);
            yield return null;
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
            if (attack)
            {
                yield break;
            }
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


    public void ParryActive(float duration)
    {
        state.parry = state.attack;
        parryTime = Time.time + duration;
        OnParryStart.Invoke();
    }

    public bool ShouldParry(DamageKnockback incomingDamage)
    {
        if (IsParrying())
        {
            if (incomingDamage.breaksBlock || incomingDamage.breaksArmor || incomingDamage.cannotRecoil)
            {
                return false;
            }
            else
            {
                DamageKnockback lastAttackDamage = GetLastDamage();
                if (lastAttackDamage != null && lastAttackDamage.isParry)
                {
                    if (lastAttackDamage.isSlash)
                    {
                        return incomingDamage.isSlash; // slashing parry
                    }
                    else if (lastAttackDamage.isThrust)
                    {
                        return incomingDamage.isThrust; // thrusting parry
                    }
                    else
                    {
                        return true; // omni parry
                    }
                }
            }
        }
        return false;
    }

    public bool IsParrying()
    {
        return (animancer.States.Current == state.parry || animancer.States.Current == state.block) && Time.time <= parryTime;
    }

    public bool IsParrySlash()
    {
        DamageKnockback lastAttackDamage = GetLastDamage();
        return lastAttackDamage != null && lastAttackDamage.isParry && lastAttackDamage.isSlash;
    }

    public bool IsParryThrust()
    {
        DamageKnockback lastAttackDamage = GetLastDamage();
        return lastAttackDamage != null && lastAttackDamage.isParry && lastAttackDamage.isThrust;
    }

    public void BasicAttack()
    {
        if (attack)
        {
            if (!inventory.IsMainDrawn() & inventory.IsMainEquipped())
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
            attack = false;
            slash = false;
            thrust = false;
        }
    }

    public void ResetAttackCancelAction()
    {
        cancelAction = AttackCancelAction.None;
    }
    [Serializable]
    enum AttackCancelAction
    {
        None,
        Slash,
        Thrust,
        Jump,
        InventorySlot0,
        InventorySlot1,
        InventorySlot2,
        InventorySlot3
    }

    public override void DeactivateHitboxes()
    {
        HitboxActive(0);
    }
    public override void FlashWarning(int hand)
    {
        //return; // flash won't appear for player

        EquippableWeapon mainWeapon = inventory.GetMainWeapon();
        EquippableWeapon offHandWeapon = inventory.GetOffWeapon();
        EquippableWeapon rangedWeapon = inventory.GetRangedWeapon();
        bool main = (mainWeapon != null && mainWeapon is IHitboxHandler);
        bool off = (offHandWeapon != null && offHandWeapon is IHitboxHandler);
        bool ranged = (rangedWeapon != null && rangedWeapon is IHitboxHandler);
        if (hand == 1 && main)
        {
            mainWeapon.FlashWarning();
        }
        else if (hand == 2 && off)
        {
            offHandWeapon.FlashWarning();
        }
        else if (hand == 3)
        {
            if (main)
            {
                mainWeapon.FlashWarning();
            }
            if (off)
            {
                offHandWeapon.FlashWarning();
            }
        }
        else if (hand == 4 && ranged)
        {
            rangedWeapon.FlashWarning();
        }

    }

    // called by animation events
    public void UseConsumable()
    {
        if (currentConsumable != null)
        {
            currentConsumable.UseConsumable();
        }
    }

    public override void SetCurrentDamage(DamageKnockback damageKnockback)
    {
        if (damageKnockback == null)
        {
            currentDamage = new DamageKnockback();
            currentDamage.source = this.gameObject;
            throw new NullReferenceException("Damage Knockback data is null");
        }
        else
        {
            if (currentDamage != null)
            {
                currentDamage.OnHit.RemoveListener(RegisterHit);
                currentDamage.OnHitWeakness.RemoveListener(HitWeakness);
            }
            currentDamage = new DamageKnockback(damageKnockback);
            currentDamage.source = this.gameObject;
            currentDamage.OnHit.AddListener(RegisterHit);
            currentDamage.OnHitWeakness.AddListener(HitWeakness);
        }
    }

    public DamageKnockback GetLastDamage()
    {
        return currentDamage;
    }

    public DamageKnockback GetLastTakenDamage()
    {
        return ((IDamageable)damageHandler).GetLastTakenDamage();
    }
    public Consumable GetCurrentConsumable()
    {
        return currentConsumable;
    }

    public override DamageResistance GetBlockResistance()
    {
        return inventory.GetBlockResistance();
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
        if (!ShouldParry(damage))
        {
            if (!isGoddess)
            {
                damageHandler.TakeDamage(damage);
            }
            ClearAttackInput();
        }
        else
        {
            if (damage.source.TryGetComponent<IDamageable>(out IDamageable damageSource))
            {
                damageSource.GetParried();
            }
            OnParrySuccess.Invoke();
        }
    }

    public void StartCritVulnerability(float time)
    {
        damageHandler.StartCritVulnerability(time);
    }

    public override void Die()
    {
        if (dead) return;
        dead = true;
        if (attributes.lives <= 0)
        {
            base.Die();

            ProcessDeath();
        }
        else
        {

            Resurrect();
        }
    }

    public void Resurrect()
    {
        resurrecting = true;
        attributes.lives--;

        StartCoroutine("ResurrectCoroutine");
        OnDie.Invoke();
    }

    public void Resurrect(AnimancerState rez)
    {
        resurrecting = true;
        attributes.lives--;
        OnDie.Invoke();
        rez.Speed *= 0.1f;
        ResurrectAnimation(rez);
    }
    public void ResurrectAnimation(AnimancerState rez)
    {
        rez.Events.OnEnd = () =>
        {
            _MoveOnEnd();
            dead = false;
            resurrecting = false;
        };
        attributes.RecoverHealth(999f);
    }

    IEnumerator ResurrectCoroutine()
    {
        yield return new WaitForSecondsRealtime(1f);
        bool facingUp = damageHandler.isFacingUp;
        state.resurrect = animancer.Play(facingUp ? resurrectFaceUp : resurrectFaceDown);
        ResurrectAnimation(state.resurrect);
        yield return new WaitForSecondsRealtime(5f);
        while (resurrecting)
        {
            _MoveOnEnd();
            dead = false;
            resurrecting = false;
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    public void HitWeakness()
    {
        OnHitWeakness.Invoke();
    }

    void _OnDodgeEnd()
    {
        if (!isGrounded && airTime > 0.1f)
        {
            state.fall = animancer.Play(fallAnim);
        }
        else if (blocking)
        {
            animancer.Play(state.block, 0.5f);
        }
        else
        {
            animancer.Play(state.move, 0.5f);
        }
    }

    public void ProcessWeaponDash()
    {
        if (isGrounded)
        {
            float dist = (GetCombatTarget() != null) ? Vector3.Distance(GetCombatTarget().transform.position, this.transform.position) : 0f;
            if (stickDirection.magnitude > 0 || (camState == CameraState.Lock && GetCombatTarget() != null && dist > weaponDashMinDist && dist < weaponDashMaxDist))
            {
                if (speed < weaponDashSpeed)
                {
                    speed = weaponDashSpeed;
                }
            }
        }
    }
    #endregion

    #region IK & Root Motion

    void GetHeadPoint()
    {
        float dist = 10f;
        Vector3 point = Vector3.zero;
        if (shouldLookAtCamera)
        {
            point = Camera.main.transform.position;
        }
        else if (this.GetCombatTarget() != null)
        {
            point = this.GetCombatTarget().transform.position;
        }
        else if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, dist, MaskReference.Terrain))
        {
            point = hit.point;
        }
        else
        {
            point = Camera.main.transform.position + Camera.main.transform.forward * 10f;
        }
        if (headPoint == Vector3.zero || Vector3.Distance(headPoint, point) > dist)
        {
            headPoint = point;
        }
        else
        {
            headPoint = Vector3.MoveTowards(headPoint, point, headPointSpeed * Time.deltaTime);
        }
        Debug.DrawLine(positionReference.Head.position, point, Color.cyan);
        Debug.DrawLine(positionReference.Head.position, headPoint, Color.magenta);
    }

    public void LookAtCamera(bool isOn)
    {
        shouldLookAtCamera = isOn;
    }
    public override Vector3 GetLaunchVector(Vector3 origin)
    {

        GameObject target = this.GetCombatTarget();
        if (target != null)
        {
            if (Vector3.Distance(target.transform.position, origin) > 2)
            {
                lastLaunchVector = (target.transform.position - origin).normalized;
                return lastLaunchVector;
            }
            else
            {
                lastLaunchVector = this.transform.forward;
                return lastLaunchVector;
            }
        }
        else if (IsAiming())
        {
            Vector3 aimPos = Camera.main.transform.position + Camera.main.transform.forward * 100f;

            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 100f, LayerMask.GetMask("Terrain", "Terrain_World1Only", "Terrain_World2Only", "Actors", "Default", "Wall", "World1Only", "World2Only", "Terrain_Invisible")) && !hit.transform.IsChildOf(this.transform.root))
            {
                aimPos = hit.point;

            }
            Debug.DrawLine(origin, aimPos, Color.red);
            lastLaunchVector = (aimPos - origin).normalized;
            return lastLaunchVector;
        }
        else
        {
            lastLaunchVector = this.transform.forward;
            return lastLaunchVector;
        }
    }
    public override bool ShouldCalcFireStrength()
    {
        return true;
    }
    private void OnAnimatorIK(int layerIndex)
    {
        //if (CanUpdate()) return;
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
        else if (IsAiming() && camState == CameraState.Aim && inventory.IsRangedDrawn())
        {
            inventory.GetRangedWeapon().moveset.aimAttack.OnIK(animancer.Animator);
        }
        else
        {
            GetHeadPoint();
            animancer.Animator.SetLookAtPosition(headPoint);
            if (IsAiming() || this.GetCombatTarget() != null || shouldLookAtCamera)
            {
                animancer.Animator.SetLookAtWeight(headPointWeights[0], headPointWeights[1], headPointWeights[2], headPointWeights[3], headPointWeights[4]);
            }
            else
            {
                animancer.Animator.SetLookAtWeight(0f);
            }
            thrustIKWeight = 0f;

            if (sliding)
            {
                Plane groundPlane = new Plane(groundNormal.normalized, groundPoint);
                Vector3 horizNormal = Vector3.ProjectOnPlane(groundNormal, Vector3.up).normalized;
                Vector3 handL = animancer.Animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
                float distanceL = groundPlane.GetDistanceToPoint(handL);
                Vector3 handR = animancer.Animator.GetBoneTransform(HumanBodyBones.RightHand).position;
                float distanceR = groundPlane.GetDistanceToPoint(handR);

                Vector3 hips = animancer.Animator.GetBoneTransform(HumanBodyBones.Hips).position;
                float distanceHips = groundPlane.GetDistanceToPoint(hips);

                this.transform.rotation = Quaternion.LookRotation(xzVel.normalized);
                animancer.Animator.bodyPosition = animancer.Animator.bodyPosition + groundNormal.normalized * (-distanceHips + 0.15f);



                animancer.Animator.SetIKPosition(AvatarIKGoal.LeftHand, groundPlane.ClosestPointOnPlane(handL));
                animancer.Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
                animancer.Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
                animancer.Animator.SetIKPosition(AvatarIKGoal.LeftFoot, groundPlane.ClosestPointOnPlane(animancer.Animator.GetBoneTransform(HumanBodyBones.LeftFoot).position));
                animancer.Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
                animancer.Animator.SetIKPosition(AvatarIKGoal.RightFoot, groundPlane.ClosestPointOnPlane(animancer.Animator.GetBoneTransform(HumanBodyBones.RightFoot).position));
                animancer.Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            }
        }
        if (!sliding && wasSlidingIK)
        {
            animancer.Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
            animancer.Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
        }
        wasSlidingIK = sliding;

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


    /*
    public void ApplyIdleBlends()
    {
        AnimancerLayer primaryLayer = animancer.Layers[HumanoidAnimLayers.Open2];
        AnimancerLayer secondaryLayer = animancer.Layers[HumanoidAnimLayers.Open1];

        primaryLayer.Weight = 0f;

        if (primaryStance != null && primaryStance.blendWeight > 0f)
        {
            state.primaryStance = (MixerState<Vector2>)primaryLayer.Play(primaryStance.blendStance);
            primaryLayer.SetMask(primaryStance.blendMask);
            primaryLayer.IsAdditive = primaryStance.additive;
        }
        else if (primaryLayer.IsAnyStatePlaying())
        {
            primaryLayer.Stop();
        }

        secondaryLayer.Weight = 0f;

        if (secondaryStance != null && secondaryStance.blendWeight > 0f)
        {
            state.secondaryStance = (MixerState<Vector2>)secondaryLayer.Play(secondaryStance.blendStance);
            secondaryLayer.SetMask(secondaryStance.blendMask);
            secondaryLayer.IsAdditive = secondaryStance.additive;
        }
        else if (secondaryLayer.IsAnyStatePlaying())
        {
            secondaryLayer.Stop();
        }
    }
    */
    /*
    public void HandleIdleBlends()
    {
        AnimancerLayer primaryLayer = animancer.Layers[HumanoidAnimLayers.Open2];
        AnimancerLayer secondaryLayer = animancer.Layers[HumanoidAnimLayers.Open1];

        float primaryWeight = 0f;
        float secondaryWeight = 0f;

        if (animancer.States.Current == state.move || (animancer.States.Current == state.block && !IsBlocking()))
        {
            float stateWeight = 1f;
            if (animancer.States.Current == state.move)
            {
                stateWeight = state.move.Weight;
            }
            else if (animancer.States.Current == state.block)
            {
                stateWeight = state.block.Weight;
            }
            if (inventory.HasRightHandedWeapon() && primaryStance != null && primaryStance.blendWeight > 0f)
            {
                primaryWeight = primaryStance.blendWeight * stateWeight;
                state.primaryStance.Parameter = GetMovementVector();
                state.primaryStance.NormalizedTime = animancer.States.Current.NormalizedTime;
            }
            if (inventory.HasLeftHandedWeapon() && secondaryStance != null && secondaryStance.blendWeight > 0f)
            {
                secondaryWeight = secondaryStance.blendWeight * stateWeight;
                state.secondaryStance.Parameter = GetMovementVector();
                state.secondaryStance.NormalizedTime = animancer.States.Current.NormalizedTime;
            }
        }

        primaryLayer.Weight = primaryWeight;
        secondaryLayer.Weight = secondaryWeight;
    }
    */
    #endregion

    #region Anim End Events

    private void _MoveOnEnd()
    {
        animancer.Play(state.move, 0.1f);
    }

    void _OnLandEnd()
    {
        walkAccelReal = walkAccel;
    }

    void _OnFinishClimb()
    {
        cc.enabled = true;
        airTime = 0f;
        yVel = 0f;
        xzVel = Vector3.zero;
        animancer.Play(state.move, 0.25f);
    }

    void _OnHurtEnd()
    {
        bool blocked = CheckStartBlockAnim();
        if (!blocked)
        {
            _MoveOnEnd();
        }
        if (mainWeaponAngle != 0f)
        {
            StartCoroutine("GradualResetMainRotation");
        }
        if (offWeaponAngle != 0f)
        {
            StartCoroutine("GradualResetOffRotation");
        }
    }

    void _AttackEnd()
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
        cancelAction = AttackCancelAction.None;
    }

    void _StopUpperLayer()
    {
        animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
    }

    void _EndDrink()
    {
        _StopUpperLayer();
        _MoveOnEnd();
    }

    void DestroyEndEvents(ref AnimancerState state)
    {
        state.Events.OnEnd = null;
    }

    #endregion

    #region State Checks

    public override bool IsAlive()
    {
        return !dead;
    }

    public bool IsResurrecting()
    {
        return resurrecting;
    }
    public bool IsSwimming()
    {
        if (animancer == null) return false;
        return animancer.States.Current == state.swim;
    }
    public bool IsMoving()
    {
        if (animancer == null) return false;
        return animancer.States.Current == state.move;
    }
    public bool IsAiming()
    {
        //return IsAimHeld();
        return animancer.States.Current == state.aim;// && (state.aim != state.move || aiming);
    }

    public override bool IsFalling()
    {
        if (animancer == null) return false;
        return animancer.States.Current == state.fall || animancer.States.Current == damageHandler.fall;
    }

    public void IsCaughtOnEdge()
    {

    }
    public bool IsHurt()
    {
        if (animancer == null) return false;
        return animancer.States.Current == damageHandler.hurt;
    }

    public bool IsProne()
    {
        if (animancer == null) return false;
        return animancer.States.Current == damageHandler.fall || animancer.States.Current == damageHandler.rise || resurrecting;

    }
    public override bool IsClimbing()
    {
        if (animancer == null) return false;
        return animancer.States.Current == state.climb;
    }

    public override bool ShouldDustOnStep()
    {
        if (animancer == null) return false;
        return animancer.States.Current == state.sprint || animancer.States.Current == state.dash;
    }
    public override bool IsGrounded()
    {
        return isGrounded;
    }
    public override bool IsDodging()
    {
        if (animancer == null) return false;
        return animancer.States.Current == state.roll;
    }
    public override bool IsJumping()
    {
        if (animancer == null) return false;
        return !IsGrounded() || animancer.States.Current == state.jump;
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
        if (animancer == null) return false;
        return animancer.States.Current == state.attack;
    }
    public override bool IsBlocking()
    {
        if (animancer == null) return false;
        return CanBlock() && (animancer.States.Current == state.block || animancer.States.Current == damageHandler.block);
    }

    public bool IsBlockingSlash()
    {
        return IsBlocking() && hasTypedBlocks && (/*lastTypedBlockParam == DamageKnockback.SLASH_INT */ (IsSlashHeld() && !IsThrustHeld()) || buffer.PollInput(blockShiftBufferWindow) == InputBuffer.Inputs.Slash);
    }

    public bool IsBlockingThrust()
    {
        return IsBlocking() && hasTypedBlocks && (/*lastTypedBlockParam == DamageKnockback.THRUST_INT */ (IsThrustHeld() && !IsSlashHeld()) || buffer.PollInput(blockShiftBufferWindow) == InputBuffer.Inputs.Thrust);
    }

    public bool IsTypedBlocking()
    {
        return IsBlockingSlash() || IsBlockingThrust();
    }

    public bool HasTypedBlocks()
    {
        return hasTypedBlocks;
    }

    public bool IsInDialogue()
    {
        if (animancer == null) return false;
        return animancer.States.Current == state.dialogue;
    }

    public bool ShouldShowTargetIcon()
    {
        return this.GetCombatTarget() != null && !IsInDialogue();
    }
    public bool ShouldSlowTime()
    {
        // No slow time power for now
        return false;
    }

    public override void SetToIdle()
    {
        animancer.Play(state.move);
    }
    public bool IsSafe()
    {
        return (isGrounded && lastGroundWasStatic) &&
            animancer.States.Current != damageHandler.hurt &&
            !dead &&
            !resurrecting &&
            IsMoving() &&
            !IsInDialogue() &&
            !this.GetComponent<ActorTimeTravelHandler>().IsRewinding();
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

        interactables.Sort((a,b) =>
        {
            bool aValid = IsInteractValid(a);
            bool bValid = IsInteractValid(b);
            if (!aValid || !bValid)
            {
                return (bValid ? 1 : 0) - (aValid ? 1 : 0);
            }

            if (!aValid && !bValid)
            {
                return 0;
            }

            if (a.priority != b.priority)
            {
                return b.priority - a.priority;
            }

            float aDist = Vector3.Distance(this.transform.position, a.transform.position);
            float bDist = Vector3.Distance(this.transform.position, b.transform.position);

            return (int)Mathf.Sign(aDist - bDist);
        });


        Interactable leadInteractible = interactables.Count > 0 ? interactables[0] : null;

        if (IsInteractValid(leadInteractible))
        {
            highlightedInteractable = leadInteractible;
        }

        if (highlightedInteractable != null)
        {
            //highlightedInteractable.SetIconVisiblity(true);
        }
        if (highlightedInteractable != lastInteractable)
        {
            onNewCurrentInteractable.Invoke();
        }
        return highlightedInteractable;
    }

    bool IsInteractValid(Interactable i)
    {
        if (i == null || !i.canInteract)
        {
            return false;
        }

        if (i.maxDistance > 0)
        {
            float dist = Vector3.Distance(this.transform.position, i.transform.position);
            return dist < i.maxDistance;
        }
        return true;
    }

    private void OnInteract()
    {
        if (!CanPlayerInput()) return;
        Interactable interactable = GetHighlightedInteractable();
        if (interactable != null)
        {
            interactable.Interact(this);
        }
        else if (ProtagDialogueController.HasDialogue())
        {
            ProtagDialogueController.PlayDialogue();
            SheatheAll();
        }
        else
        {
            InputSheathe();
        }
    }

    public void StartDialogue()
    {
        state.dialogue = animancer.States.GetOrCreate("dialogue", idleAnim);
        animancer.Play(state.dialogue);
        xzVel = Vector3.zero;
    }

    public (MixerState<float>,AnimancerState) PlayDrinkClip(LinearMixerTransition walk, ClipTransition drink)
    {
        state.drink = animancer.Play(walk) as MixerState<float>;
        state.drinkUpper = animancer.Layers[HumanoidAnimLayers.UpperBody].Play(drink);
        state.drinkUpper.Events.OnEnd += _EndDrink;
        return (state.drink, state.drinkUpper);
    }
    public AnimancerState PlayDialogueClip(ClipTransition clip)
    {
        state.dialogue = animancer.Play(clip);
        xzVel = Vector3.zero;
        return state.dialogue;
    }

    public void StopDialogue()
    {
        isMenuOpen = false;
        this.SetCombatTarget(null);
        animancer.Play(state.move);
    }

    public void SetExternalSourceState(AnimancerState state, float timeout)
    {
        this.state.externalSource = state;

    }

    public void SetExternalSourceState(AnimancerState state)
    {
        float DEFAULT_TIMEOUT = 30000f;
        SetExternalSourceState(state, DEFAULT_TIMEOUT);
    }

    public void PlayMove()
    {
        _MoveOnEnd();
    }

    public void PlayJump()
    {
        state.jump = animancer.Play(standJumpAnim);
    }

    public void PlayBackflip()
    {
        //speed = -backflipSpeed;
        state.backflip = state.jump = animancer.Play(backflipAnim);
    }
    public void StandingJump()
    {
        jump = false;
        buffer.ClearInput(InputBuffer.Inputs.Jump);
        if (!IsBlockHeld())
        {
            if (stickDirection.magnitude > 0)
            {

                lookDirection = (camState == CameraState.Lock) ? camForward : stickDirection.normalized;
                moveDirection = stickDirection.normalized;
                //xzVel = xzVel.magnitude * stickDirection.normalized;
            }
            PlayJump();//animancer.Play((move.magnitude < 0.5f) ? standJumpAnim : runJumpAnim);
        }
        else
        {
            //backflip
            if (camState == CameraState.Lock)
            {
                lookDirection = camForward;
                moveDirection = lookDirection;
            }
            else if (stickDirection.magnitude > 0)
            {
                lookDirection = stickDirection.normalized;
                moveDirection = stickDirection.normalized;
            }
            else
            {
                lookDirection = this.transform.forward;
                moveDirection = this.transform.forward;
            }
            PlayBackflip();
        }
    }
#endregion

    public bool GetGrounded()
    {
        return GetGrounded(out RaycastHit rhit);
    }

    public bool GetGrounded(out RaycastHit rayHit)
    {
        if (isGroundedLockout)
        {
            rayHit = new RaycastHit();
            return false;
        }
        // return cc.isGrounded;
        float RADIUS_MULT = 1f;
        float CAST_DISTANCE = 0.2f;
        float SPHERE_CAST_DISTANCE = 0.0f;
        Collider c = cc;
        Vector3 bottom = c.bounds.center + c.bounds.extents.y * Vector3.down + Vector3.up * groundBias;
        Vector3 top = c.bounds.center + Vector3.up * c.bounds.extents.y;

        if (lastCCHit != null)
        {
            ccHit = lastCCHit;
            lastCCHit = null;
        }
        else
        {
            ccHit = null;
        }
        bool didHit = Physics.Raycast(c.bounds.center, Vector3.down, out rayHit, c.bounds.extents.y + CAST_DISTANCE, MaskReference.Terrain);
        bool didCCHit = ccHit != null && ccHit.collider != null && MaskReference.IsTerrain(ccHit.collider);

        float slopeAngle = -1f;
        if (didHit)
        {
            slopeAngle = Vector3.Angle(Vector3.up, rayHit.normal);
        }
        else if (didCCHit)
        {
            slopeAngle = Vector3.Angle(Vector3.up, ccHit.normal);
        }
        bool slopeOK = slopeAngle <= cc.slopeLimit;
        if (didHit)
        {
            lastPhysicsMaterial = rayHit.collider.sharedMaterial;
            lastGroundWasStatic = rayHit.transform.gameObject.isStatic;
        }

        if (didHit && rayHit.point.y > this.transform.position.y && rayHit.point.y - this.transform.position.y < groundBias)
        {
            withinBias = true;
            biasHeight = rayHit.point.y;
        }
        else
        {
            withinBias = false;
        }
        /*
        Color clr = didSphereHit ? Color.magenta : Color.yellow;



        Debug.DrawRay(top + this.transform.forward * cc.radius * RADIUS_MULT, Vector3.down * (c.bounds.extents.y * 2f + (SPHERE_CAST_DISTANCE)), clr);
        Debug.DrawRay(top + -this.transform.forward * cc.radius * RADIUS_MULT, Vector3.down * (c.bounds.extents.y * 2f + (SPHERE_CAST_DISTANCE)), clr);
        Debug.DrawRay(top + this.transform.right * cc.radius * RADIUS_MULT, Vector3.down * (c.bounds.extents.y * 2f + (SPHERE_CAST_DISTANCE)), clr);
        Debug.DrawRay(top + -this.transform.right * cc.radius * RADIUS_MULT, Vector3.down * (c.bounds.extents.y * 2f + (SPHERE_CAST_DISTANCE)), clr);

        DrawCircle.DrawWireSphere(top, cc.radius, clr);
        DrawCircle.DrawWireSphere(top + Vector3.down * (c.bounds.extents.y * 2f + (SPHERE_CAST_DISTANCE)), cc.radius, clr);
        */
        Debug.DrawRay(c.bounds.center, Vector3.down * (c.bounds.extents.y + CAST_DISTANCE), didHit ? Color.red : Color.cyan);
        return ((didHit || didCCHit) && slopeOK);// || cc.isGrounded;
    }

    public void StartGroundedLockout(float duration)
    {
        StartCoroutine(GroundedLockoutCoroutine(duration));
    }

    IEnumerator GroundedLockoutCoroutine(float duration)
    {
        isGroundedLockout = true;
        yield return new WaitForSecondsRealtime(duration);
        isGroundedLockout = false;
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        lastCCHit = hit;
        ccHitNormal = hit.normal;
    }

    public void SkipCloth()
    {
        Cloth cloth = this.GetComponentInChildren<Cloth>();
        if (cloth != null)
        {
            cloth.ClearTransformMotion();
        }
    }

    IEnumerator SleepCloth()
    {
        Cloth cloth = this.GetComponentInChildren<Cloth>();
        if (cloth != null)
        {
            float clock = 1f;
            float acc = cloth.worldAccelerationScale;
            float vel = cloth.worldVelocityScale;
            while (clock > 0f)
            {
                cloth.ClearTransformMotion();
                cloth.worldAccelerationScale = 0f;
                cloth.worldVelocityScale = 0f;
                yield return null;
                clock -= Time.deltaTime;
            }
            cloth.worldAccelerationScale = acc;
            cloth.worldVelocityScale = vel;
        }
    }

    public void DisableCloth()
    {
        Cloth cloth = this.GetComponentInChildren<Cloth>();
        if (cloth != null)
        {
            cloth.enabled = false;
        }
    }

    public void EnableCloth()
    {
        Cloth cloth = this.GetComponentInChildren<Cloth>();
        if (cloth != null)
        {
            cloth.enabled = true;
        }
    }
    public void HitWall()
    {
        Debug.Log("wall hit");
        Recoil();
    }

    public GameObject GetGameObject()
    {
        return ((IDamageable)damageHandler).GetGameObject();
    }

    public void GetParried()
    {
        ((IDamageable)damageHandler).GetParried();
    }

    public bool IsCritVulnerable()
    {
        return ((IDamageable)damageHandler).IsCritVulnerable();
    }

    public void StartInvulnerability(float duration)
    {
        ((IDamageable)damageHandler).StartInvulnerability(duration);
    }

    public bool IsInvulnerable()
    {
        return ((IDamageable)damageHandler).IsInvulnerable();
    }

    public void StopCritVulnerability()
    {
        ((IDamageable)damageHandler).StopCritVulnerability();
    }
}
