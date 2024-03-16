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
    public MenuView previousView;
    public SaveDataDisplay[] displays;
    CanvasGroupFader groupFade;
    GameObject[] selectChildren;
    public UnityEvent OnCancelEvent;

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
        groupFade = this.GetComponent<CanvasGroupFader>();
        groupFade.Hide();
        selectChildren = GetComponentsInChildren<Selectable>().Select(s => s.gameObject).ToArray();
        SetChildrenSaveLoadKind();
        base.MenuStart();
    }

    public override void Focus()
    {
        UpdateSlots();
        base.Focus();
        FadeIn();
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
        if (focused)
        {
            if (EventSystem.current.currentSelectedGameObject == null || !selectChildren.Contains(EventSystem.current.currentSelectedGameObject))
            {
                EventSystem.current.SetSelectedGameObject(displays[0].gameObject);
            }
        }
    }

    public void FadeIn()
    {
        groupFade.FadeIn();
    }

    public void FadeOut()
    {
        groupFade.FadeOut(ShowStartMenu);
    }

    void ShowStartMenu()
    {
        if (previousView != null)
        {
            previousView.Focus();
        }
    }

    public void OnCancel(BaseEventData eventData)
    {
        OnCancel();
    }

    public void OnCancel()
    {
        if (focused)
        {
            FadeOut();
            OnCancelEvent.Invoke();
        }
    }
}
