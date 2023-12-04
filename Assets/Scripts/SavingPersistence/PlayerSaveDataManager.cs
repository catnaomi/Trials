using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSaveDataManager : MonoBehaviour
{
    public static PlayerSaveDataManager instance;
    public bool debugShowInventoryString;
    [TextArea(5,20)]
    public string inventory;


    PlayerInventoryData inventoryData;
    PlayerAttributeData attributeData;
    bool inventoryChanged;
    bool attributesChanged;
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
            PlayerActor.player.GetComponent<ActorAttributes>().OnHealthChange.AddListener(MarkAttributeChange);
        }
        if (TimeTravelController.time != null)
        {
            TimeTravelController.time.OnChargeChanged.AddListener(MarkAttributeChange);
        }
        MarkInventoryChange();
        MarkAttributeChange();
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
            inventoryChanged = false;
            SaveInventoryData();
        }
        if (attributesChanged)
        {
            attributesChanged = false;
            SaveAttributeData();
        }
    }

    void MarkInventoryChange()
    {
        inventoryChanged = true;
    }

    void MarkAttributeChange()
    {
        attributesChanged = true;
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
        if (debugShowInventoryString) inventory = JsonUtility.ToJson(inventoryData);
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

    public void SaveAttributeData()
    {
        if (PlayerActor.player == null || TimeTravelController.time == null) return;
        if (attributeData == null)
        {
            attributeData = new PlayerAttributeData();
        }

        attributeData.GetAttributeData(TimeTravelController.time, PlayerActor.player.GetComponent<ActorAttributes>());

    }

    public static PlayerAttributeData GetAttributeData()
    {
        if (instance != null && instance.attributeData != null)
        {
            return instance.attributeData;
        }
        return null;
    }
}
