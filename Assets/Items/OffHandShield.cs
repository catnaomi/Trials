using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Shield", menuName = "ScriptableObjects/CreateOffHandWeaponShield", order = 1)]
public class OffHandShield : EquippableWeapon
{
    [Header("Resistances")]
    public DamageResistance blockResistance;
    public bool hasTypedResistances;
    public DamageResistance slashResistance;
    public DamageResistance thrustResistance;

    public override DamageResistance GetBlockResistance()
    {
        if (hasTypedResistances && holder.IsBlockingSlash())
        {
            return slashResistance;
        }
        else if (hasTypedResistances && holder.IsBlockingThrust())
        {
            return thrustResistance;
        }
        else
        {
            return blockResistance;
        }
    }
}
