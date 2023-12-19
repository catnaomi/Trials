using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CancelEventHandler : MonoBehaviour, ICancelHandler
{
    public UnityEvent OnCancelEvent;

    void ICancelHandler.OnCancel(BaseEventData eventData)
    {
        OnCancelEvent.Invoke();
    }
}
