using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponComponent : Item
{
    [Header("Component Stats")]
    public int durability = -999; // -999 = no durability/unbreakable
    public float weight;
    [ReadOnly]
    public bool attached;
    [Space(5)]
    public StanceHandler PrfStance;
    protected GameObject model;
    public virtual int ReduceDurability(int amt)
    {
        if (durability != -999)
        {
            durability -= amt;
        }
        return durability;
    }

    public virtual float GetWeight()
    {
        return weight;
    }
        

    public virtual GameObject GenerateModel()
    {
        if (model != null)
        {
            GameObject.Destroy(model);
        }
        if (prefab != null)
        {
            model = GameObject.Instantiate(prefab);
        }
        return model;
    }

    public virtual List<WeaponComponent> GetComponents()
    {
        return new List<WeaponComponent>() { this };
    }
}
