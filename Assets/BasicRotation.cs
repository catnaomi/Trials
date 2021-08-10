﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicRotation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            this.transform.root.Rotate(Vector3.up, 180f * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            this.transform.root.Rotate(Vector3.up, -180f * Time.deltaTime, Space.World);
        }*/
        Vector2 stick = Gamepad.current.rightStick.ReadValue();
        this.transform.root.Rotate(Camera.main.transform.up, stick.x * 180f * Time.deltaTime, Space.World);
        this.transform.root.Rotate(Camera.main.transform.right, stick.y * 180f * Time.deltaTime, Space.World);
    }
}
