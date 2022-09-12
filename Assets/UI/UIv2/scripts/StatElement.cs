using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatElement : MonoBehaviour
{
    public DamageType type;
    public Image image;
    public TMP_Text text;

    [Space(20)]
    public Sprite earth;
    [Space(5)]
    public Sprite slash;
    public Sprite pierce;
    public Sprite blunt;
    [Space(10)]
    public Sprite light;
    public Sprite dark;
    [Space(10)]
    public Sprite fire;
    public Sprite water;
    public Sprite air;

    public void SetSprite()
    {
        if (type == DamageType.Earth)
        {
            image.sprite = earth;
            text.text = "Earth";
        }
        else if (type == DamageType.Slashing)
        {
            image.sprite = slash;
            text.text = "Slash";
        }
        else if (type == DamageType.Piercing)
        {
            image.sprite = pierce;
            text.text = "Pierce";
        }
        else if (type == DamageType.Blunt)
        {
            image.sprite = blunt;
            text.text = "Blunt";
        }
        else if (type == DamageType.Light)
        {
            image.sprite = light;
            text.text = "Light";
        }
        else if (type == DamageType.Dark)
        {
            image.sprite = dark;
            text.text = "Dark";
        }
        else if (type == DamageType.Fire)
        {
            image.sprite = fire;
            text.text = "Fire";
        }
        else if (type == DamageType.Water)
        {
            image.sprite = water;
            text.text = "Water";
        }
        else if (type == DamageType.Air)
        {
            image.sprite = air;
            text.text = "Wind";
        }
        else if (type == DamageType.Piercing)
        {
            image.sprite = null;
            text.text = "None";
        }
    }
}