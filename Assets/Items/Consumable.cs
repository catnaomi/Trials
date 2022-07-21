using Animancer;
using System.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "Consumable", menuName = "ScriptableObjects/Items/Consumables/Create Consumable", order = 1)]
public class Consumable : Equippable, IGeneratesModel
{
    
    [ReadOnly]public GameObject model;
    public bool removeFromInventoryOnUse;
    public bool generateModelOnUse;
    public bool sheatheMainOnUse;
    public bool sheatheOffOnUse;
    public bool parentMain;
    public bool parentOff;
    public Inventory.EquipSlot parentSlot;
    public InputAction consumeAction;
    public virtual InputAction GetAction()
    {
        return consumeAction;
    }

    public virtual void UseConsumable()
    {
        if (!CanBeUsed())
        {
            return;
        }
        // do nothing
        if (removeFromInventoryOnUse)
        {
            holder.GetInventory().RemoveOne(this);
        }
    }

    public virtual GameObject GenerateModel()
    {
        model = GameObject.Instantiate(prefab);
        return model;
    }

    public virtual GameObject GetModel()
    {
        return model;
    }

    public virtual bool CanBeUsed()
    {
        if (removeFromInventoryOnUse)
        {
            return GetUsesRemaining() > 0;
        }
        else
        {
            return true;
        }
    }

    public virtual void DestroyModel()
    {
        if (model == null) return;
        Destroy(model);
    }

    public virtual int GetUsesRemaining()
    {
        if (removeFromInventoryOnUse)
        {
            return GetInventory().GetAmountOf(this);
        }
        else
        {
            return -1;
        }
    }
}