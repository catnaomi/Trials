using CustomUtilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public enum SaveLoadKind
{
    save,
    load
}

public class SavesMenu : MenuView
{
    [Header("References")]
    public SaveDataDisplay[] displays;
    GameObject[] selectableChildren;
    public SaveLoadKind menuKind;

    public void SetMenuKind(SaveLoadKind kind)
    {
        menuKind = kind;
        SetChildrenSaveLoadKind();
    }

    public void SetChildrenSaveLoadKind()
    {
        foreach (var display in displays)
        {
            display.saveLoadKind = menuKind;
        }
    }

    public override void MenuStart()
    {
        selectableChildren = GetComponentsInChildren<Selectable>().Select(s => s.gameObject).ToArray();
        SetChildrenSaveLoadKind();
        base.MenuStart();
    }

    public override void Focus()
    {
        UpdateSlots();
        base.Focus();
    }

    public void UpdateSlots()
    {
        var saves = SaveDataController.GetSaveDatas(3);

        for (int i = 0; i < displays.Length; i++)
        {
            displays[i].SetSaveData(i, saves[i]);
            displays[i].SetMenuReference(this);
        }
    }

    void OnGUI()
    {
        if (IsFocused)
        {
            if (EventSystem.current.currentSelectedGameObject == null || !selectableChildren.Contains(EventSystem.current.currentSelectedGameObject))
            {
                EventSystem.current.SetSelectedGameObject(displays[0].gameObject);
            }
        }
    }

    public void OnCancel(BaseEventData eventData)
    {
        OnCancel();
    }

    public void OnCancel()
    {
        if (IsFocused)
        {
            PopMenu();
        }
    }
}
