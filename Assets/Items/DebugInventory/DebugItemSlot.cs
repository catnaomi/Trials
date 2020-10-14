using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugItemSlot : MonoBehaviour
{
    [ReadOnly] public Item item;
    [HideInInspector] public Inventory inventory;
    public Text itemName;
    public bool nameonly;
    public Button Slot0Equip;
    public Button Slot1Equip;
    public Button Slot2Equip;
    public Button Slot3Equip;

    // Start is called before the first frame update
    void Start()
    {
        if (!nameonly)
        {

            /*
            if (item is EquippableWeapon)
            {
                ((EquippableWeapon)item).OnEquip.AddListener(DisableButtons);
                ((EquippableWeapon)item).OnUnequip.AddListener(EnableButtons);
            }
            */


            Slot0Equip.onClick.AddListener(() => { inventory.EquipToSlot((EquippableWeapon)item, 0); });
            Slot1Equip.onClick.AddListener(() => { inventory.EquipToSlot((EquippableWeapon)item, 1); });
            Slot2Equip.onClick.AddListener(() => { inventory.EquipToSlot((EquippableWeapon)item, 2); });
            Slot3Equip.onClick.AddListener(() => { inventory.EquipToSlot((EquippableWeapon)item, 3); });


        }
        itemName.text = item.itemName;
    }
}
