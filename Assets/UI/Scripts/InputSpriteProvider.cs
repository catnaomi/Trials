using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSpriteProvider : MonoBehaviour
{
    public static InputSpriteProvider instance;
    public InputSpriteMap gamepad;
    PlayerInput input;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        input = FindObjectOfType<PlayerInput>();
    }
    
    public static Sprite GetSprite(UnityEngine.InputSystem.InputAction action)
    {
        if (instance != null && instance.gamepad != null)
        {
            return instance.gamepad.Get(GetInputString(action));
        }
        return null;
    }
    public static Sprite GetSprite(string s)
    {
        if (instance != null && instance.gamepad != null)
        {
            return instance.gamepad.Get(s);
        }
        return null;
    }

    public static string GetSpriteTMP(string inputName, InputBinding group)
    {
        if (group == InputBinding.MaskByGroup("Gamepad"))
        {
            return string.Format("<sprite=\"xbox_icons\" name=\"xbox_{0}\" tint=1>", inputName);
        }
        else
        {
            return inputName;
        }
    }

    public static string GetSpriteTMP(UnityEngine.InputSystem.InputAction action)
    {
        return GetSpriteTMP(GetInputString(action), InputBinding.MaskByGroup(instance.input.currentControlScheme));
    }

    public static string GetInputString(UnityEngine.InputSystem.InputAction action)
    {
        PlayerInput input = (instance != null && instance.input != null) ?
            instance.input :
            FindObjectOfType<PlayerInput>();
        int index = InputActionRebindingExtensions.GetBindingIndex(action, InputBinding.MaskByGroup(input.currentControlScheme));
        if (index < 0) return "";
        string buttonName = InputActionRebindingExtensions.GetBindingDisplayString(action, index, InputBinding.DisplayStringOptions.DontIncludeInteractions);
        return buttonName;
    }
}
