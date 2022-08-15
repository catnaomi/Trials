using UnityEngine;
using System.Collections;

[System.Flags]
public enum DamageType
{
    TrueDamage = (1 << 0),
    Slashing = (1 << 1),
    Piercing = (1 << 2),
    Blunt = (1 << 3),
    Earth = (1 << 4), // also contributes to slash/pierce/blunt damage reduction
    Light = (1 << 5),
    Dark = (1 << 6),
    Fire = (1 << 7),
    Water = (1 << 8),
    Air = (1 << 9),

    Standard_SlashPierce = (1 << 10), // converts to slashing or piercing on evaluation
    All = (1 << 11), // used for global dr
}
