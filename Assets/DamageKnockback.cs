using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class DamageKnockback
{
    public float staminaDamage;
    public float poiseDamage;

    [Space(10)]
    public Vector3 kbForce;

    public bool kbRadial;

    public Damage damage;

    [ReadOnly]
    public GameObject hitboxSource;
    [ReadOnly]
    public GameObject source;

    public Vector3 originPoint;

    public bool breaksArmor;
    public bool unblockable;

    public StaggerData staggers;
    
    [Serializable]
    public struct StaggerData
    {
        public StaggerType onHitOnBalance;
        public StaggerType onHitOffBalance;

        //public StaggerType onBlockOnBalance;
        //public StaggerType onBlockOffBalance;

        public StaggerType onInjure;
        public StaggerType onKill;
    }
    public enum StaggerType
    {
        None,           // 0
        // on hit
        Flinch,         // 1
        LightStagger,   // 2
        HeavyStagger,   // 3
        Knockdown,      // 4
        Stun,           // 5
        Crumple,        // 6

        // on block
        BlockStagger,   // 7
        GuardBreak,     // 8

        Recoil,          // 9 shorter stun
        FallDamage,       // 10
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
        this.staggers = damageKnockback.staggers;
        this.poiseDamage = damageKnockback.poiseDamage;
        this.hitClip = damageKnockback.hitClip;
        this.breaksArmor = damageKnockback.breaksArmor;
        this.kbRadial = damageKnockback.kbRadial;
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

    public static StaggerData StandardStaggerData = new StaggerData()
    {
        onHitOnBalance = StaggerType.LightStagger,
        onHitOffBalance = StaggerType.HeavyStagger,
        onInjure = StaggerType.Knockdown,
        onKill = StaggerType.Crumple
    };
}
