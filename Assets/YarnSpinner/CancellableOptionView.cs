using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Yarn.Unity;

public class CancellableOptionView : OptionView, ICancelHandler
{
    public DialogueOptionIcon icons;
    public CancellableOptionsListView cancelHandler;

    protected virtual void OnGUI()
    {
        SetIconState();
    }
    public bool IsSilent()
    {
        return this.Option?.Line?.Metadata != null && this.Option.Line.Metadata.Length > 0 && this.Option.Line.Metadata[0] == "silent";
    }

    void SetIconState()
    {
        if (this.currentSelectionState == SelectionState.Selected)
        {
            icons.SetState(DialogueOptionIcon.State.Selected);
        }
        else
        {
            icons.SetState((IsSilent()) ? DialogueOptionIcon.State.Cancellable : DialogueOptionIcon.State.NotSelected);
        }
    }

    public void OnCancel(BaseEventData eventData)
    {
        if (IsSilent())
        {
            OnSubmit(eventData);
        }
        else if (cancelHandler != null)
        {
            cancelHandler.OnCancel(eventData, this);
        }
    }
}
