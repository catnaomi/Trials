using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Yarn.Unity;

public class CancellableOptionsListView : OptionsListView
{
    OptionView[] views;
    [Header("Last Option Components")]
    public GameObject lastOptionContainer;
    public LineView lastOptionView;
    public override void RunOptions(DialogueOption[] dialogueOptions, Action<int> onOptionSelected)
    {
        HideLastOption();
        void OptionSelected(int option)
        {
            SetLastOption(dialogueOptions[option]);
            onOptionSelected.Invoke(option);
        }
        base.RunOptions(dialogueOptions, OptionSelected);
        views = GetComponentsInChildren<OptionView>();
    }

    public void SetLastOption(DialogueOption option)
    {
        lastOptionContainer.SetActive(true);
        lastOptionView.RunLine(option.Line, OnLastOptionComplete);
    }

    public void HideLastOption()
    {
        lastOptionView.DialogueComplete();
    }
    void OnLastOptionComplete()
    {
        // do nothing
    }

    public override void DialogueComplete()
    {
        lastOptionView.DialogueComplete();
        base.DialogueComplete();
    }


    protected virtual void OnGUI()
    {
        if (views != null && views.Length > 0)
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(views[0].gameObject);
            }
        }
    }

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
