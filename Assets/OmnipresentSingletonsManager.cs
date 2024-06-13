using System;
using UnityEngine;

public class OmnipresentSingletonManager : MonoBehaviour
{
    public static bool initialized = false;

    public GameObject saveDataControllerPrefab;
    public GameObject sceneLoaderPrefab;

    void Awake()
    {
        if (!initialized)
        {
            if (SaveDataController.instance != null
             || SceneLoader.instance != null)
            {
                throw new Exception("Duplicated omni-present singletons!");
            }
            
            DontDestroyOnLoad(Instantiate(saveDataControllerPrefab));
            DontDestroyOnLoad(Instantiate(sceneLoaderPrefab));
            initialized = true;
        }
    }
}
