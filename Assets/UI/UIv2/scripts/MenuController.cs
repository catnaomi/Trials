using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public static MenuController menu;

    public GameObject[] categories;
    public int current = 0;
    public bool showing = false;

    public const int Inventory = 0;
    public const int Dialogue = 1;
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
        SetPlayerMenuOpen(true);
    }

    public void HideMenu()
    {
        for (int i = 0; i < categories.Length; i++)
        {
            categories[i].SetActive(false);
        }
        showing = false;
        SetPlayerMenuOpen(false);
    }

    public void OpenMenu(int index)
    {
        current = index;
        ShowMenu();
    }
    public void SetPlayerMenuOpen(bool open)
    {
        if (PlayerActor.player == null) return;
        PlayerActor.player.isMenuOpen = open;
    }
}
