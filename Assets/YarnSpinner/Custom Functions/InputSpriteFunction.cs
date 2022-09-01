using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

public class InputSpriteFunction : MonoBehaviour
{
    [YarnFunction("input_sprite")]
    public static string GetInputSprite(string inputAction)
    {
        PlayerInput inputs = FindObjectOfType<PlayerInput>();
        if (inputs == null)
        {
            if (PlayerActor.player != null)
            {
                inputs = PlayerActor.player.GetComponent<PlayerInput>();
            }
        }
        try
        {
            if (inputs != null)
            {
                var action = inputs.actions[inputAction];
                int index = InputActionRebindingExtensions.GetBindingIndex(action, InputBinding.MaskByGroup("Gamepad"));
                string path = action.bindings[index].path;
                if (path.Contains("Gamepad"))
                {
                    string[] split = path.Split("<Gamepad>/");
                    string inputName = split[1];
                    return string.Format("<sprite=\"xbox_icons\" name=\"xbox_{0}\" tint=1>", inputName);
                }
                else
                {
                    return action.bindings[index].ToDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
                }
                
            }
            else
            {
                return inputAction;
            }
        }
        catch (System.IndexOutOfRangeException ex)
        {
            Debug.LogError(ex);
            return inputAction;
        }
        //return inputAction;
    }
}
