using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class YarnPlayer : MonoBehaviour
{
    public YarnProject yarnProject;
    public string[] nodes;
    public int index;
    public bool loopThroughNodes;
    public void Play()
    {
       if (nodes.Length > 0)
       {
            if (MenuController.menu != null)
            {
                MenuController.menu.OpenMenu(MenuController.Dialogue);
            }
            DialogueRunner runner = GameObject.FindGameObjectWithTag("DialogueRunner").GetComponent<DialogueRunner>();
            runner.SetProject(yarnProject);
            runner.StartDialogue(nodes[index]);
            if (loopThroughNodes)
            {
                index++;
                index %= nodes.Length;
            }
            
       }
    }
}
