using System.Collections;
using UnityEngine;


public interface IDamageable
{

    public void TakeDamage(DamageKnockback damage);

    public void Recoil();

    public void StartCritVulnerability(float time);

    public void StopCritVulnerability();
    public bool IsCritVulnerable();
    public void SetHitParticleVectors(Vector3 position, Vector3 direction);

    public void GetParried();

    public void StartInvulnerability(float duration);

    public bool IsInvulnerable();
    public DamageKnockback GetLastTakenDamage();

    public GameObject GetGameObject();
}