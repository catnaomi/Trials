using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractIcon : MonoBehaviour
{
    public Sprite icon;
    public Sprite iconSelected;
    SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        bool held = false;//Input.GetButton("Interact");
        Sprite targetIcon = (held ? iconSelected : icon);
        if (spriteRenderer.sprite != targetIcon)
        {
            spriteRenderer.sprite = targetIcon;
        }
    }
}
