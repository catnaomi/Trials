using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

// exists so i don't have to find all objects of type
public class DialogueRunnerReference : MonoBehaviour
{
    public static DialogueRunner runner;

    private void Awake()
    {
        runner = GetComponent<DialogueRunner>();
    }

    private void OnDestroy()
    {
        runner = null;
    }
}
