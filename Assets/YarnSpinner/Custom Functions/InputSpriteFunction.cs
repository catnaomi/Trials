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
                return InputSpriteProvider.GetSpriteTMP(action);
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
