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
        MenuView.StartMenuing(savesMenu, true);
        SaveDataController.instance.OnSaveComplete.AddListener(FinishSaving);
    }

    void FinishSaving()
    {
        savesMenu.UpdateSlots();
    }
}
