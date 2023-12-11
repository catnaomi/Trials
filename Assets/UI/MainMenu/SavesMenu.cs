using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SavesMenu : MenuView
{
    [Header("Prefabs")]
    public GameObject saveControllerPrefab;
    public GameObject sceneLoaderPrefab;
    [Header("References")]
    public StartMenu startMenu;
    public SaveDataDisplay[] displays;
    CanvasGroupFader groupFade;
    

    // Start is called before the first frame update
    public override void MenuStart()
    {
        groupFade = this.GetComponent<CanvasGroupFader>();
        groupFade.Alpha = 0;
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
        var saves = SaveDataController.GetSaveDatas(3);
        if (SaveDataController.instance == null)
        {
            Instantiate(saveControllerPrefab);
        }
        if (SceneLoader.instance == null)
        {
            Instantiate(sceneLoaderPrefab);
        }
        

        for (int i = 0; i < displays.Length; i++)
        {
            displays[i].SetSaveData(i, saves[i]);
            displays[i].SetMenuReference(this);
        }
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
        startMenu.Focus();
    }

    public void OnCancel(BaseEventData eventData)
    {
        if (focused)
        FadeOut();
    }
}
