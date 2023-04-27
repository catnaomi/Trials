﻿using System.Collections;
using UnityEngine;


public interface IDamageable
{

    public void TakeDamage(DamageKnockback damage);

    public void Recoil();

    public void StartCritVulnerability(float time);

    public void SetHitParticleVectors(Vector3 position, Vector3 direction);

    public void GetParried();
    public DamageKnockback GetLastTakenDamage();

    public GameObject GetGameObject();
}