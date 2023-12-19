using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SavesMenu : MenuView
{
    [Header("Prefabs")]
    public GameObject saveControllerPrefab;
    public GameObject sceneLoaderPrefab;
    [Header("References")]
    public MenuView previousView;
    public SaveDataDisplay[] displays;
    CanvasGroupFader groupFade;
    public UnityEvent OnCancelEvent;

    // Start is called before the first frame update
    public override void MenuStart()
    {
        groupFade = this.GetComponent<CanvasGroupFader>();
        groupFade.Alpha = 0;
        groupFade.SetInteractable(true);
        base.MenuStart();
    }

    public override void Focus()
    {
        base.Focus();
        Init();
        FadeIn();
    }

    public void Init()
    {
        
        if (SaveDataController.instance == null)
        {
            Instantiate(saveControllerPrefab);
        }
        if (SceneLoader.instance == null)
        {
            Instantiate(sceneLoaderPrefab);
        }

        UpdateSlots();
    }


    public void UpdateSlots()
    {
        var saves = SaveDataController.GetSaveDatas(3);


        for (int i = 0; i < displays.Length; i++)
        {
            displays[i].SetSaveData(i, saves[i]);
            displays[i].SetMenuReference(this);
        }
    }

    public SaveData GetData(int slot)
    {
        return SaveDataController.ReadSlot(slot);
    }

    void OnGUI()
    {
        if (focused)
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(displays[0].gameObject);
            }
        }
    }


    public void FadeIn()
    {
        groupFade.FadeIn();
    }

    public void FadeOut()
    {
        groupFade.FadeOut(ShowStartMenu);
    }
    void ShowStartMenu()
    {
        if (previousView != null)
        previousView.Focus();
    }


    public void OnCancel(BaseEventData eventData)
    {
        OnCancel();
    }

    public void OnCancel()
    {
        if (focused)
        {
            FadeOut();
            OnCancelEvent.Invoke();
        }
    }
}
