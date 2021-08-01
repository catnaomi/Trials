using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class Damage
{
    public float potential;

    public List<DamageType> types;


    public Damage()
    {
        potential = 0f;
        types = new List<DamageType>();
    }

    public Damage(float potential, params DamageType[] types) : this(potential, new List<DamageType>(types))
    {
        
    }

    public Damage(float potential, List<DamageType> types)
    {
        this.potential = potential;
        this.types = types;
    }

    public Damage(Damage orig)
    {
        potential = orig.potential;

        this.types = orig.types;
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
        return potential;
    }

    public float GetTotalMinusResistances(List<DamageType> resists, List<DamageType> weaknesses, float resistanceRatio, float weaknessRatio)
    {
        float total = this.GetPotential();
               
        foreach (DamageType type in types)
        {
            if (resists.Contains(type))
            {
                if (resistanceRatio == 0)
                {
                    return 0f;
                }
                total /= resistanceRatio;
            }
            if (weaknesses.Contains(type))
            {
                total *= weaknessRatio;
            }
        }

        return total;
    }

    public DamageType GetHighestType(params DamageType[] excluding)
    {
        if (types.Count > 0)
        {
            return types[0];
        }
        return DamageType.TrueDamage;
    }
}
