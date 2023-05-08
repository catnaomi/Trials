using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DojoBossMecanimActor : Actor, IDamageable, IAttacker
{
    
    Animator animator;
    HumanoidNPCInventory inventory;
    bool isHitboxActive;
    public UnityEvent OnHitboxActive;
    [Header("Mecanim Values")]
    [ReadOnly, SerializeField] bool InCloseRange;
    [ReadOnly, SerializeField] bool InMeleeRange;
    [Space(10)]
    public float closeRange = 5f;
    public float meleeRange = 1f;
    public float randomCycleSpeed = 3f;
    float randomClock = 0f;
    bool shouldRealign;
    // Start is called before the first frame update
    public override void ActorStart()
    {
        base.ActorStart();
        animator = this.GetComponent<Animator>();
        inventory = this.GetComponent<HumanoidNPCInventory>();
        CombatTarget = PlayerActor.player.gameObject;
        OnHitboxActive.AddListener(RealignToTarget);
    }

    // Update is called once per frame
    public override void ActorPostUpdate()
    {
        if (!inventory.IsMainDrawn())
        {
            inventory.SetDrawn(Inventory.MainType, true);
        }
        randomClock -= Time.deltaTime;
        InCloseRange = Vector3.Distance(this.transform.position, CombatTarget.transform.position) <= closeRange;
        InMeleeRange = Vector3.Distance(this.transform.position, CombatTarget.transform.position) <= meleeRange;
        if (shouldRealign)
        {
            this.transform.LookAt(CombatTarget.transform, Vector3.up);
            shouldRealign = false;
        }
        UpdateMecanimValues();
    }


    void UpdateMecanimValues()
    {
        if (randomClock <= 0f)
        {
            animator.SetFloat("Random", Random.value);
            randomClock = randomCycleSpeed;
        }
        
        animator.SetBool("InCloseRange", InCloseRange);
        animator.SetBool("InMeleeRange", InMeleeRange);
    }

    public override void RealignToTarget()
    {
        base.RealignToTarget();
        shouldRealign = true;
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
    public GameObject GetGameObject()
    {
        throw new System.NotImplementedException();
    }

    public DamageKnockback GetLastDamage()
    {
        return currentDamage;
    }

    public DamageKnockback GetLastTakenDamage()
    {
        throw new System.NotImplementedException();
    }

    public void GetParried()
    {
        throw new System.NotImplementedException();
    }

    public void Recoil()
    {
        throw new System.NotImplementedException();
    }

    public void StartCritVulnerability(float time)
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(DamageKnockback damage)
    {
        throw new System.NotImplementedException();
    }
}
