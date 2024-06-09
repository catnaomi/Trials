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
        group.blocksRaycasts = true;
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = this.StartRenderTimer(fadeInDuration, (elapsedFractional) => Alpha = elapsedFractional,
            () => {
                Show();
                callback?.Invoke();
            });
    }

    public void FadeOut(System.Action callback = null)
    {
        group.interactable = false;
        group.blocksRaycasts = false;
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = this.StartRenderTimer(fadeInDuration, (elapsedFractional) => Alpha = 1.0f - elapsedFractional,
            () => {
                Hide();
                callback?.Invoke();
            });
    }

    public void Hide()
    {
        group.interactable = false;
        Alpha = 0f;
        group.blocksRaycasts = false;
    }

    public void Show()
    {
        group.interactable = true;
        Alpha = 1f;
        group.blocksRaycasts = true;
    }
}
