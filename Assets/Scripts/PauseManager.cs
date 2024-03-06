using System.Collections;
using UnityEngine;

public class PauseManager : MenuView
{
    public static PauseManager instance;

    public CanvasGroup pausePanel;
    public float pauseFadeInTime;
    
    CanvasGroupFader fader;
    bool pauseMenuOpen = false;

    public void Awake()
    {
        instance = this;
        fader = gameObject.GetComponent<CanvasGroupFader>();
    }

    public void TogglePauseMenu()
    {
        pauseMenuOpen = !pauseMenuOpen;
        if (pauseMenuOpen)
        {
            TimeScaleController.instance.paused = true;
            pausePanel.alpha = 1f;
            // StartCoroutine(FadeInPauseMenu(Time.time));
        }
        else
        {
            pausePanel.alpha = 0f;
            TimeScaleController.instance.paused = false;
        }
    }
}
