using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Yarn.Unity;

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

    public UnityEvent OnMenuOpen;
    public UnityEvent OnMenuClose;

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
        if (current < 0)
        {
            HideMenu();
            return;
        }
        showing = true;
        inspectorShow = true;
        SetPlayerMenuOpen(true);
        Cursor.visible = true;
        UpdateMenus();
#if !UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Confined;
#endif
    }

    public void HideMenu()
    {
        showing = false;
        inspectorShow = false;
        SetPlayerMenuOpen(false);
        current = -1;
        UpdateMenus();
#if !UNITY_EDITOR
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
#endif
    }

    public void UpdateMenus()
    {
        for (int i = 0; i < categories.Length; i++)
        {
            if (i == Dialogue)
            {
                if (current == i)
                {
                    categories[i].GetComponent<CanvasGroup>().alpha = 1f;
                }
            }
            else
            {
                categories[i].SetActive(current == i);
            }
        }
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

    public void TryToggleInventory()
    {
        GameObject runnerObj = GameObject.FindGameObjectWithTag("DialogueRunner");

        if (runnerObj != null)
        {
            DialogueRunner runner = runnerObj.GetComponent<DialogueRunner>();
            if (runner.IsDialogueRunning)
            {
                Debug.Log("could not open inventory, dialogue is running");
                return;
            }
        }

        if (showing && current == Inventory)
        {
            HideMenu();
        }
        else if (!showing)
        {
            OpenMenu(Inventory);
        }
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
