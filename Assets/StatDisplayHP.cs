using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatDisplayHP : MonoBehaviour
{
    public int healthyHealth;
    public int damagedHealth;

    Sprite[] healthySprites;
    Sprite[] damagedSprites;
    Sprite[] allSprites;

    public Image healthyImage;
    public Image damagedImage;
    // Start is called before the first frame update
    void Awake()
    {
        allSprites = Resources.LoadAll<Sprite>("UI/temp_sheet_health");

        healthySprites = new Sprite[]
        {
            null,
            allSprites[0],
            allSprites[1],
            allSprites[2]
        };

        damagedSprites = new Sprite[]
        {
            null,
            allSprites[3],
            allSprites[4],
            allSprites[5]
        };
    }

    private void OnGUI()
    {
        if (healthyHealth > 0)
        {
            healthyImage.sprite = healthySprites[healthyHealth];
            healthyImage.color = Color.white;
        }
        else
        {
            healthyImage.color = Color.clear;
        }

        if (damagedHealth > 0)
        {
            damagedImage.sprite = damagedSprites[damagedHealth];
            damagedImage.color = Color.white;
        }
        else
        {
            damagedImage.color = Color.clear;
        }
    }
}
