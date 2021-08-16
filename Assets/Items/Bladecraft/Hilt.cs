using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
[CreateAssetMenu(fileName = "Hilt", menuName = "ScriptableObjects/Crafting/Components/New Hilt", order = 1)]
public class Hilt : InsetBearer
{
    [Header("Hilt Stats")]
    //public GameObject prefab; // inherit from Item
    // Material?

    public string hiltDescriptor;
    [Space(5)]
    public bool MainHanded;
    public bool OffHanded;
    public bool OneHanded;
    public bool TwoHanded;

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
        return String.Format("{0}:\n-Weight:{1}\n-Length:{2}\n-Insets (slots: {3}):{4}", itemName, weight, length, slots, insetTxt);
    }
}
