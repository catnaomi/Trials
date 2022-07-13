using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropDeathListener : MonoBehaviour
{
    public NavigatingHumanoidActor targetListenForDeath;
    public Item itemToDrop;
    public Transform dropLocation;
    public int autoEquipToSlot = -1;
    public Interactable interact;
    public float MoveSpeed;
    public float RotationSpeed;
    bool spawned;
    LooseItem looseItem;
    // Start is called before the first frame update
    void Start()
    {
        interact.canInteract = false;
        interact.gameObject.SetActive(false);
        targetListenForDeath.OnDie.AddListener(OnDie);
        interact.OnInteract.AddListener(OnInteract);
    }

    // Update is called once per frame
    void Update()
    {
        if (spawned)
        {
            looseItem.transform.position = dropLocation.transform.position;
            looseItem.rigidbody.isKinematic = true;
            looseItem.transform.localRotation *= Quaternion.AngleAxis(RotationSpeed * Time.deltaTime, Vector3.up);
        }
    }

    public void OnDie()
    {
        interact.canInteract = true;
        looseItem = LooseItem.CreateLooseItem(itemToDrop);
        looseItem.canInteract = false;
        //looseItem.transform.position = targetListenForDeath.transform.position + Vector3.up * 0.5f;
        looseItem.transform.position = dropLocation.position;
        
        looseItem.transform.rotation = dropLocation.rotation;
        looseItem.transform.SetParent(dropLocation);
        spawned = true;
        interact.gameObject.SetActive(true);
    }

    public void OnInteract()
    {
        PlayerActor.player.inventory.Add(looseItem.item);
        if (autoEquipToSlot >= 0 && looseItem.item is EquippableWeapon equippableWeapon)
        {
            PlayerActor.player.inventory.EquipToSlot(equippableWeapon, autoEquipToSlot);
        }
        Destroy(dropLocation.gameObject);
    }
}
