using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//[ExecuteInEditMode]
public class ScaleBasedOnText : MonoBehaviour
{
    public RectTransform rectTransform;
    public TMP_Text text;
    public bool run;
    public float offset = 0f;
    public float minimum = 40f;
    [SerializeField, ReadOnly] private float height;

    // Update is called once per frame
    void OnGUI()
    {
        if (!run || text == null || rectTransform == null) return;
        height = text.renderedHeight;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(height + offset, minimum));
    }
}
