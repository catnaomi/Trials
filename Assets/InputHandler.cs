using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomUtilities;
using System;
using UnityEngine.Events;

public class InputHandler : MonoBehaviour
{
    public static InputHandler main;

    private void Awake()
    {
        main = this;
    }
}
