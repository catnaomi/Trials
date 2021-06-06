using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickSheatheIndicator : MonoBehaviour
{
    Shadow shadow;
    Image image;
    public float alpha = 0f;
    // Start is called before the first frame update
    void Start()
    {
        image = this.GetComponent<Image>();
        shadow = this.GetComponent<Shadow>();
    }
    private void OnGUI()
    {
        if (alpha > 0f)
        {
            alpha -= Time.deltaTime;
            Color c = new Color(1f, 1f, 1f, alpha);
            image.color = c;
            shadow.effectColor = c;
        }
    }

    public void Flare()
    {
        alpha = 1f;
    }
}
