using UnityEngine;
using System.Collections;

public enum DamageType
{
    TrueDamage,
    Slashing,
    Piercing,
    Blunt,
    Earth,
    Light,
    Dark,
    Fire,
    Water,
    Air,

    Standard_SlashPierce // converts to slashing or piercing on evaluation
}
