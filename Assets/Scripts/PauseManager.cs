using System.Collections;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;

    public CanvasGroup pausePanel;
    public float pauseFadeInTime;

    bool pauseMenuOpen = false;

    public void Awake()
    {
        instance = this;
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

    IEnumerator FadeInPauseMenu(float startTime)
    {
        while (true)
        {
            var elapsed = (Time.time - startTime) / pauseFadeInTime;
            if (elapsed >= 1f)
            {
                pausePanel.alpha = 1f;
                break;
            }

            // Linear fade
            pausePanel.alpha = elapsed;
            yield return new WaitForEndOfFrame();
        }
    }
}
