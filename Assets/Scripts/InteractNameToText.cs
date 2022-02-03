using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractNameToText : MonoBehaviour
{
    public TMP_Text text;
    public string buttonName;
    PlayerInput input;

    private void Start()
    {
        input = FindObjectOfType<PlayerInput>();
        PlayerActor.player.onControlsChanged.AddListener(UpdateButtonName);
        UpdateButtonName();
    }


    public void UpdateButtonName()
    {
        PlayerInput input = FindObjectOfType<PlayerInput>();

        UnityEngine.InputSystem.InputAction action = input.actions["Interact"];
        int index = InputActionRebindingExtensions.GetBindingIndex(action, InputBinding.MaskByGroup(input.currentControlScheme));
        string buttonName = InputActionRebindingExtensions.GetBindingDisplayString(action, index, InputBinding.DisplayStringOptions.DontIncludeInteractions);
        text.text = buttonName;
        Debug.Log("current control scheme [ " + input.currentControlScheme + " ]");
        Debug.Log("new button name for interact:  [ " + buttonName + " ]");
    }
}
