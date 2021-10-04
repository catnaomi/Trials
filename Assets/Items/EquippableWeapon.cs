using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class EquippableWeapon : Item, IGeneratesModel
{   
    [ReadOnly] public bool isEquipped;

    [Header("Equip Information")]
    public bool EquippableMain = true;
    public bool EquippableOff = true;
    [Space(5)]
    public bool OneHanded = true;
    public bool TwoHanded = true;
    [Space(5)]
    public bool ParentLeftAsMain = false;
    public bool ParentRightAsOff = false;
    [Header("Slot Information")]
    public Inventory.EquipSlot MainHandEquipSlot;
    public Inventory.EquipSlot OffHandEquipSlot;
    [Header("Stance & Moveset Information")]
    public StanceHandler stance;
    public Moveset moveset;
    [Header("Stats")]
    public float AttackSpeed;
    public float weight;

    public UnityEvent OnEquip = new UnityEvent();
    public UnityEvent OnUnequip = new UnityEvent();

    [HideInInspector] public GameObject model;
    public virtual void EquipWeapon(Actor actor)
    {
        OnEquip.Invoke();
        // do nothing
    }

    public virtual void UnequipWeapon(Actor actor)
    {
        OnUnequip.Invoke();
        // do nothing
    }

    public virtual void UpdateWeapon(Actor actor)
    {
        // do nothing
    }

    public virtual void FixedUpdateWeapon(Actor actor)
    {
        // do nothing
    }

    public virtual float GetAttackSpeed(bool twoHand)
    {
        return AttackSpeed;
    }

    public virtual GameObject GenerateModel()
    {
        if (model != null)
        {
            DestroyModel();
        }
        model = GameObject.Instantiate(prefab);

        return model;
    }

    public virtual void DestroyModel()
    {
        if (model == null)
        {
            return;
        }

        Destroy(model);
        model = null;
    }

    public virtual GameObject GetModel()
    {
        return model;
    }

    public virtual DamageResistance[] GetBlockResistance()
    {
        return null;
    }

    public virtual float GetBlockPoiseDamage()
    {
        return 100f;
    }

    public virtual float GetWeight()
    {
        return weight;
    }

    public virtual bool IsEquippable()
    {
        return true;
    }

    public virtual Moveset GetMoveset()
    {
        return moveset;
    }

    public bool TwoHandOnly()
    {
        return TwoHanded && !OneHanded;
    }

    public override ItemType GetItemType()
    {
        return ItemType.Weapons;
    }
}
