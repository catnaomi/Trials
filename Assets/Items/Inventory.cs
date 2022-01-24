using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour, IInventory
{
    [SerializeField, ReadOnly] protected List<Item> contents;
    public List<Item> StartingContents;

    public UnityEvent OnChange;

    public enum EquipSlot
    {
        none,  // 0
        rHip,  // 1
        lHip,  // 2
        rBack, // 3
        lBack, // 4
        cBack,  // 5
    }

    public const int MainType = 0;
    public const int OffType = 1;
    public const int RangedType = -1;

    void Awake()
    {
        foreach(Item item in StartingContents)
        {
            contents.Add(ScriptableObject.Instantiate(item));
        }
        StartingContents.Clear();
    }
    public List<Item> GetContents()
    {
        return contents;
    }

    public bool Contains(Item item)
    {
        return contents.Contains(item);
    }

    public bool Add(Item item)
    {
        if (item.MaxStackSize > 1)
        {
            foreach (Item presentItem in contents)
            {
                if (presentItem.ItemEqual(item))
                {
                    if (presentItem.Quantity < presentItem.MaxStackSize)
                    {
                        presentItem.Quantity++;
                        return true;
                    }
                }
            }
        }
        contents.Add(item);
        return true;
    }

    public bool Remove(Item item)
    {
        return contents.Remove(item);
    }

    public bool RemoveOne(Item item)
    {
        if (item.MaxStackSize <= 1)
        {
            return Remove(item);
        }
        Item presentItem = null;
        foreach(Item pitem in contents)
        {
            if (pitem.ItemEqual(item))
            {
                presentItem = pitem;
                break;
            }
        }
        if (presentItem == null)
        {
            return false;
        }
        else if (presentItem.Quantity > 1)
        {
            presentItem.Quantity--;
            return true;
        }
        else
        {
            return Remove(presentItem);
        }
    }

    public int GetAmountOf(Item item)
    {
        int count = 0;
        bool foundAny = false;
        foreach(Item presentItem in contents)
        {
            if (item.ItemEqual(presentItem))
            {
                foundAny = true;
                if (item.MaxStackSize <= 0)
                {
                    count++;
                }
                else
                {
                    count += presentItem.Quantity;
                }
            }
        }
        return count;
    }
    public void Clear()
    {
        contents.Clear();
    }

    public UnityEvent GetChangeEvent()
    {
        return OnChange;
    }
}