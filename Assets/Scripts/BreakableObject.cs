using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BreakableObject : MonoBehaviour, IDamageable
{
    public List<DamageType> brokenByElements;
    public GameObject particlePrefab;
    public UnityEvent OnBreak;
    public void Recoil()
    {
        
    }

    public void StartCritVulnerability(float time)
    {
        
    }

    public void TakeDamage(DamageKnockback damage)
    {
        foreach (DamageType element in damage.GetTypes())
        {
            if (brokenByElements.Contains(element))
            {
                BreakObject();
                break;
            }
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
}