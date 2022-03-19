using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuickSelectItem : MonoBehaviour
{
    public Image itemImage;
    public Image frameImage;
    public Item item;
    public TMP_Text number;
    public int slot; // 0 = up, 1 = left, 2 = right, 3 = down

    public Color defaultColor = new Color(1f, 1f, 1f, 0f);
    public Color equippedColor = new Color(1f, 1f, 1f, 0.5f);
    public Color awaitInputColor = Color.magenta;
    bool awaitingInput;
    public bool equipped;
    public InventoryUI2 invUI;

    public float shadowAlpha = 0f;
    public Shadow shadow;
    // Start is called before the first frame update
    void Start()
    {
        
        PlayerActor.player.GetComponent<Inventory>().OnChange.AddListener(UpdateSlots);

        //shadow = this.GetComponentInChildren<Shadow>();
        UpdateSlots();
    }

    private void OnEnable()
    {
        if (PlayerActor.player == null) return;
        UpdateSlots();
    }
    public void UpdateSlots()
    {
        //Debug.Log("Updating Quickslot UI: " + slot);
        PlayerInventory inventory = PlayerActor.player.GetComponent<PlayerInventory>();
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
        if (item is EquippableWeapon weapon && weapon.usesAmmunition)
        {
            number.gameObject.SetActive(true);
            if (weapon is RangedGun gun)
            {
                number.text = gun.ammoCurrent + "/" + weapon.GetAmmunitionRemaining();
            }
            else
            {
                number.text = weapon.GetAmmunitionRemaining().ToString();
            }
            
        }
        else
        {
            number.gameObject.SetActive(false);
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
            number.gameObject.SetActive(false);
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
