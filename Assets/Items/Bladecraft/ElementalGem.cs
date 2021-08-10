using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Crafting/Components/New Elemental Gem", order = 1)]
public class ElementalGem : Inset, IBladeStatModifier
{
    public DamageType element;
    public Material gemMaterial;
    public float baseAttackModifier = -5f;
    public DamageType[] GetAddedElements()  
    {
        return new DamageType[] { element };
    }

    public Dictionary<string, float> GetStatMods()
    {
        Dictionary<string, float> dict = new Dictionary<string, float>();
        dict["BaseDamage"] = baseAttackModifier;
        return dict;
    }
}