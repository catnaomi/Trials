using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickupUI : MonoBehaviour
{
    public GameObject itemPickupPrefab;
    public Transform itemPickupParent;
    bool initialized;
    // Start is called before the first frame update
    void Start()
    {
        initialized = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!initialized && PlayerActor.player != null)
        {
            PlayerActor.player.inventory.OnAddItem.AddListener(OnNewItem);
            //initialized = true;
        }
    }

    public void OnNewItem(Item item)
    {
        GameObject uiObj = Instantiate(itemPickupPrefab, itemPickupParent);
        ItemPickupDisplay display = uiObj.GetComponent<ItemPickupDisplay>();
        uiObj.SetActive(true);
        display.SetItem(item);
    }
}
