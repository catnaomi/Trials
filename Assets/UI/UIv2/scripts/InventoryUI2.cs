using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI2 : MonoBehaviour
{
    public GameObject viewport;
    public GameObject itemTemplate;
    public Sprite imageGeneric;
    public GameObject source;

    IInventory inventory;

    List<InventoryItem> items;

    [Header("UI Info")]
    public float itemWidth = 191;
    public float itemHeight = 191;
    public int columns = 4;

    // Start is called before the first frame update
    void Start()
    {
        items = new List<InventoryItem>();
        if (source != null)
        {
            inventory = source.GetComponent<IInventory>();
            Populate();
        }
    }

    public void Populate()
    {
        foreach(InventoryItem displayItem in items)
        {
            GameObject.Destroy(displayItem);
        }
        items.Clear();

        List<Item> contents = inventory.GetContents();
        for (int i = 0; i < contents.Count; i++)
        {
            GameObject displayObj = GameObject.Instantiate(itemTemplate, viewport.transform);
            displayObj.SetActive(true);
            InventoryItem displayItem = displayObj.GetComponent<InventoryItem>();

            int x = i % columns;
            int y = i / columns;

            displayObj.transform.Translate(x * itemWidth, y * -itemHeight, 0);

            displayItem.SetItem(contents[i]);
            items.Add(displayItem);
        }
        ((RectTransform)viewport.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Ceil((float)contents.Count / (float)columns) * itemHeight);
        EventSystem.current.SetSelectedGameObject(items[0].gameObject);
    }
}
