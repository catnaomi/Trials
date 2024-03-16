using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveInteract : Interactable
{
    SavesMenu savesMenu;

    public override void Interact(PlayerActor player)
    {
        base.Interact(player);
        Save();
    }

    public void Save()
    {
        savesMenu = FindObjectOfType<SavesMenu>();
        if (savesMenu == null) return;
        savesMenu.Focus();

        TimeScaleController.instance.paused = true;
        SaveDataController.instance.OnSaveComplete.AddListener(FinishSaving);
        savesMenu.OnCancelEvent.AddListener(SaveCancelled);
    }

    void FinishSaving()
    {
        savesMenu.UpdateSlots();
    }

    void SaveCancelled()
    {
        SaveDataController.instance.OnSaveComplete.RemoveListener(FinishSaving);
        savesMenu.OnCancelEvent.RemoveListener(SaveCancelled);
        TimeScaleController.instance.paused = false;
    }
}
