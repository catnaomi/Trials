using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

[Serializable]
public class DamageKnockback
{
    public float healthDamage;
    public float staminaDamage;
    [SerializeField]private DamageType[] types;
    [Space(5)]
    public CriticalData critData;
    [Space(5)]
    public bool breaksArmor;
    public bool unblockable;
    public bool disarm;
    public bool isThrust;
    public bool isSlash;
    public bool bouncesOffBlock;
    public bool breaksBlock;
    public bool canDamageSelf;
    public bool cannotAutoFlinch;
    public float stunTime;
    public StaggerData staggers;
    [Space(10)]
    public Vector3 kbForce;
    public bool kbRadial;
    [Space(5)]

    
    [ReadOnly]
    public GameObject hitboxSource;
    [ReadOnly]
    public GameObject source;

    public Vector3 originPoint;
    public FXController.FXMaterial hitMaterial;

    public UnityEvent OnHit;
    public UnityEvent OnCrit;
    public UnityEvent OnBlock;
    
    public static float MAX_CRITVULN_TIME = 3f;
    [Serializable]
    public struct StaggerData
    {
        public StaggerType onHit;
        public StaggerType onArmorHit;
        public StaggerType onCritical;
        public StaggerType onKill;
        public StaggerType onInjure;
    }

    [Serializable]
    public struct CriticalData
    {
        public float criticalMultiplier;
        public bool alwaysCritical;
        public bool causesCritState;
        public bool doesNotConsumeCritState;
        public float criticalExtensionTime;
    }
    public enum StaggerType
    {
        None,           // 0
        // hitsuns
        Flinch,         // 1
        StaggerSmall,   // 2
        StaggerLarge,
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
        this.types = damageKnockback.types;
        this.critData = damageKnockback.critData;
        this.disarm = damageKnockback.disarm;
        this.stunTime = damageKnockback.stunTime;
        this.isSlash = damageKnockback.isSlash;
        this.isThrust = damageKnockback.isThrust;
        this.bouncesOffBlock = damageKnockback.bouncesOffBlock;
        
        this.breaksBlock = damageKnockback.breaksBlock;
        this.canDamageSelf = damageKnockback.canDamageSelf;

        this.critData = damageKnockback.critData;

        this.hitMaterial = damageKnockback.hitMaterial;

        this.OnHit = damageKnockback.OnHit;
        this.OnCrit = damageKnockback.OnCrit;
        this.OnBlock = damageKnockback.OnBlock;
    }

    public DamageKnockback()
    {
        this.healthDamage = 0f;
    }

    public static DamageKnockback GetDefaultDamage()
    {
        DamageKnockback damage = new DamageKnockback();
        damage.healthDamage = 25f;
        damage.staminaDamage = 25f;
        damage.kbForce = Vector3.up;
        damage.staggers = StandardStaggerData;

        damage.types = new DamageType[1] { DamageType.Standard_SlashPierce };

        damage.critData = StandardCritData;

        damage.stunTime = 1f;

        return damage;
    }

    public static Vector3 GetKnockbackRelativeToTransform(Vector3 vector, Transform transform)
    {
        return transform.forward * vector.z + transform.right * vector.x + transform.up * vector.y;
    }

    public static readonly StaggerData StandardStaggerData = new StaggerData()
    {
        onHit = StaggerType.StaggerSmall,
        onArmorHit = StaggerType.Flinch,
        onCritical = StaggerType.Stumble,
        onKill = StaggerType.Crumple,
        onInjure = StaggerType.Crumple,
    };

    public static readonly CriticalData StandardCritData = new CriticalData()
    {
        criticalMultiplier = 1f,
        alwaysCritical = false,
        causesCritState = false,
        doesNotConsumeCritState = false,
        criticalExtensionTime = 0.25f,
    };

    public static float GetTotalMinusResistances(float damage, DamageType[] typeArray, List<DamageResistance> resists)
    {

        float total = damage;
        float ratio = 1f;
        float flat = 0f;
        if (typeArray == null || typeArray.Length <= 0) return total;
        List<DamageType> types = new List<DamageType>();
        types.AddRange(typeArray);
        foreach (DamageResistance resist in resists)
        {
            //if (types.Contains(resist.type)) {
            if (DamageResistContains(resist.type, types))
            {
                ratio *= resist.ratio;
                flat += resist.flat;
            }
        }
        total *= ratio;
        total -= flat;
        return total;
    }

    public static bool DamageResistContains(DamageType defendedType, List<DamageType> attackTypes)
    {
        switch (defendedType)
        {
            case DamageType.All:
                return true;
            case DamageType.Earth:
                return attackTypes.Contains(defendedType) || attackTypes.Contains(DamageType.Slashing) || attackTypes.Contains(DamageType.Piercing) || attackTypes.Contains(DamageType.Blunt);
            default:
                return attackTypes.Contains(defendedType);
        }
    }


    public void AddTypes(DamageType[] newtypes)
    {
        List<DamageType> dtypes = new List<DamageType>();
        dtypes.AddRange(this.types);
        foreach (DamageType type in newtypes)
        {
            if (!dtypes.Contains(type))
            {
                dtypes.Add(type);
            }
        }
        types = dtypes.ToArray();
    }

    public DamageType[] GetTypes()
    {
        List<DamageType> dtypes = new List<DamageType>();
        foreach (DamageType type in this.types)
        {
            if (type == DamageType.Standard_SlashPierce)
            {
                if (isSlash)
                {
                    dtypes.Add(DamageType.Slashing);
                }
                else if (isThrust)
                {
                    dtypes.Add(DamageType.Piercing);
                }
            }
            else
            {
                dtypes.Add(type);
            }
        }
        return dtypes.ToArray();
    }

    public float GetDamageAmount()
    {
        return GetDamageAmount(false);
    }
    public float GetDamageAmount(bool isCrit)
    {
        return this.healthDamage * (isCrit ? critData.criticalMultiplier : 1f);
    }

}

[Serializable]
public struct DamageResistance
{
    public DamageType type;
    public float ratio; // percentage value reduciton
    public float flat; // flat value reduction
}