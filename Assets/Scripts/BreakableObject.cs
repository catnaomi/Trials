using System.Collections;
using System.Collections.Generic;
using CustomUtilities;
using UnityEngine;
using UnityEngine.Events;

public class BreakableObject : MonoBehaviour, IDamageable
{
    public DamageType brokenByElements;
    public GameObject particlePrefab;
    public UnityEvent OnBreak;
    public UnityEvent OnFail;
    public bool recoilOnFail = true;
    DamageKnockback lastDamage;
    public void Recoil()
    {
        
    }

    public void StartCritVulnerability(float time)
    {
        
    }

    public void TakeDamage(DamageKnockback damage)
    {
        lastDamage = damage;
        if (damage.GetTypes().HasType(brokenByElements))
        {
            BreakObject();
            return;
        }
        if (recoilOnFail)
        {
            
            if (!damage.isRanged && damage.source.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.Recoil();   
            }
            damage.OnBlock.Invoke();
        }
    }

    public void BreakObject()
    {
        if (particlePrefab != null)
        {
            GameObject particle = Instantiate(particlePrefab);
            particle.transform.SetPositionAndRotation(this.transform.position, this.transform.rotation);
            Destroy(particle, 5f);
        }
        OnBreak.Invoke();
        Destroy(this.gameObject, 0.01f);
    }

    public void SetHitParticlePosition(Vector3 position, Vector3 direction)
    {
        //throw new System.NotImplementedException();
    }

    public DamageKnockback GetLastTakenDamage()
    {
        return lastDamage;
    }
}