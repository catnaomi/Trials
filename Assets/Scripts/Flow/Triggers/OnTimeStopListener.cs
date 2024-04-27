using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class OnTimeStopListener : MonoBehaviour
{
    public UnityEvent OnTimeStop;
    public float delay = 0f;

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
