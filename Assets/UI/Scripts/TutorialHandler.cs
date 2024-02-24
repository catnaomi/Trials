using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialHandler : MonoBehaviour
{
    static TutorialHandler instance;
    public InteractionPrompt[] prompts;
    public CanvasGroup background;
    public float backgroundFadeInTime = 1f;
    AudioSource source;
    PlayerInput input;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        source = this.GetComponent<AudioSource>();
        foreach (InteractionPrompt prompt in prompts)
        {
            prompt.gameObject.SetActive(false);
        }
        input = FindObjectOfType<PlayerInput>();
    }

    private void OnGUI()
    {
        bool isTutorialActive = IsAnyTutorialActive();

        if (backgroundFadeInTime <= 0)
        {
            backgroundFadeInTime = 1;
        }
        background.alpha = Mathf.MoveTowards(background.alpha, isTutorialActive ? 1 : 0, Time.deltaTime / backgroundFadeInTime);
    }

    bool IsAnyTutorialActive()
    {
        foreach (InteractionPrompt prompt in prompts)
        {
            if (prompt.IsActive())
            {
                return true;
            }
        }
        return false;
    }
    public int ShowTutorial(string text)
    {
        int emptyIndex = -1;

        for (int i = 0; i < prompts.Length; i++)
        {
            if (!prompts[i].IsActive())
            {
                if (emptyIndex < 0)
                {
                    emptyIndex = i;
                }
            }
            else
            {
                if (prompts[i].prompt == text)
                {
                    return i;
                }
            }

        }
        if (emptyIndex >= 0)
        {
            prompts[emptyIndex].gameObject.SetActive(true);
            prompts[emptyIndex].Set(text);
            return emptyIndex;
        }
        return -1;
    }

    public void HideTutorial(int index)
    {
        if (index >= 0 && index < prompts.Length)
        {
            prompts[index].Hide();
        }
    }
    public void HideTutorial(string text)
    {
        foreach (InteractionPrompt prompt in prompts)
        {
            if (text == prompt.prompt)
            {
                prompt.Hide();
            }
        }
    }

    public void HideAll()
    {
        for (int i = 0; i < prompts.Length; i++)
        {
            prompts[i].Hide();
        }
    }
    public static int ShowTutorialStatic(string text)
    {
        if (instance == null) return -1;
        return instance.ShowTutorial(text);
    }

    public static void HideTutorialStatic(int index)
    {
        if (instance == null) return;
        instance.HideTutorial(index);
    }

    public static void HideTutorialStatic(string text)
    {
        if (instance == null) return;
        instance.HideTutorial(text);
    }

    public static void HideAllStatic()
    {
        if (instance == null) return;
        instance.HideAll();
    }

    public static string GetFullText(string text, params UnityEngine.InputSystem.InputAction[] actions)
    {
        StringBuilder sb = new StringBuilder();
        bool appendSpace = false;
        for (int i = 0; i < actions.Length; i++)
        {
            if (actions[i] == null) continue;
            if (i > 0)
            {
                sb.Append("+");
            }
            sb.Append(InputSpriteProvider.GetSpriteTMP(actions[i]));
            appendSpace = true;
        }
        if (appendSpace)
        {
            sb.Append(" ");
        }
        sb.Append(text);
        return sb.ToString();
    }
}
