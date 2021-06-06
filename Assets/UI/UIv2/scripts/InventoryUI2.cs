using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI2 : MonoBehaviour
{
    public static InventoryUI2 invUI;

    public GameObject viewport;
    public GameObject itemTemplate;
    public Sprite imageGeneric;
    public GameObject source;

    IInventory inventory;

    List<InventoryItem> items;

    [Header("UI Info")]
    public float itemWidth = 191;
    public float itemHeight = 191;
    public int columns = 4;
    bool initialized;

    public UnityEvent OnQuickSlotEquipStart;
    public UnityEvent OnQuickSlotEquipEnd;
    public bool awaitingQuickSlotEquipInput;

    public QuickSelectItem quickSlot0;
    public QuickSelectItem quickSlot1;
    public QuickSelectItem quickSlot2;
    public QuickSheatheIndicator sheathSlot;

    [ReadOnly] public EquippableWeapon quickSlotItem;
    void Awake()
    {
        invUI = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        items = new List<InventoryItem>();
        if (source != null)
        {
            inventory = source.GetComponent<IInventory>();
            
        }
        initialized = false;
    }

    private void Update()
    {
        if (!initialized)
        {
            if (inventory != null)
            {
                inventory.GetChangeEvent().AddListener(Populate);
                Populate();
            }
            initialized = true;
        }
    }

    public void Populate()
    {
        List<Item> contents = inventory.GetContents();

        if (contents.Count == items.Count) return;

        foreach (InventoryItem displayItem in items)
        {
            GameObject.Destroy(displayItem);
        }
        items.Clear();

        
        for (int i = 0; i < contents.Count; i++)
        {
            GameObject displayObj = GameObject.Instantiate(itemTemplate, viewport.transform);
            displayObj.SetActive(true);
            InventoryItem displayItem = displayObj.GetComponent<InventoryItem>();

            int x = i % columns;
            int y = i / columns;

            displayObj.transform.Translate(x * itemWidth, y * -itemHeight, 0);

            displayItem.SetItem(contents[i]);
            items.Add(displayItem);
        }
        ((RectTransform)viewport.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Ceil((float)contents.Count / (float)columns) * itemHeight);
        if (items.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(items[0].gameObject);
        }
    }

    public void StartQuickSlotEquip(Item item)
    {
        if (item != null && item is EquippableWeapon weapon)
        {
            quickSlotItem = weapon;
            OnQuickSlotEquipStart.Invoke();
            awaitingQuickSlotEquipInput = true;
        }
    }

    public void EndQuickSlotEquip()
    {
        OnQuickSlotEquipEnd.Invoke();
        awaitingQuickSlotEquipInput = false;
        foreach(InventoryItem item in items)
        {
            if (item.item == quickSlotItem)
            {
                EventSystem.current.SetSelectedGameObject(item.gameObject);
                break;
            }
        }
    }

    public void FlareSlot(int slot)
    {
        switch (slot)
        {
            case 0:
                quickSlot0.Flare();
                break;
            case 1:
                quickSlot1.Flare();
                break;
            case 2:
                quickSlot2.Flare();
                break;
            case 3:
                sheathSlot.Flare();
                break;
        }
    }
}
