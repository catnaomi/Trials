using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageablePoint : MonoBehaviour, IDamageable
{
    public AttributeValue health;
    public bool hasHealth;
    public DamageResistance resistance;
    Collider collider;
    DamageKnockback lastDamageTaken;
    float lastAmountTaken;
    bool critVulnerable;
    float critDuration;
    Vector3 hitPosition;
    Vector3 hitDirection;
    [ReadOnly] public bool isInTimeState;
    public UnityEvent OnHurt;
    public UnityEvent OnDie;

    void Start()
    {
        collider = this.GetComponent<Collider>();
    }
    void Update()
    {
        if (critVulnerable)
        {
            critDuration -= Time.deltaTime;
            if (critDuration <= 0f)
            {
                critVulnerable = false;
            }
        }
    }


    public void TakeDamage(DamageKnockback damage)
    {
        lastDamageTaken = damage;
        
        float damageAmount = damage.GetDamageAmount();
        if (IsTimeStopped())
        {
            TimeTravelController.time.TimeStopDamage(damage, this, damageAmount);
            return;
        }

        bool isCrit = IsCritVulnerable();
        damageAmount = damage.GetDamageAmount(isCrit);

        damageAmount = DamageKnockback.GetTotalMinusResistances(damageAmount, damage.unresistedMinimum, damage.GetTypes(), resistance);

        lastAmountTaken = damageAmount;

        if (hasHealth)
        {
            health.current -= damageAmount;
            if (health.current < 0)
            {
                health.current = 0;
            }
        }
        

        DamageKnockback.GetContactPoints(this, damage, collider, false);

        if (damage.GetTypes().HasType(resistance.weaknesses))
        {
            damage.OnHitWeakness.Invoke();
        }
        if (isCrit)
        {
            damage.OnCrit.Invoke();
        }
        if (hasHealth && health.current <= 0f)
        {
            OnDie.Invoke();
        }
        damage.OnHit.Invoke();      
        OnHurt.Invoke();
        
    }

    public void Recoil()
    {
        
    }

    public void StartCritVulnerability(float time)
    {
        critVulnerable = true;
        critDuration = time;
    }

    public bool IsCritVulnerable()
    {
        return critVulnerable;
    }

    public void SetHitParticleVectors(Vector3 position, Vector3 direction)
    {
        hitPosition = position;
        hitDirection = direction;
    }

    public Vector3 GetHitPosition()
    {
        return hitPosition;
    }

    public Vector3 GetHitDirection()
    {
        return hitDirection;
    }

    public void GetParried()
    {
       
    }

    public void EnableCollider(bool en)
    {
        collider.enabled = en;
    }

    public Collider GetCollider()
    {
        return collider;
    }
    public DamageKnockback GetLastTakenDamage()
    {
        return lastDamageTaken;
    }

    public float GetLastAmountTaken()
    {
        return lastAmountTaken;
    }
    public GameObject GetGameObject()
    {
        return this.gameObject;
    }


    public bool IsTimeStopped()
    {
        return isInTimeState;
    }

    public void StartInvulnerability(float duration)
    {
        throw new System.NotImplementedException();
    }

    public bool IsInvulnerable()
    {
        return false; //TODO: implement invulnerability?
    }
}
