using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    public static MenuController menu;

    public GameObject[] categories;
    public int current = 0;
    public bool showing = false;

    public const int Inventory = 0;
    public const int Dialogue = 1;

    public bool inspectorShow;
    public List<string> itemsUnderCursor;

    GameObject lastSelected;
    private void OnEnable()
    {
        menu = this;
    }

    public void Start()
    {
        HideMenu();
    }

    public void OnGUI()
    {
        /*
        itemsUnderCursor.Clear();
        List<RaycastResult> results = RaycastMouse();
        foreach (RaycastResult result in results)
        {
            itemsUnderCursor.Add(result.gameObject.ToString());
        }
        */
        if (inspectorShow && !showing)
        {
            ShowMenu();
        }
        else if (!inspectorShow && showing)
        {
            HideMenu();
        }
    }

    public void ShowMenu()
    {
        for(int i = 0; i < categories.Length; i++)
        {
            categories[i].SetActive(i == current);
        }
        showing = true;
        inspectorShow = true;
        SetPlayerMenuOpen(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HideMenu()
    {
        for (int i = 0; i < categories.Length; i++)
        {
            categories[i].SetActive(false);
        }
        showing = false;
        inspectorShow = false;
        SetPlayerMenuOpen(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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
    public List<RaycastResult> RaycastMouse()
    {

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            pointerId = -1,
        };

        pointerData.position = Mouse.current.position.ReadValue();

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);


        //Debug.Log(results.Count);
        return results;
    }
}
