using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Yarn.Unity;

public class YarnSaveDataManager : MonoBehaviour
{
    static YarnSaveDataManager instance;
    Dictionary<string, bool> boolDict;
    Dictionary<string, float> floatDict;
    Dictionary<string, string> stringDict;
    public bool get;
    [Header("Preview")]
    //[SerializeField, ReadOnly, TextArea(2, 20)] string jsonPreview;
    [SerializeField, ReadOnly, TextArea(2, 20)] string stringPreview;
    [SerializeField, ReadOnly, TextArea(2, 20)] string boolPreview;
    [SerializeField, ReadOnly, TextArea(2, 20)] string floatPreview;
    [SerializeField, ReadOnly] YarnSaveData yarnData;


    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (get)
        {
            get = false;
            GetSaveData();
        }
    }

    public YarnSaveData GetSaveData()
    {
        InMemoryVariableStorage varStorage = DialogueRunnerReference.runner.VariableStorage as InMemoryVariableStorage;

        var dicts = varStorage.GetAllVariables();

        floatDict = dicts.Item1;
        stringDict = dicts.Item2;
        boolDict = dicts.Item3;

        if (yarnData == null)
        {
            yarnData = new YarnSaveData();
        }

        yarnData.floats = floatDict;
        yarnData.strings = stringDict;
        yarnData.bools = boolDict;

        stringPreview = GetStringPreview(stringDict);
        boolPreview = GetStringPreview(boolDict);
        floatPreview = GetStringPreview(floatDict);

        yarnData.variableCount = floatDict.Count + stringDict.Count + boolDict.Count;

        return yarnData;
    }

    public static YarnSaveData GetSaveDataStatic()
    {
        if (instance == null)
        {
            return null;
        }
        return instance.GetSaveData();
    }
    string GetStringPreview(Dictionary<string, bool> dict)
    {
        StringBuilder sb = new StringBuilder();
        
        foreach (string key in dict.Keys)
        {
            sb.AppendLine($"{key}:{dict[key]}");
        }

        return sb.ToString();
    }

    string GetStringPreview(Dictionary<string, float> dict)
    {
        StringBuilder sb = new StringBuilder();

        foreach (string key in dict.Keys)
        {
            sb.AppendLine($"{key}:{dict[key]}");
        }

        return sb.ToString();
    }

    string GetStringPreview(Dictionary<string, string> dict)
    {
        StringBuilder sb = new StringBuilder();

        foreach (string key in dict.Keys)
        {
            sb.AppendLine($"{key}:{dict[key]}");
        }

        return sb.ToString();
    }

    public static void ApplyDataToMemory(YarnSaveData data)
    {
        if (data == null)
        {
            ClearMemory();
            return;
        }
        if (DialogueRunnerReference.runner.VariableStorage != null)
        {
            DialogueRunnerReference.runner.VariableStorage.SetAllVariables(data.floats, data.strings, data.bools);
        }
    }

    public static void ClearMemory()
    {
        if (DialogueRunnerReference.runner.VariableStorage != null)
        {
            DialogueRunnerReference.runner.VariableStorage.Clear();
        }
        if (instance != null)
            instance.Clear();
    }

    public void Clear()
    {
        boolDict = null;
        stringDict = null;
        floatDict = null;
        yarnData = null;
    }
}

[Serializable]
public class YarnSaveData
{
    public int variableCount;
    public Dictionary<string, bool> bools;
    public Dictionary<string, float> floats;
    public Dictionary<string, string> strings;


    public string ToJSON()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static YarnSaveData FromJSON(string json)
    {
        return JsonConvert.DeserializeObject<YarnSaveData>(json);
    }
}
