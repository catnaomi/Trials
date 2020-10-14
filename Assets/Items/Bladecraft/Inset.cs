using UnityEngine;
using System.Collections;
using System;

[Serializable]
[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Crafting/Components/New Inset", order = 1)]
public class Inset : WeaponComponent
{
    //[Header("Inset Stats")]
    //public float weight;
    public override string ToString()
    {
        return String.Format("{0}:-Weight:{1}",itemName, weight);
    }
}
