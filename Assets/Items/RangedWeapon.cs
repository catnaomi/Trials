using System.Collections;
using UnityEngine;

public class RangedWeapon : EquippableWeapon
{
    protected float strength;
    [Header("Ranged Weapon Settings")]
    [SerializeField]protected FireMode fireMode;
    public static FireMode forcedFireMode = FireMode.None;
    public virtual bool CanFire()
    {
        return false;
    }

    public virtual void SetCanFire(bool fire)
    {
        
    }
    public void SetStrength(float s)
    {
        strength = s;
    }

    public FireMode GetFireMode()
    {
        if (forcedFireMode != FireMode.None)
        {
            return forcedFireMode;
        }
        else
        {
            return fireMode;
        }
    }

    public enum FireMode
    {
        None,
        TriggerRelease,
        TriggerAndAttackPress,
        TriggerAndAttackRelease,
    }
}