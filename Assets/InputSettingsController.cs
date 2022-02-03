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
    Vector2 free_mouseSpeeds;
    public CinemachineFreeLook dialogue_mouse;
    Vector2 dialogue_mouseSpeeds;
    public CinemachineFreeLook dialogue_gamepad;
    void Start()
    {
        if (playerInput == null)
        {
            playerInput = PlayerActor.player.GetComponent<PlayerInput>();
        }
        if (playerInput != null)
        {
            PlayerActor.player.onControlsChanged.AddListener(SetCameraBasedOnControlScheme);
            SetCameraBasedOnControlScheme();
        }
        free_mouseSpeeds = new Vector2(free_mouse.m_XAxis.m_MaxSpeed, free_mouse.m_YAxis.m_MaxSpeed);
        dialogue_mouseSpeeds = new Vector2(dialogue_mouse.m_XAxis.m_MaxSpeed, dialogue_mouse.m_YAxis.m_MaxSpeed);
    }

    public void SetCameraBasedOnControlScheme()
    {
        currentControlScheme = playerInput.currentControlScheme;
        switch(currentControlScheme)
        {
            case "Keyboard":
                PlayerActor.player.vcam.free = free_mouse;
                free_mouse.gameObject.SetActive(true);
                free_gamepad.gameObject.SetActive(false);
                dialogue_gamepad.gameObject.SetActive(false);
                dialogue_mouse.gameObject.SetActive(true);
                break;
            default:
            case "Gamepad":
                PlayerActor.player.vcam.free = free_gamepad;
                free_mouse.gameObject.SetActive(false);
                free_gamepad.gameObject.SetActive(true);
                dialogue_gamepad.gameObject.SetActive(true);
                dialogue_mouse.gameObject.SetActive(false);
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
        if (currentControlScheme == "Keyboard")
        {
            float allowLook = 1f;
            if (PlayerActor.player.isMenuOpen)
            {
                allowLook = playerInput.actions["AllowMenuLook"].IsPressed() ? 1f : 0f;
                Cursor.visible = !playerInput.actions["AllowMenuLook"].IsPressed();
            }
            dialogue_mouse.m_XAxis.m_MaxSpeed = dialogue_mouseSpeeds.x * allowLook;
            dialogue_mouse.m_YAxis.m_MaxSpeed = dialogue_mouseSpeeds.y * allowLook;
            free_mouse.m_XAxis.m_MaxSpeed = free_mouseSpeeds.x * allowLook;
            free_mouse.m_YAxis.m_MaxSpeed = free_mouseSpeeds.y * allowLook;
        }
    }
}
