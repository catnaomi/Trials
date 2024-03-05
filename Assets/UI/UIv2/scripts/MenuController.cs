using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Yarn.Unity;

public class MenuController : MonoBehaviour
{
    public static MenuController menu;

    public GameObject[] categories;

    public int current = 0;
    public int lastMenu = 0;
    public bool showing = false;

    public const int Inventory = 0;
    public const int Journal = 1;

    public GameObject DialogueMenu;
    //public const int Dialogue = 1;

    public bool inspectorShow;
    public List<string> itemsUnderCursor;

    public InputActionReference nextPageAction;
    public InputActionReference previousPageAction;

    public UnityEvent OnMenuOpen;
    public UnityEvent OnMenuClose;

    public GameObject header;
    public TMP_Text headerText;
    public Button nextButton;
    public Button prevButton;
    GameObject lastSelected;
    private void OnEnable()
    {
        menu = this;
    }

    public void Start()
    {
        HideMenu();
        nextPageAction.action.performed += (c) =>
        {
            if (showing)
            {
                NextMenu();
            }
        };
        previousPageAction.action.performed += (c) =>
        {
            if (showing)
            {
                PreviousMenu();
            }
        };
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
        header.SetActive(true);
        Cursor.visible = true;
        //current = (lastMenu >= 0) ? lastMenu : 0;
        UpdateMenus();
        UpdateButtons();
#if !UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Confined;
#endif
    }

    public void HideMenu()
    {
        showing = false;
        inspectorShow = false;
        SetPlayerMenuOpen(false);
        header.SetActive(false);
        lastMenu = current;
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
            /*
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
            */
            if (i == Journal)
            {
                if (current == i && !JournalController.journal.showing)
                {
                    JournalController.journal.OpenJournal();
                }
                else if (JournalController.journal.showing)
                {
                    JournalController.journal.CloseJournal();
                }
            }
            else
            categories[i].SetActive(current == i);
        }
        UpdateButtons();
    }

    public void OpenMenu(int index)
    {
        current = index;
        ShowMenu();
    }

    public void OpenDialogue()
    {
        DialogueMenu.GetComponent<CanvasGroup>().alpha = 1f;
    }
    public void NextMenu()
    {
        if (current + 1 < categories.Length)
        {
            current++;
            current %= categories.Length;
            ShowMenu();
        }
        
    }

    public void PreviousMenu()
    {
        if (current - 1 >= 0)
        {
            current--;
            current %= categories.Length;
            ShowMenu();
        }
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
            if (runner.CheckDialogueRunning())
            {
                Debug.Log("could not open inventory, dialogue is running");
                return;
            }
        }

        if (showing)
        {
            HideMenu();
        }
        else if (!showing)
        {
            OpenMenu(GetMenuToOpen());
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

    public void UpdateButtons()
    {
        if (current >= 0)
        {
            if (current + 1 < categories.Length)
            {
                nextButton.GetComponentInChildren<TMP_Text>().text = GetStringFromIndex((current + 1) % categories.Length);
                nextButton.gameObject.SetActive(true);
            }
            else
            {
                nextButton.gameObject.SetActive(false);
            }

            if (current - 1 >= 0)
            {
                prevButton.GetComponentInChildren<TMP_Text>().text = GetStringFromIndex(current - 1 % categories.Length);
                prevButton.gameObject.SetActive(true);
            }
            else
            {
                prevButton.gameObject.SetActive(false);
            }            
            headerText.text = GetStringFromIndex(current);
            headerText.gameObject.SetActive(true);
        }
        else
        {
            nextButton.gameObject.SetActive(false);
            prevButton.gameObject.SetActive(false);
            headerText.gameObject.SetActive(false);
        }
    }

    public int GetMenuToOpen()
    {
        if (JournalController.journal != null && JournalController.journal.ShouldOpenToJournal())
        {
            return Journal;
        }
        else if (lastMenu >= 0)
        {
            return lastMenu;
        }
        else
        {
            return Inventory;
        }
    }
    string GetStringFromIndex(int index)
    {
        if (index == Inventory)
        {
            return "Inventory";
        }
        else if (index == Journal)
        {
            return "Journal";
        }
        else
        {
            return "";
        }
    }
}
