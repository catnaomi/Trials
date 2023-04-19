using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;

public class CheckYarnVariableTrigger : MonoBehaviour
{
    public string variableName;
    [SerializeField] YarnVarType variableType;
    public bool passIfNotDefined = false;
    [Header("Boolean")]
    public bool boolCheckValue = true;
    [Header("String")]
    public string stringCheckValue = "";
    [Header("Float")]
    public float floatCheckValue = 0f;
    public UnityEvent OnCheckSuccess;
    public UnityEvent OnCheckFail;
    public void Check()
    {
        DialogueRunner runner = FindObjectOfType<DialogueRunner>();
        if (runner.VariableStorage.TryGetValue(variableName, out object result))
        {
            if (variableType == YarnVarType.Boolean && result is bool boolResult)
            {
                Debug.Log($"Checking Yarn variable \"{variableName}\" of type Boolean. Returned {result}");
                if (boolResult == boolCheckValue)
                {
                    OnCheckSuccess.Invoke();
                    return;
                }
            }
            else if (variableType == YarnVarType.String && result is string stringResult)
            {
                Debug.Log($"Checking Yarn variable \"{variableName}\" of type String. Returned {result}");
                if (stringResult == stringCheckValue)
                {
                    OnCheckSuccess.Invoke();
                    return;
                }
            }
            else if (result is float floatResult)
            {
                Debug.Log($"Checking Yarn variable \"{variableName}\" of type Float. Returned {result}");
                if (variableType == YarnVarType.Float_Equals)
                {
                    if (floatResult == floatCheckValue)
                    {
                        OnCheckSuccess.Invoke();
                        return;
                    }
                }
                else if (variableType == YarnVarType.Float_Less)
                {
                    if (floatResult < floatCheckValue)
                    {
                        OnCheckSuccess.Invoke();
                        return;
                    }
                }
                else if (variableType == YarnVarType.Float_Greater)
                {
                    if (floatResult > floatCheckValue)
                    {
                        OnCheckSuccess.Invoke();
                        return;
                    }
                }
                else if (variableType == YarnVarType.Float_LessEquals)
                {
                    if (floatResult <= floatCheckValue)
                    {
                        OnCheckSuccess.Invoke();
                        return;
                    }
                }
                else if (variableType == YarnVarType.Float_GreaterEquals)
                {
                    if (floatResult >= floatCheckValue)
                    {
                        OnCheckSuccess.Invoke();
                        return;
                    }
                }
            }
        }
        else
        {
            Debug.Log($"Yarn variable \"{variableName}\" not defined.");
            if (passIfNotDefined)
            {    
                OnCheckSuccess.Invoke();
                return;
            }
        }
        OnCheckFail.Invoke();
    }

    private void OnValidate()
    {
        if (variableName != null && variableName != "" && !variableName.Contains("$"))
        {
            variableName.Insert(0, "$");
        }
    }
    [SerializeField]
    enum YarnVarType
    {
        Boolean,
        String,
        Float_Equals,
        Float_Less,
        Float_Greater,
        Float_LessEquals,
        Float_GreaterEquals
    }
}
