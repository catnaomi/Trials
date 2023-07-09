using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnTimeStopListener : MonoBehaviour
{
    public UnityEvent OnTimeStop;
    public float delay = 0f;
    // Start is called before the first frame update
    void Start()
    {
        TimeTravelController.time.OnTimeStopStart.AddListener(TimeStopEvent);
    }

    public void TimeStopEvent()
    {
        if (delay <= 0)
        {
            OnTimeStop.Invoke();
        }
        else
        {
            StartCoroutine(TimeStopDelayEvent());
        }
        
    }

    IEnumerator TimeStopDelayEvent()
    {
        yield return new WaitForSecondsRealtime(delay);
        if (TimeTravelController.time.IsFreezing() && this.enabled && this.gameObject.activeSelf)
        {
            OnTimeStop.Invoke();
        }
    }
}
