using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSaveDataManager : MonoBehaviour
{
    public static SceneSaveDataManager instance;
    public AllScenesSaveData data;

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

    public void ApplyData()
    {
        foreach (var loader in FindObjectsOfType<SceneDataLoader>())
        {
            loader.LoadSceneData(data);
        }
    }

    public static void LoadData(AllScenesSaveData loadedFromFile)
    {
        instance.data = loadedFromFile;
        instance.ApplyData();
    }
}
