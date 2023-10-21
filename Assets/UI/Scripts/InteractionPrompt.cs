using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    public string prompt;
    public TMP_Text promptText;


    public void SetText(string text)
    {
        prompt = text;
        this.promptText.text = prompt;
    }
}
