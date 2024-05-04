using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using CustomUtilities;

public class EquippableWeapon : Equippable, IGeneratesModel
{   
    [ReadOnly] public bool isEquipped;

    [Header("Equip Information")]
    public bool EquippableMain = true;
    public bool EquippableOff = true;
    public bool EquippableRanged = false;
    [Space(5)]
    public bool OneHanded = true; 
    public bool TwoHanded = true;
    [Space(5)]
    public bool ParentLeft = false;
    [Header("Slot Information")]
    public Inventory.EquipSlot MainHandEquipSlot;
    public Inventory.EquipSlot OffHandEquipSlot;
    public Inventory.EquipSlot RangedEquipSlot;
    [Header("Stance & Moveset Information")]
    public StanceHandler primaryStance;
    public StanceHandler secondaryStance;
    public Moveset moveset;
    [Header("Stats")]
    public Size size;
    public float AttackSpeed;
    public float weight;
    [Header("Ammunition")]
    public bool usesAmmunition;
    public Item ammunitionReference;

    public UnityEvent OnEquip = new UnityEvent();
    public UnityEvent OnUnequip = new UnityEvent();

    [HideInInspector] public GameObject model;

    public enum Size
    {
        Light,
        Medium,
        Heavy
    }
    public virtual void EquipWeapon(Actor actor)
    {
        holder = actor;
        OnEquip.Invoke();
        // do nothing
    }

    public virtual void UnequipWeapon(Actor actor)
    {
        holder = null;
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
        SetModelLayer();
        return model;
    }

    public void SetModelLayer()
    {
        if (model != null && holder != null)
        {
            int layer = holder.gameObject.layer;
            Renderer firstRenderer = holder.GetComponentInChildren<Renderer>();
            if (firstRenderer != null)
            {
                layer = firstRenderer.gameObject.layer;
            }
            foreach (Renderer r in model.GetComponentsInChildren<Renderer>())
            {
                if (r.enabled)
                {
                    r.gameObject.layer = layer;
                }
            }
        }
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

    public virtual DamageResistance GetBlockResistance()
    {
        return new DamageResistance();
    }

    public virtual float GetBlockPoiseDamage()
    {
        return 100f;
    }

    public virtual float GetWeight()
    {
        return weight;
    }

    public virtual Bounds GetBlockBounds()
    {
        if (model != null)
        {
            Transform blockTransform = InterfaceUtilities.FindRecursively(model.transform, "_blockCollider");
            if (blockTransform != null && blockTransform.TryGetComponent<Collider>(out Collider collider))
            {
                return collider.bounds;
            }
        }
        return new Bounds();
    }

    public virtual bool IsEquippable()
    {
        return true;
    }

    public virtual Moveset GetMoveset()
    {
        return moveset;
    }

    public virtual void FlashWarning()
    {
        if (model != null)
        {
            GameObject fx = FXController.instance.CreateBladeWarning();
            fx.transform.position = model.transform.position;
            Destroy(fx, 1f);
        }
    }

    public int GetAmmunitionRemaining()
    {
        if (!usesAmmunition)
        {
            return -1;
        }
        else if (holder == null)
        {
            return -1;
        }
        else
        {
            return holder.GetComponent<Inventory>().GetAmountOf(ammunitionReference);
        }
    }
    public bool TwoHandOnly()
    {
        return TwoHanded && !OneHanded;
    }

    public override ItemType GetItemType()
    {
        return ItemType.Weapons;
    }

    public UnityEvent GetEquipEvent()
    {
        return OnEquip;
    }

    public UnityEvent GetUnequipEvent()
    {
        return OnUnequip;
    }
}
