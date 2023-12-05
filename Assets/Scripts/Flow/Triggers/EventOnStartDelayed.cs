using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventOnStartDelayed : MonoBehaviour
{
    public UnityEvent OnStartEvent;
    public int delay = 0;
    int frames = 0;
    
    private void Start()
    {
        frames = 0;
    }

    private void Update()
    {
        if (frames == delay)
        {
            OnStartEvent.Invoke();
            frames++;
        }
        else
        {
            frames++;
        }
    }
}
