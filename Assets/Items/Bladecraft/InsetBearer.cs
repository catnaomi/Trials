using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsetBearer : WeaponComponent
{
    public float length;
    public float width;
    [Header("Insets")]
    public List<Inset> insets;
    public int slots;

    
    private void OnEnable()
    {
        //insets.Clear();
        /*
        for (int i = 0; i < slots; i++)
        {
            //Inset empty = null;//Inset.CreateEmptyInset();
            if (insets.Count <= i)
            {
                insets.Add(null);
            }
            /*
            else if (insets[i] == null)
            {
                insets[i] = empty;
            }
            
        }
        while (insets.Count > slots)
        {
            insets.RemoveAt(insets.Count - 1);
        }
        */
    }
    

    public bool AttachInset(Inset inset, int slot)
    {
        return AttachInset(inset, slot, out Inset unused);
    }
    public bool AttachInset(Inset inset, int slot, out Inset prevInset)
    {
        bool previousInSlot = UnattachInset(slot, out prevInset);
        if (insets.Count > slot && insets[slot] != null)
        {
            insets[slot].attached = false;
            /*
            if (insets[slot].itemName == "_EmptyInset")
            {
                ScriptableObject.Destroy(inset);
            }
            */
        }
        insets[slot] = inset;
        inset.attached = true;
        return previousInSlot;
    }

    public bool UnattachInset(int slot)
    {
        return UnattachInset(slot, out Inset unused);
    }

    public bool UnattachInset(int slot, out Inset prevInset)
    {
        bool previousInSlot = false;
        prevInset = null;
        if (insets.Count > slot && insets[slot] != null)
        {
            prevInset = insets[slot];
            prevInset.attached = false;

            previousInSlot = true;
            /*
            if (prevInset.itemName == "_EmptyInset")
            {
                ScriptableObject.Destroy(prevInset);
                previousInSlot = false;
            }*/
        }
        insets[slot] = null; // Inset.CreateEmptyInset();
        return previousInSlot;
    }

    public int UnattachAll(out List<Inset> removedInsets)
    {
        removedInsets = new List<Inset>();
        for (int i = 0; i < slots; i++)
        {
            if (UnattachInset(i, out Inset remove))
            {
                removedInsets.Add(remove);
            }
        }
        return removedInsets.Count;
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

    public float GetLength()
    {
        return length;
    }

    public float GetWidth()
    {
        return width;
    }
    public override List<WeaponComponent> GetComponents()
    {
        List<WeaponComponent> l = base.GetComponents();
        foreach (Inset inset in insets)
        {
            if (inset != null)
            {
                l.AddRange(inset.GetComponents());
            }
        }
        return l;
    }

    public override int ReduceDurability(int amt)
    {
        int d = base.ReduceDurability(amt);
        foreach (Inset inset in insets)
        {
            d += inset.ReduceDurability(amt);
        }

        return d;
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