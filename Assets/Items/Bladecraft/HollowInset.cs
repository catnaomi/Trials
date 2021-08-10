using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Crafting/Components/Hollow", order = 1)]
public class HollowInset : Inset, IBladeStatModifier
{
    public DamageType[] GetAddedElements()
    {
        return new DamageType[0];
    }

    public Dictionary<string, float> GetStatMods()
    {
        return new Dictionary<string, float>();
    }
}