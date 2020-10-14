using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class Damage
{
    public float potential;

    public float truedmg = 0f;
    public float slash = 0f;
    public float pierce = 0f;
    public float earth = 0f;
    public float light = 0f;
    public float dark = 0f;
    public float fire = 0f;
    public float water = 0f;
    public float air = 0f;


    public Damage()
    {
        truedmg = 0f;
        slash = 0f;
        pierce = 0f;
        earth = 0f;
        light = 0f;
        dark = 0f;
        fire = 0f;
        water = 0f;
        air = 0f;
    }

    public Damage(float potential, DamageType type) : this()
    {
        this.potential = potential;
        this.SetRatio(type, 1f);
    }

    public Damage(float potential) : this(potential, DamageType.TrueDamage)
    {
    }

    public Damage(Damage orig)
    {
        potential = orig.potential;

        truedmg = orig.truedmg;
        slash = orig.slash;
        pierce = orig.pierce;
        earth = orig.earth;
        light = orig.light;
        dark = orig.dark;
        fire = orig.fire;
        water = orig.water;
        air = orig.air;
    }

    public float GetRatio(DamageType type)
    {
        switch (type)
        {
            default:
                return truedmg;
            case DamageType.Slashing:
                return slash;
            case DamageType.Piercing:
                return pierce;
            case DamageType.Earth:
                return earth;
            case DamageType.Light:
                return light;
            case DamageType.Dark:
                return dark;
            case DamageType.Fire:
                return fire;
            case DamageType.Water:
                return water;
            case DamageType.Air:
                return air;
        }
    }

    public Damage SetRatio(DamageType type, float ratio)
    {
        switch (type)
        {
            case DamageType.TrueDamage:
                truedmg = ratio;
                break;
            case DamageType.Slashing:
                slash = ratio;
                break;
            case DamageType.Piercing:
                pierce = ratio;
                break;
            case DamageType.Earth:
                earth = ratio;
                break;
            case DamageType.Light:
                light = ratio;
                break;
            case DamageType.Dark:
                dark = ratio;
                break;
            case DamageType.Fire:
                fire = ratio;
                break;
            case DamageType.Water:
                water = ratio;
                break;
            case DamageType.Air:
                air = ratio;
                break;
        }
        return this;
    }

    public Damage AddRatio(DamageType type, float ratio)
    {
        SetRatio(type, GetRatio(type) + ratio);
        return this;
    }

    public float GetPotential()
    {
        return potential;
    }

    public Damage SetPotential(float dmg)
    {
        potential = dmg;
        return this;
    }

    public Damage MultPotential(float mult)
    {
        potential *= mult;
        return this;
    }
    public float GetTotal()
    {
        float total = 0f;
        foreach (DamageType type in Enum.GetValues(typeof(DamageType)))
        {
            if (GetRatio(type) > 0f)
            {
                total += potential * GetRatio(type);
            }
            
        }
        return total;
    }

    public Damage GetTotalMinusResistances(params Damage[] resists)
    {
        Damage total = new Damage(this);
               
        foreach (Damage resist in resists)
        {
            foreach (DamageType type in Enum.GetValues(typeof(DamageType)))
            {
                total.AddRatio(type, -resist.GetRatio(type));

                if (type == DamageType.Earth)
                {
                    total.AddRatio(DamageType.Slashing, -resist.GetRatio(type));
                    total.AddRatio(DamageType.Piercing, -resist.GetRatio(type));
                }

                if (total.GetRatio(type) < 0)
                {
                    total.SetRatio(type, 0);
                }
            }
        }

        return total;
    }

    public DamageType GetHighestType(params DamageType[] excluding)
    {
        List<DamageType> excl = new List<DamageType>();
        excl.AddRange(excluding);
        float lead = 0f;
        DamageType leadType = DamageType.TrueDamage;

        foreach (DamageType type in Enum.GetValues(typeof(DamageType)))
        {
            if (GetRatio(type) > lead && !excl.Contains(type))
            {
                lead = GetRatio(type);
                leadType = type;
            }
        }
        return leadType;
    }

    public Damage Add(Damage damage)
    {
        foreach (DamageType type in Enum.GetValues(typeof(DamageType)))
        {
            AddRatio(type, damage.GetRatio(type));
        }
        return this;
    }
}
