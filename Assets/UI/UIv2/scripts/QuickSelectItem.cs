using System;
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
    public Image primaryIndicator;
    public int slot; // 0 = up, 1 = left, 2 = right, 3 = down

    public Color defaultColor = new Color(1f, 1f, 1f, 0f);
    public Color equippedColor = new Color(1f, 1f, 1f, 0.5f);
    public Color unequippedColor = Color.clear;
    public Color awaitInputColor = Color.magenta;
    public Gradient flashGradient;
    bool awaitingInput;
    public bool equipped;
    public InventoryUI2 invUI;

    public Shadow shadow;
    public Image highlight;

    public float flashTime = 1f;
    [Header("VizData")]
    public bool useVizData;
    public ItemVizData vizData;
    public bool forceUpdate;
    public bool forceFlash;
    [Header("Sprite Reference")]
    public Sprite itemBorder;
    public Sprite itemHighlight;
    public Sprite consumableBorder;
    public Sprite consumableHighlight;
    public Sprite primary;
    public Sprite secondary;
    [Header("Size Reference")]
    public float sizeEmpty = 50f;
    public float sizeUnequipped = 90f;
    public float sizeLight = 110f;
    public float sizeMedium = 155f;
    public float sizeHeavy = 200f;
    public float sizeConsumable = 155f;

    float clock;
    float t;

    [Serializable]
    public struct ItemVizData {
        public EquippableWeapon.Size size;
        public bool isEmpty;
        public bool isPrimary;
        public bool isConsumable;
        public bool isEquipped;
        public int quantity;
    }
    // Start is called before the first frame update
    void Start()
    {

        if (PlayerActor.player != null)
        {
            PlayerActor.player.GetComponent<Inventory>().OnChange.AddListener(UpdateSlots);
            UpdateSlots();
        }

        //shadow = this.GetComponentInChildren<Shadow>();
        
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
                SetItem(inventory.Slot0Equippable);
                break;
            case 1:
                SetItem(inventory.Slot1Equippable);
                break;
            case 2:
                SetItem(inventory.Slot2Equippable);
                break;
            case 3:
                SetItem(inventory.Slot3Equippable);
                break;
        }
        if (item != null)
        {
            vizData.isEmpty = false;

            if (item == inventory.GetMainWeapon() || item == inventory.GetOffWeapon())
            {
                

                if (!equipped)
                {
                    if (shadow != null) shadow.effectColor = equippedColor;
                    Flare();
                }
                equipped = true;
                if (useVizData)
                {
                    if (item is EquippableWeapon equippableWeapon)
                    {
                        vizData.size = equippableWeapon.size;
                        vizData.isPrimary = (item == inventory.GetMainWeapon());
                        vizData.isConsumable = false;
                        vizData.isEquipped = true;
                        if (equippableWeapon.usesAmmunition)
                        {
                            vizData.quantity = equippableWeapon.GetAmmunitionRemaining();
                        }
                        else
                        {
                            vizData.quantity = -1;
                        }
                    }


                }
                else
                {
                    frameImage.color = equippedColor;
                }
            }
            else
            {
               
                equipped = false;
                if (useVizData)
                {
                    primaryIndicator.enabled = false;
                    vizData.isEquipped = false;
                    if (item is Consumable consumable)
                    {
                        vizData.isConsumable = true;
                        vizData.quantity = consumable.GetUsesRemaining();
                    }
                    else if (item is EquippableWeapon equippableWeapon && equippableWeapon.usesAmmunition)
                    {
                        vizData.quantity = equippableWeapon.GetAmmunitionRemaining();
                    }
                    else
                    {
                        vizData.quantity = -1;
                    }
                }
                else
                {
                    frameImage.color = defaultColor;
                }
            }
        }
        else
        {
            if (useVizData)
            {
                vizData.isEmpty = true;
                vizData.quantity = -1;
            }
        }
        UpdateFromVizdata();
    }

    public void UpdateFromVizdata()
    {
        primaryIndicator.enabled = vizData.isEquipped;
        if (!vizData.isConsumable)
        {
            frameImage.sprite = itemBorder;
            highlight.sprite = itemHighlight;
        }
        else
        {
            frameImage.sprite = consumableBorder;
            highlight.sprite = consumableHighlight;
        }
        if (vizData.isEquipped)
        {
            primaryIndicator.sprite = (vizData.isPrimary) ? primary : secondary;
            frameImage.color = equippedColor;
        }
        else if (!vizData.isEmpty)
        {
            frameImage.color = defaultColor;
        }
        else
        {
            frameImage.color = unequippedColor;
        }
        if (vizData.quantity >= 0)
        {
            number.gameObject.SetActive(true);
            number.text = vizData.quantity.ToString();
        }
        else
        {
            number.gameObject.SetActive(false);
        }
        float size = 100f;
        if (vizData.isEmpty)
        {
            size = sizeEmpty;
        }
        else if (!vizData.isEquipped)
        {
            size = sizeUnequipped;
        }
        else if (vizData.size == EquippableWeapon.Size.Light)
        {
            size = sizeLight;
        }
        else if (vizData.size == EquippableWeapon.Size.Medium)
        {
            size = sizeMedium;
        }
        else if (vizData.size == EquippableWeapon.Size.Heavy)
        {
            size = sizeHeavy;
        }
        this.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
        this.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
    }

    private void Update()
    {
        if (forceUpdate)
        {
            forceUpdate = false;
            UpdateFromVizdata();
        }
        if (forceFlash)
        {
            forceFlash = false;
            Flare();
        }
    }
    private void OnGUI()
    {
        // flash
        if (clock > 0f)
        {
            clock -= Time.deltaTime;
            t = 1f - Mathf.Clamp01(clock / flashTime);
            
            if (shadow != null)
            {
                Color c = shadow.effectColor;
                shadow.effectColor = new Color(c.r, c.g, c.b, 1f - t);
            }
            if (highlight != null)
            {
                highlight.color = flashGradient.Evaluate(t);
                frameImage.color = new Color(highlight.color.r, highlight.color.g, highlight.color.b, frameImage.color.a);
            }
            
        }
        else
        {
            highlight.color = Color.clear;
            frameImage.color = new Color(1f, 1f, 1f, frameImage.color.a);
        }
        if (useVizData)
        {
            if (item != null)
            {
                if (item is EquippableWeapon weapon && weapon.usesAmmunition)
                {
                    //number.text = gun.ammoCurrent + "/" + weapon.GetAmmunitionRemaining();
                    //number.text = weapon.GetAmmunitionRemaining().ToString();
                    //number.gameObject.SetActive(true);
                    int quantity = weapon.GetAmmunitionRemaining();
                    if (vizData.quantity != quantity)
                    {
                        vizData.quantity = quantity;
                        UpdateFromVizdata();
                    }

                }
                else
                {
                    vizData.quantity = -1;
                    //number.gameObject.SetActive(false);
                }
            }
            
        }
        else
        {
            if (item != null && item is EquippableWeapon weapon && weapon.usesAmmunition)
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
        clock = flashTime;
    }
}
