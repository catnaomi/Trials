using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    [Space(5)]
    public WeaponStatBlock statBlock;
    [Space(5)]
    public GameObject slotSelectContainer;
    public InventoryItemDisplay slotSelect_main;
    public InventoryItemDisplay[] slotSelect_insets;
    
    BladeWeapon currentWeapon;
    public WeaponComponent currentComponent;
    InventoryItemDisplay hovered;
    bool init;
    public enum CraftState
    {
        Assembly_Select,
        Assembly_Hilt,
        Assembly_Hilt_SelectSlot,
        Assembly_Blade,
        Assembly_Blade_SelectSlot,
        Assembly_Adornment,
        Assembly_Adornment_SelectSlot
    }
    // Start is called before the first frame update
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(previewToggle.gameObject);
        inventoryMenu.source = this.source;
        init = false;
    }

    public void SetCategory()
    {
        if (previewToggle.isOn)
        {
            state = CraftState.Assembly_Select;
            currentComponent = null;
            inventoryMenu.filterType = "BladeWeapon,CraftableWeapon";
            inventoryMenu.Populate(true);
        }
        else if (hiltToggle.isOn)
        {
            if (state != CraftState.Assembly_Hilt)
            {
                state = CraftState.Assembly_Hilt;
                currentComponent = null;
                inventoryMenu.filterType = "Hilt";
                inventoryMenu.Populate(true);
            }
        }
        else if (bladeToggle.isOn)
        {
            if (state != CraftState.Assembly_Blade)
            {
                state = CraftState.Assembly_Blade;
                currentComponent = null;
                inventoryMenu.filterType = "Blade";
                inventoryMenu.Populate(true);
            }
        }
        else if (adornmentToggle.isOn)
        {
            if (state != CraftState.Assembly_Adornment)
            {
                state = CraftState.Assembly_Adornment;
                currentComponent = null;
                inventoryMenu.filterType = "Adornment";
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
                                (state == CraftState.Assembly_Hilt_SelectSlot);
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
    public void UpdateStats()
    {
        if (currentWeapon != null)
        {
            statBlock.SetWeapon(currentWeapon);
        }
    }

    public void UpdateSlots()
    {
        bool showSlotsState = (state == CraftState.Assembly_Adornment) || (state == CraftState.Assembly_Adornment_SelectSlot) ||
                            (state == CraftState.Assembly_Blade) || (state == CraftState.Assembly_Blade_SelectSlot) ||
                            (state == CraftState.Assembly_Hilt) || (state == CraftState.Assembly_Hilt_SelectSlot);

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
        else if (state == CraftState.Assembly_Hilt || state == CraftState.Assembly_Hilt_SelectSlot)
        {
            if (itemDisplay.item is Hilt hilt)
            {
                currentComponent = hilt;
                statBlock.SetCompare(false);
                if (currentWeapon != null && currentWeapon is CraftableWeapon craftableWeapon)
                {
                    craftableWeapon.GetStatDifferencesHiltChange(hilt, ref statBlock);
                }

                UpdateStats();
                UpdateSlots();

                if (currentWeapon != null)
                {
                    EventSystem.current.SetSelectedGameObject(slotSelect_main.gameObject);
                }
            }
            state = CraftState.Assembly_Hilt_SelectSlot;
        }
        else if (state == CraftState.Assembly_Blade || state == CraftState.Assembly_Blade_SelectSlot)
        {
            if (itemDisplay.item is Blade blade)
            {
                currentComponent = blade;
                statBlock.SetCompare(false);
                if (currentWeapon != null && currentWeapon is CraftableWeapon craftableWeapon)
                {
                    craftableWeapon.GetStatDifferencesBladeChange(blade, ref statBlock);
                }

                UpdateStats();
                UpdateSlots();
                if (currentWeapon != null)
                {
                    EventSystem.current.SetSelectedGameObject(slotSelect_main.gameObject);
                }
            }
            state = CraftState.Assembly_Blade_SelectSlot;
        }
        else if (state == CraftState.Assembly_Adornment || state == CraftState.Assembly_Adornment_SelectSlot)
        {
            if (itemDisplay.item is Adornment adornment)
            {
                currentComponent = adornment;
                statBlock.SetCompare(false);
                if (currentWeapon != null && currentWeapon is CraftableWeapon craftableWeapon)
                {
                    craftableWeapon.GetStatDifferencesAdornmentChange(adornment, ref statBlock);
                }

                UpdateStats();
                UpdateSlots();
                if (currentWeapon != null)
                {
                    EventSystem.current.SetSelectedGameObject(slotSelect_main.gameObject);
                }
            }
            state = CraftState.Assembly_Adornment_SelectSlot;
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
                            inventoryMenu.inventory.Add(inset);
                        }
                    }
                    inventoryMenu.inventory.Add(craftableWeapon.hilt);
                }
                inventoryMenu.inventory.Remove(currentComponent);
                craftableWeapon.hilt = hilt;
                currentComponent = null;
                inventoryMenu.Populate(true);
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
                            inventoryMenu.inventory.Add(inset);
                        }
                    }
                    inventoryMenu.inventory.Add(craftableWeapon.blade);
                }

                inventoryMenu.inventory.Remove(currentComponent);
                craftableWeapon.blade = blade;
                currentComponent = null;
                inventoryMenu.Populate(true);
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
                UpdateStats();
                UpdateSlots();
                GenerateModel();
            }
        }
        Debug.Log("select main slot");
        currentComponent = null;
    }

    public void OnSelectSlotInset(int slot)
    {
        Debug.Log("select inset slot #" + slot);
        currentComponent = null;
    }
}
