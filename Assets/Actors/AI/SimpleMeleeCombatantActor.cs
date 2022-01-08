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
    public InputAttack GapCloserAttack;
    public float GapCloserRange = 5f;
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
    public DamageAnims damageAnims;
    HumanoidDamageHandler damageHandler;
    [Space(10)]
    public float clock;
    public static float CLOCK_DEFAULT = 2f;
    
    public float LowHealthThreshold = 50f;
    public bool isLowHealth;
    bool isHitboxActive;
    DamageKnockback currentDamage;
    public UnityEvent OnHitboxActive;

    protected CombatState cstate;
    protected struct CombatState
    {
        public AnimancerState attack;
        public AnimancerState approach;
        public AnimancerState dodge;
        public AnimancerState sheathe;
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
        damageHandler = new HumanoidDamageHandler(this, damageAnims, animancer);
        damageHandler.SetEndAction(_MoveOnEnd);
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
        bool shouldAct = clock <= 0f;

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

        if (shouldAct)
        {
            clock = CLOCK_DEFAULT * Random.value;
            float navdist = GetDistanceToTarget();
            float realdist = Vector3.Distance(this.transform.position, GetCombatTarget().transform.position);

            InSightRange = realdist <= SightRange;
            InMeleeRange = navdist <= MeleeAttackRange && nav.hasPath;
            InGapRange = navdist <= GapCloserRange && nav.hasPath;
            InPowerRange = navdist <= PowerAttackRange && nav.hasPath;

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
            if (InMeleeRange && CanAct())
            {
                StartMeleeAttack();
            }
        }
    }

    public void StartMeleeAttack()
    {
        cstate.attack = animancer.Play(MeleeAttack.GetClip());
        cstate.attack.Events.OnEnd = _MoveOnEnd;
        SetCurrentDamage(MeleeAttack.GetDamage());
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

    public bool IsHitboxActive()
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

    public void SetCurrentDamage(DamageKnockback damageKnockback)
    {
        currentDamage = new DamageKnockback(damageKnockback);
        currentDamage.source = this.gameObject;
    }

    public DamageKnockback GetCurrentDamage()
    {
        return currentDamage;
    }


    public bool DetermineCombatTarget(out GameObject target)
    {
        target = PlayerActor.player.gameObject;
        return PlayerActor.player.gameObject.tag != "Corpse";
    }

    public override void ProcessDamageKnockback(DamageKnockback damageKnockback)
    {
        damageHandler.TakeDamage(damageKnockback);
    }

    public void BeingAttacked()
    {
        if (CombatTarget != null && currentDistance < bufferRange)
        {
            animator.SetTrigger("GettingAttacked");
        }
    }

    public bool CanAct()
    {
        return animancer.States.Current == navstate.move || animancer.States.Current == navstate.idle;
    }

    public void TakeDamage(DamageKnockback damage)
    {
        ((IDamageable)damageHandler).TakeDamage(damage);
    }

    public void Recoil()
    {
        ((IDamageable)damageHandler).Recoil();
    }
}
