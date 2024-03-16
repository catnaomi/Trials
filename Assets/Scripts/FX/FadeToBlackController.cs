using UnityEngine;
using UnityEngine.UI;
using CustomUtilities;

public class FadeToBlackController : MonoBehaviour
{
    public static FadeToBlackController instance;
    public static readonly float DEFAULT_FADEIN = 3f;
    public static readonly float DEFAULT_FADEOUT = 2f;

    public bool FadeInOnStart;

    CanvasGroup group;
    public Image fadeGraphic;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        group = GetComponent<CanvasGroup>();
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
            if (duration <= 0)
            {
                Debug.LogWarning("Fade In Started With Zero Duration!");
            }
            instance.fadeGraphic.color = color;
            instance.StartRenderTimer(duration, (elapsedFractional) => instance.group.alpha = (1.0f - elapsedFractional), callback);
        }
    }

    public static void FadeOut(System.Action callback)
    {
        FadeOut(DEFAULT_FADEOUT, callback, Color.black);
    }
    public static void FadeOut(float duration)
    {
        FadeOut(duration, null, Color.black);
    }
    public static void FadeOut(float duration, System.Action callback, Color color)
    {
        if (instance != null)
        {
            if (duration <= 0)
            {
                Debug.LogWarning("Fade Out Started With Zero Duration!");
            }
            instance.fadeGraphic.color = color;
            instance.StartRenderTimer(duration, (elapsedFractional) => instance.group.alpha = elapsedFractional, callback);
        }
    }
}
