using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryDescription : MonoBehaviour
{
    public TMP_Text item_desc;
    public TMP_Text item_name;

    public void SetItem(Item item)
    {
        item_desc.text = item.GetDescription();
        item_name.text = item.GetName();
    }
}
