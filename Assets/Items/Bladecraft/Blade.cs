using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
[CreateAssetMenu(fileName = "Blade", menuName = "ScriptableObjects/Crafting/Components/New Blade", order = 1)]
public class Blade : InsetBearer
{
    [Header("Blade Stats")]
    //public GameObject prefab; // inherit from Item

    public float baseDamage;
    public float slashingModifier;
    public float piercingModifier;

    public string bladeDescriptor;

    

    public override string ToString()
    {
        string insetTxt = "";
        foreach (Inset inset in insets)
        {
            if (inset != null)
            {
                insetTxt += "\n--" + inset.ToString();
            }
        }
        return String.Format("{0}:\n-Weight:{1}\n-Length:{2}\n-Width:{5}\n-Insets (slots: {3}):{4}", itemName, weight, length, slots, insetTxt, width);
    }

    public float GetBaseDamage()
    {
        return baseDamage;
    }

    public float GetSlashingModifier()
    {
        return slashingModifier;
    }

    public float GetPiercingModifier()
    {
        return piercingModifier;
    }

    public override ItemType GetItemType()
    {
        return ItemType.Blades;
    }
}
