using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftMenuController : MonoBehaviour
{
    public GameObject source;
    public CraftCategory category;
    [Header("UI References")]
    public Toggle previewToggle;
    public Toggle hiltToggle;
    public Toggle bladeToggle;
    public Toggle adornmentToggle;
    [Space(5)]
    public InventoryUI2 inventoryMenu;

    public enum CraftCategory
    {
        Preview,
        Hilt,
        Blade,
        Adornment
    }
    // Start is called before the first frame update
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(previewToggle.gameObject);
        inventoryMenu.source = this.source;
    }

    public void SetCategory()
    {
        if (previewToggle.isOn)
        {
            category = CraftCategory.Preview;
        }
        else if (hiltToggle.isOn)
        {
            if (category != CraftCategory.Hilt)
            {
                category = CraftCategory.Hilt;
                inventoryMenu.filterType = "Hilt";
                inventoryMenu.Populate(true);
            }
        }
        else if (bladeToggle.isOn)
        {
            if (category != CraftCategory.Blade)
            {
                category = CraftCategory.Blade;
                inventoryMenu.filterType = "Blade";
                inventoryMenu.Populate(true);
            }
        }
        else if (adornmentToggle.isOn)
        {
            if (category != CraftCategory.Adornment)
            {
                category = CraftCategory.Adornment;
                inventoryMenu.filterType = "Adornment";
                inventoryMenu.Populate(true);
            }
        }
    }
}
