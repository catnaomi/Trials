using System;
using System.Collections.Generic;
using UnityEngine;

public class PerSceneSaveData
{
    public Dictionary<string, bool> persistentSceneFlags = new Dictionary<string, bool>();
}

[Serializable]
public class SceneSaveData {
    public Dictionary<string, PerSceneSaveData> dataByScene = new Dictionary<string, PerSceneSaveData>();
}
