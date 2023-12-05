using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class VariableStorageHelper : MonoBehaviour
{
    public static VariableStorageBehaviour variableStorage;
    public GameObject variableStoragePrefab;
    public DialogueRunner runner;


    private void Awake()
    {
        if (variableStorage == null)
        {
            GameObject obj = Instantiate(variableStoragePrefab);
            DontDestroyOnLoad(obj);
            variableStorage = obj.GetComponent<VariableStorageBehaviour>();
        }
        runner.VariableStorage = variableStorage;
    }
}
