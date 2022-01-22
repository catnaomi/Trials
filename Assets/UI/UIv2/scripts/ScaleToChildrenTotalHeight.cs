using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScaleToChildrenTotalHeight : MonoBehaviour
{
    public Transform parent;
    public RectTransform rectTransform;
    public bool run;
    public float offset = 0f;
    [SerializeField, ReadOnly] private float height;

    void OnGUI()
    {
        if (!run || parent == null) return;
        height = 0;
        foreach (Transform child in parent)
        {
            RectTransform rect = child.GetComponent<RectTransform>();
            height += rect.rect.height;
        }
        if (rectTransform == null) return;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height + offset);
    }
}
