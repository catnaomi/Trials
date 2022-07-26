using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddToPlayerInventoryOnEvent : MonoBehaviour
{
    public Item itemToGive;
    public int autoEquipToSlot = -1;
    [ReadOnly, SerializeField] bool given = false;
    public void GiveItem()
    {
        if (!given && PlayerActor.player != null && itemToGive != null)
        {
            Item item = Instantiate(itemToGive);
            PlayerActor.player.inventory.Add(item);
            if (autoEquipToSlot >= 0 && item is Equippable equippable)
            {
                PlayerActor.player.inventory.EquipToSlot(equippable, autoEquipToSlot);
            }
            given = true;
        }
    }
}
