using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSpriteProvider : MonoBehaviour
{
    public static InputSpriteProvider instance;
    public InputSpriteMap gamepad;

    private void Awake()
    {
        instance = this;
    }
    
    public static Sprite GetSprite(string s)
    {
        if (instance != null && instance.gamepad != null)
        {
            return instance.gamepad.Get(s);
        }
        return null;
    }
}
