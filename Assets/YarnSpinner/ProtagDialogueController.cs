using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomUtilities;
using UnityEngine.Events;
using Yarn.Unity;

[RequireComponent(typeof(DialogueFaceCameraOverride), typeof(YarnPlayer))]
public class ProtagDialogueController : MonoBehaviour
{
    public static ProtagDialogueController instance;
    public string node;
    string lastNode;
    YarnPlayer player;
    DialogueFaceCameraOverride dialogueFace;
    public UnityEvent OnDialoguePrompt;
    public UnityEvent OnDialogueConfirm;
    public UnityEvent OnDialogueEnd;
    public UnityEvent OnDialogueTimeout;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        player = this.GetComponent<YarnPlayer>();
        dialogueFace = this.GetComponent<DialogueFaceCameraOverride>();
        DialogueRunner runner = FindObjectOfType<DialogueRunner>();
        if (runner != null)
        {
            runner.onNodeComplete.AddListener(DialogueEnd);
        }
    }

    public static void PromptDialogue(string node, float duration)
    {
        if (instance == null) return;

        instance.PromptDialogueLocal(node, duration);
        
    }

    void PromptDialogueLocal(string node, float duration)
    {
        this.node = node;

        this.StartTimer(duration, () => instance.Dismiss(node));

        OnDialoguePrompt.Invoke();
    }

    public static void DismissDialogue(string node)
    {
        if (instance == null) return;
        instance.Dismiss(node);
    }

    void Dismiss(string node)
    {
        if (this.node == node)
        {
            this.node = "";
            OnDialogueTimeout.Invoke();
        }
    }

    public static bool HasDialogue()
    {
        if (instance == null) return false;
        return instance.node != "";
    }

    public static string GetNode()
    {
        if (instance == null) return "";
        return instance.node;
    }

    public static void PlayDialogue()
    {
        if (instance == null) return;
        instance.PlayDialogueLocal();
        
    }

    void PlayDialogueLocal()
    {
        if (HasDialogue())
        {
            player.SetNodes(instance.node);
            dialogueFace.StartDialogue();
            lastNode = instance.node;
            node = "";
            DialogueConfirm();
        }
    }

    void DialogueConfirm()
    {
        OnDialogueConfirm.Invoke();
    }

    void DialogueEnd(string playedNode)
    {
        if (lastNode == playedNode)
        {
            OnDialogueEnd.Invoke();
        }   
    }
}
