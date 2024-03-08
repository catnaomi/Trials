using UnityEngine;

public class PauseMenuManager : MenuView
{
    public static PauseMenuManager instance;

    public float pauseFadeInTime;
    
    CanvasGroup pausePanel;
    CanvasGroupFader fader;
    bool pauseMenuOpen = false;

    public void Awake()
    {
        instance = this;
        pausePanel = gameObject.GetComponent<CanvasGroup>();
        fader = gameObject.GetComponent<CanvasGroupFader>();
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
