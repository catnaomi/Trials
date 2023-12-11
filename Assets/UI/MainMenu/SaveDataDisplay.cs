using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SaveDataDisplay : MonoBehaviour, ICancelHandler
{
    public int slot;
    [Header("UI References")]
    public TMP_Text previewText;
    SavesMenu saveMenu;
    [Header("Data Preview")]
    [SerializeField, ReadOnly] SaveData data;

    public void SetMenuReference(SavesMenu menu)
    {
        saveMenu = menu;
    }
    public void SetSaveData(int slot, SaveData data)
    {
        this.slot = slot;
        this.data = data;
        UpdateUI();
    }
    public void UpdateUI()
    {
        if (data != null && data.IsDataValid())
        {
            previewText.text = $"Prologue, name: Antiquity, "+
                $"{data.playerWorldData.activeScene},"+
                $"hp: {data.playerAttributeData.health.current}/{data.playerAttributeData.health.max},"+
                $"charges {data.playerAttributeData.timeCharges.current}/{data.playerAttributeData.timeCharges.max},"+
                $"lives 3/3";
        }
        else
        {
            previewText.text = "New Game";
        }
    }

    public void LoadDataFromSave()
    {
        if (data != null && data.IsDataValid())
        {
            SaveDataController.SetSlotStatic(slot);
            SaveDataController.LoadSaveDataStatic(data);
        }
        else
        {
            NewGameOnSave();
        }
    }

    public void NewGameOnSave()
    {
        SaveDataController.SetSlotStatic(slot);
        SaveDataController.NewGameStatic();
    }

    public void OnCancel(BaseEventData eventData)
    {
        if (saveMenu != null)
        {
            saveMenu.OnCancel(eventData);
        }
    }

    void OnGUI()
    {
        if (saveMenu != null && EventSystem.current.currentSelectedGameObject == this.gameObject && !saveMenu.IsFocused)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
