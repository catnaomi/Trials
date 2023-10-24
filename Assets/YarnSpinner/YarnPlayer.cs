using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;

public class YarnPlayer : MonoBehaviour
{
    public YarnProject yarnProject;
    public string[] nodes;
    public int index;
    public bool loopThroughNodes;
    public bool closeAlreadyRunningDialogue = true;
    public UnityEvent OnFinish;

    DialogueRunner runner;

    public void Play()
    {
       if (nodes.Length > 0)
       {
            if (MenuController.menu != null)
            {
                MenuController.menu.OpenDialogue();
            }
            StartCoroutine(PlayRoutine());
       }
    }


    public void SetNodes(params string[] nodeNames)
    {
        nodes = nodeNames;
    }

    IEnumerator PlayRoutine()
    {
        runner = GameObject.FindGameObjectWithTag("DialogueRunner").GetComponent<DialogueRunner>();

        if (runner.CheckDialogueRunning())
        {
            if (closeAlreadyRunningDialogue)
            {
                runner.Stop();
                yield return new WaitWhile(runner.CheckDialogueRunning);
            }
            else
            {
                yield break;
            }
            
            
        }
        yield return null;
        runner.SetProject(yarnProject);
        runner.StartDialogue(nodes[index]);
        runner.onDialogueComplete.AddListener(CallFinish);
        if (loopThroughNodes)
        {
            index++;
            index %= nodes.Length;
        }
    }
    void CallFinish()
    {
        OnFinish.Invoke();
        runner.onDialogueComplete.RemoveListener(CallFinish);
    }
}
