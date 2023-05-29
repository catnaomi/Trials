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
        if (holder is PlayerActor player)
        {
            if (hasTypedResistances && player.IsBlockingSlash())
            {
                return slashResistance;
            }
            else if (hasTypedResistances && player.IsBlockingThrust())
            {
                return thrustResistance;
            }
            else
            {
                return blockResistance;
            }
        }
        else
        {
            return blockResistance;
        }
        
    }
}
