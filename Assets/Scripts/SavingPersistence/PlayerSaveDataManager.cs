using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSaveDataManager : MonoBehaviour
{
    public static PlayerSaveDataManager instance;
    [TextArea(5,20)]
    public string inventory;
    
    PlayerInventoryData inventoryData;
    bool inventoryChanged;
    public bool save;
    public bool load;
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    public void OnSceneStart()
    {
        if (PlayerActor.player != null)
        {
            PlayerActor.player.GetComponent<PlayerInventory>().OnChange.AddListener(MarkInventoryChange);
        }
    }
    private void Update()
    {
        if (save)
        {
            save = false;
            SaveData();
        }
        if (load)
        {
            load = false;
            LoadData();
        }
    }

    private void LateUpdate()
    {
        if (inventoryChanged)
        {
            SaveInventoryData();
            inventoryChanged = false;
        }
    }

    void MarkInventoryChange()
    {
        inventoryChanged = true;
    }

    public void SaveData()
    {
        SaveInventoryData();
    }

    public void LoadData()
    {
        LoadInventoryData();
    }

    public void SaveInventoryData()
    {
        if (PlayerActor.player == null) return;
        if (inventoryData == null)
        {
            inventoryData = new PlayerInventoryData();
        }
        inventoryData.CopyDataFromPlayerInventory(PlayerActor.player.GetComponent<PlayerInventory>());
        inventory = JsonUtility.ToJson(inventoryData);
    }

    public static bool HasInventoryData()
    {
        return instance != null && instance.inventoryData != null;
    }
    public static PlayerInventoryData GetInventoryData()
    {
        if (instance != null && instance.inventoryData != null)
        {
            return instance.inventoryData;
        }
        return null;
    }

    public void LoadInventoryData()
    {
        if (PlayerActor.player == null || inventory == "") return;
        inventoryData = PlayerInventoryData.GetDataFromJSON(inventory);
        if (PlayerActor.player.TryGetComponent<PlayerInventory>(out PlayerInventory inventoryComponent))
        {
            inventoryData.LoadDataToInventory(inventoryComponent);
        }
    }
}
