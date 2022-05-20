using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasicTutorialItem : MonoBehaviour
{
    public Sprite button1;
    public Sprite button2;
    public Sprite button3;
    public string text;
    [Space(15)]
    public TMP_Text textbox;
    public Image icon1;
    public Image icon2;
    public Image icon3;
    public CanvasGroup group;
    public float timeToExpire = 30f;
    public float fadeStartTime = 5f;
    public float fadeInTime = 0.25f;
    float alpha;
    public float clock;
    // Start is called before the first frame update
    void Start()
    {
        //PopulateTutorial();
    }

    // Update is called once per frame
    void OnGUI()
    {
        if (clock > 0f)
        {
            clock -= Time.deltaTime;
        }
        if (clock >= timeToExpire - fadeInTime)
        {
            alpha = Mathf.Clamp01((timeToExpire - clock) / fadeInTime);
        }
        else
        {
            alpha = Mathf.Clamp01(clock / fadeStartTime);
        }
        
        group.alpha = alpha;
    }

    public void PopulateTutorial()
    {
        clock = timeToExpire;
        if (button1 != null)
        {
            icon1.gameObject.SetActive(true);
            icon1.sprite = button1;
        }
        else
        {
            icon1.gameObject.SetActive(false);
        }
        if (button2 != null)
        {
            icon2.gameObject.SetActive(true);
            icon2.sprite = button2;
        }
        else
        {
            icon2.gameObject.SetActive(false);
        }
        if (button3 != null)
        {
            icon3.gameObject.SetActive(true);
            icon3.sprite = button3;
        }
        else
        {
            icon3.gameObject.SetActive(false);
        }
        textbox.text = text;
    }
}
