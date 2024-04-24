using System;
using UnityEngine;

[CreateAssetMenu(fileName = "InputSpriteMap", menuName = "ScriptableObjects/UI/Create Input Sprite Map", order = 1)]
public class InputSpriteMap : ScriptableObject
{
    
    [SerializeField] InputSprite[] inputSprites;
    public Sprite defaultSprite;

    [Serializable]
    struct InputSprite
    {
        public string name;
        public Sprite sprite;
    }

    public Sprite Get(string s)
    {
        foreach (InputSprite inputSprite in inputSprites)
        {
            if (inputSprite.name == s)
            {
                return inputSprite.sprite;
            }
        }
        return defaultSprite;
    }
}
