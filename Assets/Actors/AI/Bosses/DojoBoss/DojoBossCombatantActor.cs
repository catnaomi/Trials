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
    public const float MaxTimePillar = 2f;
    public const float TimeIdleGrounded = 2f;
    public const float SummonCooldown = 10f;
    public const int   CountSpawns = 2;
}

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
    // The blackboard is used in state transitions. It has two purposes:
    // 1. Data that's shared across different states
    // 2. Code that a state needs to run even when that state is not active (e.g. updating a timer)
    public class StateBlackboard
    {
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
            Boss.nav.enabled = false;
        }

        public override void Update()
        {
            Boss.shouldNavigate = false;
            Boss.nav.enabled = false;

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

            // Play animations
            // @spader Is there a better way to chain animations together? I see this a lot.
            AnimancerState SummonAnimation = Boss.animancer.Play(Boss.SummonStart);
            SummonAnimation.Events.OnEnd = () =>
            {
                Boss.animancer.Play(Boss.SummonHold);
            };

            Boss.summonParticle.Play();
        }

        public override void Update()
        {
            SummonTimer.Update();
            if (SummonTimer.Ready())
            {
                SummonTimer.Reset();
                AnimancerState SummonEndAnimation = Boss.animancer.Play(Boss.SummonEnd);
                SummonEndAnimation.Events.OnEnd = Boss._MoveOnEnd;

                Boss.summonParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

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
                    bool IsInsideNavMesh = NavMesh.SamplePosition(
                        ProjectedPosition,
                        out NavMeshHit Hit,
                        Vector3.Distance(PlayerPosition, BossPosition) + Tolerance,
                        NavMesh.AllAreas);

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
                    AnimancerState PlungeJump = Boss.animancer.Play(Boss.PlungeJump);
                    PlungeJump.Events.OnEnd = null;

                    Boss.OnAttack.Invoke();
                    Boss.jumpParticle.Play();

                    Boss.nav.enabled = false;

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
                    Boss.animancer.Play(Boss.PlungeFall);
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
                    // @spader What is _MoveOnEnd()? If I call it here, the hammer animation plays.
                    // If I call it below, in PlungeState.Attack, it does not play. Also, I see this get
                    // called in lots of places, so I know it's not specific to this animation.
                    Boss.Plunge.ProcessHumanoidAction(Boss, () => { 
                        Boss._MoveOnEnd();
                    });

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
                    Boss.nav.enabled = true;
                    //Boss._MoveOnEnd();
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

    // @spader: New fields
    public States BossStates;
    public SpawnPointInfo SpawnPointInfo;
    public StateBlackboard Blackboard;
    public List<NavigatingHumanoidActor> Dummies;

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
        UpdateCombatTarget();

        base.ActorPostUpdate();

        if (inventory.IsMainEquipped() && !inventory.IsMainDrawn())
        {
            inventory.SetDrawn(true, true);
        }
        if (inventory.IsOffEquipped() && !inventory.IsOffDrawn())
        {
            inventory.SetDrawn(false, true);
        }

        Blackboard.Update();
        BossStates.Current.Update();

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

    public void UpdateRanges()
    {
        if (isLowHealth)
        {
            bufferRange = 12f;
            closeRange = 8f;
        }
        else
        {
            bufferRange = 8f;
            closeRange = 4f;
        }
    }

    public bool IsCloseRange()
    {
        return GetDistanceToTarget() < closeRange;
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
	
    public void StartCrouch()
    {
        RealignToTarget();
        animancer.Play(CrouchDown).Events.OnEnd = () => { animancer.Play(Crouch); };
    }

    public void ProcessCrouch()
    {
        crouchClock -= Time.deltaTime;
        if (crouchClock <= 0f)
        {
            crouching = false;
            switch (actionAfterCrouch)
            {
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

    public Vector3 GetProjectedPosition(float timeOut)
    {
        Debug.DrawLine(CombatTarget.transform.position, CombatTarget.transform.position + targetSpeed * timeOut, Color.blue, 5f);
        return CombatTarget.transform.position + targetSpeed * timeOut;
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

    //void AfterDodge()
    //{
    //    float r = Random.value;
    //    if (Vector3.Distance(this.transform.position, CombatTarget.transform.position) < 6f)
    //    {
    //        if (timeSincePillar > pillarJumpDelay && TryGetAvailablePillar(out CrouchAction crouchAction, false))
    //        {
    //            StartCrouch(crouchAction);
    //        }
    //        else if (r < 0.3f)
    //        {
    //            StartCrossParry();
    //        }
    //        else if (r < 0.6f) {
    //            StartCircleParry();
    //        }
    //        else if (r < 0.9f && Vector3.Distance(this.transform.position, center.position) > 10f)
    //        {
    //            StartCrouch(CrouchAction.JumpTo_Center);
    //        }
    //        else
    //        {
    //            _MoveOnEnd();
    //        }
    //    }
    //    else
    //    {
    //        if (r < 0.5f)
    //        {
    //            StartAiming();
    //        }
    //        else
    //        {
    //            _MoveOnEnd();
    //        }
    //    }
    //}

    public DamageKnockback GetLastTakenDamage()
    {
        return ((IDamageable)damageHandler).GetLastTakenDamage();
    }
    #endregion
}
