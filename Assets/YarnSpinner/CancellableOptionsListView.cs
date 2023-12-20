using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Yarn.Unity;

public class CancellableOptionsListView : OptionsListView
{
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
