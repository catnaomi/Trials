using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickupUI : MonoBehaviour
{
    public GameObject itemPickupPrefab;
    public Transform itemPickupParent;
    bool initialized;
    AudioSource audioSource;
    Dictionary<string, ItemPickupDisplay> activeDisplayMap;
    // Start is called before the first frame update
    void Start()
    {
        initialized = false;
        audioSource = this.GetComponent<AudioSource>();
        activeDisplayMap = new Dictionary<string, ItemPickupDisplay>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!initialized && PlayerActor.player != null)
        {
            PlayerActor.player.inventory.OnAddItem.AddListener(OnNewItem);
            initialized = true;
        }
    }

    public void OnNewItem(Item item)
    {
        if (activeDisplayMap.TryGetValue(item.invID, out ItemPickupDisplay activeDisplay) && activeDisplayMap[item.invID] != null && item.MaxStackSize > 0)
        {
            activeDisplay.IncreaseNumberDisplay(item.Quantity);
            audioSource.Play();
        }
        else
        {
            GameObject uiObj = Instantiate(itemPickupPrefab, itemPickupParent);
            ItemPickupDisplay display = uiObj.GetComponent<ItemPickupDisplay>();
            uiObj.SetActive(true);
            display.SetItem(item);
            activeDisplayMap[item.invID] = display;
            audioSource.Play();
        }
        
    }
}
