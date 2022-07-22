using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasGroupOnStart : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        CanvasGroup group = this.GetComponent<CanvasGroup>();
        if (group.alpha <= 0)
        {
            group.alpha = 1f;
        }
    }
}
