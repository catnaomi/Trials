using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AttributeSimple : AttributeDisplay
{
    public Text text;
    public bool showMax;
    public override void UpdateGUI()
    {
        if (attribute == null) return;
        string val = Mathf.FloorToInt(attribute.current).ToString();
        if (showMax)
        {
            val += "/" + Mathf.FloorToInt(attribute.max).ToString();
        }
        text.text = val;
    }
}