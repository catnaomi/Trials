using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugInventoryMenu : MonoBehaviour
{
    bool initialized;
    public bool active;

    public PlayerActor player;
    public GameObject prefab;
    public GameObject prefabNameOnly;
    public RectTransform viewport;
    public GameObject invContainer;
    public GameObject slotContainer;

    [Space(10)]
    // main hand
    public Text mainName;
    public Text mainDesc;
    public Button mainUnequip;
    [Space(5)]
    // offhand
    public Text offName;
    public Text offDesc;
    public Button offUnequip;

    // d-pad inventory
    [Space(10)]
    public Text Slot0Name; // right
    public Image Slot0Panel;
    public Text Slot1Name; // left
    public Image Slot1Panel;
    public Text Slot2Name; // up
    public Image Slot2Panel;
    public Text Slot3Name; // down
    public Image Slot3Panel;
    [Space(5)]
    public Color mainHandColor;
    public Color offHandColor;
    public Color unequippedColor;
    public Color twoHandColor;

    List<GameObject> slots;

    private void Start()
    {
        slots = new List<GameObject>();
    }
    // Update is called once per frame
    void OnGUI()
    {

        if (!initialized && player.GetComponent<Inventory>() != null)
        {
            mainUnequip.onClick.AddListener(() => { player.inventory.UnequipMainWeapon(); });
            offUnequip.onClick.AddListener(() => { player.inventory.UnequipOffHandWeapon(); });

            player.GetComponent<Inventory>().OnChange.AddListener(ResetSlots);

            ResetSlots();
            invContainer.SetActive(false);
            initialized = true;
            active = false;
        }

        mainName.text = "(empty)";
        mainDesc.text = "(empty)";
        offName.text = "(empty)";
        offDesc.text = "(empty)";

        if (player.inventory.IsMainEquipped())
        {
            mainName.text = player.inventory.MainWeapon.itemName;
            mainDesc.text = player.inventory.MainWeapon.itemDesc;
        }
        if (player.inventory.IsOffEquipped())
        {
            offName.text = player.inventory.OffWeapon.itemName;
            offDesc.text = player.inventory.OffWeapon.itemDesc;
        }

        Slot0Name.text = "-";
        Slot1Name.text = "-";
        Slot2Name.text = "-";
        Slot3Name.text = "-";

        Color mainColor = (player.inventory.IsTwoHanding() ? twoHandColor : mainHandColor);
        
        if (player.inventory.Slot0Weapon != null)
        {
            Slot0Name.text = player.inventory.Slot0Weapon.itemName;
            if (player.inventory.GetItemHand(player.inventory.Slot0Weapon) >= 1)
            {
                Slot0Panel.color = mainColor;
            }
            else if (player.inventory.GetItemHand(player.inventory.Slot0Weapon) <= -1)
            {
                Slot0Panel.color = offHandColor;
            }
            else
            {
                Slot0Panel.color = unequippedColor;
            }
        }
        else
        {
            Slot0Panel.color = unequippedColor;
        }
        if (player.inventory.Slot1Weapon != null)
        {
            Slot1Name.text = player.inventory.Slot1Weapon.itemName;
            if (player.inventory.GetItemHand(player.inventory.Slot1Weapon) >= 1)
            {
                Slot1Panel.color = mainColor;
            }
            else if (player.inventory.GetItemHand(player.inventory.Slot1Weapon) <= -1)
            {
                Slot1Panel.color = offHandColor;
            }
            else
            {
                Slot1Panel.color = unequippedColor;
            }
        }
        else
        {
            Slot1Panel.color = unequippedColor;
        }
        if (player.inventory.Slot2Weapon != null)
        {
            Slot2Name.text = player.inventory.Slot2Weapon.itemName;
            if (player.inventory.GetItemHand(player.inventory.Slot2Weapon) >= 1)
            {
                Slot2Panel.color = mainColor;
            }
            else if (player.inventory.GetItemHand(player.inventory.Slot2Weapon) <= -1)
            {
                Slot2Panel.color = offHandColor;
            }
            else
            {
                Slot2Panel.color = unequippedColor;
            }
        }
        else
        {
            Slot2Panel.color = unequippedColor;
        }
        if (player.inventory.Slot3Weapon != null)
        {
            Slot3Name.text = player.inventory.Slot3Weapon.itemName;
            if (player.inventory.GetItemHand(player.inventory.Slot3Weapon) >= 1)
            {
                Slot3Panel.color = mainColor;
            }
            else if (player.inventory.GetItemHand(player.inventory.Slot3Weapon) <= -1)
            {
                Slot3Panel.color = offHandColor;
            }
            else
            {
                Slot3Panel.color = unequippedColor;
            }
        }
        else
        {
            Slot3Panel.color = unequippedColor;
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            active = !active;
            invContainer.SetActive(active);
        }
    }
    private void ResetSlots()
    {
        if (slots != null)
        {
            foreach(Transform slot in slotContainer.transform)
            {
                Destroy(slot.gameObject);
            }
        }
        slots.Clear();
        
        for (int i = 0; i < player.inventory.contents.Count; i++)
        {
            bool nameonly = !(player.inventory.contents[i] is EquippableWeapon);
            GameObject slot = Instantiate((!nameonly ? prefab : prefabNameOnly), viewport.position + Vector3.right * (75f) + Vector3.up * (-125f * (i+1)), Quaternion.identity, viewport);
            DebugItemSlot controller = slot.GetComponent<DebugItemSlot>();
            controller.item = player.inventory.contents[i];
            controller.inventory = player.inventory;
            controller.nameonly = nameonly;
        }
    }
}
