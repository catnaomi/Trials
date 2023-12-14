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
    public string invID;
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


    public virtual string GetItemSaveString()
    {
        string name = $"{this.name}${this.Quantity}";
        name = name.Replace("(Clone)", "").Trim();
        return name;
    }

    public virtual void UpdateItemFromSaveString(string itemData)
    {
        try
        {
            string[] split = itemData.Split('$');
            if (!this.name.Contains(itemData[0]))
            {
                Debug.LogWarning($"item name mismatch! current:{this.name} src:{itemData[0]}");
            }
            if (int.TryParse(split[1],out int quantity))
            {
                this.Quantity = quantity;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    
    public static Item GetItemFromSaveString(string itemData)
    {
        if (itemData == null || itemData == "")
        {
            Debug.LogError("Item Data String was Blank!");
            return null;
        }
        try
        {
            string[] split = itemData.Split('$');
            string itemName = split[0];
            int quantity = int.Parse(split[1]);
            /*
            if (existingItem != null)
            {
                if (existingItem.name.Replace("(Clone)","").Trim() == itemName)
                {
                    existingItem.Quantity = quantity;
                    return existingItem;
                }
            }
            */
            Item item = Resources.Load<Item>($"Items/{itemName}");
            if (item != null)
            {
                item = Instantiate(item);
                item.Quantity = quantity;

                return item;
            }
            else
            {
                Debug.LogError($"Failed to find Item: {itemData}");
            }
        }
        catch (System.FormatException)
        {
            Debug.LogError($"Failed To Parse Item Quantity \n{itemData}");
        }
        return null;
        
    }
}
