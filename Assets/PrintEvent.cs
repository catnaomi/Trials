using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintEvent : MonoBehaviour
{
    public string str;
    public void Log()
    {
        Debug.Log(str);
    }
}
