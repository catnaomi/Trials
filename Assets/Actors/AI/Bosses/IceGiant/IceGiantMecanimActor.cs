using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceGiantMecanimActor : Actor, IAttacker, IDamageable
{
    [Header("Position Reference")]
    public Transform RightHand;
    public Transform LeftHand;
    public GameObject leftLeg;
    public DamageablePoint leftLegWeakPoint;
    public GameObject rightLeg;
    public DamageablePoint rightLegWeakPoint;
    public DamageablePoint weakPoint;
    [Header("Weapons")]
    public float RightWeaponLength = 1f;
    public float RightWeaponRadius = 1f;
    [Space(15)]
    public float LeftWeaponLength = 1f;
    public float LeftWeaponRadius = 1f;
    [Header("Attacks")]
    public DamageKnockback tempDamage;
    public float getupDelay = 5f;
    float getupClock = 0f;
    HitboxGroup rightHitboxes;
    DamageKnockback lastTakenDamage;
    HitboxGroup leftHitboxes;
    [Header("Mecanim Values")]
    [ReadOnly, SerializeField] bool Dead;
    [ReadOnly, SerializeField] bool IsFallen;
    [ReadOnly, SerializeField] bool Fall;
    public override void ActorStart()
    {
        base.ActorStart();
        animator = this.GetComponent<Animator>();
        GenerateWeapons();
        leftLegWeakPoint.OnHurt.AddListener(() => TakeDamageFromDamagePoint(leftLegWeakPoint));
        rightLegWeakPoint.OnHurt.AddListener(() => TakeDamageFromDamagePoint(rightLegWeakPoint));
        weakPoint.OnHurt.AddListener(() => TakeDamageFromDamagePoint(weakPoint));
        //EnableWeakPoint(false);
    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();

        if (IsFallen)
        {
            getupClock -= Time.deltaTime;
            if (getupClock <= 0f)
            {
                GetUp();
            }
        }
        UpdateMecanimValues();
    }

    void UpdateMecanimValues()
    {
        animator.SetBool("IsFallen", IsFallen);
        animator.UpdateTrigger("Fall", ref Fall);
    }
    void GenerateWeapons()
    {
        if (rightHitboxes != null)
        {
            rightHitboxes.DestroyAll();
        }
        if (leftHitboxes != null)
        {
            leftHitboxes.DestroyAll();
        }
        rightHitboxes = Hitbox.CreateHitboxLine(RightHand.position, RightHand.up, RightWeaponLength, RightWeaponRadius, RightHand, new DamageKnockback(tempDamage), this.gameObject);
        leftHitboxes = Hitbox.CreateHitboxLine(LeftHand.position, LeftHand.up, LeftWeaponLength, LeftWeaponRadius, LeftHand, new DamageKnockback(tempDamage), this.gameObject);
    }
    public DamageKnockback GetLastDamage()
    {
        return tempDamage;
    }


    public void TakeDamageFromDamagePoint(DamageablePoint point)
    {
        attributes.health.current -= point.GetLastAmountTaken();
        if (point.hasHealth && point.health.current <= 0f)
        {
            BreakDamageablePoint(point);
        }
        lastDamageTaken = point.GetLastTakenDamage();
        SetHitParticleVectors(point.GetHitPosition(), point.GetHitDirection());
        OnHurt.Invoke();
    }

    void BreakDamageablePoint(DamageablePoint point)
    {
        point.gameObject.SetActive(false);
        if (point == rightLegWeakPoint)
        {
            rightLeg.SetActive(false);
        }
        else if (point == leftLegWeakPoint)
        {
            leftLeg.SetActive(false);
        }
        FallOver();
    }

    public void FallOver()
    {
        getupClock = getupDelay;
        //EnableWeakPoint(true);
        weakPoint.StartCritVulnerability(getupDelay);
        IsFallen = true;
    }

    public void GetUp()
    {
        IsFallen = false;
        //EnableWeakPoint(false);
    }
    public void EnableWeakPoint(bool active)
    {
        weakPoint.gameObject.SetActive(active);
    }
    public void HitboxActive(int active)
    {
        if (active == 1)
        {
            rightHitboxes.SetActive(true);
            leftHitboxes.SetActive(false);
        }
        else if (active == 2)
        {
            rightHitboxes.SetActive(false);
            leftHitboxes.SetActive(true);
        }
        else if (active == 0)
        {
            rightHitboxes.SetActive(false);
            leftHitboxes.SetActive(false);
        }

    }

    public void TakeDamage(DamageKnockback damage)
    {
        // do nothing, never takes damage directly
    }

    public void Recoil()
    {
        
    }

    public void StartCritVulnerability(float time)
    {
        
    }

    public bool IsCritVulnerable()
    {
        return false;
    }

    public void GetParried()
    {
        
    }

    public DamageKnockback GetLastTakenDamage()
    {
        return lastDamageTaken;
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }
}
