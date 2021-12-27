using System.Collections;
using UnityEngine;


public interface IDamageable
{

    public void TakeDamage(DamageKnockback damage);

    public void Recoil();
}