using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveDataDisplay : MonoBehaviour, ICancelHandler
{
    public int slot;
    public bool isDeleteButton;
    [Header("UI References")]
    public TMP_Text previewText;
    public Button uiButton;
    SavesMenu saveMenu;
    bool updateOnEndOfFrame;
    [Header("Data Preview")]
    [SerializeField, ReadOnly] SaveData data;
    public SaveLoadKind saveLoadKind;

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
            previewText.text = $"Prologue, name: Antiquity, " +
                $"{data.playerWorldData.activeScene}," +
                $"hp: {data.playerAttributeData.health.current}/{data.playerAttributeData.health.max}," +
                $"charges {data.playerAttributeData.timeCharges.current}/{data.playerAttributeData.timeCharges.max}," +
                $"lives 3/3";
        }
        else
        {
            if (!isDeleteButton)
            {
                previewText.text = "New Game";
            }
            else
            {
                previewText.text = "Empty";
            }
        }
    }

    public void SaveLoadSlot()
    {
        if (saveLoadKind == SaveLoadKind.save)
        {
            SaveDataToSave();
        }
        else
        {
            LoadDataFromSave();
        }
    }

    public void LoadDataFromSave()
    {
        if (data != null && data.IsDataValid())
        {
            SaveDataController.instance.Load(slot);
        }
        else
        {
            NewGameOnSave();
        }
    }

    public void SaveDataToSave()
    {
        SaveDataController.instance.Save(slot);
    }

    public void NewGameOnSave()
    {
        SaveDataController.instance.NewGame();
    }

    public void DeleteSave()
    {
        SaveDataController.DeleteSlot(slot);
        data = null;
        UpdateUI();
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
        if (updateOnEndOfFrame)
        {
            UpdateUI();
        }
    }
}
