using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SceneSaveData : MonoBehaviour {
    public Dictionary<string, bool> persistentSceneFlags = new Dictionary<string, bool>();
}
