using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSaveDataManager : MonoBehaviour
{
    public static SceneSaveDataManager instance;
    public SceneSaveData data;

    public bool apply = false;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (apply)
        {
            apply = false;
            ApplyData();
        }
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
        EnsureSaveData();
        var objectPath = SearchUtils.GetHierarchyPath(objectAttachedTo, false);
        var sceneName = SceneManager.GetActiveScene().name;
        data.dataByScene.TryAdd(sceneName, new PerSceneSaveData());
        var dataForActiveScene = data.dataByScene[sceneName];
        dataForActiveScene.persistentSceneFlags[objectPath] = flag;
    }

    public void ApplyData()
    {
        EnsureSaveData();
        var sceneName = SceneManager.GetActiveScene().name;
        PerSceneSaveData dataForActiveScene;
        if (data.dataByScene.TryGetValue(sceneName, out dataForActiveScene))
        {
            foreach (KeyValuePair<string, bool> persistentFlag in dataForActiveScene.persistentSceneFlags)
            {
                var gameObject = GameObject.Find(persistentFlag.Key);
                gameObject.GetComponent<IPersistentFlagLoader>().LoadFlag(persistentFlag.Value);
            }
        }
        else
        {
            Debug.Log($"No scene save data to apply for scene {sceneName}");
        }
    }

    public static void LoadData(SceneSaveData loadedFromFile)
    {
        instance.data = loadedFromFile;
        instance.ApplyData();
    }
}
