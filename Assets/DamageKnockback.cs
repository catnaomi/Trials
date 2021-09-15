using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class DamageKnockback
{
    public float staminaDamage;

    [Space(10)]
    public Vector3 kbForce;

    public bool kbRadial;

    public float healthDamage;
    public DamageType[] types;
    public float heartsDamage;
    public float criticalMultiplier;

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
        public StaggerType onHit;
        public StaggerType onArmorHit;
        public StaggerType onCritical;
        public StaggerType onInjure;
        public StaggerType onKill;
        public StaggerType onHelpless;
    }
    public enum StaggerType
    {
        None,           // 0
        // hitsuns
        Flinch,         // 1
        Stagger,   // 2
        Stumble,   // 3
        // knockouts
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
        this.hitClip = damageKnockback.hitClip;
        this.breaksArmor = damageKnockback.breaksArmor;
        this.kbRadial = damageKnockback.kbRadial;
        this.healthDamage = damageKnockback.healthDamage;
        this.heartsDamage = damageKnockback.heartsDamage;
        this.types = damageKnockback.types;
    }

    public DamageKnockback()
    {
        this.healthDamage = 0f;
    }

    public static Vector3 GetKnockbackRelativeToTransform(Vector3 vector, Transform transform)
    {
        return transform.forward * vector.z + transform.right * vector.x + transform.up * vector.y;
    }

    public static StaggerData StandardStaggerData = new StaggerData()
    {
        onHit = StaggerType.Stagger,
        onArmorHit = StaggerType.Flinch,
        onCritical = StaggerType.Stumble,
        onInjure = StaggerType.Stumble,
        onKill = StaggerType.Crumple,
        onHelpless = StaggerType.Crumple,
    };

    public static float GetTotalMinusResistances(float damage, DamageType[] typeArray, List<DamageResistance> resists)
    {

        float total = damage;

        if (typeArray == null || typeArray.Length <= 0) return total;
        List<DamageType> types = new List<DamageType>();
        types.AddRange(typeArray);
        foreach (DamageResistance resist in resists)
        {
            if (types.Contains(resist.type)) {
                total *= resist.ratio;
            }
        }

        return total;
    }
}

[Serializable]
public struct DamageResistance
{
    public DamageType type;
    public float ratio; // percentage value reduciton
}