using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

[Serializable]
public class EffectResistPhysical : Effect
{
    int mag;
    public EffectResistPhysical(int mag)
    {
        displayName = "Resist Physical";
        this.mag = mag;
        desc = String.Format("Increases Slashing and Piercing resistance by {0}", this.mag);
        applied = false;
    }
    protected override bool ApplyEffect(ActorAttributes attributes)
    {
        return false;
        try
        {
            //attributes.resistances.resist[(int)DamageType.Slashing] += mag;
            //attributes.resistances.resist[(int)DamageType.Piercing] += mag;
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    protected override bool RemoveEffect(ActorAttributes attributes)
    {
        return false;
        try
        {
            //attributes.resistances.resist[(int)DamageType.Slashing] -= mag;
            //attributes.resistances.resist[(int)DamageType.Piercing] -= mag;

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

}
