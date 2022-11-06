using Animancer;
using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public static class Constants
{
    public const float MaxTimeGrounded = 10f;
    public const float MaxTimePillar = 3f;
    public const float TimeIdleGrounded = 2f;
    public const float SummonCooldown = 10f;
    public const int   CountSpawns = 2;
}

public delegate void OnClipEnd();

public enum SpawnedEnemy
{
    GroundedMelee,
    GroundedShield,
    PillarArcher
}

public enum SpawnPointKind
{
    Grounded,
    Pillar
};

public enum RandomGroundedAttack
{
    ShootArrow,
    GroundSlam,
    HammerJump,
    Count
}

public enum RandomPillarAction
{
    HammerJump,
    ReturnToGround,
    JumpToNewPillar,
    Count
}

public class Timer 
{
    public enum TimerBehavior 
    {
        Once,
        Repeat
    }

    public float Time;
    public float Accumulated;
    public bool Enabled;
    public TimerBehavior Behavior;

    public Timer(float time, TimerBehavior behavior) 
    {
        this.Time = time;
        this.Accumulated = 0;
        this.Enabled = true;
        this.Behavior = behavior;
    }

    public Timer(float time) : this(time, Timer.TimerBehavior.Once)
    {
    }

    public Timer(TimerBehavior behavior) : this(0f, behavior)
    {
    }

    public float GetTime() 
    {
        return this.Time;
    }

    public void SetTime(float time) 
    {
        this.Time = time;
        this.Accumulated = 0;
    }

    public void SetBehavior(TimerBehavior behavior)
    {
        this.Behavior = behavior;
    }


    public void Update() 
    {
        if (!this.Enabled) return;

        this.Accumulated += UnityEngine.Time.deltaTime;

        if (this.Accumulated >= this.Time)
        {
            if (this.Behavior == TimerBehavior.Repeat)
            {
                Reset();
            }
        }
    }

    public bool Ready() 
    {
        return this.Accumulated >= this.Time;
    }

    public void Reset() 
    {
        Accumulated = 0;
    }

    public void Enable()
    {
        this.Enabled = true;
    }

    public void Disable()
    {
        this.Enabled = false;
    }

    public float GetPercentDone()
    {
        return Mathf.Clamp01(Accumulated / Time);
    }
}

public class SpawnPoint
{
    public Transform Transform;
    public bool Available;

    public SpawnPointKind Kind;
}

public class SpawnPointInfo
{
    List<SpawnPoint> SpawnPoints;

    public SpawnPointInfo()
    {
        SpawnPoints = new List<SpawnPoint>();
    }

    public void AddSpawnPoint(Transform Transform, SpawnPointKind Kind)
    {
        SpawnPoint SpawnPoint = new SpawnPoint();
        SpawnPoint.Transform = Transform;
        SpawnPoint.Available = true;
        SpawnPoint.Kind = Kind;
        SpawnPoints.Add(SpawnPoint);
    }

    public SpawnPoint GetNextAndMarkUsed(SpawnPointKind Kind)
    {
        foreach(SpawnPoint SpawnPoint in SpawnPoints)
        {
            if (SpawnPoint.Kind != Kind) continue;

            if (SpawnPoint.Available)
            {
                MarkUsed(SpawnPoint);
                return SpawnPoint;
            }
        }

        return null;
    }

    public void MarkUnused(SpawnPoint SpawnPoint)
    {
        foreach (SpawnPoint OtherSpawnPoint in SpawnPoints)
        {
            if (OtherSpawnPoint == SpawnPoint)
            {
                SpawnPoint.Available = true;
            }
        }
    }

    public void MarkAllUnused(SpawnPointKind Kind)
    {
        foreach (SpawnPoint SpawnPoint in SpawnPoints)
        {
            if (SpawnPoint.Kind == Kind)
            {
                SpawnPoint.Available = true;
            }
        }
    }

    public void MarkUsed(SpawnPoint SpawnPoint)
    {
        SpawnPoint.Available = false;
    }

    public SpawnPoint GetClosestSpawnPoint(Vector3 Position, SpawnPointKind Kind)
    {
        SpawnPoint Closest = null;
        float MinDistance = float.PositiveInfinity;

        foreach (SpawnPoint SpawnPoint in SpawnPoints)
        {
            if (SpawnPoint.Kind != Kind) continue;
            if (!SpawnPoint.Available) continue;

            float Distance = Vector3.Distance(Position, SpawnPoint.Transform.position);
            if (Distance < MinDistance)
            {
                MinDistance = Distance;
                Closest = SpawnPoint;
            }
        }

        return Closest;
    }

    public int CountAvailable(SpawnPointKind Kind)
    {
        int Count = 0;
        foreach (SpawnPoint SpawnPoint in SpawnPoints)
        {
            if (SpawnPoint.Kind != Kind) continue;
            if (!SpawnPoint.Available) continue;

            Count++;
        }

        return Count;
    }
}

public struct QiMixerState
{
    public LinearMixerState Idle;
    public DirectionalMixerState Move;
}

public struct QiMixerAssets
{
    public LinearMixerTransitionAsset Idle;
    public MixerTransition2DAsset Move;
}

[RequireComponent(typeof(HumanoidNPCInventory))]
public class DojoBossCombatantActor : HumanoidActor
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
    [Header("hello it's me")]
    public LinearMixerTransitionAsset IdleAnim;
    public MixerTransition2DAsset MoveAnim;
    public NavAnims NavAnims;

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
    public float SummonTime;
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

    #region States
    public class StateBlackboard
    {
        // The blackboard is used in state transitions. It has two purposes:
        // 1. Data that's shared across different states
        // 2. Code that a state needs to run even when that state is not active (e.g. updating a timer)

        public bool IsGrounded = true;
        public Timer GroundedTimer;
        public Timer PillarTimer;
        public Timer SummonTimer;

        public StateBlackboard()
        {
            GroundedTimer = new Timer(Constants.MaxTimeGrounded, Timer.TimerBehavior.Once);
            GroundedTimer.Enable(); // We start grounded

            PillarTimer = new Timer(Constants.MaxTimePillar, Timer.TimerBehavior.Once);
            PillarTimer.Disable();
            
            SummonTimer = new Timer(Constants.SummonCooldown, Timer.TimerBehavior.Once);
            SummonTimer.Enable();
        }

        public void Update()
        {
            GroundedTimer.Update();
            PillarTimer.Update();
            SummonTimer.Update();
        }

        public void GroundedStart()
        {
            IsGrounded = true;

            GroundedTimer.Reset();
            GroundedTimer.Enable();

            PillarTimer.Reset();
            PillarTimer.Disable();

        }

        public void PillarStart()
        {
            IsGrounded = false;

            GroundedTimer.Reset();
            GroundedTimer.Disable();

            PillarTimer.Reset();
            PillarTimer.Enable();
        }
    }

    public class State
    {
        public DojoBossCombatantActor Boss;
        public StateBlackboard Blackboard;

        public State(DojoBossCombatantActor Boss)
        {
            this.Boss = Boss;
            this.Blackboard = Boss.Blackboard;
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

        public State GetNextIdleState()
        {
            if (Blackboard.IsGrounded)
            {
                return Boss.BossStates.IdleGrounded;
            }
            else
            {
                return Boss.BossStates.IdlePillar;
            }
        }

        public State GetOppositeIdleState()
        {
            if (Blackboard.IsGrounded)
            {
                return Boss.BossStates.IdlePillar;
            }
            else
            {
                return Boss.BossStates.IdleGrounded;
            }
        }

    }

    public class StateIdleGrounded : State
    {
        Timer IdleTimer;

        public StateIdleGrounded(DojoBossCombatantActor Boss) : base(Boss)
        {
            //bool shouldPillarJump = (Boss.timeSincePillar >= Boss.pillarJumpDelay) && isPillarAvailable;
            IdleTimer = new Timer(Constants.TimeIdleGrounded, Timer.TimerBehavior.Once);
        }

        public override void Enter()
        {
            IdleTimer.Reset();
            IdleTimer.SetTime(Random.Range(Boss.ActionDelayMinimum, Boss.ActionDelayMaximum));

            if (!Blackboard.IsGrounded)
            {
                Blackboard.GroundedStart();
            }
        }

        public override void Update()
        {
            IdleTimer.Update();

            if (!IdleTimer.Ready()) return;
            if (Boss.CombatTarget == null) return;

            Boss.UpdateRanges();

            if (Blackboard.GroundedTimer.Ready())
            {
                // Find the pillar we want to go to
                SpawnPoint TargetPillar = Boss.SpawnPointInfo.GetNextAndMarkUsed(SpawnPointKind.Pillar);
                Boss.BossStates.IdlePillar.SetPillar(TargetPillar);
                Boss.BossStates.Jump.SetTargetPosition(TargetPillar.Transform.position);

                Boss.SetState(Boss.BossStates.JumpSquat);
            }
            else
            {
                if (Boss.IsCloseRange())
                {

                }
                else
                {
                    if (Boss.IsLowHealth())
                    {
                        if (Boss.CanSummon())
                        {
                            Boss.SetState(Boss.BossStates.Summon);
                        }
                        else
                        {
                            ChooseRandomAttack();
                        }
                    }
                    else
                    {
                        ChooseRandomAttack();
                    }
                }
            }


            return;
        }

        void ChooseRandomAttack()
        {
            RandomGroundedAttack Attack = (RandomGroundedAttack)Random.Range(0, (int)RandomGroundedAttack.Count);
            if (Attack == RandomGroundedAttack.ShootArrow)
            {
                Boss.SetState(Boss.BossStates.Aim);
            }
            else if (Attack == RandomGroundedAttack.GroundSlam)
            {
                return;
            }
            else if (Attack == RandomGroundedAttack.HammerJump)
            {
                Boss.SetState(Boss.BossStates.PlungeAttack);
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
            Boss.animancer.Play(Boss.RangedAttack.GetMovement());
            Boss.animancer.Layers[HumanoidAnimLayers.UpperBody].Play(Boss.RangedAttack.GetStartClip());
            Boss.aimParticle.Play();
            Boss.ApplyAnimatorIK();

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
                Boss.SetState(GetNextIdleState());
            }
        }
    }

    public class StateIdlePillar : State
    {
        SpawnPoint CurrentPillar;

        public StateIdlePillar(DojoBossCombatantActor Boss) : base(Boss)
        {

        }

        public override void Enter()
        {
            // If we're not already on the pillar, set up tracking for our pillar time
            if (Blackboard.IsGrounded)
            {
                Blackboard.PillarStart();
            }
            
            // We should return to the ground
            if (Blackboard.PillarTimer.Ready())
            {
                ChooseRandomAction();
            }
            // We should stay on the pillar
            else
            {
                if (Boss.CanSummon())
                {
                    Boss.SetState(Boss.BossStates.Summon);
                }
                else
                {
                    Boss.SetState(Boss.BossStates.Aim);
                }
            }
        }

        public void SetPillar(SpawnPoint Pillar)
        {
            CurrentPillar = Pillar;
        }

        public void ChooseRandomAction()
        {
            RandomPillarAction Attack = (RandomPillarAction)Random.Range(0, (int)RandomPillarAction.Count);
            if (Attack == RandomPillarAction.ReturnToGround)
            {
                Boss.SpawnPointInfo.MarkUnused(CurrentPillar);
                Boss.BossStates.Jump.SetTargetPosition(Boss.center.position);
                Boss.SetState(Boss.BossStates.Jump);
            }
            else if (Attack == RandomPillarAction.HammerJump)
            {
                Boss.SpawnPointInfo.MarkUnused(CurrentPillar);
                Boss.SetState(Boss.BossStates.PlungeAttack);
            }
            else
            {
                if (Boss.SpawnPointInfo.CountAvailable(SpawnPointKind.Pillar) > 0)
                {
                    SpawnPoint TargetPillar = Boss.SpawnPointInfo.GetNextAndMarkUsed(SpawnPointKind.Pillar);
                    Boss.SpawnPointInfo.MarkUnused(CurrentPillar);
                    SetPillar(TargetPillar);
                    Boss.BossStates.Jump.SetTargetPosition(TargetPillar.Transform.position);
                    Boss.SetState(Boss.BossStates.JumpSquat);
                }
                return;
            }
        }
    }

    public class StateJumpSquat : State
    {
        Timer CrouchTimer;

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
        }

        public override void Update()
        {
            // Find thenext position
            Vector3 NextPosition = Vector3.Lerp(
                StartPosition,
                TargetPosition,
                Boss.jumpHorizCurve.Evaluate(JumpAnimation.NormalizedTime));
            Vector3 VerticalOffset = Vector3.up * Boss.jumpVertCurve.Evaluate(JumpAnimation.NormalizedTime);
            NextPosition += VerticalOffset;
            Boss.transform.position = NextPosition;

            Boss.yVel = 0f;

            Vector3 Distance = NextPosition - TargetPosition;
            if (Distance.magnitude < 1f)
            {
                // If we're currently grounded, the jump brings us to a pillar. Vice-versa.
                Boss.SetState(GetOppositeIdleState());
            }
        }

        public void SetTargetPosition(Vector3 Position)
        {
            TargetPosition = Position;
        }
    }

    public class StateSummon : State
    {
        Timer SummonTimer;

        public StateSummon(DojoBossCombatantActor Boss) : base(Boss)
        {
            SummonTimer = new Timer(Boss.SummonTime);
        }

        public override void Enter()
        {
            SummonTimer.Reset();

            Boss.PlayClip(Boss.SummonStart, () => { Boss.PlayClip(Boss.SummonHold); });

            Boss.StartSummonParticle();
        }

        public override void Update()
        {
            SummonTimer.Update();
            if (SummonTimer.Ready())
            {
                SummonTimer.Reset();

                Boss.PlayClip(Boss.SummonEnd, () => { Boss._MoveOnEnd(); });
                Boss.StopSummonParticle();

                InstantiateDummies();

                Boss.SetState(GetNextIdleState());
            }
        }

        void InstantiateDummies()
        {
            // Instantiate
            SpawnPointKind SpawnPointKind = Blackboard.IsGrounded ? SpawnPointKind.Grounded : SpawnPointKind.Pillar;
            GameObject DummyKind = Blackboard.IsGrounded ? Boss.slashDummy : Boss.archerDummy;
            Vector3 Midpoint = (Boss.transform.position + Boss.CombatTarget.transform.position) * 0.5f;

            for (int i = 0; i < Constants.CountSpawns; i++)
            {
                SpawnPoint SpawnPoint = Boss.SpawnPointInfo.GetClosestSpawnPoint(Midpoint, SpawnPointKind);
                Boss.SpawnPointInfo.MarkUsed(SpawnPoint);

                Vector3 PlayerToDummy = SpawnPoint.Transform.position - Boss.CombatTarget.transform.position;
                Vector3 DummyToPlayer = -1 * PlayerToDummy;
                DummyToPlayer.Normalize();

                GameObject DummyObject = Instantiate(
                    DummyKind,
                    SpawnPoint.Transform.position,
                    Quaternion.LookRotation(DummyToPlayer)
                );
                NavigatingHumanoidActor Dummy = DummyObject.GetComponent<NavigatingHumanoidActor>();
                AnimancerComponent DummyAnimancer = DummyObject.GetComponent<AnimancerComponent>();

                Boss.Dummies.Add(Dummy);

                AnimancerState SpawnAnimation = DummyAnimancer.Play(Boss.SpawnAnim);
                SpawnAnimation.Events.OnEnd = () => {
                    Dummy.shouldNavigate = true;
                    Dummy.actionsEnabled = true;
                    Dummy.PlayIdle();
                };

                Dummy.OnDie.AddListener(() =>
                {
                    Boss.SpawnPointInfo.MarkUnused(SpawnPoint);
                    Boss.Dummies.Remove(Dummy);
                });

                Boss.SummonBalls[i].Fly(Boss.summonParticle.transform.position, SpawnPoint.Transform.position);
            }

            // Spawning ground entities doesn't "occupy" the spawn point. We just mark them used when creating
            // so we choose different spawn points. Clear that.
            Boss.SpawnPointInfo.MarkAllUnused(SpawnPointKind.Grounded);
        }
    }

    public class StatePlungeAttack : State
    {
        Timer CrouchTimer;
        Timer PlungeTimer;

        Vector3 StartPosition;
        Vector3 TargetPosition;

        enum PlungeState
        {
            Crouch,
            Rise,
            Descend,
            Attack
        };
        PlungeState State;

        public StatePlungeAttack(DojoBossCombatantActor Boss) : base(Boss)
        {
            CrouchTimer = new Timer(Boss.CrouchTime);
            PlungeTimer = new Timer(Boss.PlungeTime);
        }

        public override void Enter()
        {
            CrouchTimer.Reset();
            PlungeTimer.Reset();

            State = PlungeState.Crouch;
            Boss.StartCrouch();
        }

        public override void Update()
        {
            if (State == PlungeState.Crouch)
            {
                CrouchTimer.Update();
                if (CrouchTimer.Ready())
                {
                    // Find the target of the plunge attack
                    Vector3 ProjectedPosition = GetProjectedPosition();
                    Vector3 PlayerPosition = Boss.CombatTarget.transform.position;
                    Vector3 BossPosition = Boss.transform.position;
                    Vector3 Offset = BossPosition - PlayerPosition;
                    Offset.y = 0f;
                    Offset.Normalize();
                    Offset *= 1.5f;

                    // @spader What does all the offset stuff do?
                    //Vector3 NaiveTarget = BossPosition + Vector3.ClampMagnitude(ProjectedPosition + Offset - BossPosition, 1.5f);

                    // Check that the target is inside the NavMesh
                    float Tolerance = 10f;
                    bool IsInsideNavMesh = UnityEngine.AI.NavMesh.SamplePosition(
                        ProjectedPosition,
                        out NavMeshHit Hit,
                        Vector3.Distance(PlayerPosition, BossPosition) + Tolerance,
                        UnityEngine.AI.NavMesh.AllAreas);

                    if (IsInsideNavMesh)
                    {
                        StartPosition = BossPosition;
                        TargetPosition = Hit.position;
                    }
                    else
                    {
                        Boss._MoveOnEnd();
                        Boss.SetState(Boss.BossStates.IdleGrounded);
                    }

                    // Look at the target
                    Vector3 LookDirection = TargetPosition - BossPosition;
                    LookDirection.y = 0f;
                    Boss.transform.rotation = Quaternion.LookRotation(LookDirection, Vector3.up);

                    // Play animations
                    Boss.PlayClip(Boss.PlungeJump);
                    AnimancerState PlungeJump = Boss.animancer.Play(Boss.PlungeJump);
                    PlungeJump.Events.OnEnd = null;

                    Boss.OnAttack.Invoke();
                    Boss.jumpParticle.Play();

                    State = PlungeState.Rise;
                }
            }
            else if (State == PlungeState.Rise)
            {
                PlungeTimer.Update();
                float T = PlungeTimer.GetPercentDone();

                LerpPosition(T);

                if (T >= Boss.DescentPoint)
                {
                    Boss.PlayClip(Boss.PlungeFall);
                    State = PlungeState.Descend;
                }
            }
            else if (State == PlungeState.Descend)
            {
                PlungeTimer.Update();
                float T = PlungeTimer.GetPercentDone();

                LerpPosition(T);

                if (T >= Boss.AttackPoint)
                {
                    Boss.PlayClip(Boss.Plunge.GetClip(), () => { Boss._MoveOnEnd(); });

                    State = PlungeState.Attack;
                }
            }
            else if (State == PlungeState.Attack)
            {
                PlungeTimer.Update();
                float T = PlungeTimer.GetPercentDone();

                LerpPosition(T);

                if (T >= 1f)
                {
                    Boss.transform.position = TargetPosition;
                    Boss.SetState(Boss.BossStates.IdleGrounded);
                }
            }
        }

        Vector3 GetProjectedPosition()
        {
            Vector3 PlayerPosition = Boss.CombatTarget.transform.position;
            Vector3 PlayerSpeed = Boss.targetSpeed;
            Vector3 ProjectedPosition = PlayerPosition + (PlayerSpeed * Boss.PlungeTime);
            Debug.DrawLine(PlayerPosition, ProjectedPosition, Color.blue, 5f);
            return ProjectedPosition;
        }

        void LerpPosition(float T)
        {
            Vector3 Position;
            if (Blackboard.IsGrounded)
            {
                Position = Vector3.Lerp(StartPosition, TargetPosition, Boss.horizPlungeCurve.Evaluate(T));
                Position.y = Mathf.Lerp(StartPosition.y, Boss.PlungeJumpHeight, Boss.heightPlungeCurve.Evaluate(T));
            }
            else
            {
                Position = Vector3.Lerp(StartPosition, TargetPosition, Boss.horizPlungePillarCurve.Evaluate(T));
                Position.y = Mathf.Lerp(TargetPosition.y, StartPosition.y, Boss.heightPlungePillarCurve.Evaluate(T));
            }

            Boss.transform.position = Position;
        }
    }

    public struct States
    {
        public State Current;
        public StateIdleGrounded IdleGrounded;
        public StateAim Aim;
        public StateRangedAttack RangedAttack;
        public StateIdlePillar IdlePillar;
        public StateJumpSquat JumpSquat;
        public StateJump Jump;
        public StateSummon Summon;
        public StatePlungeAttack PlungeAttack;
    }

    private void SetState(State State)
    {
        if (State == null) State = BossStates.IdleGrounded;
        Debug.Log(State.GetType().Name);
        BossStates.Current.Exit();
        BossStates.Current = State;
        BossStates.Current.Enter();
    }
    #endregion

    public States BossStates;
    public SpawnPointInfo SpawnPointInfo;
    public StateBlackboard Blackboard;
    public List<NavigatingHumanoidActor> Dummies;

    public NavAnims RuntimeNavAnims;
    public ClipTransition JumpHorizontal;
    public ClipTransition JumpDown;
    public ClipTransition FallAnim;
    public ClipTransition LandAnim;
    public QiMixerState MixerState;
    public QiMixerAssets MixerAssets;

    public NavMeshAgent NavMesh;


    public float BufferRange;
    public float CloseRange;

    public override void ActorStart()
    {
        base.ActorStart();
        _MoveOnEnd = () =>
        {
            animancer.Play(MixerState.Move, 0.1f);
        };

        damageHandler = new SimplifiedDamageHandler(this, damageAnims, animancer);

        cc = this.GetComponent<CharacterController>();
        // @spader Used to realign to target on hitbox active
        OnHurt.AddListener(() => {
            HitboxActive(0);
            crouching = false;
            aiming = false;
            pillarStunned = false;
            parryTime = 0f;
            animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
        });

        //animancer.Layers[HumanoidAnimLayers.UpperBody].SetMask(GetComponent<HumanoidPositionReference>().upperBodyMask);
        animancer.Layers[HumanoidAnimLayers.UpperBody].IsAdditive = false;

        animancer.Play(MixerState.Move);
        initRot = this.GetComponent<HumanoidPositionReference>().MainHand.transform.localRotation;

        recentlySpawned = new List<NavigatingHumanoidActor>();

        BossHealthIndicator.SetTarget(this.gameObject);

        // States
        Blackboard = new StateBlackboard();

        BossStates.IdleGrounded = new StateIdleGrounded(this);
        BossStates.Aim          = new StateAim(this);
        BossStates.RangedAttack = new StateRangedAttack(this);
        BossStates.IdlePillar   = new StateIdlePillar(this);
        BossStates.JumpSquat    = new StateJumpSquat(this);
        BossStates.Jump         = new StateJump(this);
        BossStates.Summon       = new StateSummon(this);
        BossStates.PlungeAttack = new StatePlungeAttack(this);
        BossStates.Current = BossStates.IdleGrounded;

        // Pillars
        SpawnPointInfo = new SpawnPointInfo();
        SpawnPointInfo.AddSpawnPoint(pillar1, SpawnPointKind.Pillar);
        SpawnPointInfo.AddSpawnPoint(pillar2, SpawnPointKind.Pillar);
        SpawnPointInfo.AddSpawnPoint(pillar3, SpawnPointKind.Pillar);
        SpawnPointInfo.AddSpawnPoint(spawn1, SpawnPointKind.Grounded);
        SpawnPointInfo.AddSpawnPoint(spawn2, SpawnPointKind.Grounded);
        SpawnPointInfo.AddSpawnPoint(spawn3, SpawnPointKind.Grounded);
    }

    void Awake()
    {
        inventory = this.GetComponent<HumanoidNPCInventory>();
    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();

        UpdateDrawnWeapon();

        Blackboard.Update();
        BossStates.Current.Update();
        

        if (BossStates.Current == BossStates.Aim)
        {
            animancer.Layers[0].ApplyAnimatorIK = true;
        }
    }
        
    public void UpdateRanges()
    {
        if (isLowHealth)
        {
            BufferRange = 12f;
            CloseRange = 8f;
        }
        else
        {
            BufferRange = 8f;
            CloseRange = 4f;
        }
    }

    public bool IsCloseRange()
    {
        return Vector3.Distance(this.transform.position, GetPlayer().transform.position) < CloseRange;
    }

    public bool CanSummon()
    {
        return Blackboard.SummonTimer.Ready() && Dummies.Count == 0;
    }
    public bool IsLowHealth()
    {
        return true;
        return attributes.health.current <= attributes.health.max * 0.5f;
    }

    public void UpdateDrawnWeapon()
    {
        // @spader: If we draw the weapon whenever it's equipped, why can't we do this
        // in HumanoidNPCInverntory::EquipXXXWeapon()?
        //
        // Even if not, this feels like one of those situations where we're checking something
        // every frame that we should just set once when it happens
        if (inventory.IsMainEquipped() && !inventory.IsMainDrawn())
        {
            inventory.SetDrawn(HumanoidNPCInventory.WeaponSlot.Main, true);
        }
        if (inventory.IsOffEquipped() && !inventory.IsOffDrawn())
        {
            inventory.SetDrawn(HumanoidNPCInventory.WeaponSlot.Off, true);
        }

    }

    public void ApplyAnimatorIK()
    {
        animancer.Layers[0].ApplyAnimatorIK = true;
    }

    public void PlayClip(ClipTransition Clip, System.Action OnEnd = null)
    {
        AnimancerState ClipState = animancer.Play(Clip);
        ClipState.Events.OnEnd = OnEnd;
    }

    public void StartSummonParticle()
    {
        summonParticle.Play();
    }

    public void StopSummonParticle()
    {
        summonParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

    }

    public PlayerActor GetPlayer()
    {
        return PlayerActor.player;
    }

    private void SetupAnimancer()
    {
        // Instantiate a copy based on the template set in the editor
        RuntimeNavAnims = ScriptableObject.Instantiate(NavAnims);

        // Pull the fields into members
        JumpHorizontal = RuntimeNavAnims.jumpHorizontal;
        JumpDown = RuntimeNavAnims.jumpDown;
        FallAnim = RuntimeNavAnims.fallAnim;
        LandAnim = RuntimeNavAnims.landAnim;

        // Copy inspector fields into our struct and initialize
        MixerAssets.Move = MoveAnim;
        MixerAssets.Idle = IdleAnim;

        MixerState.Move = (DirectionalMixerState)animancer.States.GetOrCreate(MoveAnim);
        MixerState.Move.Key = "move";
        MixerState.Idle = (LinearMixerState)animancer.States.GetOrCreate(IdleAnim);

        // Cache components
        animancer = GetComponent<AnimancerComponent>();
        cc = GetComponent<CharacterController>();

        NavMesh = GetComponent<NavMeshAgent>();
        NavMesh.updatePosition = false;
        NavMesh.updateRotation = false;
        NavMesh.angularSpeed = 180f;
        NavMesh.autoTraverseOffMeshLink = false;

        positionReference = GetComponent<HumanoidPositionReference>();
        animancer.Layers[HumanoidAnimLayers.UpperBody].SetMask(positionReference.upperBodyMask);
        animancer.Layers[HumanoidAnimLayers.UpperBody].IsAdditive = true;
        animancer.Layers[HumanoidAnimLayers.UpperBody].Weight = 1f;
    }
    public void StartCrouch()
    {
        PlayClip(CrouchDown, () => { PlayClip(Crouch); });
    }

    // old

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
}
