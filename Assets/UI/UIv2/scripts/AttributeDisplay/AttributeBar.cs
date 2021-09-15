using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttributeBar : AttributeDisplay
{
    public RectTransform display;
    public RectTransform spentDisplay;
    public RectTransform maxDisplay;
    public Text text;
    public float scale = 5f;
    public override void UpdateGUI()
    {
        if (attribute == null)
        {
            return;
        }

        display.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, attribute.current * scale);
        spentDisplay.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, smoothed * scale);
        maxDisplay.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, attribute.max * scale);
        text.text = Mathf.Floor(attribute.current).ToString();
    }
}
