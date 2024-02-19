using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class SceneSaveDataManager : MonoBehaviour
{
    public static SceneSaveDataManager instance;
    public SceneSaveData data;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        
    }

    public void EnsureSaveData()
    {
        if (data == null)
        {
            data = new SceneSaveData();
        }
    }

    public void PersistSceneFlag(GameObject objectAttachedTo, bool flag)
    {
        var objectPath = SearchUtils.GetHierarchyPath(objectAttachedTo);
        data.persistentSceneFlags[objectPath] = flag;
    }

    public void ApplyData()
    {
        foreach (KeyValuePair<string, bool> persistentFlag in data.persistentSceneFlags)
        {
            var gameObject = GameObject.Find(persistentFlag.Key);
            gameObject.GetComponent<PersistentFlagLoader>().LoadFlag(persistentFlag.Value);
        }
	}
}
