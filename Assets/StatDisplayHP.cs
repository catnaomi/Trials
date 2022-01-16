using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatDisplayHP : MonoBehaviour
{
    public int healthyHealth;
    public int damagedHealth;
    public int maxHealth;
    int lastHealth;
    public Animator[] hearts;
    [SerializeField,ReadOnly]private int[] heartValues;
    int heartCount;
    // Start is called before the first frame update
    void Start()
    {
        heartValues = new int[hearts.Length];
        UpdateHearts();
    }

    public void UpdateHearts()
    {
        int hp = healthyHealth;
        heartCount = (int)Mathf.CeilToInt(Mathf.Max(maxHealth / 3f, 1));
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i >= heartCount)
            {
                hearts[i].gameObject.SetActive(false);
            }
            else
            {
                hearts[i].gameObject.SetActive(true);
                if (hp > 3)
                {
                    heartValues[i] = 3;
                    hp -= 3;
                }
                else
                {
                    heartValues[i] = hp;
                    hp = 0;
                }
                hearts[i].SetInteger("value", heartValues[i]);
            }
        }
    }
    private void OnGUI()
    {
        if (healthyHealth != lastHealth)
        {
            lastHealth = healthyHealth;
            UpdateHearts();
        }
    }
}
