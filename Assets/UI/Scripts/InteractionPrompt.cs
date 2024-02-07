using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPrompt : MonoBehaviour
{
    public string prompt;
    public TMP_Text promptText;
    public Image promptSprite;
    public CanvasGroup group;
    [Header("Animation")]
    public float easeTime = 1f;
    bool active;

    public void Start()
    {
        group.alpha = 0;
        this.gameObject.SetActive(false);
        if (easeTime <= 0)
        {
            easeTime = 1;
        }
    }
    public void SetText(string text)
    {
        prompt = text;
        
        active = true;
    }

    public void Set(string text, Sprite sprite)
    {
        this.promptText.text = text;
        this.promptSprite.sprite = sprite;
        this.promptSprite.enabled = sprite != null;
        active = true;
    }
    public void Hide()
    {
        active = false;
    }
    public bool IsActive()
    {
        return active;
    }

    public void OnGUI()
    {
        if (active)
        {
            group.alpha = Mathf.MoveTowards(group.alpha, 1, Time.deltaTime / easeTime);
        }
        else
        {
            group.alpha = Mathf.MoveTowards(group.alpha, 0, Time.deltaTime / easeTime);
            if (group.alpha <= 0)
            {
                this.gameObject.SetActive(false);
            }
        }
    }
}
