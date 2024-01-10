using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class VariableStorageHelper : MonoBehaviour
{
    public GameObject variableStoragePrefab;
    public DialogueRunner runner;


    private void Awake()
    {
        var vs = FindObjectOfType<VariableStorageBehaviour>();
        if (vs == null)
        {
            GameObject obj = Instantiate(variableStoragePrefab);
            DontDestroyOnLoad(obj);
            vs = obj.GetComponent<VariableStorageBehaviour>();
        }
        runner.VariableStorage = vs;
    }

    private void OnApplicationQuit()
    {
        Destroy(runner.VariableStorage);
    }
}
