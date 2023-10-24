using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ProtagDialogueTrigger : MonoBehaviour
{
    public string node;
    public float timeoutDuration;


    public UnityEvent OnDialogueConfirm;
    public UnityEvent OnDialogueEnd;
    public UnityEvent OnDialogueTimeout;

    private void Start()
    {
        if (ProtagDialogueController.instance == null)
        {
            this.enabled = false;
            return;
        }
    }
    public void PromptDialogue()
    {
        ProtagDialogueController.PromptDialogue(node, timeoutDuration);
        RegisterEvents();
    }

    public void DismissDialogue()
    {
        ProtagDialogueController.DismissDialogue(node);
        DeregisterEvents();
    }

    void RegisterEvents()
    {
        ProtagDialogueController.instance.OnDialogueConfirm.AddListener(OnConfirm);
        ProtagDialogueController.instance.OnDialogueEnd.AddListener(OnEnd);
        ProtagDialogueController.instance.OnDialogueTimeout.AddListener(OnTimeout);
    }

    void DeregisterEvents()
    {
        ProtagDialogueController.instance.OnDialogueConfirm.RemoveListener(OnConfirm);
        ProtagDialogueController.instance.OnDialogueEnd.RemoveListener(OnEnd);
        ProtagDialogueController.instance.OnDialogueTimeout.RemoveListener(OnTimeout);
    }

    void OnConfirm()
    {
        OnDialogueConfirm.Invoke();
    }

    void OnEnd()
    {
        OnDialogueEnd.Invoke();
        DeregisterEvents();
    }

    void OnTimeout()
    {
        OnDialogueTimeout.Invoke();
        DeregisterEvents();
    }
}
