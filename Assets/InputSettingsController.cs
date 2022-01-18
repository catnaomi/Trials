using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSettingsController : MonoBehaviour
{
    public PlayerInput playerInput;
    [ReadOnly] public string currentControlScheme;

    public CinemachineFreeLook free_gamepad;
    public CinemachineFreeLook free_mouse;
    void Start()
    {
        if (playerInput == null)
        {
            playerInput = PlayerActor.player.GetComponent<PlayerInput>();
        }
        if (playerInput != null)
        {
            currentControlScheme = playerInput.currentControlScheme;
            SetCameraBasedOnControlScheme(currentControlScheme);
        }
        
    }

    public void SetCameraBasedOnControlScheme(string scheme)
    {
        switch(scheme)
        {
            case "Keyboard":
                PlayerActor.player.vcam.free = free_mouse;
                break;
            default:
            case "Gamepad":
                PlayerActor.player.vcam.free = free_gamepad;
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
