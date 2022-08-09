using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSettingsController : MonoBehaviour
{
    public PlayerInput playerInput;
    [ReadOnly] public string currentControlScheme;

    //public float gamepad2Mouse = 1.3f;
    //public float gamepad2MouseAim = 3f;

    public PlayerActor.VirtualCameras mouseVcams;
    public PlayerActor.VirtualCameras gamepadVcams;

    public float fov;

    Vector2 free_speedsMouse;
    Vector2 dialogue_speedsMouse;

    Vector2 aim_speedsGamepad;
    Vector2 aim_speedsMouse;
    //Vector2 climb_speeds;
    void Start()
    {
        if (playerInput == null)
        {
            playerInput = PlayerActor.player.GetComponent<PlayerInput>();
        }

        aim_speedsGamepad = new Vector2(((CinemachineFreeLook)gamepadVcams.aim).m_XAxis.m_MaxSpeed, ((CinemachineFreeLook)gamepadVcams.aim).m_YAxis.m_MaxSpeed);
        aim_speedsMouse = new Vector2(((CinemachineFreeLook)mouseVcams.aim).m_XAxis.m_MaxSpeed, ((CinemachineFreeLook)mouseVcams.aim).m_YAxis.m_MaxSpeed);

        free_speedsMouse = new Vector2(((CinemachineFreeLook)mouseVcams.free).m_XAxis.m_MaxSpeed, ((CinemachineFreeLook)mouseVcams.free).m_YAxis.m_MaxSpeed);
        dialogue_speedsMouse = new Vector2(((CinemachineFreeLook)mouseVcams.dialogue).m_XAxis.m_MaxSpeed, ((CinemachineFreeLook)mouseVcams.dialogue).m_YAxis.m_MaxSpeed);


        if (playerInput != null)
        {
            PlayerActor.player.onControlsChanged.AddListener(SetCameraBasedOnControlScheme);
            SetCameraBasedOnControlScheme();
            playerInput.actions["AllowMenuLook"].started += (c) =>
            {
                CheckLook();
            };
            playerInput.actions["AllowMenuLook"].canceled += (c) =>
            {
                CheckLook();
            };
        }
        
        if (TimeTravelController.time != null)
        {
            TimeTravelController.time.OnSlowTimeStart.AddListener(OnStartSlowTime);
            TimeTravelController.time.OnSlowTimeStop.AddListener(OnStopSlowTime);
        }

        fov = Camera.main.fieldOfView;

    }

    public void SetCameraBasedOnControlScheme()
    {
        currentControlScheme = playerInput.currentControlScheme;

        switch(currentControlScheme)
        {
            case "Keyboard":
                if (MenuController.menu != null)
                {
                    MenuController.menu.OnMenuOpen.AddListener(CheckLook);
                    MenuController.menu.OnMenuClose.AddListener(CheckLook);
                }
                SetVirtualCamerasActive(gamepadVcams, false);
                SetVirtualCamerasActive(mouseVcams, true);
                PlayerActor.player.vcam = mouseVcams;
                break;
            default:
            case "Gamepad":
                if (MenuController.menu != null)
                {
                    MenuController.menu.OnMenuOpen.RemoveListener(CheckLook);
                    MenuController.menu.OnMenuClose.RemoveListener(CheckLook);
                }
                SetVirtualCamerasActive(mouseVcams, false);
                SetVirtualCamerasActive(gamepadVcams, true);
                PlayerActor.player.vcam = gamepadVcams;
                break;
        }

    }

    public void CheckLook()
    {
        bool allowLook = playerInput.actions["AllowMenuLook"].IsPressed();
        if (currentControlScheme == "Keyboard" && !allowLook && PlayerActor.player.isMenuOpen)
        {
            ((CinemachineFreeLook)mouseVcams.dialogue).m_XAxis.m_MaxSpeed = 0f;
            ((CinemachineFreeLook)mouseVcams.dialogue).m_YAxis.m_MaxSpeed = 0f;
            ((CinemachineFreeLook)mouseVcams.free).m_XAxis.m_MaxSpeed = 0f;
            ((CinemachineFreeLook)mouseVcams.free).m_YAxis.m_MaxSpeed = 0f;
        }
        else
        {
            ((CinemachineFreeLook)mouseVcams.free).m_XAxis.m_MaxSpeed = free_speedsMouse.x;
            ((CinemachineFreeLook)mouseVcams.free).m_YAxis.m_MaxSpeed = free_speedsMouse.y;

            ((CinemachineFreeLook)mouseVcams.dialogue).m_XAxis.m_MaxSpeed = dialogue_speedsMouse.x;
            ((CinemachineFreeLook)mouseVcams.dialogue).m_YAxis.m_MaxSpeed = dialogue_speedsMouse.y;
            if (currentControlScheme == "Keyboard" && PlayerActor.player.isMenuOpen)
            {
                Cursor.visible = false;
            }
        }
    }

    public void AdjustAimCameraSpeed(bool slow)
    {
        float multiplier = slow ? (1f / TimeTravelController.time.timeSlowAmount) : 1f;
        Vector2 speeds = (currentControlScheme == "Keyboard") ? aim_speedsMouse : aim_speedsGamepad;
        PlayerActor.VirtualCameras vcam = (currentControlScheme == "Keyboard") ? mouseVcams : gamepadVcams;
        ((CinemachineFreeLook)vcam.aim).m_XAxis.m_MaxSpeed = speeds.x * multiplier;
        ((CinemachineFreeLook)vcam.aim).m_YAxis.m_MaxSpeed = speeds.y * multiplier;
    } 

    public void ChangeVerticalFOV(float f)
    {
        fov = f;
        Camera.main.fieldOfView = fov;
    }

    public void ChangeHorizontalFOV(float f)
    {
        fov = Camera.HorizontalToVerticalFieldOfView(f, Camera.main.aspect);
        Camera.main.fieldOfView = fov;
    }
    public void OnStartSlowTime()
    {
        AdjustAimCameraSpeed(true);
    }
    public void OnStopSlowTime()
    {
        AdjustAimCameraSpeed(false);
    }

    public void SetVirtualCamerasActive(PlayerActor.VirtualCameras vcams, bool active)
    {
        vcams.aim.gameObject.SetActive(active);
        vcams.climb.gameObject.SetActive(active);
        vcams.dialogue.gameObject.SetActive(active);
        vcams.free.gameObject.SetActive(active);
        vcams.target.gameObject.SetActive(active);
    }
}
