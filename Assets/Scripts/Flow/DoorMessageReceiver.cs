using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DoorMessageReceiver : MonoBehaviour
{
    public UnityEvent OnOpen;
    
    public void Open()
    {
        OnOpen.Invoke();
    }
}
