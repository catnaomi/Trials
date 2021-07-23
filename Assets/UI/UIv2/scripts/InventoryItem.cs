using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : Selectable, ISelectHandler
{
    public Item item;
    public Sprite generic;
    public TMP_Text desc_text;
    public TMP_Text name_text;

    Image image;
    bool updateOnGUI;
    // Start is called before the first frame update
    void Start()
    {
        image = this.transform.GetChild(0).GetComponent<Image>();
        
    }

    void OnEnable()
    {
        //this.GetComponent<Button>().onClick.AddListener(StartEquip);
    }
    private void OnGUI()
    {
        if (updateOnGUI)
        {
            if (item.displayImage != null)
            {
                image.sprite = item.displayImage;
            }
            else
            {
                image.sprite = generic;
            }
            image.color = item.displayColor;
            updateOnGUI = false;
        }

        //Check if the GameObject is being highlighted
        if (IsHighlighted() || IsPressed() == true)
        {
            //UpdatePreview();
        }
    }
    public void SetItem(Item item)
    {
        this.item = item;
        updateOnGUI = true;
    }

    public override void OnSelect(BaseEventData eventData)
    {
        UpdatePreview();
        Transform content = this.transform.parent;
        RectTransform viewport = (RectTransform)content.parent.parent;

        int cnt = 1000;
        //while (this.transform.position.y + ((RectTransform)this.transform).rect.height < viewport.position.y - viewport.rect.height)
        //float diff = (this.transform.position.y - ((RectTransform)this.transform).rect.height/2) - (viewport.position.y - viewport.rect.height / 2);
        float diffBot = (this.transform.position.y - ((RectTransform)this.transform).rect.height) - (viewport.position.y);
        float diffTop = (this.transform.position.y) - (viewport.position.y + ((RectTransform)viewport).rect.height);
        //Debug.Log("dist - bottom: " + (diffBot) + " top: " + diffTop);
        if (diffBot < 0)
        {
            content.Translate(0, -diffBot, 0);
            
            cnt--;
            if (cnt <= 0)
            {
                Debug.Log("timeout");
                //break;
            }
        }
        else if (diffTop > 0)
        {
            content.Translate(0, -diffTop, 0);
        }
    }

    public void StartEquip()
    {
        InventoryUI2.invUI.StartQuickSlotEquip(item);
    }
    public void UpdatePreview()
    {
        if (item != null)
        {
            desc_text.text = item.GetDescription();
            name_text.text = item.GetName();
        }
    }
}
