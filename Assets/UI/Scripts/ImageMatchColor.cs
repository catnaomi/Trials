using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class ImageMatchColor : MonoBehaviour
{
    public Graphic match;
    public Selectable selectMatch;
    Graphic target;
    private void Start()
    {
        target = this.GetComponent<Graphic>();
    }
    private void OnGUI()
    {
        if (match != null)
        {
            target.color = match.color;
            if (selectMatch != null)
            {
                target.color *= selectMatch.GetCurrentColor();
            }
        }
    }
}
