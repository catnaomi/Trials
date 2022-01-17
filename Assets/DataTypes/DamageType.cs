using UnityEngine;
using System.Collections;

public enum DamageType
{
    TrueDamage,
    Slashing,
    Piercing,
    Blunt,
    Earth, // also contributes to slash/pierce/blunt damage reduction
    Light,
    Dark,
    Fire,
    Water,
    Air,

    Standard_SlashPierce, // converts to slashing or piercing on evaluation
    All, // used for global dr
}
