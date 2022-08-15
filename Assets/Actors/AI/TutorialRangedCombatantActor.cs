using UnityEngine;
using System.Collections;
using Animancer;
using UnityEngine.Events;

[RequireComponent(typeof(HumanoidNPCInventory))]
public class TutorialRangedCombatantActor : NavigatingHumanoidActor, IAttacker, IDamageable
{
    HumanoidNPCInventory inventory;
    [Header("Combatant Settings")]
    public float SightRange = 15f;
    public bool InSightRange;
    [Space(5)]
    public AimAttack RangedAttack;
    public float RangedAttackRange = 25f;
    public bool InRangedAttackRange;
    [Space(5)]
    public DamageAnims damageAnims;
    HumanoidDamageHandler damageHandler;
    [Space(5)]
    public GameObject deathParticle;
    [Space(10)]
    public float clock;
    public float ActionDelayMinimum = 2f;
    public float ActionDelayMaximum = 5f;
    
    public float LowHealthThreshold = 50f;
    public bool isLowHealth;
    bool isHitboxActive;
    bool aiming;
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
        _MoveOnEnd = () =>
        {
            animancer.Play(navstate.move, 0.1f);
        };

        damageHandler = new SimplifiedDamageHandler(this, damageAnims, animancer);
        //damageHandler.SetEndAction(_MoveOnEnd);

        OnHurt.AddListener(() => {
            HitboxActive(0);
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
        bool shouldAct = (clock <= 0f);

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

        if (inventory.IsRangedEquipped() && !inventory.IsRangedDrawn())
        {
            inventory.SetDrawn(Inventory.RangedType, true);
        }
        if (shouldAct && CanAct())
        {
            clock = Random.Range(ActionDelayMinimum, ActionDelayMaximum);
            if (CombatTarget != null)
            {
                float navdist = GetDistanceToTarget();
                float realdist = Vector3.Distance(this.transform.position, GetCombatTarget().transform.position);

                InSightRange = realdist <= SightRange;
                InRangedAttackRange = realdist <= RangedAttackRange && IsClearLineToTarget();

                if (InSightRange && !IsClearLineToTarget())
                {
                    SetDestination(CombatTarget);
                    ResumeNavigation();
                }
                else
                {
                    StopNavigation();
                }
                if (InRangedAttackRange)
                {
                    if (!aiming)
                    {
                        StartAiming();
                    }
                    else
                    {
                        StartRangedAttack();
                    }
                   
                }
            }
        }
        if (aiming)
        {
            RangedAttack.OnUpdate(this);
            Vector3 lookDirection;
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
            navstate.idle.Parameter = angle;
        }
        animancer.Layers[0].ApplyAnimatorIK = aiming;
    }

    public void StartAiming()
    {
        animancer.Play(navstate.idle);
        animancer.Layers[HumanoidAnimLayers.UpperBody].Play(RangedAttack.GetStartClip());
        aiming = true;
    }
    public void StartRangedAttack()
    {
        animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
        cstate.attack = animancer.Play(RangedAttack.GetFireClip(), 0f);
        cstate.attack.Events.OnEnd = _MoveOnEnd;
        aiming = false;
        OnAttack.Invoke();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (aiming)
        {
            RangedAttack.OnIK(animancer.Animator);
        }
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

    public DamageKnockback GetCurrentDamage()
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
        return true;
    }
    public override bool IsDodging()
    {
        return animancer.States.Current == cstate.dodge;
    }

    public override bool IsAttacking()
    {
        return animancer.States.Current == cstate.attack;
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

    public override void Die()
    {
        if (dead) return;
        OnDie.Invoke();
        GameObject particle = Instantiate(deathParticle);
        particle.transform.position = this.GetComponent<Collider>().bounds.center;
        Destroy(particle, 5f);
        Destroy(this.gameObject);
        return;
        /*
        foreach (Renderer r in this.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
        foreach (Collider c in this.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
        this.GetComponent<Collider>().enabled = false;
        
        actionsEnabled = false;
        */
    }
}
