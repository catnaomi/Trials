using UnityEngine;
using System.Collections;
using Animancer;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(HumanoidNPCInventory))]
public class TutorialShieldCombatantActor : NavigatingHumanoidActor, IAttacker, IDamageable
{
    HumanoidNPCInventory inventory;
    [Header("Combatant Settings")]
    public float MinEngageRange = 3.5f;
    public float MaxEngageRange = 10f;
    [Space(5)]
    public float SightRange = 15f;
    public bool InSightRange;
    [Space(5)]
    public InputAttack CloseAttack;
    public float CloseAttackRange = 1.5f;
    public bool InCloseAttackRange;
    public float CloseAttackAngleOffset = 0f;
    [Space(5)]
    public DamageAnims damageAnims;
    public AvatarMask rightHandMask;
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

        damageHandler = new HumanoidDamageHandler(this, damageAnims, animancer);
        damageHandler.SetEndAction(_MoveOnEnd);
        damageHandler.SetBlockEndAction(_MoveOnEnd);

        animancer.Layers[HumanoidAnimLayers.BilayerBlend].SetMask(rightHandMask);

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

        if (inventory.IsMainEquipped() && !inventory.IsMainDrawn())
        {
            inventory.SetDrawn(true, true);
        }
        if (inventory.IsOffEquipped() && !inventory.IsOffDrawn())
        {
            inventory.SetDrawn(false, true);
        }
        if (shouldAct && CanAct())
        {
            clock = Random.Range(ActionDelayMinimum, ActionDelayMaximum);
            if (CombatTarget != null)
            {
                float navdist = GetDistanceToTarget();
                float realdist = Vector3.Distance(this.transform.position, GetCombatTarget().transform.position);

                InSightRange = realdist <= SightRange;
                InCloseAttackRange = navdist <= CloseAttackRange && nav.hasPath;
 

                if (InSightRange)
                {
                    SetDestination(CombatTarget);
                    ResumeNavigation();
                }
                else
                {
                    StopNavigation();
                }
                if (InCloseAttackRange)
                {
                    StartCloseAttack();
                }
            }
        }
    }

    public void StartCloseAttack()
    {
        RealignToTargetWithOffset(CloseAttackAngleOffset);
        cstate.attack = CloseAttack.ProcessHumanoidAttack(this, _MoveOnEnd);
        OnAttack.Invoke();
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

    public override bool IsDodging()
    {
        return animancer.States.Current == cstate.dodge;
    }

    public override bool IsAttacking()
    {
        return animancer.States.Current == cstate.attack;
    }

    public override bool IsBlocking()
    {
        return true;
    }

    public override List<DamageResistance> GetBlockResistance()
    {
        List<DamageResistance> dr = new List<DamageResistance>();
        dr.AddRange(inventory.GetBlockResistance());
        return dr;
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

    public override void Die()
    {
        base.Die();
        foreach(Renderer r in this.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
        GameObject particle = Instantiate(deathParticle);
        particle.transform.position = this.GetComponent<Collider>().bounds.center;
        Destroy(particle, 5f);

    }
}
