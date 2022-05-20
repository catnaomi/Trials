using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    static TutorialHandler instance;
    public BasicTutorialItem tutorial1;
    public BasicTutorialItem tutorial2;
    public BasicTutorialItem tutorial3;
    private void Awake()
    {
        instance = this;
    }

    public void ShowTutorial(Sprite icon1, Sprite icon2, Sprite icon3, string text)
    {
        BasicTutorialItem tutorial = null;
        if (instance.tutorial1.group.alpha <= 0)
        {
            tutorial = tutorial1;
        }
        else if (instance.tutorial2.group.alpha <= 0)
        {
            tutorial = tutorial2;
        }
        else if (instance.tutorial3.group.alpha <= 0)
        {
            tutorial = tutorial3;
        }
        else
        {
            tutorial = tutorial1;
        }
        tutorial.button1 = icon1;
        tutorial.button2 = icon2;
        tutorial.button2 = icon3;
        tutorial.text = text;
        tutorial.PopulateTutorial();
    }
    public static void ShowTutorialStatic(Sprite icon1, Sprite icon2, Sprite icon3, string text)
    {
        if (instance == null) return;
        instance.ShowTutorial(icon1, icon2, icon3, text);
    }
}
