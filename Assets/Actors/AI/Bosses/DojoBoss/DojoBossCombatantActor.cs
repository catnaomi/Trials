using Animancer;
using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Timer 
{
    public enum TimerBehavior 
    {
        Once,
        Repeat
    }

    float time;
    float accumulated;
    bool enabled;
    TimerBehavior behavior;

    public Timer(float time, TimerBehavior behavior) 
    {
        this.time = time;
        this.accumulated = 0;
        this.enabled = true;
        this.behavior = behavior;
    }

    public Timer(TimerBehavior behavior) 
    {
        this.time = 0;
        this.accumulated = 0;
        this.behavior = behavior;
    }


    public float GetTime() 
    {
        return this.time;
    }

    public void SetTime(float time) 
    {
        this.time = time;
        this.accumulated = 0;
    }

    public void SetBehavior(TimerBehavior behavior)
    {
        this.behavior = behavior;
    }


    public void Update() 
    {
        if (!this.enabled) return;

        this.accumulated += Time.deltaTime;

        if (this.accumulated >= this.time)
        {
            if (this.behavior == TimerBehavior.Repeat)
            {
                Reset();
            }
        }
    }

    public bool Ready() 
    {
        return this.accumulated >= this.time;
    }

    public void Reset() 
    {
        accumulated = 0;
    }

    public void Enable()
    {
        this.enabled = true;
    }

    public void Disable()
    {
        this.enabled = false;
    }
}

public class Pillar
{
    public Transform Transform;
    public bool Available;
}

public class PillarInfo
{
    List<Pillar> Pillars;

    public PillarInfo()
    {
        Pillars = new List<Pillar>();
    }

    public void AddPillar(Transform Transform)
    {
        Pillar Pillar = new Pillar();
        Pillar.Transform = Transform;
        Pillar.Available = true;
        Pillars.Add(Pillar);
    }

    public Pillar GetNextAndMarkUsed()
    {
        foreach(Pillar Pillar in Pillars)
        {
            if (Pillar.Available)
            {
                Pillar.Available = false;
                return Pillar;
            }
        }

        return null;
    }

    public void MarkPillarUnused(Pillar Pillar)
    {
        foreach (Pillar OtherPillar in Pillars)
        {
            if (OtherPillar == Pillar)
            {
                Pillar.Available = true;
            }
        }
    }
}

[RequireComponent(typeof(HumanoidNPCInventory))]
public class DojoBossCombatantActor : NavigatingHumanoidActor, IAttacker, IDamageable
{
    #region shit
    HumanoidNPCInventory inventory;
    /*
     * attacks:
     * 3 hit melee combo
     * triple stab -> swipe
     * shoot from ice pillars
     * jump shot
     * jump dodge (may include shot)
     * jumping plunge (from ground)
     * jumping plunge (from pillar)
     * cross parry
     * circle parry
     * summon
     */
    [Header("Combatant Settings")]
    public InputAttack SlashAttack; 
    public InputAttack ThrustAttack;
    [Space(10)]
    public InputAttack Plunge;
    public ClipTransition PlungeJump;
    public ClipTransition PlungeFall;
    public AnimationCurve heightPlungeCurve;
    public AnimationCurve heightPlungePillarCurve;
    public AnimationCurve horizPlungeCurve;
    public AnimationCurve horizPlungePillarCurve;
    public float PlungeJumpHeight = -950f;
    public float StopAdjustPoint = 0.75f;
    public float DescentPoint = 0.75f;
    public float AttackPoint = 0.9f;
    public float PlungeTime = 5f;
    float plungeClock;
    bool plunging;
    Vector3 plungeTarget;
    Vector3 plungeStart;
    [Space(10)]
    public ClipTransition CrouchDown;
    public ClipTransition Crouch;
    public float CrouchTime = 1f;
    bool crouching;
    float crouchClock;
    public CrouchAction actionAfterCrouch;
    [Space(10)]
    public ClipTransition CrossParry;
    public MixerTransition2DAsset CrossParryMove;
    public ClipTransition IntoCrossParry;
    public ClipTransition CrossParryHit;
    public InputAttack CrossParryFollowup;
    [Space(5)]
    public ClipTransition CircleParry;
    public MixerTransition2DAsset CircleParryMove;
    public ClipTransition IntoCircleParry;
    public InputAttack CircleParryFollowup;
    [Space(5)]
    public UnityEvent ParrySuccess;
    bool wasLastParrySuccess;
    bool lastParryWasCircle;
    bool wasLastParryIgnored;
    [SerializeField,ReadOnly]float timeSinceLastParry = 0f;
    public float MinTimeBetweenParries = 10f;
    public float MaxParryTime = 30f;
    float parryTime;
    float parryStrafeTime;
    public float MaxParryOutOfBoundsTime = 5f; 
    float parryOutOfBoundsTime;
    bool crossParrying;
    bool circleParrying;
    bool spinning;
    Quaternion initRot;
    [Space(10)]
    public ClipTransition JumpDodge;
    public ClipTransition JumpLand;
    public ClipTransition DodgeBack;
    public ClipTransition DodgeLeft;
    public ClipTransition DodgeRight;
    [Space(10)]
    public AimAttack RangedAttack;
    public AimAttack RangedAttackMulti; // triple shot
    public AimAttack JumpShot;
    public ClipTransition JumpShotUp;
    public ClipTransition JumpShotLand;
    public float JumpShotTime = 0.2f;
    public float aimTime = 2f;
    [ReadOnly]public bool aiming;
    [Space(10)]
    public ClipTransition SummonStart;
    public ClipTransition SummonHold;
    public ClipTransition SummonEnd;
    public ClipTransition SpawnAnim;
    public FlyToTargetThenEmitSecondary[] SummonBalls;
    bool summoning;
    public float SummonTime;
    float summonClock;
    [Space(5)]
    public DamageAnims damageAnims;
    HumanoidDamageHandler damageHandler;
    [Space(5)]
    public ControllerTransition PillarHit;
    public ClipTransition PillarFallLand;
    public ClipTransition PillarKneel;
    public ClipTransition PillarKneelStand;
    [ReadOnly] public bool pillarStunned;
    bool pillarFallAirborne;
    public Vector3 pillarFallAdjustVector;
    public float pillarFallAdjustTime;
    public float pillarMinYVel = -1f;
    float pillarFallClock;
    public float pillarFallMaxTime = 10f;
    [Space(10)]
    public float pillarJumpDelay = 30f;
    [SerializeField, ReadOnly] float timeSincePillar;
    public float pillarStayDelay = 15f;
    [SerializeField, ReadOnly] float timeOnPillar;
    [Space(10)]
    public float AttackRotationSpeed = 720f;
    [Space(10)]
    public ControllerTransition KnockdownOverride;
    public float knockdownMoveSpeed;
    public float knockdownMoveTime;
    [Space(10)]
    public float ActionDelayMinimum = 2f;
    public float ActionDelayMaximum = 5f;
    
    public float LowHealthThreshold = 0.5f;
    public bool isLowHealth;
    bool isHitboxActive;
    public ClipTransition transformAnimReference;
    [Header("Map Information")]
    public Transform pillar1;
    public Transform pillar2;
    public Transform pillar3;
    public Transform center;
    public Transform spawn1;
    public Transform spawn2;
    public Transform spawn3;
    [ReadOnly]public bool onPillar;
    public int currentPillar = 0;
    public float nonPillarHeight = -1000f;
    [Space(10)]
    [SerializeField] AnimationCurve jumpHorizCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] AnimationCurve jumpVertCurve = AnimationCurve.Constant(0f, 1f, 0f);
    [SerializeField] float jumpVertMult = 1f;
    Vector3 startJumpPosition;
    Vector3 endJumpPosition;
    [Header("Spawn Data")]
    public GameObject stabDummy;
    public GameObject slashDummy;
    [Space(5)]
    public GameObject shieldDummy;
    [Space(5)]
    public GameObject archerDummy;
    [Space(5)]
    [SerializeField, ReadOnly] 
    List<NavigatingHumanoidActor> spawnedEnemies;
    List<NavigatingHumanoidActor> recentlySpawned;
    public int spawnLimit = 3;
    public float MinTimeBetweenSummons = 10f;
    [SerializeField, ReadOnly] float timeSinceLastSummon;
    [SerializeField, ReadOnly] bool pillar1Occupied;
    [SerializeField, ReadOnly] bool pillar2Occupied;
    [SerializeField, ReadOnly] bool pillar3Occupied;
    [SerializeField, ReadOnly] bool spawn1Occupied;
    [SerializeField, ReadOnly] bool spawn2Occupied;
    [SerializeField, ReadOnly] bool spawn3Occupied;
    [Header("Enumerated States")]
    public WeaponState weaponState;
    public UnityEvent OnWeaponTransform;
    
    Vector3 lastTargetPos;
    Vector3 targetSpeed;
    
    protected CombatState combatState;

    [Header("Particles")]
    public ParticleSystem summonParticle;
    public ParticleSystem aimParticle;
    public ParticleSystem fireParticle;
    public ParticleSystem circleParticle;
    public ParticleSystem crossParticle;
    public ParticleSystem jumpParticle;
    public ParticleSystem hammerParticle;
    protected struct CombatState
    {
        public AnimancerState attack;
        public AnimancerState jump;
        public AnimancerState plunge_rise;
        public AnimancerState plunge_fall;
        public AnimancerState plunge_attack;
        public AnimancerState ranged_idle;
        public AnimancerState ranged_air;
        public DirectionalMixerState parry_cross;
        public DirectionalMixerState parry_circle;
        public AnimancerState hurt;
        public AnimancerState summon;
        public AnimancerState pillar_fall;
        public AnimancerState dodge;
    }

    public enum WeaponState
    {
        None,           // 0
        Quarterstaff,   // 1
        Scimitar,       // 2
        Greatsword,     // 3
        Rapier,         // 4
        Bow,            // 5
        Hammer,         // 6
        Daox2,          // 7
        MagicStaff,     // 8
        Spear           // 9
    }

    public enum CrouchAction
    {
        Plunge,
        JumpTo_Pillar1,
        JumpTo_Pillar2,
        JumpTo_Pillar3,
        JumpTo_Center,
        JumpShot_Pillar1,
        JumpShot_Pillar2,
        JumpShot_Pillar3,
        JumpShot_Center,

    }
    System.Action _MoveOnEnd;
    #endregion

    public class State
    {
        public DojoBossCombatantActor Boss;

        public State(DojoBossCombatantActor Boss)
        {
            this.Boss = Boss;
        }

        public virtual void Enter()
        {

        }
        public virtual void Update()
        {

        }
        public virtual void Exit()
        {

        }

        public void SetBoss(DojoBossCombatantActor boss)
        {
            this.Boss = boss;
        }
    }

    public class StateIdle : State
    {
        Timer IdleTimer;
        float PillarDelay = 2f;
        float GroundedStartTime;
        bool Grounded;

        public StateIdle(DojoBossCombatantActor Boss) : base(Boss)
        {
            //bool shouldPillarJump = (Boss.timeSincePillar >= Boss.pillarJumpDelay) && isPillarAvailable;
            IdleTimer = new Timer(2f, Timer.TimerBehavior.Once);
        }

        public override void Enter()
        {
            IdleTimer.Reset();
            IdleTimer.SetTime(Random.Range(Boss.ActionDelayMinimum, Boss.ActionDelayMaximum));

            if (!Grounded)
            {
                GroundedStartTime = Time.time;
            }
            Grounded = true;
        }

        public override void Update()
        {
            IdleTimer.Update();

            if (!IdleTimer.Ready()) return;
            if (Boss.CombatTarget == null) return;

            UpdateRanges();

            float GroundedTime = Time.time - GroundedStartTime;
            if (GroundedTime > PillarDelay)
            {
                // Find the pillar we want to go to
                Pillar TargetPillar = Boss.PillarInfo.GetNextAndMarkUsed();

                // Set up the later states in the chain
                Boss.BossStates.OnPillar.SetPillar(TargetPillar);
                Boss.BossStates.Jump.SetTargetPosition(TargetPillar.Transform.position);
                Boss.BossStates.Jump.SetNextState(Boss.BossStates.OnPillar);

                // Enter the first jumping state
                Boss.SetState(Boss.BossStates.JumpSquat);
                Grounded = false;
            }
            else
            {
                // Tell the RangedAttack state to come back here when it's done.
                Boss.BossStates.RangedAttack.SetNextState(Boss.BossStates.Idle);

                Boss.SetState(Boss.BossStates.Aim);
            }


            return;

            //bool shouldPillarShoot = Boss.isLowHealth && r > 0.5f;
            //CrouchAction pillarTarget = CrouchAction.Plunge;
            //bool isPillarAvailable = Boss.TryGetAvailablePillar(out pillarTarget, shouldPillarShoot);
            //bool shouldPillarJump = (Boss.timeSincePillar >= Boss.pillarJumpDelay) && isPillarAvailable;
            //bool shouldJumpDown = (Boss.timeOnPillar >= Boss.pillarStayDelay);
            //bool canSummon = isLowHealth && timeSinceLastSummon >= MinTimeBetweenSummons && spawnedEnemies.Count < spawnLimit;
            //bool shouldParry = ShouldParry(out bool shouldCircleParry);

            //if (aiming)
            //{
            //    StartRangedAttack();
            //}
            //else if (onPillar)
            //{
            //    if (isLowHealth)
            //    {
            //        if (canSummon)
            //        {
            //            StartSummon();
            //        }
            //        else
            //        {
            //            StartAiming();
            //        }
            //    }
            //    else if (shouldJumpDown)
            //    {
            //        if (r < 0.4f)
            //        {
            //            StartCrouch(CrouchAction.Plunge);
            //        }
            //        else if (r < 0.8f)
            //        {
            //            StartCrouch(CrouchAction.JumpTo_Center);
            //        }
            //        else
            //        {
            //            if (isPillarAvailable)
            //            {
            //                timeOnPillar = 0f;
            //                StartCrouch(pillarTarget);
            //            }
            //            else
            //            {
            //                StartCrouch(CrouchAction.JumpTo_Center);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        StartAiming();
            //    }
            //}
            //else if (shouldPillarJump)
            //{
            //    StartCrouch(pillarTarget);
            //}
            //else if (!inCloseRange)
            //{
            //    if (this.isLowHealth && canSummon)
            //    {
            //        StartSummon();
            //    }
            //    else
            //    {
            //        if (r < 0.4f)
            //        {
            //            StartAiming();
            //        }
            //        else // TODO: ground slam attack
            //        {
            //            StartCrouch(CrouchAction.Plunge);
            //        }
            //    }
            //}
            //else if (shouldParry && !wasLastParryIgnored)
            //{
            //    if (shouldCircleParry)
            //    {
            //        StartCircleParry();
            //    }
            //    else
            //    {
            //            StartCrossParry();
            //        }
            //    }
            //    else
            //    {
            //        wasLastParryIgnored = false;
            //        if (r > 0.5f)
            //        {
            //            StartSlash();
            //        }
            //        else
            //        {
            //            StartThrust();
            //        }
            //    }
            //}        //            StartCrossParry();
            //        }
            //    }
            //    else
            //    {
            //        wasLastParryIgnored = false;
            //        if (r > 0.5f)
            //        {
            //            StartSlash();
            //        }
            //        else
            //        {
            //            StartThrust();
            //        }
            //    }
            //}
        }

        public void UpdateRanges()
        {
            Boss.isLowHealth = Boss.attributes.health.current <= Boss.attributes.health.max * 0.5f;
            if (Boss.isLowHealth)
            {
                Boss.bufferRange = 12f;
                Boss.closeRange = 8f;
            }
            else
            {
                Boss.bufferRange = 8f;
                Boss.closeRange = 4f;
            }
        }
    }

    public class StateAim : State
    {
        public Timer AimTimer;
        float AimTime = 4f;

        public StateAim(DojoBossCombatantActor Boss) : base(Boss)
        {
            AimTimer = new Timer(AimTime, Timer.TimerBehavior.Once);
        }

        public override void Enter()
        {
            AimTimer.Reset();
            Boss.weaponState = DojoBossCombatantActor.WeaponState.Bow;
            Boss.OnWeaponTransform.Invoke();
            Boss.combatState.ranged_idle = Boss.animancer.Play(Boss.RangedAttack.GetMovement());
            Boss.animancer.Layers[HumanoidAnimLayers.UpperBody].Play(Boss.RangedAttack.GetStartClip());
            Boss.aimParticle.Play();
            Boss.animancer.Layers[0].ApplyAnimatorIK = true;
        }

        public override void Update()
        {
            AimTimer.Update();
            if (AimTimer.Ready())
            {
                Boss.SetState(Boss.BossStates.RangedAttack);
            }
        }
    }

    public class StateRangedAttack : State
    {
        State NextState;
        Timer FireTimer;

        public StateRangedAttack(DojoBossCombatantActor Boss) : base(Boss)
        {
            FireTimer = new Timer(Boss.RangedAttack.GetFireClip().Length, Timer.TimerBehavior.Once);
        }

        public override void Enter()
        {
            Boss.animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
            AnimancerState FireAnimation = Boss.combatState.attack = Boss.animancer.Play(Boss.RangedAttack.GetFireClip(), 0f);

            FireTimer.Reset();

            Boss.combatState.attack.Events.OnEnd = Boss._MoveOnEnd;
            Boss.OnAttack.Invoke();

            Boss.aimParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Boss.fireParticle.Play();
        }

        public override void Update()
        {
            FireTimer.Update();
            if (FireTimer.Ready())
            {
                Boss.SetState(NextState);
            }
        }

        public void SetNextState(State State)
        {
            NextState = State;
        }
    }

    public class StateOnPillar : State
    {
        bool OnPillar = false;
        float TimeStart = 0f;
        const float MaxTimeOnPillar = 2f;
        Pillar CurrentPillar;

        public StateOnPillar(DojoBossCombatantActor Boss) : base(Boss)
        {

        }

        public override void Enter()
        {
            // Accumulate time on the pillar. If we're not already on the pillar, mark our start time.
            if (!OnPillar)
            {
                TimeStart = Time.time;
                OnPillar = true;
            }
            float TimeOnPillar = Time.time - TimeStart;
            
            // If we've been on the pillar long enough, jump down and clean up our state
            if (TimeOnPillar > MaxTimeOnPillar)
            {
                Boss.PillarInfo.MarkPillarUnused(CurrentPillar);
                OnPillar = false;

                Boss.BossStates.Jump.SetTargetPosition(Boss.center.position);
                Boss.BossStates.Jump.SetNextState(Boss.BossStates.Idle);
                Boss.SetState(Boss.BossStates.Jump);
            }
            // If we're on the pillar, shoot an arrow.
            else
            {
                Boss.BossStates.RangedAttack.SetNextState(Boss.BossStates.OnPillar);
                Boss.SetState(Boss.BossStates.Aim);
            }
        }

        public void SetPillar(Pillar Pillar)
        {
            CurrentPillar = Pillar;
        }
    }

    public class StateJumpSquat : State
    {
        Timer CrouchTimer;
        Vector3 TargetPosition;

        public StateJumpSquat(DojoBossCombatantActor Boss) : base(Boss)
        {
            CrouchTimer = new Timer(1f, Timer.TimerBehavior.Once);
        }

        public override void Enter()
        {
            Boss.StartCrouch();
            CrouchTimer.Reset();
        }

        public override void Update()
        {
            CrouchTimer.Update();
            if (CrouchTimer.Ready())
            {
                Boss.SetState(Boss.BossStates.Jump);
            }
        }

    }

    public class StateJump : State
    {
        Timer CrouchTimer;
        State NextState;
        AnimancerState JumpAnimation;
        Vector3 StartPosition;
        Vector3 TargetPosition;

        public StateJump(DojoBossCombatantActor Boss) : base(Boss)
        {

        }

        public override void Enter()
        {
            // Start the animation
            JumpAnimation = Boss.animancer.Play(Boss.JumpDodge);
            JumpAnimation.Events.OnEnd = () =>
            {
                Boss.animancer.Play(Boss.JumpLand);
            };
            Boss.jumpParticle.Play();

            // Mark current position 
            StartPosition = Boss.transform.position;
            Boss.nav.enabled = false;
        }

        public override void Update()
        {
            Boss.cc.enabled = false;
            Boss.shouldNavigate = false;

            // Find thenext position
            Vector3 NextPosition = Vector3.Lerp(
                StartPosition,
                TargetPosition,
                Boss.jumpHorizCurve.Evaluate(JumpAnimation.NormalizedTime));
            Vector3 VerticalOffset = Vector3.up * Boss.jumpVertCurve.Evaluate(JumpAnimation.NormalizedTime);
            NextPosition += VerticalOffset;
            Boss.transform.position = NextPosition;

            Boss.cc.enabled = true;
            Boss.yVel = 0f;

            Vector3 Distance = NextPosition - TargetPosition;
            if (Distance.magnitude < 1f)
            {
                Boss.SetState(NextState);
            }
        }

        public void SetNextState(State State)
        {
            NextState = State;
        }

        public void SetTargetPosition(Vector3 Position)
        {
            TargetPosition = Position;
        }
    }

    private void SetState(State State)
    {
        if (State == null) State = BossStates.Idle;
        BossStates.Current.Exit();
        BossStates.Current = State;
        BossStates.Current.Enter();
    }

    public struct States
    {
        public State Current;
        public StateIdle Idle;
        public StateAim Aim;
        public StateRangedAttack RangedAttack;
        public StateOnPillar OnPillar;
        public StateJumpSquat JumpSquat;
        public StateJump Jump;
    }

    // @spader: New fields
    public States BossStates;
    public PillarInfo PillarInfo;
     

    public override void ActorStart()
    {
        base.ActorStart();
        _MoveOnEnd = () =>
        {
            animancer.Play(navstate.move, 0.1f);
        };

        damageHandler = new SimplifiedDamageHandler(this, damageAnims, animancer);
        damageHandler.SetEndAction(TakeDefensiveAction);

        cc = this.GetComponent<CharacterController>();
        OnHitboxActive.AddListener(RealignToTarget);
        OnHurt.AddListener(() => {
            HitboxActive(0);
            crouching = false;
            plunging = false;
            aiming = false;
            pillarStunned = false;
            parryTime = 0f;
            animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
        });

        //animancer.Layers[HumanoidAnimLayers.UpperBody].SetMask(GetComponent<HumanoidPositionReference>().upperBodyMask);
        animancer.Layers[HumanoidAnimLayers.UpperBody].IsAdditive = false;

        animancer.Play(navstate.idle);
        initRot = this.GetComponent<HumanoidPositionReference>().MainHand.transform.localRotation;

        recentlySpawned = new List<NavigatingHumanoidActor>();

        BossHealthIndicator.SetTarget(this.gameObject);

        // States
        BossStates.Idle         = new StateIdle(this);
        BossStates.Aim          = new StateAim(this);
        BossStates.RangedAttack = new StateRangedAttack(this);
        BossStates.OnPillar     = new StateOnPillar(this);
        BossStates.JumpSquat    = new StateJumpSquat(this);
        BossStates.Jump         = new StateJump(this);
        BossStates.Current = BossStates.Idle;

        // Pillars
        PillarInfo = new PillarInfo();
        PillarInfo.AddPillar(pillar1);
        PillarInfo.AddPillar(pillar2);
        PillarInfo.AddPillar(pillar3);
    }

    void Awake()
    {
        inventory = this.GetComponent<HumanoidNPCInventory>();
    }

    public override void ActorPostUpdate()
    {
        UpdateCombatTarget();

        //base.ActorPostUpdate();

        if (inventory.IsMainEquipped() && !inventory.IsMainDrawn())
        {
            inventory.SetDrawn(true, true);
        }
        if (inventory.IsOffEquipped() && !inventory.IsOffDrawn())
        {
            inventory.SetDrawn(false, true);
        }

        BossStates.Current.Update();

        if (animancer.States.Current == combatState.jump)
        {
            float t = combatState.jump.NormalizedTime;
            cc.enabled = false;
            shouldNavigate = false;

            Vector3 targetPosition = Vector3.Lerp(startJumpPosition, endJumpPosition, jumpHorizCurve.Evaluate(t)) + Vector3.up * jumpVertCurve.Evaluate(t) * jumpVertMult;

            this.transform.position = targetPosition;

            cc.enabled = true;
            yVel = 0f;

            if (aiming)
            {
                RealignToTarget();
                if (t > JumpShotTime)
                {
                    JumpShotFire();
                }
            }
        }
        if (animancer.States.Current == combatState.attack)
        {
            if (!IsHitboxActive())
            {
                Vector3 dir = (destination - this.transform.position).normalized;
                this.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(this.transform.forward, dir, AttackRotationSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f));
            }
            if (!GetGrounded() && airTime > 1f)
            {
                navstate.fall = animancer.Play(fallAnim, 1f);
                HitboxActive(0);
            }
        }
        if (animancer.States.Current == combatState.parry_cross || animancer.States.Current == combatState.parry_circle)
        {
            bool inBufferRange = currentDistance <= bufferRange;

            nav.enabled = true;
            nav.isStopped = true;

            parryStrafeTime += Time.deltaTime;
            parryTime += Time.deltaTime;
            if (parryStrafeTime > strafeDelay)
            {
                parryStrafeTime = 0f;
                strafeDirection = CheckStrafe();
                if (Random.value > 0.7f)
                {
                    strafeDirection = 0;
                }
            }
            float xmov = Mathf.Sign(strafeDirection);

            if (inBufferRange && parryTime < MaxParryTime)
            {
                moveDirection = this.transform.right * xmov;
                Vector3 dir = (CombatTarget.transform.position - this.transform.position);
                dir.y = 0f;
                dir.Normalize();

                this.transform.rotation = Quaternion.LookRotation(dir);

                if (crossParrying)
                {
                    combatState.parry_cross.ParameterX = xmov;
                }
                else if (circleParrying)
                {
                    combatState.parry_circle.ParameterX = xmov;
                }
                parryOutOfBoundsTime = 0f;
            }
            else if (!inBufferRange && parryTime < MaxParryTime && parryOutOfBoundsTime < MaxParryOutOfBoundsTime)
            {
                parryOutOfBoundsTime += Time.deltaTime;
            }
            else
            {
                parryTime = 0f;
                animancer.Play(navstate.move);

                // @spader Does this mean we just go Idle while we move?
                //if (ActionTimer.GetTime() < 2f) 
                //{
                //    ActionTimer.SetTime(2f);
                //}

                circleParrying = false;
                crossParrying = false;
                wasLastParryIgnored = true;
            }
        }
        else if (animancer.States.Current != combatState.attack)
        {
            if (circleParrying || crossParrying)
            {
                circleParrying = false;
                crossParrying = false;
            }

        }

        if (circleParrying && !circleParticle.isPlaying)
        {
            circleParticle.Play();
        }
        else if (!circleParrying && circleParticle.isPlaying)
        {
            circleParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        if (crossParrying && !crossParticle.isPlaying)
        {
            crossParticle.Play();
        }
        else if (!crossParrying && crossParticle.isPlaying)
        {
            crossParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (Vector3.Distance(CombatTarget.transform.position, this.transform.position) < 10f)
        {
            //animancer.Layers[0].ApplyAnimatorIK = true;
            //animancer.Animator.SetLookAtPosition(headPoint);
        }
        else
        {
            //animancer.Layers[0].ApplyAnimatorIK = false;
        }

        if (plunging)
        {
            ProcessPlunge();
        }
        else if (crouching)
        {
            ProcessCrouch();
        }

        if (BossStates.Current == BossStates.Aim)
        {
            animancer.Layers[0].ApplyAnimatorIK = true;
        }

        if (combatState.ranged_idle is MixerState mix && mix.ChildStates[0] is LinearMixerState rangedIdle)
        {
            Vector3 lookDirection = transform.forward;
            nav.enabled = true;
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
            rangedIdle.Parameter = angle;
        }
        if (CombatTarget != null)
        {
            Vector3 delta = CombatTarget.transform.position - lastTargetPos;
            if (Time.deltaTime > 0f)
            {
                targetSpeed = delta / Time.deltaTime;
            }
            else
            {
                targetSpeed = Vector3.zero;
            }
            lastTargetPos = CombatTarget.transform.position;
        }
        if (summoning)
        {
            ProcessSummon();
            if (summonParticle != null && !summonParticle.isEmitting)
            {
                summonParticle.Play();
            }
            timeSinceLastSummon = 0f;
        }
        else
        {
            if (summonParticle != null && summonParticle.isEmitting)
            {
                summonParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            timeSinceLastSummon += Time.deltaTime;
        }
        if (pillarStunned)
        {
            ProcessPillarStun();
        }
        if (!circleParrying && !crossParrying)
        {
            timeSinceLastParry += Time.deltaTime;
        }
        if (onPillar)
        {
            timeSincePillar = 0f;
            timeOnPillar += Time.deltaTime;
        }
        else
        {
            timeSincePillar += Time.deltaTime;
            timeOnPillar = 0f;
        }
    }

    public void UpdateCombatTarget()
    {
        // If we've already got the combat target, just make sure the player isn't dead.
        if (CombatTarget)
        {
            if (CombatTarget.tag == "Corpse")   CombatTarget = null; 
            return;
        }

        if (DetermineCombatTarget(out GameObject target))
        {
            CombatTarget = target;
            SetDestination(target);

            if (target.TryGetComponent<Actor>(out Actor actor))
            {
                actor.OnAttack.AddListener(BeingAttacked);
            }
        }
    }

    /*
    public void StartMeleeCombo1()
    {
        RealignToTarget();
        //cstate.attack = CloseAttack.ProcessHumanoidAttack(this, _MoveOnEnd);
        nav.enabled = true;
        ignoreRoot = false;
        Vector3 pos = Vector3.zero;

        Vector3 centerPoint = CombatTarget.transform.position + (center.position - CombatTarget.transform.position).normalized * MeleeCombo1StartDistance;
        bool centerOnNav = NavMesh.SamplePosition(centerPoint, out NavMeshHit hit, 10f, 0);
        if (centerOnNav)
        {
            centerPoint = hit.position;
        }
        
        Vector3 farPoint = this.transform.position + (this.transform.position - CombatTarget.transform.position).normalized * MeleeCombo1StartDistance;
        bool farOnNav = NavMesh.SamplePosition(farPoint, out NavMeshHit hit2, 10f, 0);
        if (farOnNav)
        {
            farPoint = hit.position;
        }

        float centerDist = Vector3.Distance(CombatTarget.transform.position, centerPoint);
        float farDist = Vector3.Distance(CombatTarget.transform.position, farPoint);

        if (centerOnNav && !farOnNav)
        {
            pos = centerPoint;
        }
        else if (farOnNav && !centerOnNav)
        {
            pos = farPoint;
        }
        else if (!farOnNav && !centerOnNav)
        {
            return;
        }
        else if (centerDist < MeleeCombo1StartDistance && farDist >= MeleeCombo1StartDistance)
        {
            pos = farPoint;
        }
        else if (farDist < MeleeCombo1StartDistance && centerDist >= MeleeCombo1StartDistance)
        {
            pos = centerPoint;
        }
        else if (farDist < MeleeCombo1StartDistance && centerDist < MeleeCombo1StartDistance)
        {
            if (centerDist > farDist)
            {
                pos = centerPoint;
            }
            else
            {
                pos = farPoint;
            }
        }
        else
        {
            pos = centerPoint;
        }

        void _Then()
        {
            RealignToTarget();
            nav.enabled = true;
            cstate.attack = MeleeCombo1.ProcessHumanoidAction(this, () => { });
            OnAttack.Invoke();
        }

        DodgeJumpThen(pos, _Then, 2f);
    }
    */

    // basic attack template
    /*
    public void StartFarAttack()
    {
        RealignToTarget();
        cstate.attack = FarAttack.ProcessHumanoidAttack(this, _MoveOnEnd);
        OnAttack.Invoke();
    }
    */

    /*public void StartMeleeCombo2()
    {
        RealignToTarget();
        cstate.attack = MeleeCombo2.ProcessHumanoidAction(this, _MoveOnEnd);
        OnAttack.Invoke();
    }*/

    public void StartSlash()
    {
        RealignToTarget();
        combatState.attack = SlashAttack.ProcessHumanoidAction(this, _MoveOnEnd);
        OnAttack.Invoke();
    }

    public void StartThrust()
    {
        RealignToTarget();
        combatState.attack = ThrustAttack.ProcessHumanoidAction(this, _MoveOnEnd);
        OnAttack.Invoke();
    }

    public void StartCrossParry()
    {
        RealignToTarget();
        AnimancerState state = animancer.Play(IntoCrossParry);
        
        state.Events.OnEnd = PlayCrossParry;
        combatState.attack = state;
        crossParrying = true;
        parryTime = 0f;
        timeSinceLastParry = 0f;
        wasLastParryIgnored = false;
        wasLastParrySuccess = false;
        lastParryWasCircle = false;
        //crossParticle.Play();
    }

    public void PlayCrossParry()
    {
        combatState.parry_cross = (DirectionalMixerState)animancer.Play(CrossParryMove);
        animancer.Layers[HumanoidAnimLayers.UpperBody].Play(CrossParry);
        animancer.Layers[HumanoidAnimLayers.UpperBody].IsAdditive = false;
        crossParrying = true;


    }

    public void StartCircleParry()
    {
        RealignToTarget();
        AnimancerState state = animancer.Play(IntoCircleParry);
        
        state.Events.OnEnd = PlayCircleParry;
        combatState.attack = state;
        circleParrying = true;
        parryTime = 0f;
        timeSinceLastParry = 0f;
        wasLastParryIgnored = false;
        wasLastParrySuccess = false;
        lastParryWasCircle = true;
        //circleParticle.Play();
    }

    public void PlayCircleParry()
    {
        combatState.parry_circle = (DirectionalMixerState)animancer.Play(CircleParryMove);
        animancer.Layers[HumanoidAnimLayers.UpperBody].Play(CircleParry);
        animancer.Layers[HumanoidAnimLayers.UpperBody].IsAdditive = false;
        circleParrying = true;
    }
	
    public void StartCrouch(CrouchAction action)
    {
        RealignToTarget();
        crouchClock = CrouchTime;
        animancer.Play(CrouchDown).Events.OnEnd = () => { animancer.Play(Crouch); };
        crouching = true;
        actionAfterCrouch = action;
    }
    public void StartCrouch()
    {
        RealignToTarget();
        animancer.Play(CrouchDown).Events.OnEnd = () => { animancer.Play(Crouch); };
    }

    public void StartPlungeAttack()
    {
        Vector3 position = GetProjectedPosition(PlungeTime);
        Vector3 offset = (this.transform.position - CombatTarget.transform.position);
        offset.y = 0f;
        offset = offset.normalized * 1.5f;
        plungeTarget = CombatTarget.transform.position + Vector3.ClampMagnitude((position + offset) - CombatTarget.transform.position, 1.5f);

        if (NavMesh.SamplePosition(plungeTarget, out NavMeshHit hit, Vector3.Distance(CombatTarget.transform.position, position) + 10f, NavMesh.AllAreas))
        {
            plungeTarget = hit.position;
        }
        else
        {
            _MoveOnEnd(); // cancel attack if destination isn't on mesh
            return;
        }

        Vector3 dir = plungeTarget - this.transform.position;
        dir.y = 0f;
        this.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        plungeClock = PlungeTime;
        combatState.plunge_rise = animancer.Play(PlungeJump);
        combatState.plunge_rise.Events.OnEnd = null;
        plungeStart = this.transform.position;
        OnAttack.Invoke();
        plunging = true;
        jumpParticle.Play();
    }

    public void ProcessCrouch()
    {
        crouchClock -= Time.deltaTime;
        if (crouchClock <= 0f)
        {
            crouching = false;
            switch (actionAfterCrouch)
            {
                case CrouchAction.Plunge:
                    StartPlungeAttack();
                    return;
                case CrouchAction.JumpTo_Center:
                    onPillar = false;
                    DodgeJump(center.position);
                    return;
                case CrouchAction.JumpTo_Pillar1:
                    onPillar = true;
                    DodgeJump(pillar1.position);
                    currentPillar = 1;
                    return;
                case CrouchAction.JumpTo_Pillar2:
                    onPillar = true;
                    DodgeJump(pillar2.position);
                    currentPillar = 2;
                    return;
                case CrouchAction.JumpTo_Pillar3:
                    onPillar = true;
                    DodgeJump(pillar3.position);
                    currentPillar = 3;
                    return;
                case CrouchAction.JumpShot_Center:
                    onPillar = false;
                    StartJumpShot(center.position);
                    return;
                case CrouchAction.JumpShot_Pillar1:
                    onPillar = true;
                    StartJumpShot(pillar1.position);
                    currentPillar = 1;
                    return;
                case CrouchAction.JumpShot_Pillar2:
                    onPillar = true;
                    StartJumpShot(pillar2.position);
                    currentPillar = 2;
                    return;
                case CrouchAction.JumpShot_Pillar3:
                    onPillar = true;
                    StartJumpShot(pillar3.position);
                    currentPillar = 3;
                    return;
            }
            //StartPlungeAttack();
        }
    }

    public void ProcessPlunge()
    {
        float t = 1f - Mathf.Clamp01(plungeClock/PlungeTime);
        nav.enabled = false;

        Vector3 pos;

        if (!onPillar)
        {
            pos = Vector3.Lerp(plungeStart, plungeTarget, horizPlungeCurve.Evaluate(t));
        }
        else
        {
            pos = Vector3.Lerp(plungeStart, plungeTarget, horizPlungePillarCurve.Evaluate(t));
        }

        float vert = 0f;
        if (!onPillar)
        {
            vert = Mathf.Lerp(plungeStart.y, PlungeJumpHeight, heightPlungeCurve.Evaluate(t));
        }
        else
        {
            vert = Mathf.Lerp(plungeTarget.y, plungeStart.y, heightPlungePillarCurve.Evaluate(t));
        }
       

        pos.y = vert;

        this.transform.position = pos;

        
        if (t >= 1f)
        {
            this.transform.position = plungeTarget;
            //EndPlunge();
        }
        else if (t >= AttackPoint)
        {
            if (animancer.States.Current == combatState.plunge_fall)
            {
                combatState.plunge_attack = Plunge.ProcessHumanoidAction(this, EndPlunge);
            }
        }
        else if (t >= DescentPoint)
        {
            if (animancer.States.Current == combatState.plunge_rise)
            {
                combatState.plunge_fall = animancer.Play(PlungeFall);//Plunge.ProcessHumanoidAttack(this, EndPlunge);
            }
        }


        plungeClock -= Time.deltaTime;
    }

    void EndPlunge()
    {
        nav.enabled = true;
        plunging = false;
        onPillar = false;
        _MoveOnEnd();
    }


    public void DodgeJump(Vector3 position)
    {
        DodgeJumpThen(position, JumpEnd, 1f);
    }

    public void DodgeJumpThen(Vector3 position, System.Action endingAction, float speed)
    {
        AnimancerState jump = animancer.Play(JumpDodge);
        jump.Events.OnEnd = endingAction;
        jump.Speed *= speed;
        combatState.jump = jump;
        endJumpPosition = position;
        startJumpPosition = this.transform.position;
        nav.enabled = false;
        jumpParticle.Play();
    }

    void JumpEnd()
    {
        shouldNavigate = !onPillar;
        AnimancerState land = animancer.Play(JumpLand);
        land.Events.OnEnd = _MoveOnEnd;
        nav.enabled = true;
    }

    public void StartAiming()
    {
    }
    public void StartRangedAttack()
    {
    }

    public void StartRangedAttackMulti()
    {
        aiming = true;
        weaponState = DojoBossCombatantActor.WeaponState.Bow;
        OnWeaponTransform.Invoke();
        animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
        combatState.attack = animancer.Play(RangedAttackMulti.GetFireClip(), 0f);
        combatState.attack.Events.OnEnd = QuickRangedEnd;
        OnAttack.Invoke();
    }

    public void StartJumpShot(Vector3 position)
    {
        AnimancerState jump = animancer.Play(JumpShotUp);
        jump.Events.OnEnd = () =>
        {
            shouldNavigate = !onPillar;
            AnimancerState land = animancer.Play(JumpShotLand);
            land.Events.OnEnd = _MoveOnEnd;
            nav.enabled = true;
        };
        combatState.jump = jump;
        endJumpPosition = position;
        startJumpPosition = this.transform.position;
        nav.enabled = false;

        aiming = true;
        weaponState = DojoBossCombatantActor.WeaponState.Bow;
        OnWeaponTransform.Invoke();
    }

    public void JumpShotFire()
    {
        AnimancerState fire = animancer.Layers[HumanoidAnimLayers.UpperBody].Play(JumpShot.GetFireClip());
        fire.Time = 0f;
        fire.Events.OnEnd = () =>
        {
            animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
        };
        aiming = false;
        OnAttack.Invoke();
    }

    void QuickRangedEnd()
    {
        aiming = false;
        _MoveOnEnd();
    }

    public void StartSummon()
    {
        combatState.summon = animancer.Play(SummonStart);
        combatState.summon.Events.OnEnd = () =>
        {
            combatState.summon = animancer.Play(SummonHold);
            summoning = true;
        };
        summonClock = SummonTime;
    }

    public void ProcessSummon()
    {
        if (!animancer.IsPlayingClip(SummonHold.Clip))
        {
            summoning = false;
            return;
        }
        float t = 1f - Mathf.Clamp01(summonClock / SummonTime);

        if (t >= 1f)
        {
            combatState.summon = animancer.Play(SummonEnd);
            combatState.summon.Events.OnEnd = _MoveOnEnd;
            summoning = false;
        }

        summonClock -= Time.deltaTime;
    }

    public void InstantiateSummons()
    {
        if (spawnedEnemies.Count >= spawnLimit) return;

        int type = (spawnedEnemies.Count < 2) ? Random.Range(1, 4) : Random.Range(2, 4); // don't spawn doubles if there's no space

        Vector3 midpoint = (this.transform.position + CombatTarget.transform.position) * 0.5f;
        if (type == 1) // stab and slash dummies
        {
            List<Transform> availableTransforms = new List<Transform>();
            if (!spawn1Occupied)
            {
                availableTransforms.Add(spawn1);
            }
            if (!spawn2Occupied)
            {
                availableTransforms.Add(spawn2);
            }
            if (!spawn3Occupied)
            {
                availableTransforms.Add(spawn3);
            }

            availableTransforms = (List<Transform>)AxisUtilities.GetSortedTransformsByDistances(midpoint, availableTransforms);

            Vector3 pos1 = availableTransforms[0].position;
            Vector3 pos2 = availableTransforms[1].position;

            GameObject actor1Obj = Instantiate(slashDummy, pos1, Quaternion.LookRotation(-(pos1 - CombatTarget.transform.position).normalized));
            NavigatingHumanoidActor actor1 = actor1Obj.GetComponent<NavigatingHumanoidActor>();


            actor1Obj.GetComponent<AnimancerComponent>().Play(SpawnAnim).Events.OnEnd = () =>
            {
                actor1.shouldNavigate = true;
                actor1.actionsEnabled = true;
                actor1.PlayIdle();
            };
            //recentlySpawned.Add(actor1);
            spawnedEnemies.Add(actor1);
            if (availableTransforms[0] == spawn1)
            {
                spawn1Occupied = true;
                actor1.OnDie.AddListener(() => {
                    spawn1Occupied = false;
                    spawnedEnemies.Remove(actor1);
                });
            }
            else if (availableTransforms[0] == spawn2)
            {
                spawn2Occupied = true;
                actor1.OnDie.AddListener(() => {
                    spawn2Occupied = false;
                    spawnedEnemies.Remove(actor1);
                });
            }
            else if (availableTransforms[0] == spawn3)
            {
                spawn3Occupied = true;
                actor1.OnDie.AddListener(() => {
                    spawn3Occupied = false;
                    spawnedEnemies.Remove(actor1);
                });
            }

            GameObject actor2Obj = Instantiate(stabDummy, pos2, Quaternion.LookRotation(-(pos2 - CombatTarget.transform.position).normalized));
            NavigatingHumanoidActor actor2 = actor2Obj.GetComponent<NavigatingHumanoidActor>();

            actor2Obj.GetComponent<AnimancerComponent>().Play(SpawnAnim).Events.OnEnd = () =>
            {
                actor2.shouldNavigate = true;
                actor2.actionsEnabled = true;
                actor2.PlayIdle();
            };

            spawnedEnemies.Add(actor2);
            if (availableTransforms[1] == spawn1)
            {
                spawn1Occupied = true;
                actor2.OnDie.AddListener(() => {
                    spawn1Occupied = false;
                    spawnedEnemies.Remove(actor2);
                });
            }
            else if (availableTransforms[1] == spawn2)
            {
                spawn2Occupied = true;
                actor2.OnDie.AddListener(() => {
                    spawn2Occupied = false;
                    spawnedEnemies.Remove(actor2);
                });
            }
            else if (availableTransforms[1] == spawn3)
            {
                spawn3Occupied = true;
                actor2.OnDie.AddListener(() => {
                    spawn3Occupied = false;
                    spawnedEnemies.Remove(actor2);
                });
            }

            SummonBalls[0].Fly(summonParticle.transform.position, pos1);
            SummonBalls[1].Fly(summonParticle.transform.position, pos2);
        }
        else if (type == 2) // shield dummy
        {
            List<Transform> availableTransforms = new List<Transform>();
            if (!spawn1Occupied)
            {
                availableTransforms.Add(spawn1);
            }
            if (!spawn2Occupied)
            {
                availableTransforms.Add(spawn2);
            }
            if (!spawn3Occupied)
            {
                availableTransforms.Add(spawn3);
            }

            availableTransforms = (List<Transform>)AxisUtilities.GetSortedTransformsByDistances(midpoint, availableTransforms);

            Vector3 pos1 = availableTransforms[0].position;

            GameObject actor1Obj = Instantiate(shieldDummy, pos1, Quaternion.LookRotation(-(pos1 - CombatTarget.transform.position).normalized));
            NavigatingHumanoidActor actor1 = actor1Obj.GetComponent<NavigatingHumanoidActor>();

            actor1Obj.GetComponent<AnimancerComponent>().Play(SpawnAnim).Events.OnEnd = () =>
            {
                actor1.shouldNavigate = true;
                actor1.actionsEnabled = true;
                actor1.PlayIdle();
            };

            spawnedEnemies.Add(actor1);
            if (availableTransforms[0] == spawn1)
            {
                spawn1Occupied = true;
                actor1.OnDie.AddListener(() => {
                    spawn1Occupied = false;
                    spawnedEnemies.Remove(actor1);
                });
            }
            else if (availableTransforms[0] == spawn2)
            {
                spawn2Occupied = true;
                actor1.OnDie.AddListener(() => {
                    spawn2Occupied = false;
                    spawnedEnemies.Remove(actor1);
                });
            }
            else if (availableTransforms[0] == spawn3)
            {
                spawn3Occupied = true;
                actor1.OnDie.AddListener(() => {
                    spawn3Occupied = false;
                    spawnedEnemies.Remove(actor1);
                });
            }
            SummonBalls[0].Fly(summonParticle.transform.position, pos1);
            //SummonBalls[1].Fly(summonParticle.transform.position, pos2);
        }
        else if (type == 3) // archer dummy
        {
            List<Transform> availableTransforms = new List<Transform>();
            if (!pillar1Occupied && (!onPillar || currentPillar != 1))
            {
                availableTransforms.Add(pillar1);
            }
            if (!pillar2Occupied && (!onPillar || currentPillar != 2))
            {
                availableTransforms.Add(pillar2);
            }
            if (!pillar3Occupied && (!onPillar || currentPillar != 3))
            {
                availableTransforms.Add(pillar3);
            }

            availableTransforms = (List<Transform>)AxisUtilities.GetSortedTransformsByDistances(this.transform.position, availableTransforms);

            Vector3 pos1 = availableTransforms[0].position;

            GameObject actor1Obj = Instantiate(archerDummy, pos1, Quaternion.LookRotation(-(pos1 - CombatTarget.transform.position).normalized));
            NavigatingHumanoidActor actor1 = actor1Obj.GetComponent<NavigatingHumanoidActor>();

            actor1Obj.GetComponent<AnimancerComponent>().Play(SpawnAnim).Events.OnEnd = () =>
            {
                actor1.shouldNavigate = true;
                actor1.actionsEnabled = true;
                actor1.PlayIdle();
            };

            spawnedEnemies.Add(actor1);
            if (availableTransforms[0] == pillar1)
            {
                pillar1Occupied = true;
                actor1.OnDie.AddListener(() => {
                    pillar1Occupied = false;
                    spawnedEnemies.Remove(actor1);
                });
            }
            else if (availableTransforms[0] == pillar2)
            {
                pillar2Occupied = true;
                actor1.OnDie.AddListener(() => {
                    pillar2Occupied = false;
                    spawnedEnemies.Remove(actor1);
                });
            }
            else if (availableTransforms[0] == pillar3)
            {
                pillar3Occupied = true;
                actor1.OnDie.AddListener(() => {
                    pillar3Occupied = false;
                    spawnedEnemies.Remove(actor1);
                });
            }
            SummonBalls[0].Fly(summonParticle.transform.position, pos1);
            //SummonBalls[1].Fly(summonParticle.transform.position, pos2);
        }
    }


    public Vector3 GetProjectedPosition(float timeOut)
    {
        Debug.DrawLine(CombatTarget.transform.position, CombatTarget.transform.position + targetSpeed * timeOut, Color.blue, 5f);
        return CombatTarget.transform.position + targetSpeed * timeOut;
    }

    public Transform GetFirstAvailablePillar()
    {
        if (!pillar1Occupied) return pillar1;
        if (!pillar2Occupied) return pillar2;
        if (!pillar3Occupied) return pillar3;
        return null;
    }

    public bool TryGetAvailablePillar(out CrouchAction crouchAction, bool shoot)
    {
        crouchAction = CrouchAction.JumpTo_Center;

        if (pillar1Occupied && pillar2Occupied && pillar3Occupied) return false;

        List<Transform> availableTransforms = new List<Transform>();
        if (!pillar1Occupied && (!onPillar || currentPillar != 1))
        {
            availableTransforms.Add(pillar1);
        }
        if (!pillar2Occupied && (!onPillar || currentPillar != 2))
        {
            availableTransforms.Add(pillar2);
        }
        if (!pillar3Occupied && (!onPillar || currentPillar != 3))
        {
            availableTransforms.Add(pillar3);
        }

        if (availableTransforms.Count <= 0) return false;

        availableTransforms = (List<Transform>)AxisUtilities.GetSortedTransformsByDistances(this.transform.position, availableTransforms);

        if (availableTransforms[0] == pillar1)
        {
            crouchAction = (!shoot) ? CrouchAction.JumpTo_Pillar1 : CrouchAction.JumpShot_Pillar1;

        }
        else if (availableTransforms[0] == pillar2)
        {
            crouchAction = (!shoot) ? CrouchAction.JumpTo_Pillar2 : CrouchAction.JumpShot_Pillar2;
        }
        else if (availableTransforms[0] == pillar3)
        {
            crouchAction = (!shoot) ? CrouchAction.JumpTo_Pillar3 : CrouchAction.JumpShot_Pillar3;
        }
        return true;
    }
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
            isHitboxActive = false;
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

    public override bool IsHitboxActive()
    {
        return isHitboxActive;
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
            if (inventory.GetMainWeapon() is BladeWeapon blade)
            {
                origin += inventory.GetMainWeapon().GetModel().transform.up * blade.length;
            }
        }
        else if (active == 2 && off)
        {
            origin = inventory.GetOffWeapon().GetModel().transform.position;
            if (inventory.GetOffWeapon() is BladeWeapon blade)
            {
                origin += inventory.GetOffWeapon().GetModel().transform.up * blade.length;
            }
        }

        Collider[] colliders = Physics.OverlapSphere(origin, SHOCKWAVE_RADIUS, LayerMask.GetMask("Actors"));
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<IDamageable>(out IDamageable damageable) && (collider.transform.root != this.transform.root || currentDamage.canDamageSelf))
            {
                damageable.TakeDamage(currentDamage);
            }
        }
        hammerParticle.transform.position = origin;
        hammerParticle.Play();
        Debug.DrawRay(origin, Vector3.forward * SHOCKWAVE_RADIUS, Color.red, 5f);
        Debug.DrawRay(origin, Vector3.back * SHOCKWAVE_RADIUS, Color.red, 5f);
        Debug.DrawRay(origin, Vector3.right * SHOCKWAVE_RADIUS, Color.red, 5f);
        Debug.DrawRay(origin, Vector3.left * SHOCKWAVE_RADIUS, Color.red, 5f);
    }

    public void FrontHitbox(int active)
    {
        if (currentDamage == null) return;
        currentDamage.source = this.gameObject;
        float SHOCKWAVE_RADIUS = 2f;

        Vector3 OFFSET = new Vector3(0f, 0.5f, 1f);

        bool main = (inventory.IsMainDrawn());
        bool off = (inventory.IsOffDrawn());
        Vector3 dirToTarget = -(this.transform.position - CombatTarget.transform.position);
        dirToTarget.y = 0f;
        Vector3 origin = this.transform.position + Vector3.Cross(dirToTarget.normalized, Vector3.up) * OFFSET.x + Vector3.up * OFFSET.y + dirToTarget.normalized * OFFSET.z;
        currentDamage.originPoint = origin;

        Collider[] colliders = Physics.OverlapSphere(origin, SHOCKWAVE_RADIUS, LayerMask.GetMask("Actors"));
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<IDamageable>(out IDamageable damageable) && (collider.transform.root != this.transform.root || currentDamage.canDamageSelf))
            {
                damageable.TakeDamage(currentDamage);
            }
        }
        Debug.DrawRay(origin, Vector3.forward * SHOCKWAVE_RADIUS, Color.red, 5f);
        Debug.DrawRay(origin, Vector3.back * SHOCKWAVE_RADIUS, Color.red, 5f);
        Debug.DrawRay(origin, Vector3.right * SHOCKWAVE_RADIUS, Color.red, 5f);
        Debug.DrawRay(origin, Vector3.left * SHOCKWAVE_RADIUS, Color.red, 5f);
        Debug.DrawRay(origin, Vector3.up * SHOCKWAVE_RADIUS, Color.red, 5f);
        Debug.DrawRay(origin, Vector3.down * SHOCKWAVE_RADIUS, Color.red, 5f);
    }


    public void AnimTransWep(int wep)
    {
        weaponState = (WeaponState)wep;
        OnWeaponTransform.Invoke();
    }

    public void AnimDrawWeapon(int slot)
    {
        if (inventory.IsMainEquipped()) inventory.SetDrawn(0, true);
        if (inventory.IsOffEquipped()) inventory.SetDrawn(1, true);
        if (inventory.IsRangedEquipped()) inventory.SetDrawn(2, true);
    }

    public void AnimSheathWeapon(int slot)
    {
        if (inventory.IsMainEquipped()) inventory.SetDrawn(0, false);
        if (inventory.IsOffEquipped()) inventory.SetDrawn(1, false);
        if (inventory.IsRangedEquipped()) inventory.SetDrawn(2, false);
    } 

    public DamageKnockback GetLastDamage()
    {
        return currentDamage;
    }

    public bool DetermineCombatTarget(out GameObject target)
    {
        if (PlayerActor.player == null)
        {
            target = null;
            return false;
        }
        target = PlayerActor.player.gameObject;
        return PlayerActor.player.gameObject.tag != "Corpse";
    }

    public override bool IsArmored()
    {
        return IsAttacking() && !onPillar;
    }
    public override bool IsDodging()
    {
        return animancer.States.Current == combatState.jump || animancer.States.Current == combatState.dodge;
    }

    public override bool IsAttacking()
    {
        return animancer.States.Current == combatState.attack || animancer.States.Current == combatState.plunge_attack;
    }

    public override bool IsFalling()
    {
        return animancer.States.Current == navstate.fall || animancer.States.Current == damageHandler.fall;
    }

    public override void ProcessDamageKnockback(DamageKnockback damageKnockback)
    {
        damageHandler.TakeDamage(damageKnockback);
    }

    public void BeingAttacked()
    {
        if (CombatTarget != null && currentDistance < bufferRange && CanAct())
        {
            //TryDodge();
        }
    }

    public override bool CanAct()
    {
        return base.CanAct() || (aiming && actionsEnabled);
    }

    public void Recoil()
    {
        ((IDamageable)damageHandler).Recoil();
    }

    public void StartCritVulnerability(float time)
    {
        ((IDamageable)damageHandler).StartCritVulnerability(time);
    }

    public void EndAnim()
    {
        _MoveOnEnd();
    }
    public override void Die()
    {
        if (dead) return;
        base.Die();
        
        foreach(Renderer r in this.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
        foreach (Collider c in this.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
        this.GetComponent<Collider>().enabled = false;

    }

    public override void FlashWarning(int hand)
    {
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

    private void OnAnimatorIK(int layerIndex)
    {
        RangedAttack.OnIK(animancer.Animator);
    }
    public void SetHitParticlePosition(Vector3 position, Vector3 direction)
    {
        SetHitParticleVectors(position, direction);
    }
    #region Damage Handling

    public virtual void TakeDamage(DamageKnockback damage)
    {
        if (!this.IsAlive()) return;
        if (this.IsTimeStopped() || this.IsDodging())
        {
            damageHandler.TakeDamage(damage);
            return;
        }

        aimParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (!onPillar && this.transform.position.y > (-1055.5f))
        {
            // isn't on a pillar but is above for some reason? probably a bug
            bool b = false;
        }
        bool isCrit = damageHandler.IsCritVulnerable() || damage.critData.alwaysCritical || onPillar;
        bool willKill = damage.GetDamageAmount(isCrit) >= attributes.health.current;
        bool hitFromBehind = !(Vector3.Dot(-this.transform.forward, (damage.source.transform.position - this.transform.position).normalized) <= 0f);

        float damageAmount = damage.GetDamageAmount(isCrit);

        if (onPillar && !crouching)
        {
            PillarFall(damage);
            this.attributes.ReduceHealth(damageAmount);
        }
        else if ((circleParrying || crossParrying) && damage.isRanged)
        {
            damage.OnBlock.Invoke();
            this.OnBlock.Invoke();
        }
        else if (crossParrying && !damage.isSlash)
        {
            if (damage.source.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.StartCritVulnerability(3f);
                damageable.Recoil();
            }
            CrossParryComplete(damage);

            OnBlock.Invoke();
            damage.OnBlock.Invoke();
        }
        else if (circleParrying && !damage.isThrust)
        {
            if (damage.source.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.StartCritVulnerability(3f);
                damageable.Recoil();
            }
            CircleParryComplete(damage);

            OnBlock.Invoke();
            damage.OnBlock.Invoke();
        }
        else if ((crossParrying && damage.isSlash) || (circleParrying && damage.isThrust))
        {
            animancer.Layers[HumanoidAnimLayers.Flinch].Stop();
            animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
            ClipTransition clip = damageAnims.guardBreak;
            AnimancerState state = animancer.Play(clip);
            state.Events.OnEnd = _MoveOnEnd;
            damageHandler.hurt = state;
            this.OnHurt.Invoke();
            damage.OnCrit.Invoke();
            damage.OnBlock.Invoke();
            this.OnBlock.Invoke();
            StartCritVulnerability(clip.MaximumDuration / clip.Speed);
        }
        else
        {
            damageHandler.TakeDamage(damage);
        }
        /*else if (!willKill)
        {
            animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();

            bool isArmored = this.IsArmored() && !damage.breaksArmor;
            bool isCounterhit = this.IsAttacking();

            DamageKnockback.StaggerType stagger = DamageKnockback.StaggerType.None;

            if (isCrit)
            {
                stagger = damage.staggers.onCritical;
            }
            else if (isArmored)
            {
                stagger = damage.staggers.onArmorHit;
            }
            else if (isCounterhit)
            {
                stagger = damage.staggers.onCounterHit;
                if (stagger == DamageKnockback.StaggerType.None)
                {
                    stagger = damage.staggers.onHit;
                }
            }
            else
            {
                stagger = damage.staggers.onHit;
            }

            if (stagger == DamageKnockback.StaggerType.Knockdown || stagger == DamageKnockback.StaggerType.Crumple)
            {
                RealignToTarget();
                cstate.hurt = animancer.Play(KnockdownOverride);
                cstate.hurt.NormalizedTime = 0f;
                damageHandler.SetInvulnClip(cstate.hurt);
                this.attributes.ReduceHealth(damageAmount);
                damage.OnHit.Invoke();
                this.OnHurt.Invoke();
                StartCoroutine(MoveForDuration(this.transform.forward * -knockdownMoveSpeed, knockdownMoveTime));
                if (isCrit)
                {
                    damage.OnCrit.Invoke();
                }
                damageHandler.StopCritVulnerability();
            }
            else
            {
                damageHandler.TakeDamage(damage);
            }

        }
        else if (willKill)
        {
            damageHandler.TakeDamage(damage);
        }*/
        DeactivateHitboxes();
    }


    public bool ShouldParry(out bool shouldCircleParry)
    {
        if (wasLastParrySuccess)
        {
            shouldCircleParry = lastParryWasCircle;
            return true;
        }
        else if (timeSinceLastParry > MinTimeBetweenParries)
        {
            float r = Random.value;
            if (r > 0.5f)
            {
                shouldCircleParry = true;
            }
            else
            {
                shouldCircleParry = false;
            }
            return true;
        }
        else
        {
            shouldCircleParry = false;
            return false;
        }
    }
    public void CrossParryComplete(DamageKnockback damage)
    {
        ParrySuccess.Invoke();
        wasLastParrySuccess = true;
        OnBlock.Invoke();
        damage.OnBlock.Invoke();
        Actor actor = damage.source.GetComponent<Actor>();
        AnimancerComponent otherAnimancer;
        
        float otherSpeed = 0f;
        if (actor.TryGetComponent<AnimancerComponent>(out otherAnimancer))
        {
            otherSpeed = otherAnimancer.States.Current.Speed;
            otherAnimancer.States.Current.Speed = 0.25f;
        }
        
        actor.DeactivateHitboxes();

        animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
        AnimancerState hit = animancer.Play(CrossParryHit);
        
        hit.Events.OnEnd = () =>
        {
            RealignToTarget();
            combatState.attack = CrossParryFollowup.ProcessHumanoidAction(this, _MoveOnEnd);
            if (otherAnimancer != null)
            {
                otherAnimancer.States.Current.Speed = otherSpeed;
            }
            crossParrying = false;
        };
        
    }

    public void CircleParryComplete(DamageKnockback damage)
    {
        ParrySuccess.Invoke();
        wasLastParrySuccess = true;
        OnBlock.Invoke();
        damage.OnBlock.Invoke();
        Actor actor = damage.source.GetComponent<Actor>();
        AnimancerComponent otherAnimancer;

        float otherSpeed = 0f;
        if (actor.TryGetComponent<AnimancerComponent>(out otherAnimancer))
        {
            otherSpeed = otherAnimancer.States.Current.Speed;
            otherAnimancer.States.Current.Speed = 0.25f;
        }

        actor.DeactivateHitboxes();

        animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
        combatState.attack = CircleParryFollowup.ProcessHumanoidAction(this, () => { });
        OnHitboxActive.AddListener(End);
        void End()
        {
            RealignToTarget();
            if (otherAnimancer != null)
            {
                otherAnimancer.States.Current.Speed = otherSpeed;
            }
            OnHitboxActive.RemoveListener(End);
            DeactivateHitboxes();
            circleParrying = false;
        };

    }

    public void PillarFall(DamageKnockback damage)
    {
        OnHurt.Invoke();
        damage.OnHit.Invoke();
        damage.OnCrit.Invoke();
        ignoreRoot = false;
        nav.enabled = false;
        pillarStunned = true;
        aiming = false;
        RealignToTargetWithOffset(90f);
        combatState.hurt = animancer.Play(PillarHit);
        combatState.hurt.NormalizedTime = 0f;
        damageHandler.SetCritVulnState(combatState.hurt, 10f);

        Vector3 adjust = this.transform.forward * pillarFallAdjustVector.z + this.transform.right * pillarFallAdjustVector.x + this.transform.up * pillarFallAdjustVector.y;
        StartCoroutine(MoveForDuration(adjust, pillarFallAdjustTime));
        pillarFallClock = 0f;

        
    }

    public void ProcessPillarStun()
    {
        if (yVel > pillarMinYVel)
        {
            yVel = pillarMinYVel;
        }
        if (pillarFallAirborne)
        {
            if (GetGrounded())
            {
                onPillar = false;
                pillarFallAirborne = false;
                nav.enabled = true;
                combatState.hurt = animancer.Play(PillarFallLand);
                damageHandler.SetCritVulnState(combatState.hurt, 10f);
                combatState.hurt.Events.OnEnd = () =>
                {
                    combatState.hurt = animancer.Play(PillarKneel);
                    damageHandler.SetCritVulnState(combatState.hurt, 10f);
                };
            }
        }
        pillarFallClock += Time.deltaTime;
        if (pillarFallClock >= pillarFallMaxTime)
        {
            AnimancerState state = animancer.Play(PillarKneelStand);
            state.Events.OnEnd = () =>
            {
                pillarStunned = false;
                TakeDefensiveAction();
            };
            pillarFallClock = -1f;
        }
    }

    public void AnimSetPillarFallAirborne()
    {
        pillarFallAirborne = true;
    }

    IEnumerator MoveForDuration(Vector3 delta, float time)
    {
        float clock = 0f;
        while (clock < time)
        {
            cc.Move(delta * Time.deltaTime);
            clock += Time.deltaTime;
            yield return null;
        }
    }

    public void TakeDefensiveAction()
    {
        damageHandler.StopCritVulnerability();
        RealignToTarget();
        nav.enabled = true;
        pillarStunned = false;
        //_MoveOnEnd();
        Vector3 origin = this.transform.position + Vector3.up * 0.5f;
        float dist = 5f;
        bool isBackToWall = Physics.SphereCast(origin, 0.25f, -this.transform.forward, out RaycastHit hit, dist, MaskReference.Terrain);
        ignoreRoot = false;
        if (Vector3.Distance(this.transform.position, CombatTarget.transform.position) < 10f)
        {
            if (!isBackToWall)
            {
                combatState.dodge = animancer.Play(DodgeBack);
                jumpParticle.Play();
                StartCoroutine(MoveForDuration(-this.transform.forward * 5f, 0.75f));
                combatState.dodge.Events.OnEnd = AfterDodge;
                Debug.DrawRay(origin, -this.transform.forward * dist, Color.magenta, 5f);
            }
            else
            {
                int side = CheckStrafe();
                combatState.dodge = animancer.Play((side < 0) ? DodgeLeft : DodgeRight);
                jumpParticle.Play();
                StartCoroutine(MoveForDuration(this.transform.right * ((side < 0) ? -1 : 1) * 5f, 0.75f));
                combatState.dodge.Events.OnEnd = AfterDodge;
                Debug.DrawRay(origin, -this.transform.forward * dist, (side < 0) ? Color.blue : Color.red, 5f);
            }
        }
        else
        {
            _MoveOnEnd();
        }
    }

    void AfterDodge()
    {
        float r = Random.value;
        if (Vector3.Distance(this.transform.position, CombatTarget.transform.position) < 6f)
        {
            if (timeSincePillar > pillarJumpDelay && TryGetAvailablePillar(out CrouchAction crouchAction, false))
            {
                StartCrouch(crouchAction);
            }
            else if (r < 0.3f)
            {
                StartCrossParry();
            }
            else if (r < 0.6f) {
                StartCircleParry();
            }
            else if (r < 0.9f && Vector3.Distance(this.transform.position, center.position) > 10f)
            {
                StartCrouch(CrouchAction.JumpTo_Center);
            }
            else
            {
                _MoveOnEnd();
            }
        }
        else
        {
            if (r < 0.5f)
            {
                StartAiming();
            }
            else
            {
                _MoveOnEnd();
            }
        }
    }

    public DamageKnockback GetLastTakenDamage()
    {
        return ((IDamageable)damageHandler).GetLastTakenDamage();
    }
    #endregion
}
