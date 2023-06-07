using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

[Serializable]
public class DamageKnockback
{
    public float healthDamage;
    public float unresistedMinimum;
    [SerializeField]private DamageType types;
    [Space(5)]
    public CriticalData critData;
    [Space(5)]
    public bool breaksArmor;
    public bool unblockable;
    public bool disarm;
    public bool isThrust;
    public bool isSlash;
    public bool isRanged;
    public bool isParry;
    public bool bouncesOffBlock;
    public bool breaksBlock;
    public bool canDamageSelf;
    public bool cannotAutoFlinch;
    public bool cannotRecoil;
    public bool cannotKill;
    public float stunTime;
    public float repositionLength;
    public StaggerStrength stagger;
    public StaggerData staggers;
    [Space(10)]
    public FXData fxData;
    [Space(10)]
    public Vector3 kbForce;
    public bool kbRadial;
    [Space(5)]
    [ReadOnly] public bool didCrit;
    
    [ReadOnly]
    public GameObject hitboxSource;
    [ReadOnly]
    public GameObject source;

    public Vector3 originPoint;
    public FXController.FXMaterial hitMaterial;

    public UnityEvent OnHit;
    public UnityEvent OnCrit;
    public UnityEvent OnBlock;
    public UnityEvent OnHitWeakness;
    
    public static float MAX_CRITVULN_TIME = 5f;
    public static readonly int SLASH_INT = -1;
    public static readonly int THRUST_INT = 1;

    [Serializable]
    public struct StaggerData
    {
        public StaggerType onHit;
        public StaggerType onArmorHit;
        public StaggerType onCritical;
        public StaggerType onKill;
        public StaggerType onCounterHit;
        [Space(5)]
        public BlockStaggerType onBlock;
        public BlockStaggerType onTypedBlock;
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

    public struct FXData
    {
        public bool isHeavyAttack;
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
        SpinDeath,           // 5
        Crumple,        // 6
        FallOver,
        /*
        // on block
        BlockStagger,   // 7
        GuardBreak,     // 8
        Recoil,          // 9 shorter stun

        FallDamage,       // 10
        */
    }

    public enum BlockStaggerType
    {
        BlockSmall,
        BlockLarge,
        BlockHeavy,
        Flinch,
        GuardBreak,
        Recoil,
        AutoParry,
    }
    public enum StaggerStrength
    {
        Light,
        Heavy,
    }

    //public bool breaksArmor;

    public AudioClip hitClip;

    public DamageKnockback(Vector3 force)
    {
        this.kbForce = force;
    }

    public DamageKnockback(DamageKnockback damageKnockback)
    {
        this.kbForce = damageKnockback.kbForce.normalized * damageKnockback.kbForce.magnitude;
        this.stagger = damageKnockback.stagger;
        this.hitClip = damageKnockback.hitClip;
        this.breaksArmor = damageKnockback.breaksArmor;
        this.kbRadial = damageKnockback.kbRadial;
        this.healthDamage = damageKnockback.healthDamage;
        this.unresistedMinimum = damageKnockback.unresistedMinimum;
        this.types = damageKnockback.types;
        this.critData = damageKnockback.critData;
        this.disarm = damageKnockback.disarm;
        this.stunTime = damageKnockback.stunTime;
        this.isSlash = damageKnockback.isSlash;
        this.isThrust = damageKnockback.isThrust;
        this.isRanged = damageKnockback.isRanged;
        this.isParry = damageKnockback.isParry;
        this.bouncesOffBlock = damageKnockback.bouncesOffBlock;
        this.cannotRecoil = damageKnockback.cannotRecoil;
        this.cannotKill = damageKnockback.cannotKill;

        this.repositionLength = damageKnockback.repositionLength;
        this.breaksBlock = damageKnockback.breaksBlock;
        this.canDamageSelf = damageKnockback.canDamageSelf;

        this.critData = damageKnockback.critData;

        this.hitMaterial = damageKnockback.hitMaterial;

        this.staggers = damageKnockback.staggers;

        this.originPoint = damageKnockback.originPoint;
        this.hitboxSource = damageKnockback.hitboxSource;
        this.source = damageKnockback.source;

        this.OnHit = damageKnockback.OnHit ?? new UnityEvent();
        this.OnCrit = damageKnockback.OnCrit ?? new UnityEvent();
        this.OnBlock = damageKnockback.OnBlock ?? new UnityEvent();
        this.OnHitWeakness = damageKnockback.OnHitWeakness ?? new UnityEvent();
    }

    public DamageKnockback()
    {
        this.healthDamage = 0f;
    }

    public static DamageKnockback GetDefaultDamage()
    {
        DamageKnockback damage = new DamageKnockback();
        damage.healthDamage = 1f;
        damage.unresistedMinimum = 1f;
        damage.kbForce = Vector3.up;
        damage.stagger = StaggerStrength.Light;

        damage.types = DamageType.Standard_SlashPierce;

        damage.critData = StandardCritData;

        damage.stunTime = 1f;
        damage.repositionLength = 1f;
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
        onCounterHit = StaggerType.StaggerSmall,
        onBlock = BlockStaggerType.BlockSmall,
        onTypedBlock = BlockStaggerType.BlockSmall,
    };

    public static readonly CriticalData StandardCritData = new CriticalData()
    {
        criticalMultiplier = 1f,
        alwaysCritical = false,
        causesCritState = false,
        doesNotConsumeCritState = false,
        criticalExtensionTime = 0.25f,
    };

    public static float GetTotalMinusResistances(float damage, DamageType types, DamageResistance resistance)
    {
        return GetTotalMinusResistances(damage, 0f, types, resistance);
    }

    public static float GetTotalMinusResistances(float damage, float minimum, DamageType types, DamageResistance resistance)
    {

        float total = damage;
        float ratio = 1f;
        float flat = 0f;
        bool neutral = true;
        bool weak = false;
        bool strong = false;
        if (types == 0) return total;
        if ((types & resistance.weaknesses) != 0)
        {
            ratio *= resistance.weaknessMultiplier;
            flat += resistance.weaknessFlat;
            neutral = false;
            weak = true;
        }
        if ((types & resistance.strengths) != 0)
        {
            ratio *= resistance.strengthMultiplier;
            flat += resistance.strengthsFlat;
            strong = true;
            neutral = false;
        }
        if (neutral)
        {
            ratio *= resistance.neutralMultiplier;
            flat += resistance.neutralFlat;
        }
        total *= ratio;
        total -= flat;
        total = Mathf.Ceil(total);
        if ((neutral || weak) && total < minimum)
        {
            total = minimum;
        }
       
        return total;
    }
    public void AddTypes(DamageType newtypes)
    {
        types |= newtypes;
    }

    public void AddTypes(DamageType[] newtypes)
    {
        foreach (DamageType type in newtypes)
        {
            types |= type;
        }
    }

    public DamageType GetTypes()
    {
        DamageType dtypes = types;
        if (types.HasFlag(DamageType.Standard_SlashPierce))
        {
            if (isSlash)
            {
                return types | DamageType.Slashing;
            }
            else if (isThrust)
            {
                return types | DamageType.Piercing;
            }
        }
        return types;
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
public class DamageResistance
{
    public DamageType strengths;
    public float strengthMultiplier = 1f;
    public float strengthsFlat = 0f;
    [Space(10)]
    public DamageType weaknesses;
    public float weaknessMultiplier = 1f;
    public float weaknessFlat = 0f;
    [Space(10)]
    public float neutralMultiplier = 1f;
    public float neutralFlat = 0f;

    public static DamageResistance Add(DamageResistance dr1, DamageResistance dr2)
    {
        DamageResistance dr = new DamageResistance();
        dr.strengths = dr1.strengths | dr2.strengths;
        dr.strengthMultiplier = dr1.strengthMultiplier * dr2.strengthMultiplier;
        dr.strengthsFlat = dr1.strengthsFlat + dr2.strengthsFlat;

        dr.weaknesses = dr1.weaknesses | dr2.weaknesses;
        dr.weaknessMultiplier = dr1.weaknessMultiplier * dr2.weaknessMultiplier;
        dr.weaknessFlat = dr1.weaknessFlat + dr2.weaknessFlat;

        dr.neutralMultiplier = dr1.neutralMultiplier * dr2.neutralMultiplier;
        dr.neutralFlat = dr1.neutralFlat + dr2.neutralFlat;
        return dr;
    }

    public DamageResistance()
    {
        strengthMultiplier = 1f;
        weaknessMultiplier = 1f;
        neutralMultiplier = 1f;
    }
}