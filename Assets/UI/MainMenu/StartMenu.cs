using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

// TODO: rename MainMenu
public class StartMenu : MenuView
{
    [Header("References")]
    public SavesMenu savesMenu;

    public override void Focus()
    {
        base.Focus();
        InputSystem.onAnyButtonPress.CallOnce((InputControl _) =>
        {
            PushMenu(savesMenu);
        });
    }
}
