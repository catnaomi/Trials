using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PerspectiveEventListener : MonoBehaviour
{
    public UnityEvent OnSnap;
    public void Snap()
    {
        OnSnap.Invoke();
    }
}
