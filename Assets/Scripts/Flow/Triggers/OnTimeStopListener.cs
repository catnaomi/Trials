using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnTimeStopListener : MonoBehaviour
{
    public UnityEvent OnTimeStop;
    // Start is called before the first frame update
    void Start()
    {
        TimeTravelController.time.OnTimeStopStart.AddListener(TimeStopEvent);
    }

    public void TimeStopEvent()
    {
        OnTimeStop.Invoke();
    }
}
