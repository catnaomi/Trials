using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour, IInventory
{
    [SerializeField, ReadOnly] protected List<Item> contents;
    public List<Item> StartingContents;

    public UnityEvent OnChange;
    public float lastChanged;
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

    protected virtual void Awake()
    {
        foreach (Item item in StartingContents)
        {
            Add(ScriptableObject.Instantiate(item));
        }
        StartingContents.Clear();
        OnChange.AddListener(() => { lastChanged = Time.time; });
    }

    public List<Item> GetContents()
    {
        return contents;
    }

    public void SetContents(List<Item> items)
    {
        contents = items;
    }

    public bool Contains(Item item)
    {
        return contents.Contains(item);
    }

    public virtual bool Add(Item item)
    {
        if (item == null) return false;
        if (item.MaxStackSize > 1)
        {
            foreach (Item presentItem in contents)
            {
                if (presentItem.ItemEqual(item))
                {
                    if (presentItem.Quantity < presentItem.MaxStackSize)
                    {
                        presentItem.Quantity += item.Quantity;
                        OnChange.Invoke();
                        return true;
                    }
                }
            }
        }
        item.holder = this.GetComponent<Actor>();
        contents.Add(item);
        OnChange.Invoke();
        return true;
    }

    public virtual bool AddMany(Item item, int amount)
    {
        if (item == null) return false;
        if (item.MaxStackSize > 1)
        {
            foreach (Item presentItem in contents)
            {
                if (presentItem.ItemEqual(item))
                {
                    if (presentItem.Quantity < presentItem.MaxStackSize)
                    {
                        presentItem.Quantity += item.Quantity * amount;
                        OnChange.Invoke();
                        return true;
                    }
                }
            }

            // if could not find an item
            item.Quantity *= amount;
            contents.Add(item);
        }
        else
        {
            item.holder = this.GetComponent<Actor>();
            for (int i = 0; i < amount; i++)
            {
                contents.Add(item);
            }
        }
        OnChange.Invoke();
        return true;
    }

    public bool Remove(Item item)
    {
        bool r = contents.Remove(item);
        if (r) OnChange.Invoke();
        return r;
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
            OnChange.Invoke();
            return true;
        }
        else
        {
            return Remove(presentItem);
        }
    }

    public int RemoveNumber(Item item, int amount)
    {
        if (amount <= 0) return 0;
        Item presentItem = null;
        foreach (Item pitem in contents)
        {
            if (pitem.ItemEqual(item))
            {
                presentItem = pitem;
                break;
            }
        }
        if (presentItem == null)
        {
            return 0;
        }
        else if (presentItem.Quantity > amount)
        {
            presentItem.Quantity -= amount;
            OnChange.Invoke();
            return amount;
        }
        else if (presentItem.Quantity > 0)
        {
            int q = presentItem.Quantity;
            Remove(presentItem);
            return q;
        }
        else
        {
            Remove(presentItem);
            return 0;
        }
    }
    public int GetAmountOf(Item item)
    {
        int count = 0;
        foreach(Item presentItem in contents)
        {
            if (item.ItemEqual(presentItem))
            {
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
    public virtual void Clear()
    {
        contents.Clear();
        OnChange.Invoke();
    }

    public UnityEvent GetChangeEvent()
    {
        return OnChange;
    }

    public int GetCount()
    {
        int amt = 0;
        foreach (Item item in contents)
        {
            if (item.MaxStackSize > 1)
            {
                amt += item.Quantity;
            }
            else
            {
                amt++;
            }
        }
        return amt;
    }
}