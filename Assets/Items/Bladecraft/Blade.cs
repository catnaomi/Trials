using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
[CreateAssetMenu(fileName = "Blade", menuName = "ScriptableObjects/Crafting/Components/New Blade", order = 1)]
public class Blade : WeaponComponent
{
    [Header("Blade Stats")]
    //public GameObject prefab; // inherit from Item
    // Material?
    public float length;
    //public float weight;
    public float width;
    public float slashingModifier;
    public float piercingModifier;
    public List<Inset> insets;
    public int slots;

    public string bladeDescriptor;

    private void OnEnable()
    {
        while (insets.Count < slots)
        {
            insets.Add(null);
        }
    }

    public void AttachInset(Inset inset, int slot)
    {
        if (insets[slot] != null)
        {
            insets[slot].attached = false;
        }
        insets[slot] = inset;
        inset.attached = true;
    }

    public void UnattachInset(int slot)
    {
        if (insets[slot] != null)
        {
            insets[slot].attached = false;
        }
        insets[slot] = null;
    }

    public override float GetWeight()
    {
        float addit = 0f;
        foreach (Inset inset in insets)
        {
            if (inset != null)
            {
                addit += inset.GetWeight();
            }
        }
        return this.weight + addit;
    }

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

    public override GameObject GenerateModel()
    {
        if (model != null)
        {
            GameObject.Destroy(model);
        }
        if (prefab != null)
        {
            model = GameObject.Instantiate(prefab);
            for (int i = 0; i < slots; i++)
            {
                Transform insetSlot = model.transform.Find("inset_" + (i + 1));
                if (insets[i] != null)
                {
                    GameObject newInset = insets[i].GenerateModel();
                    if (newInset != null)
                    {
                        newInset.transform.SetParent(insetSlot, false);
                    }
                    Transform insetDefault = insetSlot.transform.Find("inset_" + (i + 1) + "_default");
                    if (insetDefault != null)
                    {
                        insetDefault.gameObject.SetActive(false);
                    }
                }
            }
        }
        return model;
    }
}
