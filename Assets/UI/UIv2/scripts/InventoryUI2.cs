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

    [HideInInspector]
    public IInventory inventory;

    List<InventoryItemDisplay> items;

    [Header("UI Info")]
    public float itemWidth = 191;
    public float itemHeight = 191;
    public int columns = 4;
    bool initialized;

    [Header("Quickslot Info")]
    public bool usingQuickslots = true;
    [Space(5)]
    public UnityEvent OnQuickSlotEquipStart;
    public UnityEvent OnQuickSlotEquipEnd;
    public bool awaitingQuickSlotEquipInput;

    public QuickSelectItem quickSlot0;
    public QuickSelectItem quickSlot1;
    public QuickSelectItem quickSlot2;
    public QuickSheatheIndicator sheathSlot;

    [ReadOnly] public EquippableWeapon quickSlotItem;

    public string filterType = "";
    void Awake()
    {
        invUI = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        items = new List<InventoryItemDisplay>();
        if (source != null)
        {
            //inventory = source.GetComponent<IInventory>();
            
        }
        initialized = false;
    }

    private void Update()
    {
        if (!initialized)
        {
            if (source != null && inventory == null)
            {
                inventory = source.GetComponent<IInventory>();
            }
            if (inventory != null)
            {
                inventory.GetChangeEvent().AddListener(Populate);
                Populate();
                initialized = true;
            }
           
        }
    }

    public void Populate()
    {
        Populate(false);
    }

    public void Populate(bool force)
    {
        List<Item> contents = inventory.GetContents();

        if (!force && contents.Count == items.Count) return;

        foreach (InventoryItemDisplay displayItem in items)
        {
            GameObject.Destroy(displayItem.gameObject);
        }
        items.Clear();

        int count = 0;
        List<string> filterList = new List<string>();
        filterList.AddRange(filterType.Split(','));
        for (int i = 0; i < contents.Count; i++)
        {
            
            if (filterType != "" && !filterList.Contains(contents[i].GetItemType())) continue;
            GameObject displayObj = GameObject.Instantiate(itemTemplate, viewport.transform);
            displayObj.SetActive(true);
            InventoryItemDisplay displayItem = displayObj.GetComponent<InventoryItemDisplay>();

            int x = i % columns;
            int y = i / columns;

            //displayObj.transform.Translate(x * itemWidth, y * -itemHeight, 0);

         
            displayItem.SetItem(contents[i]);
            if (usingQuickslots)
            {
                displayItem.GetComponent<Button>().onClick.AddListener(displayItem.StartEquip);
            }
            items.Add(displayItem);
            count++;
        }
        for (int j = 1; j < items.Count; j++)
        {
            Button b = items[j].GetComponent<Button>();

            Navigation nav = new Navigation();
            nav.mode =  Navigation.Mode.Automatic;

            if (j >= 3) // up
            {
                if (items[j-3] != null)
                {
                    nav.selectOnUp = items[j - 3];
                }
            }
            if (j <= items.Count - 3) // down
            {
                if (j + 3 < items.Count && items[j + 3] != null)
                {
                    nav.selectOnDown = items[j + 3];
                }
            }
            if (j % 3 != 0) // left
            {
                if (items[j - 1] != null)
                {
                    nav.selectOnLeft = items[j - 1];
                }
            }
            if (j % 3 != 2) // right
            {
                if (j + 1 < items.Count && items[j+1] != null)
                {
                    nav.selectOnRight = items[j + 1];
                }
            }
            b.navigation = nav;

        }
        ((RectTransform)viewport.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Ceil((float)count / (float)columns) * itemHeight);
        if (items.Count > 0 && usingQuickslots)
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
        foreach(InventoryItemDisplay item in items)
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
