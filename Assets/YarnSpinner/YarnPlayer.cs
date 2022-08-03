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
                MenuController.menu.OpenMenu(MenuController.Dialogue);
            }
            runner = GameObject.FindGameObjectWithTag("DialogueRunner").GetComponent<DialogueRunner>();

            if (runner.IsDialogueRunning && closeAlreadyRunningDialogue)
            {
                runner.Stop();
            }
            runner.SetProject(yarnProject);
            runner.StartDialogue(nodes[index]);
            runner.onDialogueComplete.AddListener(CallFinish);
            if (loopThroughNodes)
            {
                index++;
                index %= nodes.Length;
            }
            
       }
    }

    void CallFinish()
    {
        OnFinish.Invoke();
        runner.onDialogueComplete.RemoveListener(CallFinish);
    }
}
