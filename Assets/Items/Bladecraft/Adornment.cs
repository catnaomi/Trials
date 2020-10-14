using UnityEngine;
using System.Collections;
using System;

[Serializable]
[CreateAssetMenu(fileName = "Adornment", menuName = "ScriptableObjects/Crafting/Components/New Adornment", order = 1)]
public class Adornment : WeaponComponent
{
    //[Header("Adornment Stats")]
    //public GameObject prefab; // inherit from Item
    //public float weight;

    public override string ToString()
    {
        return String.Format("{0}:-Weight:{1}", itemName, weight);
    }
}