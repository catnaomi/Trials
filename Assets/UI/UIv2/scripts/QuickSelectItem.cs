using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickSelectItem : MonoBehaviour
{
    public Image itemImage;
    public Item item;
    public int slot; // 0 = up, 1 = left, 2 = right, 3 = down
    // Start is called before the first frame update
    void Start()
    {
        UpdateSlots();
        PlayerActor.player.GetComponent<Inventory>().OnChange.AddListener(UpdateSlots);
    }

    public void UpdateSlots()
    {
        Inventory inventory = PlayerActor.player.GetComponent<Inventory>();
        switch (slot)
        {
            case 0:
                SetItem(inventory.Slot0Weapon);
                break;
            case 1:
                SetItem(inventory.Slot1Weapon);
                break;
            case 2:
                SetItem(inventory.Slot2Weapon);
                break;
        }
    }
    public void ClearItem()
    {
        item = null;
        itemImage.enabled = false;
    }
    public void SetItem(Item item)
    {
        if (item != null)
        {
            itemImage.enabled = true;
            itemImage.sprite = item.displayImage;
            itemImage.color = item.displayColor;
        }
        else
        {
            ClearItem();
        }
    }
}
