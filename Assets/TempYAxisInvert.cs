using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TempYAxisInvert : MonoBehaviour
{

    public string key;
    PlayerInput input;
    CinemachineFreeLook cam;
    // Start is called before the first frame update
    void Start()
    {
        input = GameObject.FindObjectOfType<PlayerInput>();
        cam = this.GetComponent<CinemachineFreeLook>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.FindKeyOnCurrentKeyboardLayout(key).wasPressedThisFrame)
        {
            Debug.Log("inverted y axis on " + cam.ToString());
            cam.m_YAxis.m_InvertInput = !cam.m_YAxis.m_InvertInput;
        }
    }
}
