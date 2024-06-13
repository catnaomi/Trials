using UnityEngine;
using System.Collections.Generic;
public class MenuView : MonoBehaviour
{
    public static MenuView currentlyFocused;
    static Stack<MenuView> menuStack = new();

    public bool IsFocused
    {
        get { return currentlyFocused == this; }
    }
    public static bool IsMenuActive
    {
        get { return menuStack.Count != 0; }
    }
    public bool autoStart;

    CanvasGroupFader fader;

    public static void OnPressPause()
    {
        if (IsMenuActive)
        {
            FinishMenuing();
        }
        else
        {
            StartMenuing(PauseMenuManager.instance, true);
        }
    }

    void Start()
    {
        fader = GetComponent<CanvasGroupFader>();
        MenuStart();
        if (autoStart)
        {
            StartMenuing(this, false);
        }
        else
        {
            fader.Hide();
        }
    }

    public static void StartMenuing(MenuView menu, bool shouldPause)
    {
        if (shouldPause)
        {
            TimeScaleController.instance.paused = true;
            MusicController.instance.paused = true;
        }

        PushMenu(menu);
    }

    public static void FinishMenuing()
    {
        while (menuStack.TryPop(out var menu))
        {
            menu.fader.Hide();
        }

        AfterMenuing();
    }

    public static void ClearMenuStack()
    {
        menuStack.Clear();
    }

    public static void AfterMenuing()
    {
        currentlyFocused = null;
        TimeScaleController.instance.paused = false;
        MusicController.instance.paused = false;
    }

    public static void PushMenu(MenuView menu)
    {
        System.Action AfterFadeCurrentMenu = delegate ()
        {
            currentlyFocused = menu;
            menuStack.Push(menu);
            menu.Focus();
        };

        if (currentlyFocused != null)
        {
            currentlyFocused.FadeOut(AfterFadeCurrentMenu);
        }
        else
        {
            AfterFadeCurrentMenu();
        }
    }

    public static void PopMenu()
    {
        var finishedMenu = menuStack.Pop();
        if (menuStack.TryPeek(out var newInFocusMenu))
        {
            currentlyFocused = newInFocusMenu;
            newInFocusMenu.Focus();
        }
        else
        {
            AfterMenuing();
        }
        finishedMenu.Unfocus();
    }

    public virtual void MenuStart()
    {
        // use this for initialization
    }

    public virtual void Focus()
    {
        if (currentlyFocused != null && currentlyFocused != this)
        {
            currentlyFocused.Unfocus();
        }
        currentlyFocused = this;
        FadeIn();
    }

    public virtual void Unfocus()
    {
        FadeOut();
    }

    public virtual void FadeIn(System.Action callback = null)
    {
        fader.FadeIn(callback);
    }

    public virtual void FadeOut(System.Action callback = null)
    {
        fader.FadeOut(callback);
    }
}
