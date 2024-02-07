using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputNameTesting : MonoBehaviour
{
    [TextArea(2,20)]
    public string output;
    public bool go;
    public string inputName;
    public Sprite sprite;
    public bool getSprite;
    // Update is called once per frame
    void Update()
    {
        if (go || Gamepad.current.selectButton.wasPressedThisFrame)
        {
            go = false;
            Test();
        }
        if (getSprite || Gamepad.current.selectButton.wasPressedThisFrame)
        {
            getSprite = false;
            sprite = InputSpriteProvider.GetSprite(inputName);
        }
    }

    public void Test()
    {
        try
        {
            PlayerInput input = FindObjectOfType<PlayerInput>();

            StringBuilder sb = new StringBuilder();

            foreach (var action in input.actions)
            {
                int index = InputActionRebindingExtensions.GetBindingIndex(action, InputBinding.MaskByGroup(input.currentControlScheme));
                if (index < 0) continue;
                string buttonName = InputActionRebindingExtensions.GetBindingDisplayString(action, index, InputBinding.DisplayStringOptions.DontIncludeInteractions);
                sb.AppendLine("Action: " + action.name + " | Button: " + buttonName);
            }
            //UnityEngine.InputSystem.InputAction action = input.actions[actionName];
            
            output = sb.ToString();
            //Debug.Log("current control scheme [ " + input.currentControlScheme + " ]");
            //Debug.Log("new button name for interact:  [ " + buttonName + " ]");
        }
        catch (System.ArgumentOutOfRangeException ex)
        {
            Debug.LogError(ex);
            output = ex.ToString();
        }
    }
}
