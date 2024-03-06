using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFader : MonoBehaviour
{
    public float fadeInDuration = 1f;
    public float fadeInDelay = 0f;
    public float fadeOutDuration = 1f;
    public float fadeOutDelay = 0f;
    public float Alpha {
        get
        {
            return (group != null) ? group.alpha : 0;
        }
        set
        {
            if (group != null)
            {
                group.alpha = value;
            }
        }
    }

    CanvasGroup group;
    Coroutine fadeRoutine;

    void Awake()
    {
        group = this.GetComponent<CanvasGroup>();
    }

    public void FadeIn(System.Action callback)
    {
        FadeCanvasGroup(group, 1f, callback, fadeInDuration, fadeInDelay);
    }

    public void FadeIn()
    {
        FadeIn(null);
    }

    public void FadeOut(System.Action callback)
    {
        FadeCanvasGroup(group, 0f, callback, fadeOutDuration, fadeOutDelay);
    }

    public void FadeOut()
    {
        FadeOut(null);
    }

    public void FadeTo(float target, System.Action callback)
    {
        FadeCanvasGroup(group, target, callback, (target > group.alpha) ? fadeInDuration : fadeOutDuration, (target > group.alpha) ? fadeInDelay : fadeOutDelay);
    }

    public void FadeTo(float target)
    {
        FadeTo(target, null);
    }

    public void Hide()
    {
        FadeCanvasGroup(group, 0, null, 0, 0);
    }

    public void Show()
    {
        FadeCanvasGroup(group, 1, null, 0, 0);
    }

    public void FadeCanvasGroup(CanvasGroup group, float target, System.Action callback, float duration = 1f, float delay = 0f)
    {
        if (duration < 0)
        {
            throw new System.InvalidOperationException("Fade duration must be a value greater than or equal to zero.");
        }
        else
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }
            fadeRoutine = StartCoroutine(FadeCanvasRoutine(group, target, callback, duration, delay));
        }
    }

    static IEnumerator FadeCanvasRoutine(CanvasGroup group, float target, System.Action callback, float duration, float delay)
    {
        float clock = 0f;
        float current = group.alpha;
        float realDuration = (duration + delay);
        group.interactable = false;
        while (clock < realDuration)
        {
            clock += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01((clock - delay) / duration);
            group.alpha = Mathf.Lerp(current, target, t);
            yield return null;
        }
        group.alpha = target;
        group.interactable = target > 0;
        callback?.Invoke();
    }
}
