using System.Collections;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;

    public CanvasGroup pausePanel;
    public float pauseMenuAlpha;
    public float pauseFadeInTime;
    public bool paused;

    public void Awake()
    {
        instance = this;
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0.0f;
        StartCoroutine(FadeInPauseMenu(Time.time));
    }

    public void Unpause()
    {
        paused = false;
        pausePanel.alpha = 0.0f;
        Time.timeScale = 1.0f;
    }

    public void TogglePause()
    {
        if (paused)
        {
            Unpause();
        }
        else
        {
            Pause();
        }
    }

    IEnumerator FadeInPauseMenu(float startTime)
    {
        while (true)
        {
            var elapsed = (Time.time - startTime) / pauseFadeInTime;
            if (elapsed >= 1.0f)
            {
                pausePanel.alpha = pauseMenuAlpha;
                break;
            }

            // Linear fade
            pausePanel.alpha = elapsed * pauseMenuAlpha;
            yield return new WaitForEndOfFrame();
        }
    }
}
