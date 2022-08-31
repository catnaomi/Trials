using UnityEngine;
using System.Collections;
using Animancer;
using UnityEngine.Events;

[RequireComponent(typeof(HumanoidNPCInventory))]
public class SimpleMeleeCombatantActor : NavigatingHumanoidActor, IAttacker, IDamageable
{
    HumanoidNPCInventory inventory;
    [Header("Combatant Settings")]
    public float MinEngageRange = 5f;
    public float MaxEngageRange = 10f;
    [Space(5)]
    public float SightRange = 15f;
    public bool InSightRange;
    [Space(5)]
    public InputAttack MeleeAttack;
    public float MeleeAttackRange = 1.5f;
    public bool InMeleeRange;
    [Space(5)]
    public InputAttack QuickAttack;
    public float QuickAttackRange = 2f;
    public bool InQuickRange;
    [Space(5)]
    public InputAttack GapCloserAttack;
    public ClipTransition GapCloserAnim;
    public float GapCloserMaxRange = 5f;
    public float GapCloserMinRange = 3f;
    public float GapCloserRotationSpeed = 45f;
    public bool InGapRange;
    [Space(5)]
    public InputAttack PowerAttack;
    public float PowerAttackRange = 2f;
    public bool InPowerRange;
    [Space(5)]
    public ClipTransition Dodge;
    public ClipTransition Draw;
    public ClipTransition Sheath;
    [Space(5)]
    public float dodgeChance = 0.6f;
    public float powerAttackChance = 0.2f;
    [Space(5)]
    public DamageAnims damageAnims;
    HumanoidDamageHandler damageHandler;
    [Space(10)]
    public float clock;
    public float ActionDelayMinimum = 2f;
    public float ActionDelayMaximum = 5f;
    
    public float LowHealthThreshold = 50f;
    public bool isLowHealth;
    bool isHitboxActive;

    bool tryCounterAttack;
    protected CombatState cstate;
    protected struct CombatState
    {
        public AnimancerState attack;
        public AnimancerState approach;
        public AnimancerState dodge;
        public AnimancerState sheathe;
    }

    Attacks lastAttack;
    enum Attacks
    {
        Melee,
        Quick,
        GapCloser,
        Power
    }
    System.Action _MoveOnEnd;
    public override void ActorStart()
    {
        base.ActorStart();
        
        closeRange = MinEngageRange;
        bufferRange = MaxEngageRange;
        _MoveOnEnd = () =>
        {
            animancer.Play(navstate.move, 0.1f);
        };

        Draw.Events.OnEnd = _MoveOnEnd;
        Sheath.Events.OnEnd = _MoveOnEnd;
        Dodge.Events.OnEnd = _MoveOnEnd;
        damageHandler = new SimplifiedDamageHandler(this, damageAnims, animancer);
        //damageHandler.SetEndAction(_MoveOnEnd);
        
        damageHandler.SetEndAction(() =>
        {
            bool dodge = TryDodge();
            if (!dodge)
            {
                if (Random.value < powerAttackChance)
                {
                    StartPowerAttack();
                }
                else
                {
                    _MoveOnEnd();
                }
                
            }
        });
        OnHurt.AddListener(() => {
            HitboxActive(0);
        });
        OnDodgeSuccess.AddListener(() => {
            Debug.Log("dodge success!");
            tryCounterAttack = true;
            clock = 0f;
        });
    }

    void Awake()
    {
        inventory = this.GetComponent<HumanoidNPCInventory>();
    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();
        
        if (clock > -1)
        {
            clock -= Time.deltaTime;
        }
        bool shouldAct = (clock <= 0f) || tryCounterAttack;

        if (CombatTarget == null)
        {
            if (DetermineCombatTarget(out GameObject target))
            {
                CombatTarget = target;

                StartNavigationToTarget(target);

                if (target.TryGetComponent<Actor>(out Actor actor))
                {
                    actor.OnAttack.AddListener(BeingAttacked);
                }
            }
        }
        else if (CombatTarget.tag == "Corpse")
        {
            CombatTarget = null;
        }

        if (shouldAct && CanAct())
        {
            clock = Random.Range(ActionDelayMinimum, ActionDelayMaximum);
            if (CombatTarget != null)
            {
                float navdist = GetDistanceToTarget();
                float realdist = Vector3.Distance(this.transform.position, GetCombatTarget().transform.position);

                InSightRange = realdist <= SightRange;
                InMeleeRange = navdist <= MeleeAttackRange && nav.hasPath;
                InQuickRange = navdist <= QuickAttackRange && nav.hasPath;
                InGapRange = navdist <= GapCloserMaxRange && navdist >= GapCloserMinRange && nav.hasPath;
                InPowerRange = navdist <= PowerAttackRange && nav.hasPath;

                bool counterattack = false;
                if (tryCounterAttack)
                {
                    counterattack = true;
                    tryCounterAttack = false;
                }
                if (InSightRange)
                {
                    SetDestination(CombatTarget);
                    ResumeNavigation();
                    if (!inventory.IsMainDrawn())
                    {
                        cstate.sheathe = animancer.Play(Draw);
                    }
                }
                else
                {
                    StopNavigation();
                    if (inventory.IsMainDrawn())
                    {
                        cstate.sheathe = animancer.Play(Sheath);
                    }

                }
                if (CanAct())
                {
                    if (InMeleeRange && counterattack)
                    {
                        StartQuickAttack();
                    }
                    else if (InPowerRange && Random.value < powerAttackChance)
                    {
                        RealignToTarget();
                        StartPowerAttack();
                    }
                    else if (InQuickRange && InMeleeRange)
                    {
                        RealignToTarget();
                        if (Random.value > 0.5f)
                        {
                            StartMeleeAttack();
                        }
                        else
                        {
                            StartQuickAttack();
                        }
                    }
                    else if (InMeleeRange)
                    {
                        RealignToTarget();
                        StartMeleeAttack();
                    }
                    else if (InQuickRange)
                    {
                        RealignToTarget();
                        StartQuickAttack();
                    }
                    else if (InGapRange)
                    {
                        RealignToTarget();
                        StartGapCloser();
                    }

                }
            }
        }

        if (animancer.States.Current == cstate.approach)
        {
            Vector3 dir = (destination - this.transform.position).normalized;
            this.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(this.transform.forward, dir, GapCloserRotationSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f));
        }
    }

    public void StartMeleeAttack()
    {
        lastAttack = Attacks.Melee;
        cstate.attack = animancer.Play(MeleeAttack.GetClip());
        cstate.attack.Events.OnEnd = _MoveOnEnd;
        SetCurrentDamage(MeleeAttack.GetDamage());
        OnAttack.Invoke();
    }

    public void StartQuickAttack()
    {
        lastAttack = Attacks.Quick;
        cstate.attack = animancer.Play(QuickAttack.GetClip());
        cstate.attack.Events.OnEnd = _MoveOnEnd;
        SetCurrentDamage(QuickAttack.GetDamage());
        OnAttack.Invoke();
    }

    public void StartPowerAttack()
    {
        lastAttack = Attacks.Power;
        cstate.attack = animancer.Play(PowerAttack.GetClip());
        cstate.attack.Events.OnEnd = _MoveOnEnd;
        SetCurrentDamage(PowerAttack.GetDamage());
        OnAttack.Invoke();
    }

    public void StartGapCloser()
    {
        lastAttack = Attacks.GapCloser;
        cstate.approach = animancer.Play(GapCloserAnim);
        ClipTransition atkClip = GapCloserAttack.GetClip();
        atkClip.Events.OnEnd = _MoveOnEnd;
        cstate.approach.Events.OnEnd = () =>
        {
            cstate.attack = animancer.Play(atkClip);
        };
        
        SetCurrentDamage(GapCloserAttack.GetDamage());
        StartCoroutine("GapCloserCoroutine");
        OnAttack.Invoke();
    }

    public bool TryDodge()
    {
        HitboxActive(0);
        if (Random.value >= dodgeChance)
        {
            RealignToTarget();
            cstate.dodge = animancer.Play(Dodge);
            return true;
        }
        return false;
    }


    IEnumerator GapCloserCoroutine()
    {
        do
        {
            yield return new WaitForEndOfFrame();
            if (GetDistanceToTarget() < GapCloserMinRange)
            {
                ClipTransition atkClip = GapCloserAttack.GetClip();
                atkClip.Events.OnEnd = _MoveOnEnd;
                cstate.attack = animancer.Play(atkClip);
                break;
            }

        } while (animancer.States.Current == cstate.approach);
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

    public override bool IsDodging()
    {
        return animancer.States.Current == cstate.dodge;
    }

    public override bool IsArmored()
    {
        return animancer.States.Current == cstate.approach || (IsAttacking() && (lastAttack == Attacks.GapCloser || lastAttack == Attacks.Power));
    }

    public override bool IsAttacking()
    {
        return animancer.States.Current == cstate.attack;
    }

    public override void ProcessDamageKnockback(DamageKnockback damageKnockback)
    {
        damageHandler.TakeDamage(damageKnockback);
    }

    public void BeingAttacked()
    {
        if (CombatTarget != null && currentDistance < bufferRange && CanAct())
        {
            TryDodge();
        }
    }

    public bool CanAct()
    {
        return (animancer.States.Current == navstate.move || animancer.States.Current == navstate.idle) && actionsEnabled;
    }

    public void TakeDamage(DamageKnockback damage)
    {
        ((IDamageable)damageHandler).TakeDamage(damage);
    }

    public void Recoil()
    {
        ((IDamageable)damageHandler).Recoil();
    }

    public void StartCritVulnerability(float time)
    {
        ((IDamageable)damageHandler).StartCritVulnerability(time);
    }

    public void SetHitParticlePosition(Vector3 position, Vector3 direction)
    {
        SetHitParticleVectors(position, direction);
    }

    public DamageKnockback GetLastTakenDamage()
    {
        return ((IDamageable)damageHandler).GetLastTakenDamage();
    }
}
