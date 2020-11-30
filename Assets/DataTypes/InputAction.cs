using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class InputAction : ScriptableObject
{
    public string desc;

    public string GetDescription()
    {
        return desc;
    }
}
