using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayPassEvent : MonoBehaviour
{
    public float delay;
    float clock;
    public UnityEvent OnDelayComplete;

    public int repeatEventsRequired = 0;
    [SerializeField, ReadOnly] int eventsPassed;
    // Update is called once per frame
    void Update()
    {
        if (clock > 0f)
        {
            clock -= Time.deltaTime;
            if (clock <= 0f)
            {
                OnDelayComplete.Invoke();
            }
        }
    }

    public void DelayEvent()
    {
        if (repeatEventsRequired > 0)
        {
            eventsPassed++;
            if (eventsPassed < repeatEventsRequired)
            {
                return;
            }
        }
        clock = delay;
    }
}