using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemPickupDisplay : MonoBehaviour
{
    public InventoryItemDisplay itemDisplay;
    public Item item;

    public TMP_Text itemNameText;
    public CanvasGroup group;
    public float timeToExpire = 30f;
    public float fadeStartTime = 5f;
    public float fadeInTime = 0.25f;
    float clock;
    float alpha;
    // Start is called before the first frame update
    void Start()
    {
        clock = timeToExpire;
    }

    // Update is called once per frame
    void Update()
    {
        if (clock > 0f)
        {
            clock -= Time.deltaTime;
        }
        else
        {
            Destroy(this.gameObject);
        }
        if (clock >= timeToExpire - fadeInTime)
        {
            alpha = Mathf.Clamp01((timeToExpire - clock) / fadeInTime);
        }
        else
        {
            alpha = Mathf.Clamp01(clock / fadeStartTime);
        }
        group.alpha = alpha;
    }

    public void SetItem(Item item)
    {
        this.item = item;
        itemDisplay.SetItem(item);
        itemNameText.text = item.GetName();
    }

    public void IncreaseNumberDisplay(int amount)
    {
        itemDisplay.number.text = (int.Parse(itemDisplay.number.text) + amount).ToString();
        if (clock < timeToExpire - fadeInTime)
        {
            clock = timeToExpire - fadeInTime;
        }
    }
}
