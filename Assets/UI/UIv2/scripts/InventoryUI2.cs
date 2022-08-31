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
    public QuickSelectItem quickSlot3;
    public QuickSheatheIndicator sheathSlot;

    public GameObject selectPopup;
    GameObject lastSelected;
    Item selectedItem;
    int lastCount;
    [ReadOnly] public Equippable quickSlotItem;

    public Item.ItemType[] filterType;
    void Awake()
    {
        invUI = this;
        if (items != null && items.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(items[0].gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        items = new List<InventoryItemDisplay>();
        if (source != null)
        {
            inventory = source.GetComponent<IInventory>();
        }
        initialized = false;
        quickSlot0.InitInventory(this);
        quickSlot1.InitInventory(this);
        quickSlot2.InitInventory(this);
        quickSlot3.InitInventory(this);
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
        if (EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.activeInHierarchy)
        {
            if (lastSelected != null && lastSelected.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(lastSelected);
            }
            else
            {
                if (GetFirstItem() != null)
                {
                    EventSystem.current.SetSelectedGameObject(GetFirstItem().gameObject);
                }
                
            }
        }
        lastSelected = EventSystem.current.currentSelectedGameObject;
    }

    public void Populate()
    {
        Populate(false);
    }

    public void Populate(bool force)
    {
        List<Item> contents = inventory.GetContents();

        if (!force && inventory.GetContents().Count == items.Count && inventory.GetCount() == lastCount)
        {
            return;
        }

        foreach (InventoryItemDisplay displayItem in items)
        {
            GameObject.Destroy(displayItem.gameObject);
        }
        items.Clear();

        int count = 0;
        List<Item.ItemType> filterList =  new List<Item.ItemType>();
        filterList.AddRange(filterType);
        //sfilterList.AddRange(filterType.Split(','));

        for (int i = 0; i < contents.Count; i++)
        {
            
            if (filterList.Count > 0 && !filterList.Contains(contents[i].GetItemType())) continue;
            GameObject displayObj = GameObject.Instantiate(itemTemplate, viewport.transform);
            displayObj.SetActive(true);
            InventoryItemDisplay displayItem = displayObj.GetComponent<InventoryItemDisplay>();

            int x = i % columns;
            int y = i / columns;

            //displayObj.transform.Translate(x * itemWidth, y * -itemHeight, 0);

         
            displayItem.SetItem(contents[i]);
            
            
            if (usingQuickslots)
            {
                //displayItem.GetComponent<Button>().onClick.AddListener(displayItem.StartEquip);
                displayItem.button.onClick.AddListener(() => { ItemSelect(displayItem); });
            }
            
            
            items.Add(displayItem);
            count++;
        }
        items.Sort((a, b) => 
        {
            if (a.item.GetItemType() != b.item.GetItemType())
            {
                return filterList.IndexOf(b.item.GetItemType()) - filterList.IndexOf(a.item.GetItemType());
            }
            else
            {
                return a.item.itemName.CompareTo(b.item.itemName);
            }
            
        });
        /*
        for (int j = 0; j < items.Count; j++)
        {
            Selectable b = items[j].button;

            Navigation nav = new Navigation();
            nav.mode =  Navigation.Mode.Explicit;

            int c = columns;

            if (j >= c) // up
            {
                if (items[j-c] != null)
                {
                    nav.selectOnUp = items[j - c].button;
                }
            }
            if (j <= items.Count - c) // down
            {
                if (j + c < items.Count && items[j + c] != null)
                {
                    nav.selectOnDown = items[j + c].button;
                }
            }
            if (j % c != 0) // left
            {
                if (items[j - 1] != null)
                {
                    nav.selectOnLeft = items[j - 1].button;
                }
            }
            if (j % c != c-1) // right
            {
                if (j + 1 < items.Count && items[j+1] != null)
                {
                    nav.selectOnRight = items[j + 1].button;
                }
            }
            b.navigation = nav;

        }
        */
        lastCount = inventory.GetCount();
        ((RectTransform)viewport.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Ceil((float)count / (float)columns) * itemHeight);
        if (items.Count > 0 && usingQuickslots)
        {
            EventSystem.current.SetSelectedGameObject(items[0].gameObject);
        }
    }

    public InventoryItemDisplay GetFirstItem()
    {
        if (items.Count > 0)
        {
            return items[0];
        }
        return null;
    }
    public void StartQuickSlotEquip(Item item)
    {
        if (item != null && item is Equippable weapon)
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

    public void ItemSelect(InventoryItemDisplay inventoryItemDisplay)
    {
        selectedItem = inventoryItemDisplay.item;
        selectPopup.SetActive(true);
        EventSystem.current.SetSelectedGameObject(selectPopup.GetComponentInChildren<Button>().gameObject);
    }

    public void SelectEquip()
    {
        selectPopup.SetActive(false);
        StartQuickSlotEquip(selectedItem);
    }

    public void SelectDrop()
    {
        selectPopup.SetActive(false);
        inventory.Remove(selectedItem);
        LooseItem li = LooseItem.CreateLooseItem(selectedItem);
        li.gameObject.transform.position = source.transform.position + Vector3.up + source.transform.forward * 2f;
        EventSystem.current.SetSelectedGameObject(items[0].gameObject);
    }

    public void SelectCancel()
    {
        selectPopup.SetActive(false);
        EventSystem.current.SetSelectedGameObject(FindItemDisplay(selectedItem).gameObject);
        Debug.Log("inv cancel!!!!");
    }
    public InventoryItemDisplay FindItemDisplay(Item targetItem)
    {
        foreach (InventoryItemDisplay item in items)
        {
            if (item.item == targetItem)
            {
                return item;
            }
        }
        return null;
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
                quickSlot3.Flare();
                break;
        }
    }
}
