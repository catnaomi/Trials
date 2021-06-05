using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public static MenuController menu;

    public GameObject[] categories;
    public int current = 0;
    public bool showing = false;
    private void OnEnable()
    {
        menu = this;
    }

    public void Start()
    {
        HideMenu();
    }

    public void ShowMenu()
    {
        for(int i = 0; i < categories.Length; i++)
        {
            categories[i].SetActive(i == current);
        }
        showing = true;
    }

    public void HideMenu()
    {
        for (int i = 0; i < categories.Length; i++)
        {
            categories[i].SetActive(false);
        }
        showing = false;
    }

}
