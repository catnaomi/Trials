using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class StartMenu : MenuView
{
    [Header("References")]
    public SavesMenu savesMenu;
    CanvasGroupFader fadeGroup;
    [Header("Visual Settings")]
    public float fadeInTime = 3f;
    public float fadeOutTime = 0.5f;

    public override void MenuStart()
    {
        fadeGroup = this.GetComponent<CanvasGroupFader>();
        fadeGroup.Alpha = 0f;
        base.MenuStart();
    }

    public override void Focus()
    {
        base.Focus();
        FadeIn();
        InputSystem.onAnyButtonPress.CallOnce(OnAnyInput);
    }

    public void OnAnyInput(InputControl c)
    {
        FadeOut();
    }

    public void FadeIn()
    {
        fadeGroup.FadeIn();
    }

    public void FadeOut()
    {
        fadeGroup.FadeOut(ShowSaveMenu);
    }
    void ShowSaveMenu()
    {
        savesMenu.Focus();
    }
}
