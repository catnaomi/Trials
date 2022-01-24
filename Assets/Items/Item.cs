using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Items/Create Item", order = 1)]
public class Item : ScriptableObject
{
    public string itemName;
    public string itemDesc;
    [HideInInspector] public Actor holder;
    public int MaxStackSize = 0;
    public int Quantity = 1;
    public int invID;
    public GameObject prefab;
    public Sprite displayImage;
    public Color displayColor = Color.white;

    public enum ItemType
    {
        Misc,
        Weapons,
        Consumables,
        Blades,
        Hilts,
        Insets,
        Accessories,
    }
    public virtual string GetName()
    {
        return itemName;
    }

    public virtual string GetDescription()
    {
        return itemDesc;
    }

    protected Actor GetHeldActor()
    {
        return (Actor)holder;
    }

    protected IInventory GetInventory()
    {
        // TODO: make this cleaner

        if (holder is PlayerActor player)
        {
            return player.GetComponent<PlayerInventory>();
        }
        else if (holder.TryGetComponent<IInventory>(out IInventory inventory))
        {
            return inventory;
        }
        return null;
    }

    protected HumanoidPositionReference GetPositionReference()
    {
        if (holder.TryGetComponent<HumanoidPositionReference>(out HumanoidPositionReference positionReference))
        {
            return positionReference;
        }
        return null;
    }
    public virtual ItemType GetItemType()
    {
        return ItemType.Misc;
    }

    public bool ItemEqual(Item item)
    {
        return item.itemName == this.itemName && item.invID == this.invID;
    }
}
