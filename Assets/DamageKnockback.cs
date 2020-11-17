using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class DamageKnockback
{

    //public float healthDamagePotential;
    public float poiseDamage;
    public float staminaDamage;

    [Space(10)]
    public Vector3 kbForce;

    public Damage damage;

    [HideInInspector]
    public GameObject hitboxSource;
    [HideInInspector]
    public GameObject source;

    public bool breaksArmor;
    public bool unblockable;

    public StaggerType minStaggerType;
    public StaggerType staggerType;
    public enum StaggerType
    {
        None,           // 0
        // on hit
        Flinch,         // 1
        Stagger,        // 2 AKA light stagger
        // require low poise
        HeavyStagger,      // 3 AKA heavy stagger
        Knockdown,      // 4 also on injure
        // special
        Stun,         // 5 

        // on block
        BlockStagger,   // 6
        GuardBreak,     // 7

        Recoil,          // 8 shorter stun
        FallDamage,       // 9
    }

    //public bool breaksArmor;

    public AudioClip hitClip;

    public DamageKnockback(Vector3 force)
    {
        this.kbForce = force;
    }

    public DamageKnockback(DamageKnockback damageKnockback)
    {
        this.staminaDamage = damageKnockback.staminaDamage;
        this.kbForce = damageKnockback.kbForce.normalized * damageKnockback.kbForce.magnitude;
        this.poiseDamage = damageKnockback.poiseDamage;
        this.staggerType = damageKnockback.staggerType;
        this.minStaggerType = damageKnockback.minStaggerType;
        this.hitClip = damageKnockback.hitClip;
        this.breaksArmor = damageKnockback.breaksArmor;

        this.damage = new Damage(damageKnockback.damage);
    }

    public DamageKnockback()
    {
        this.damage = new Damage();
    }

    public static Vector3 GetKnockbackRelativeToTransform(Vector3 vector, Transform transform)
    {
        return transform.forward * vector.z + transform.right * vector.x + transform.up * vector.y;
    }
}
