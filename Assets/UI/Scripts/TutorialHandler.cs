using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    static TutorialHandler instance;
    public InteractionPrompt[] prompts;
    
    AudioSource source;
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
    }
    public int ShowTutorial(string text)
    {
        for (int i = 0; i < prompts.Length; i++)
        {
            if (!prompts[i].gameObject.activeInHierarchy)
            {
                prompts[i].gameObject.SetActive(true);
                prompts[i].SetText(text);
                source.Play();
                return i;
            }
        }
        return -1;
    }

    public void HideTutorial(int index)
    {
        if (index >= 0 && index < prompts.Length)
        {
            prompts[index].gameObject.SetActive(false);
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
}
