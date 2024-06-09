using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuManager : MenuView
{
    public static PauseMenuManager instance;

    public SavesMenu savesMenu;

    GameObject[] buttons;

    public void Awake()
    {
        instance = this;
        buttons = GetComponentsInChildren<Button>().Select(button => button.gameObject).ToArray();
    }

    public override void Focus()
    {
        base.Focus();
    }

    public override void Unfocus()
    {
        base.Unfocus();
    }

    void OnGUI()
    {
        if (IsFocused)
        {
            if (EventSystem.current.currentSelectedGameObject == null || !buttons.Contains(EventSystem.current.currentSelectedGameObject))
            {
                EventSystem.current.SetSelectedGameObject(buttons[0]);
            }
        }
    }

    public void Resume()
    {
        FinishMenuing();
    }

    public void Load()
    {
        savesMenu.SetMenuKind(SaveLoadKind.load);
        PushMenu(savesMenu);
    }

    public static void Quit()
    {
        Application.Quit();
    }

    public void OnCancel(BaseEventData eventData)
    {
        Resume();
    }
}
