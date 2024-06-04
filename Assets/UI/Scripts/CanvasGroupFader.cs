using System.Collections;
using UnityEngine;
using CustomUtilities;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFader : MonoBehaviour
{
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;
    public float Alpha
    {
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
        group = GetComponent<CanvasGroup>();
    }

    public void FadeIn(System.Action callback = null)
    {
        group.interactable = true;
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = this.StartRenderTimer(fadeInDuration, (elapsedFractional) => Alpha = elapsedFractional, callback);
    }

    public void FadeOut(System.Action callback = null)
    {
        group.interactable = false;
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = this.StartRenderTimer(fadeInDuration, (elapsedFractional) => Alpha = 1.0f - elapsedFractional, callback);
    }

    public void Hide()
    {
        group.interactable = false;
        Alpha = 0f;
    }
}
