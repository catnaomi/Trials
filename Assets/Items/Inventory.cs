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
        contents.Add(item);
        return true;
    }

    public bool Remove(Item item)
    {
        return contents.Remove(item);
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