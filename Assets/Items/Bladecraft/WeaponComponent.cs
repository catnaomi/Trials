using UnityEngine;
using System.Collections;

public class WeaponComponent : Item
{
    [Header("Component Stats")]
    public int durability;
    public float weight;
    [ReadOnly]
    public bool attached;
    public Damage ratios;
    [Space(5)]
    public StanceHandler PrfMainHandStance;
    public StanceHandler PrfOffHandStance;
    public StanceHandler PrfTwoHandStance;
    protected GameObject model;
    public int ReduceDurability(int amt)
    {
        durability -= amt;
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
}
