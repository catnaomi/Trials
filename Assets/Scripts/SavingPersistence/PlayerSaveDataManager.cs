using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSaveDataManager : MonoBehaviour
{
    [TextArea(5,20)]
    public string inventory;
    public PlayerInventoryData inventoryData;
    public bool save;

    PlayerActor player;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (save)
        {
            save = false;
            SaveData();
        }
    }

    public void SaveData()
    {
        player = PlayerActor.player;
        SaveInventoryData();
    }

    public void SaveInventoryData()
    {
        if (player == null) return;
        inventoryData = PlayerInventoryData.GetDataFromPlayerInventory(player.GetComponent<PlayerInventory>());
        inventory = JsonUtility.ToJson(inventoryData);
    }
}
