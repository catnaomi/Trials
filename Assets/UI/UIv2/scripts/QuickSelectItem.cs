using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickSelectItem : MonoBehaviour
{
    public Image itemImage;
    public Image frameImage;
    public Item item;
    public int slot; // 0 = up, 1 = left, 2 = right, 3 = down

    public Color defaultColor = new Color(1f, 1f, 1f, 0f);
    public Color equippedColor = new Color(1f, 1f, 1f, 0.5f);
    public Color awaitInputColor = Color.magenta;
    bool awaitingInput;
    public bool equipped;
    public InventoryUI2 invUI;

    public float shadowAlpha = 0f;
    Shadow shadow;
    // Start is called before the first frame update
    void Start()
    {
        UpdateSlots();
        PlayerActor.player.GetComponent<Inventory>().OnChange.AddListener(UpdateSlots);

        shadow = this.GetComponentInChildren<Shadow>();
    }

    public void UpdateSlots()
    {
        //Debug.Log("Updating Quickslot UI: " + slot);
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
        if (item == inventory.GetMainWeapon() || item == inventory.GetOffWeapon())
        {
            frameImage.color = equippedColor;

            if (!equipped)
            {
                shadow.effectColor = equippedColor;
                Flare();
            }
            equipped = true;
        }
        else
        {
            frameImage.color = defaultColor;
            equipped = false;
        }
    }

    private void OnGUI()
    {
        if (shadowAlpha > 0f)
        {
            Color c = shadow.effectColor;
            shadowAlpha -= Time.deltaTime;
            shadow.effectColor = new Color(c.r, c.g, c.b, shadowAlpha);
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
            this.item = item;
            itemImage.enabled = true;
            itemImage.sprite = item.displayImage;
            itemImage.color = item.displayColor;
        }
        else
        {
            ClearItem();
        }
    }

    public void AwaitInput()
    {
        awaitingInput = true;
        frameImage.color = (true) ? awaitInputColor : defaultColor;
    }

    public void EndAwait()
    {
        frameImage.color = defaultColor;
        awaitingInput = false;
    }

    public void Flare()
    {
        shadowAlpha = 1f;
    }
}
