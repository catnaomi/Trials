using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class IceGolemMecanimActor : Actor, IAttacker, IAdjustRootMotion
{
    HumanoidNPCInventory inventory;
    CapsuleCollider collider;
    CharacterController cc;
    [ReadOnly, SerializeField] NavMeshAgent nav;
    public float attackTimer = 2f;
    public float waterDashTimer = 3f;
    public bool isHitboxActive;
    public UnityEvent OnHitboxActive;
    [Header("Navigation")]
    public bool actionsEnabled;
    public float closeRange = 4f;
    public float meleeRange = 2f;
    public float farRange = 10f;
    bool shouldRealign;


    
    bool wasDashingLastFrame;
    [Header("Mecanim Values")]
    [ReadOnly, SerializeField] bool InCloseRange;
    [ReadOnly, SerializeField] bool InMeleeRange;
    [ReadOnly, SerializeField] bool InFarRange;
    [ReadOnly, SerializeField] bool ShouldAttack;
    [ReadOnly, SerializeField] bool ShouldDash;
    [ReadOnly, SerializeField] bool ActionsEnabled;
    [ReadOnly, SerializeField] float Speed;
    [Header("Events")]
    public UnityEvent StartDash;

    public override void ActorStart()
    {
        base.ActorStart();
        nav = GetComponent<NavMeshAgent>();
        cc = GetComponent<CharacterController>();
        collider = this.GetComponent<CapsuleCollider>();
    }
    protected override void ActorOnEnable()
    {
        base.ActorOnEnable();
        this.StartTimer(0.1f, true, SetDestination);
        this.StartTimer(attackTimer, true, BeginAttack);
        this.StartTimer(waterDashTimer, true, BeginWaterDash);
    }


    void Awake()
    {
        inventory = this.GetComponent<HumanoidNPCInventory>();
    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();
        if (inventory.IsMainEquipped() && !inventory.IsMainDrawn())
        {
            inventory.SetDrawn(true, true);
        }
        if (inventory.IsOffEquipped() && !inventory.IsOffDrawn())
        {
            inventory.SetDrawn(false, true);
        }
        UpdateTarget();
        if (IsMoving())
        {
            nav.enabled = true;
            nav.updatePosition = true;
            nav.updateRotation = true;
            cc.enabled = false;
        }
        else
        {
            nav.enabled = false;
            nav.updatePosition = false;
            nav.updateRotation = false;
            cc.enabled = true;
        }
        if (shouldRealign || IsDashing())
        {
            RealignToTarget();
        }
        if (IsDashing() && !wasDashingLastFrame)
        {
            StartDash.Invoke();
        }
        wasDashingLastFrame = IsDashing();
        if (CombatTarget != null)
        {
            float dist = Vector3.Distance(this.transform.position, CombatTarget.transform.position);
            InCloseRange = dist < closeRange;
            InMeleeRange = dist < meleeRange;
            InFarRange = dist < farRange;

            
        }
        
        UpdateMecanimValues();
    }

    void UpdateMecanimValues()
    {
        animator.SetBool("InCloseRange", InCloseRange);
        animator.SetBool("InMeleeRange", InMeleeRange); 
        animator.SetBool("InFarRange", InFarRange);
        ActionsEnabled = actionsEnabled;
        animator.SetBool("ActionsEnabled", ActionsEnabled);
        animator.UpdateTrigger("ShouldAttack", ref ShouldAttack);
        Speed = nav.desiredVelocity.magnitude;
        animator.SetFloat("Speed", Speed);
        animator.UpdateTrigger("ShouldDash", ref ShouldDash);
    }
    void UpdateTarget()
    {
        if (CombatTarget == null && PlayerActor.player != null)
        {
            CombatTarget = PlayerActor.player.gameObject;
        }
    }

    void SetDestination()
    {
        if (CombatTarget != null && IsMoving() && nav.enabled)
        {
            nav.SetDestination(CombatTarget.transform.position);
        }
    }

    public bool ShouldAdjustRootMotion()
    {
        return CombatTarget != null && (IsAttacking() || IsDashing());
    }

    public Vector3 GetAdjustmentRelativePosition()
    {
        if (CombatTarget != null)
        {
            return CombatTarget.transform.position;
        }
        else
        {
            return Vector3.zero;
        }
    }
    public void BeginAttack()
    {
        if (!isInTimeState && actionsEnabled)
        {
            ShouldAttack = true;
        }     
    }

    public void BeginWaterDash()
    {
        if (!isInTimeState && actionsEnabled)
        {
            ShouldDash = true;
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

    public override void RealignToTarget()
    {
        base.RealignToTarget();
        shouldRealign = true;
    }

    public override void DeactivateHitboxes()
    {
        HitboxActive(0);
    }
    public override bool IsHitboxActive()
    {
        return isHitboxActive;
    }

    public bool IsMoving()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("MOVE");
    }

    public bool IsDashing()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("DASH");
    }

    public override bool IsAttacking()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("ATTACK");
    }
}
