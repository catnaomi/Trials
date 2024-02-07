using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputNameToText : MonoBehaviour
{
    public TMP_Text text;
    public string actionName;
    public string buttonName;
    PlayerInput input;
    string defaultText;

    private void Start()
    {
        defaultText = text.text;
        input = FindObjectOfType<PlayerInput>();
        PlayerActor.player.onControlsChanged.AddListener(UpdateButtonName);
        UpdateButtonName();
    }


    public void UpdateButtonName()
    {
        try
        {
            PlayerInput input = FindObjectOfType<PlayerInput>();

            UnityEngine.InputSystem.InputAction action = input.actions[actionName];
            int index = InputActionRebindingExtensions.GetBindingIndex(action, InputBinding.MaskByGroup(input.currentControlScheme));
            string buttonName = InputActionRebindingExtensions.GetBindingDisplayString(action, index, InputBinding.DisplayStringOptions.DontIncludeInteractions);
            text.text = buttonName;
            //Debug.Log("current control scheme [ " + input.currentControlScheme + " ]");
            //Debug.Log("new button name for interact:  [ " + buttonName + " ]");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Debug.LogError(ex);
            text.text = defaultText;
        }
    }
}
