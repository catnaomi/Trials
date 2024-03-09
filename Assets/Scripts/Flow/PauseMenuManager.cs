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
        pauseMenuOpen = !pauseMenuOpen;
        if (pauseMenuOpen)
        {
            TimeScaleController.instance.paused = true;
            Focus();
        }
        else
        {
            Unfocus();
            TimeScaleController.instance.paused = false;
        }
    }
}
