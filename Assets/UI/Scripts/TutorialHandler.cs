using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    static TutorialHandler instance;
    public BasicTutorialItem tutorial1;
    public BasicTutorialItem tutorial2;
    public BasicTutorialItem tutorial3;
    AudioSource source;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        source = this.GetComponent<AudioSource>();
    }
    public void ShowTutorial(Sprite icon1, Sprite icon2, Sprite icon3, string text)
    {
        BasicTutorialItem tutorial = tutorial1;
        float lastUpdateTime = instance.tutorial1.lastUpdateTime;
        if (instance.tutorial2.lastUpdateTime < lastUpdateTime)
        {
            tutorial = tutorial2;
            lastUpdateTime = instance.tutorial2.lastUpdateTime;
        }
        if (instance.tutorial3.lastUpdateTime < lastUpdateTime)
        {
            tutorial = tutorial3;
            lastUpdateTime = instance.tutorial3.lastUpdateTime;
        }
        tutorial.button1 = icon1;
        tutorial.button2 = icon2;
        tutorial.button3 = icon3;
        tutorial.text = text;
        tutorial.PopulateTutorial();
        source.Play();
    }
    public static void ShowTutorialStatic(Sprite icon1, Sprite icon2, Sprite icon3, string text)
    {
        if (instance == null) return;
        instance.ShowTutorial(icon1, icon2, icon3, text);
    }
}
