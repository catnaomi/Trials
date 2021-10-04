using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class CraftMenuController : MonoBehaviour
{
    // TODO: blade comparisons, actually setting slots 

    public GameObject source;
    public CraftState state;
    [Header("UI References")]
    public Toggle previewToggle;
    public Toggle hiltToggle;
    public Toggle bladeToggle;
    public Toggle adornmentToggle;
    [Space(5)]
    public InventoryUI2 inventoryMenu;
    public GameObject itemPreview;
    public GameObject hideable;
    public InventoryItemDisplay removeButton;
    public InventoryItemDisplay hollowHiltButton;
    public InventoryItemDisplay hollowBladeButton;
    public InventoryItemDisplay newWeaponButton;
    [Space(5)]
    public WeaponStatBlock statBlock;
    [Space(5)]
    public GameObject slotSelectContainer;
    public InventoryItemDisplay slotSelect_main;
    public InventoryItemDisplay[] slotSelect_insets;
    
    public BladeWeapon currentWeapon;
    public WeaponComponent currentComponent;
    InventoryItemDisplay hovered;

    public UnityEvent onExit;
    bool exit;
    bool init;
    bool hide;
    public enum CraftState
    {
        Assembly_Select,
        Assembly_Hilt,
        Assembly_Hilt_SelectSlot,
        Assembly_Hilt_Remove,
        Assembly_Blade,
        Assembly_Blade_SelectSlot,
        Assembly_Blade_Remove,
        Assembly_Adornment,
        Assembly_Adornment_SelectSlot,
        Assembly_Adornment_Remove,
    }
    // Start is called before the first frame update
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(previewToggle.gameObject);
        inventoryMenu.source = this.source;
        init = false;
    }

    private void Update()
    {
        if (Gamepad.current.rightStickButton.wasReleasedThisFrame) ToggleHide();
    }
    public void SetCategory()
    {
        if (previewToggle.isOn)
        {
            state = CraftState.Assembly_Select;
            currentComponent = null;
            inventoryMenu.filterType = new Item.ItemType[] { Item.ItemType.Weapons };// "BladeWeapon,CraftableWeapon";
            inventoryMenu.Populate(true);
        }
        else if (hiltToggle.isOn)
        {
            if (state != CraftState.Assembly_Hilt)
            {
                state = CraftState.Assembly_Hilt;
                currentComponent = null;
                inventoryMenu.filterType = new Item.ItemType[] { Item.ItemType.Hilts, Item.ItemType.Insets }; //"Hilt,Inset,ElementalGem";
                inventoryMenu.Populate(true);
            }
        }
        else if (bladeToggle.isOn)
        {
            if (state != CraftState.Assembly_Blade)
            {
                state = CraftState.Assembly_Blade;
                currentComponent = null;
                inventoryMenu.filterType = new Item.ItemType[] { Item.ItemType.Blades, Item.ItemType.Insets }; //"Blade,Inset,ElementalGem";
                inventoryMenu.Populate(true);
            }
        }
        else if (adornmentToggle.isOn)
        {
            if (state != CraftState.Assembly_Adornment)
            {
                state = CraftState.Assembly_Adornment;
                currentComponent = null;
                inventoryMenu.filterType = new Item.ItemType[] { Item.ItemType.Accessories }; //"Adornment";
                inventoryMenu.Populate(true);
            }
        }
        UpdateStats();
        UpdateSlots();
    }

    public void GenerateModel()
    {
        foreach(Transform child in itemPreview.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        GameObject model = null;
        if (currentWeapon != null)
        {
            model = currentWeapon.GenerateModel();
        }
        
        if (model != null)
        {
            model.transform.SetParent(itemPreview.transform, false);
        }
        InterfaceUtilities.SetLayerRecursively(itemPreview, "UI-3D");
    }

    private void OnGUI()
    {
        //hiltToggle.interactable = (currentWeapon != null);
        //bladeToggle.interactable = (currentWeapon != null);
        //hiltToggle.interactable = (currentWeapon != null);

        if (!init)
        {
            init = true;
            SetCategory();
        }

        bool inSelectState = (state == CraftState.Assembly_Adornment_SelectSlot) ||
                                (state == CraftState.Assembly_Blade_SelectSlot) ||
                                (state == CraftState.Assembly_Hilt_SelectSlot) ||
                                (state == CraftState.Assembly_Adornment_Remove) ||
                                (state == CraftState.Assembly_Blade_Remove) ||
                                (state == CraftState.Assembly_Hilt_Remove);
        if (currentWeapon != null && inSelectState && currentComponent != null)
        {
            if (currentComponent is Hilt || currentComponent is Blade || currentComponent is Adornment)
            {
                slotSelect_main.interactable = true;
                slotSelect_main.showSelectHighlight = true;
                for (int i = 0; i < slotSelect_insets.Length; i++)
                {
                    slotSelect_insets[i].interactable = false;
                    slotSelect_insets[i].showSelectHighlight = false;
                }
            }
            else if (currentComponent is Inset)
            {
                slotSelect_main.interactable = false;
                slotSelect_main.showSelectHighlight = false;
                for (int i = 0; i < slotSelect_insets.Length; i++)
                {
                    slotSelect_insets[i].interactable = true;
                    slotSelect_insets[i].showSelectHighlight = true;
                }
            }
        }
        else
        {
            slotSelect_main.interactable = true;
            slotSelect_main.showSelectHighlight = false;
            for (int i = 0; i < slotSelect_insets.Length; i++)
            {
                slotSelect_insets[i].interactable = true;
                slotSelect_insets[i].showSelectHighlight = false;
            }
        }

        if (currentWeapon != null && (state == CraftState.Assembly_Hilt || state == CraftState.Assembly_Hilt_SelectSlot || state == CraftState.Assembly_Hilt_Remove))
        {
            removeButton.gameObject.SetActive(true);
            hollowHiltButton.gameObject.SetActive(true);
            hollowBladeButton.gameObject.SetActive(false);
        }
        else if (currentWeapon != null && (state == CraftState.Assembly_Blade || state == CraftState.Assembly_Blade_SelectSlot || state == CraftState.Assembly_Blade_Remove))
        {
            removeButton.gameObject.SetActive(true);
            hollowHiltButton.gameObject.SetActive(false);
            hollowBladeButton.gameObject.SetActive(true);
        }
        else if (currentWeapon != null && (state == CraftState.Assembly_Adornment || state == CraftState.Assembly_Adornment_SelectSlot || state == CraftState.Assembly_Adornment_Remove))
        {
            removeButton.gameObject.SetActive(true);
            hollowHiltButton.gameObject.SetActive(false);
            hollowBladeButton.gameObject.SetActive(false);
        }
        else
        {
            removeButton.gameObject.SetActive(false);
            hollowHiltButton.gameObject.SetActive(false);
            hollowBladeButton.gameObject.SetActive(false);
        }
        if (state == CraftState.Assembly_Select)
        {
            newWeaponButton.gameObject.SetActive(true);
        }
        else
        {
            newWeaponButton.gameObject.SetActive(false);
        }
    }

    public void SetupUIInputModule()
    {
        EventSystem.current.GetComponent<InputSystemUIInputModule>().cancel.action.performed += (context) =>
        {
            OnCancel();
        };
    }
    // returns true if weapon is different than current
    public bool SetCurrentWeapon(BladeWeapon weapon)
    {
        if (currentWeapon != weapon)
        {
            currentWeapon = weapon;
            if (currentWeapon is CraftableWeapon craftable)
            {
                craftable.SetProperties();
            }
            return true;
        }
        return false;
    }

    public void CreateNewWeapon()
    {
        CraftableWeapon newCraft = (CraftableWeapon)ScriptableObject.CreateInstance("CraftableWeapon");
        newCraft.itemName = "New Craftable Weapon";
        newCraft.itemDesc = "hot off the presses!";
        inventoryMenu.inventory.Add(newCraft);
        SetCurrentWeapon(newCraft);
        inventoryMenu.Populate(true);
        InventoryItemDisplay itemDisplay = inventoryMenu.FindItemDisplay(newCraft);
        if (itemDisplay != null)
        {
            EventSystem.current.SetSelectedGameObject(itemDisplay.gameObject);
        }
    }
    public void UpdateStats()
    {
        if (currentWeapon != null)
        {
            if (currentWeapon is CraftableWeapon craftable)
            {
                craftable.SetProperties();
            }
            statBlock.SetWeapon(currentWeapon);
        }
        else
        {
            statBlock.Clear();
        }
    }

    public void UpdateSlots()
    {
        bool showSlotsState = (state == CraftState.Assembly_Adornment) || (state == CraftState.Assembly_Adornment_SelectSlot) || (state == CraftState.Assembly_Adornment_Remove) ||
                            (state == CraftState.Assembly_Blade) || (state == CraftState.Assembly_Blade_SelectSlot) || (state == CraftState.Assembly_Blade_Remove) ||
                            (state == CraftState.Assembly_Hilt) || (state == CraftState.Assembly_Hilt_SelectSlot) || (state == CraftState.Assembly_Hilt_Remove);

        if (showSlotsState && currentWeapon != null && currentWeapon is CraftableWeapon craftableWeapon)
        {
            WeaponComponent component = null;
            if ((state == CraftState.Assembly_Blade) || (state == CraftState.Assembly_Blade_SelectSlot) && craftableWeapon.blade != null)
            {
                component = craftableWeapon.blade;
            }
            else if ((state == CraftState.Assembly_Hilt) || (state == CraftState.Assembly_Hilt_SelectSlot) && craftableWeapon.hilt != null)
            {
                component = craftableWeapon.hilt;
            }
            else if ((state == CraftState.Assembly_Adornment) || (state == CraftState.Assembly_Adornment_SelectSlot) && craftableWeapon.adornment != null)
            {
                component = craftableWeapon.adornment;
            }
            slotSelectContainer.SetActive(true);

            slotSelect_main.SetItem(component);
            if (component != null && component is InsetBearer currentBearer)
            {
                for (int i = 0; i < slotSelect_insets.Length; i++)
                {
                    if (i >= currentBearer.slots)
                    {
                        slotSelect_insets[i].gameObject.SetActive(false);
                    }
                    else if ((currentBearer.insets.Count < currentBearer.slots && currentBearer.insets.Count <= i) || (currentBearer.insets.Count > i && currentBearer.insets[i] == null))
                    {
                        slotSelect_insets[i].gameObject.SetActive(true);
                        slotSelect_insets[i].SetItem(null);
                    }
                    else
                    {
                        slotSelect_insets[i].gameObject.SetActive(true);
                        slotSelect_insets[i].SetItem(currentBearer.insets[i]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < slotSelect_insets.Length; i++)
                {
                    slotSelect_insets[i].gameObject.SetActive(false);
                }
            }

            
        }
        else
        {
            slotSelectContainer.SetActive(false);
        }
    }

    public void OnSelectItem(InventoryItemDisplay itemDisplay)
    {
        if (state == CraftState.Assembly_Select)
        {
            if (itemDisplay.item is BladeWeapon weapon)
            {
                SetCurrentWeapon(weapon);
                GenerateModel();
                statBlock.SetCompare(false);
                UpdateStats();
                UpdateSlots();
            }

        }
        else if (currentWeapon != null && currentWeapon is CraftableWeapon craftableWeapon)
        {
            if (state == CraftState.Assembly_Hilt || state == CraftState.Assembly_Hilt_SelectSlot || state == CraftState.Assembly_Hilt_Remove)
            {
                if (itemDisplay.item is Hilt hilt)
                {
                    currentComponent = hilt;
                    statBlock.SetCompare(false);
                    craftableWeapon.GetStatDifferencesHiltChange(hilt, ref statBlock);

                    UpdateStats();
                    UpdateSlots();
                    EventSystem.current.SetSelectedGameObject(slotSelect_main.gameObject);
                }
                else if (itemDisplay.item is Inset inset)
                {
                    currentComponent = inset;
                    statBlock.SetCompare(false);
                    if (inset is IBladeStatModifier bsm)
                    {
                        craftableWeapon.GetStatDifferencesBladeStatModifier(bsm, ref statBlock);
                    }

                    UpdateStats();
                    UpdateSlots();

                    if (craftableWeapon.hilt != null && craftableWeapon.hilt.slots > 0)
                    {
                        EventSystem.current.SetSelectedGameObject(slotSelect_insets[0].gameObject);
                    }
                }
                state = CraftState.Assembly_Hilt_SelectSlot;
            }
            else if (state == CraftState.Assembly_Blade || state == CraftState.Assembly_Blade_SelectSlot || state == CraftState.Assembly_Blade_Remove)
            {
                if (itemDisplay.item is Blade blade)
                {
                    currentComponent = blade;
                    statBlock.SetCompare(false);

                    craftableWeapon.GetStatDifferencesBladeChange(blade, ref statBlock);

                    UpdateStats();
                    UpdateSlots();
                    if (currentWeapon != null && currentWeapon is CraftableWeapon)
                    {
                        EventSystem.current.SetSelectedGameObject(slotSelect_main.gameObject);
                    }
                }
                else if (itemDisplay.item is Inset inset)
                {
                    currentComponent = inset;
                    statBlock.SetCompare(false);
                    if (inset is IBladeStatModifier bsm)
                    {
                        craftableWeapon.GetStatDifferencesBladeStatModifier(bsm, ref statBlock);
                    }

                    UpdateStats();
                    UpdateSlots();

                    if (craftableWeapon.blade != null && craftableWeapon.blade.slots > 0)
                    {
                        EventSystem.current.SetSelectedGameObject(slotSelect_insets[0].gameObject);
                    }
                }
                state = CraftState.Assembly_Blade_SelectSlot;
            }
            else if (state == CraftState.Assembly_Adornment || state == CraftState.Assembly_Adornment_SelectSlot || state == CraftState.Assembly_Adornment_Remove)
            {
                if (itemDisplay.item is Adornment adornment)
                {
                    currentComponent = adornment;
                    statBlock.SetCompare(false);

                        craftableWeapon.GetStatDifferencesAdornmentChange(adornment, ref statBlock);


                    UpdateStats();
                    UpdateSlots();

                        EventSystem.current.SetSelectedGameObject(slotSelect_main.gameObject);

                }
                state = CraftState.Assembly_Adornment_SelectSlot;
            }
        }
    }

    public void OnItemHover(InventoryItemDisplay itemDisplay)
    {
        hovered = itemDisplay;
        if (currentWeapon != null && currentComponent == null)
        {
            if (itemDisplay.item is Hilt hilt)
            {
                statBlock.SetCompare(false);
                if (currentWeapon != null && currentWeapon is CraftableWeapon craftableWeapon)
                {
                    craftableWeapon.GetStatDifferencesHiltChange(hilt, ref statBlock);
                }

                UpdateStats();
            }
            else if (itemDisplay.item is Blade blade)
            {
  
                statBlock.SetCompare(false);
                if (currentWeapon != null && currentWeapon is CraftableWeapon craftableWeapon)
                {
                    craftableWeapon.GetStatDifferencesBladeChange(blade, ref statBlock);
                }

                UpdateStats();
            }
            else if (itemDisplay.item is Adornment adornment)
            {
                statBlock.SetCompare(false);
                if (currentWeapon != null && currentWeapon is CraftableWeapon craftableWeapon)
                {
                    craftableWeapon.GetStatDifferencesAdornmentChange(adornment, ref statBlock);
                }

                UpdateStats();
            }
            else if (itemDisplay.item is CraftableWeapon weapon)
            {
                statBlock.SetCompare(false);
                if (currentWeapon != null)
                {
                    currentWeapon.GetStatDifferencesWeaponComparison(weapon, ref statBlock);
                }

                UpdateStats();
            }
        }
    }

    public void ClearCompareIfNoSelect()
    {
        if (currentComponent == null)
        {
            statBlock.SetCompare(false);
            UpdateStats();
        }
    }
    public void OnSelectSlotMain()
    {
        if (currentWeapon != null && currentWeapon is CraftableWeapon craftableWeapon)
        {
            if (state == CraftState.Assembly_Hilt_SelectSlot && currentWeapon != null && currentComponent != null && currentComponent is Hilt hilt)
            {
                List<Inset> removedInsets = new List<Inset>();
                if (craftableWeapon.hilt != null)
                {
                    if (craftableWeapon.hilt.UnattachAll(out removedInsets) > 0)
                    {
                        foreach (Inset inset in removedInsets)
                        {
                            if (!(inset is HollowInset))
                            {
                                inventoryMenu.inventory.Add(inset);
                            }
                        }
                    }
                    inventoryMenu.inventory.Add(craftableWeapon.hilt);
                }
                inventoryMenu.inventory.Remove(currentComponent);
                craftableWeapon.hilt = hilt;
                currentComponent = null;
                inventoryMenu.Populate(true);
                statBlock.SetCompare(false);
                UpdateStats();
                UpdateSlots();
                GenerateModel();
            }
            else if (state == CraftState.Assembly_Blade_SelectSlot && currentWeapon != null && currentComponent != null && currentComponent is Blade blade)
            {
                List<Inset> removedInsets = new List<Inset>();
                if (craftableWeapon.blade != null)
                {
                    if (craftableWeapon.blade.UnattachAll(out removedInsets) > 0)
                    {
                        foreach (Inset inset in removedInsets)
                        {
                            if (!(inset is HollowInset))
                            {
                                inventoryMenu.inventory.Add(inset);
                            }
                        }
                    }
                    inventoryMenu.inventory.Add(craftableWeapon.blade);
                }

                inventoryMenu.inventory.Remove(currentComponent);
                craftableWeapon.blade = blade;
                currentComponent = null;
                inventoryMenu.Populate(true);
                statBlock.SetCompare(false);
                UpdateStats();
                UpdateSlots();
                GenerateModel();
            }
            else if (state == CraftState.Assembly_Adornment_SelectSlot && currentWeapon != null && currentComponent != null && currentComponent is Adornment adornment)
            {
                if (craftableWeapon.adornment != null)
                {
                    inventoryMenu.inventory.Add(craftableWeapon.adornment);
                }
                inventoryMenu.inventory.Remove(currentComponent);
                craftableWeapon.adornment = adornment;
                currentComponent = null;
                inventoryMenu.Populate(true);
                statBlock.SetCompare(false);
                UpdateStats();
                UpdateSlots();
                GenerateModel();
            }
            else if (state == CraftState.Assembly_Hilt_Remove)
            {
                if (craftableWeapon.hilt != null)
                {
                    inventoryMenu.inventory.Add(craftableWeapon.hilt);
                }
                craftableWeapon.hilt = null;
                state = CraftState.Assembly_Hilt;
                inventoryMenu.Populate(true);
                statBlock.SetCompare(false);
                UpdateStats();
                UpdateSlots();
                GenerateModel();
            }
            else if (state == CraftState.Assembly_Blade_Remove)
            {
                if (craftableWeapon.blade != null)
                {
                    inventoryMenu.inventory.Add(craftableWeapon.blade);
                }
                craftableWeapon.blade = null;
                state = CraftState.Assembly_Blade;
                inventoryMenu.Populate(true);
                statBlock.SetCompare(false);
                UpdateStats();
                UpdateSlots();
                GenerateModel();
            }
            else if (state == CraftState.Assembly_Adornment_Remove)
            {
                inventoryMenu.inventory.Add(craftableWeapon.adornment);
                craftableWeapon.adornment = null;
                state = CraftState.Assembly_Adornment;
                inventoryMenu.Populate(true);
                statBlock.SetCompare(false);
                UpdateStats();
                UpdateSlots();
                GenerateModel();
            }

        }
        InventoryItemDisplay item = inventoryMenu.GetFirstItem();
        if (item != null)
        {
            EventSystem.current.SetSelectedGameObject(item.gameObject);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(previewToggle.gameObject);
        }
        Debug.Log("select main slot");
        currentComponent = null;
    }

    public void OnSelectSlotInset(int slot)
    {
        if (currentWeapon != null && currentWeapon is CraftableWeapon craftableWeapon)
        {
            if (state == CraftState.Assembly_Hilt_SelectSlot && currentComponent is Inset inseth)
            {
                if (craftableWeapon.hilt != null)
                {
                    Inset prev;
                    craftableWeapon.hilt.UnattachInset(slot, out prev);
                    craftableWeapon.hilt.AttachInset(inseth, slot);
                    inventoryMenu.inventory.Remove(inseth);
                    if (prev != null && !(prev is HollowInset))
                    {
                        inventoryMenu.inventory.Add(prev);
                    }
                    inventoryMenu.inventory.Add(craftableWeapon.hilt);
                    state = CraftState.Assembly_Hilt;
                    inventoryMenu.Populate(true);
                    statBlock.SetCompare(false);
                    UpdateStats();
                    UpdateSlots();
                    GenerateModel();
                }
            }
            else if (state == CraftState.Assembly_Blade_SelectSlot && currentComponent is Inset insetb)
            {
                if (craftableWeapon.blade != null)
                {
                    Inset prev;
                    craftableWeapon.blade.UnattachInset(slot, out prev);
                    craftableWeapon.blade.AttachInset(insetb, slot);
                    inventoryMenu.inventory.Remove(insetb);
                    if (prev != null && !(prev is HollowInset))
                    {
                        inventoryMenu.inventory.Add(prev);
                    }
                    inventoryMenu.inventory.Add(craftableWeapon.blade);
                    state = CraftState.Assembly_Blade;
                    inventoryMenu.Populate(true);
                    statBlock.SetCompare(false);
                    UpdateStats();
                    UpdateSlots();
                    GenerateModel();
                }
            }
            else if (state == CraftState.Assembly_Hilt_Remove)
            {
                if (craftableWeapon.hilt != null)
                {
                    Inset prev;
                    craftableWeapon.hilt.UnattachInset(slot, out prev);
                    if (prev != null && !(prev is HollowInset))
                    {
                        inventoryMenu.inventory.Add(prev);
                    }
                    state = CraftState.Assembly_Hilt;
                    inventoryMenu.Populate(true);
                    statBlock.SetCompare(false);
                    UpdateStats();
                    UpdateSlots();
                    GenerateModel();
                }
            }
            else if (state == CraftState.Assembly_Blade_Remove)
            {
                if (craftableWeapon.blade != null)
                {
                    Inset prev;
                    craftableWeapon.blade.UnattachInset(slot, out prev);
                    if (prev != null && !(prev is HollowInset))
                    {
                        inventoryMenu.inventory.Add(prev);
                    }
                    state = CraftState.Assembly_Blade;
                    inventoryMenu.Populate(true);
                    statBlock.SetCompare(false);
                    UpdateStats();
                    UpdateSlots();
                    GenerateModel();
                }
            }
        }
        InventoryItemDisplay item = inventoryMenu.GetFirstItem();
        if (item != null)
        {
            EventSystem.current.SetSelectedGameObject(item.gameObject);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(previewToggle.gameObject);
        }
        Debug.Log("select inset slot #" + slot);
        currentComponent = null;
    }
    
    public void SetRemove()
    {
        if (state == CraftState.Assembly_Hilt || state == CraftState.Assembly_Hilt_SelectSlot)
        {
            state = CraftState.Assembly_Hilt_Remove;
        }
        else if (state == CraftState.Assembly_Blade || state == CraftState.Assembly_Blade_SelectSlot)
        {
            state = CraftState.Assembly_Blade_Remove;
        }
        else if (state == CraftState.Assembly_Adornment)
        {
            state = CraftState.Assembly_Adornment_Remove;
        }
        EventSystem.current.SetSelectedGameObject(slotSelect_main.gameObject);
    }

    public void ToggleHide()
    {

        hide = !hide;
        Debug.Log("hide? " + hide);
        hideable.SetActive(!hide);
    }

    public void OnCancel()
    {
        if (state == CraftState.Assembly_Hilt_Remove || state == CraftState.Assembly_Hilt_SelectSlot)
        {
            currentComponent = null;
            state = CraftState.Assembly_Hilt;
            InventoryItemDisplay firstItem = inventoryMenu.GetFirstItem();
            if (firstItem != null)
            {
                EventSystem.current.SetSelectedGameObject(firstItem.gameObject);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(hiltToggle.gameObject);
            }
        }
        else if(state == CraftState.Assembly_Blade_Remove || state == CraftState.Assembly_Blade_SelectSlot)
        {
            currentComponent = null;
            state = CraftState.Assembly_Blade;
            InventoryItemDisplay firstItem = inventoryMenu.GetFirstItem();
            if (firstItem != null)
            {
                EventSystem.current.SetSelectedGameObject(firstItem.gameObject);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(bladeToggle.gameObject);
            }
        }
        else if (state == CraftState.Assembly_Adornment_Remove || state == CraftState.Assembly_Adornment_SelectSlot)
        {
            currentComponent = null;
            state = CraftState.Assembly_Adornment;
            InventoryItemDisplay firstItem = inventoryMenu.GetFirstItem();
            if (firstItem != null)
            {
                EventSystem.current.SetSelectedGameObject(firstItem.gameObject);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(adornmentToggle.gameObject);
            }
        }
        else if (state == CraftState.Assembly_Blade || state == CraftState.Assembly_Hilt || state == CraftState.Assembly_Adornment)
        {
            state = CraftState.Assembly_Select;
            currentComponent = null;
            currentWeapon = null;
            UpdateStats();
        }
        else if (state == CraftState.Assembly_Select && currentWeapon != null)
        {
            currentComponent = null;
            currentWeapon = null;
            UpdateStats();
        }
        else if (currentWeapon == null && !exit)
        {
            exit = true;
            onExit.Invoke();
        }
    }
}
