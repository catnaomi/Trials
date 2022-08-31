using System.Collections;
using UnityEngine;


public interface IDamageable
{

    public void TakeDamage(DamageKnockback damage);

    public void Recoil();

    public void StartCritVulnerability(float time);

    public void SetHitParticlePosition(Vector3 position, Vector3 direction);

    public DamageKnockback GetLastTakenDamage();
}