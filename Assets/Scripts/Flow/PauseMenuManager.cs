using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuManager : MenuView
{
    public static PauseMenuManager instance;

    public float pauseFadeInTime;
    
    CanvasGroupFader fader;
    GameObject[] buttons;
    bool pauseMenuOpen = false;

    public void Awake()
    {
        instance = this;
        fader = GetComponent<CanvasGroupFader>();
        buttons = GetComponentsInChildren<Button>().Select(button => button.gameObject).ToArray();
    }

    public override void MenuStart()
    {
        base.MenuStart();
        fader.Hide();
    }

    public override void Focus()
    {
        base.Focus();
        fader.FadeIn();
    }

    public override void Unfocus()
    {
        base.Focus();
        fader.FadeOut();
    }

    void OnGUI()
    {
        if (focused)
        {
            if (EventSystem.current.currentSelectedGameObject == null || !buttons.Contains(EventSystem.current.currentSelectedGameObject))
            {
                EventSystem.current.SetSelectedGameObject(buttons[0]);
            }
        }
    }

    public void TogglePauseMenu()
    {
        if (pauseMenuOpen)
        {
            ClosePauseMenu();
        }
        else
        {
            OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        pauseMenuOpen = true;
        TimeScaleController.instance.paused = true;
        Focus();
    }

    public void ClosePauseMenu()
    {
        pauseMenuOpen = false;
        Unfocus();
        TimeScaleController.instance.paused = false;
    }

    public static void Quit()
    {
        Application.Quit();
    }
}
