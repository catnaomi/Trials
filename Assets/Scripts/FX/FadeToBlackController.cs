using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeToBlackController : MonoBehaviour
{
    public static FadeToBlackController instance;
    public static readonly float DEFAULT_FADEIN = 3f;
    public static readonly float DEFAULT_FADEOUT = 2f;

    public bool FadeInOnStart;
    public AnimationCurve fadeInCurve = AnimationCurve.Linear(1f, 1f, 0f, 0f);
    public AnimationCurve fadeOutCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    CanvasGroup group;
    public Image fadeGraphic;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        group = this.GetComponent<CanvasGroup>();
        group.alpha = FadeInOnStart ? 1f : 0f;
        if (FadeInOnStart) FadeIn(DEFAULT_FADEIN);
    }

    public static void FadeIn(float duration)
    {
        FadeIn(duration, null, Color.black);
    }
    public static void FadeIn(float duration, System.Action callback, Color color)
    {
        if (instance != null)
        {
            instance.fadeGraphic.color = color;
            instance.StartCoroutine(instance.FadeInCoroutine(duration, callback));
        }
    }

    public static void FadeOut(float duration)
    {
        FadeOut(duration, null, Color.black);
    }

    public static void FadeOut(float duration, System.Action callback, Color color)
    {
        if (instance != null)
        {
            instance.fadeGraphic.color = color;
            instance.StartCoroutine(instance.FadeOutCoroutine(duration, callback));
        }
    }

    IEnumerator FadeInCoroutine(float duration, System.Action callback)
    {
        if (duration <= 0)
        {
            group.alpha = 0f;
            Debug.LogWarning("Fade In Started With Zero Duration!");
        }
        else
        {
            group.alpha = 1f;
            float clock = 0f;
            while (clock < duration)
            {
                yield return null;
                clock += Time.deltaTime;
                group.alpha = fadeInCurve.Evaluate(Mathf.Clamp01(clock / duration));
            }
            group.alpha = 0f;
        }
        callback?.Invoke();
    }

    IEnumerator FadeOutCoroutine(float duration, System.Action callback)
    {
        if (duration <= 0)
        {
            group.alpha = 1f;
            Debug.LogWarning("Fade Out Started With Zero Duration!");
        }
        else
        {
            group.alpha = 0f;
            float clock = 0f;
            while (clock < duration)
            {
                yield return null;
                clock += Time.deltaTime;
                group.alpha = fadeOutCurve.Evaluate(Mathf.Clamp01(clock / duration));
            }
            group.alpha = 1f;
        }
        callback?.Invoke();
    }
}
