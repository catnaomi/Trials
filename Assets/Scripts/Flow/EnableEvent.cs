using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnableEvent : MonoBehaviour
{
    bool started;

    public UnityEvent Enable;
    // Start is called before the first frame update
    void Start()
    {
        started = true;
    }

    private void OnEnable()
    {
        if (started)
        {
            Enable.Invoke();
        }
    }
}