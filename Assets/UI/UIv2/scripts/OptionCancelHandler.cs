using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionCancelHandler : MonoBehaviour
{
    public void OnCancel(UnityEngine.EventSystems.BaseEventData eventData, CancellableOptionView option)
    {
       
        CancellableOptionView[] options = GetComponentsInChildren<CancellableOptionView>();

        foreach (CancellableOptionView o in options)
        {
            if (o.IsSilent())
            {
                o.OnCancel(eventData);
            }
        }
    }
}
