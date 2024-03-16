using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static readonly string FIRST_SCENE = "TerrainSkyScene";

    public string primarySceneToLoad;
    public string[] secondaryScenesToLoad;
    public bool loadOnStart;
    public bool allowSceneActivation;
    public string initSceneName = "_InitScene";
    public bool shouldLoadInitScene = true;

    [ReadOnly] public bool isLoadingComplete;
    [ReadOnly] public bool isPreloadingComplete;
    [ReadOnly] public bool didSceneLoadFail;
    [ReadOnly] public bool isLoading;
    [ReadOnly] public float totalLoading;
    public bool shouldReloadScenes;
    [SerializeField, ReadOnly] List<SceneLoadingData> sceneLoadingDatas;

    public static bool isAfterFirstLoad;

    public static SceneLoader instance;
    bool loadingFromLoadScreen;
    public UnityEvent OnFinishLoad;
    public UnityEvent OnActiveSceneChange;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            isAfterFirstLoad = false;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        if (loadOnStart)
        {
            LoadScenes();
        }
        OnFinishLoad.AddListener(ValidateDoubleInitScenes);
    }

    private void Update()
    {
        isAfterFirstLoad = true;
    }
    public void LoadScenes()
    {
        isLoadingComplete = false;
        isPreloadingComplete = false;
        //StartCoroutine(DisableObjectsRoutine());
        StartCoroutine(LoadScenesThenSetActive());
    }
    IEnumerator LoadScenesRoutine()
    {
        isLoading = true;
        isLoadingComplete = false;
        isPreloadingComplete = false;
        didSceneLoadFail = false;

        if (loadingFromLoadScreen)
        {
            yield return new WaitForSecondsRealtime(1f);
        }

        if (primarySceneToLoad == "")
        {
            didSceneLoadFail = true;
            Debug.LogError(new System.ArgumentException("Primary Scene Name cannot be blank.", "primarySceneToLoad"));
            yield break;
        }
        int indicesCount = secondaryScenesToLoad.Length + 1;

        // create scene loading objects

        if (sceneLoadingDatas != null)
        {
            sceneLoadingDatas.Clear();
        }
        else
        {
            sceneLoadingDatas = new List<SceneLoadingData>();
        }

        SceneLoadingData primarySceneData = new SceneLoadingData()
        {
            name = primarySceneToLoad,
        };
        Scene primary = SceneManager.GetSceneByName(primarySceneToLoad);
        if (primary != null && primary.isLoaded)
        {
            primarySceneData.isAlreadyLoaded = true;
            primarySceneData.scene = primary;
        }

        if (shouldLoadInitScene)
        {
            SceneLoadingData initSceneData = new SceneLoadingData()
            {
                name = "_InitScene",
            };
            sceneLoadingDatas.Add(initSceneData);
            initSceneData.loadOperation = SceneManager.LoadSceneAsync(initSceneName, LoadSceneMode.Additive);
            initSceneData.loadOperation.allowSceneActivation = allowSceneActivation;
            if (allowSceneActivation)
            {
                yield return initSceneData.loadOperation;
            }
        }

        if (!primarySceneData.isAlreadyLoaded || shouldReloadScenes)
        {
            sceneLoadingDatas.Add(primarySceneData);
            primarySceneData.loadOperation = SceneManager.LoadSceneAsync(primarySceneToLoad, LoadSceneMode.Additive);
            primarySceneData.loadOperation.allowSceneActivation = allowSceneActivation;
            if (allowSceneActivation)
            {
                yield return primarySceneData.loadOperation;
            }
        }

        foreach (string secondarySceneName in secondaryScenesToLoad)
        {
            if (secondarySceneName == "")
            {
                Debug.LogError(new System.ArgumentException("Scene Name cannot be blank. Skipping.", "secondarySceneName"));
                continue;
            }
            SceneLoadingData sceneData = new SceneLoadingData()
            {
                name = secondarySceneName,
            };
            Scene scene = SceneManager.GetSceneByName(secondarySceneName);
            if (scene != null && scene.isLoaded)
            {
                sceneData.isAlreadyLoaded = true;
                sceneData.scene = scene;
            }
            if (!sceneData.isAlreadyLoaded || shouldReloadScenes)
            {
                sceneLoadingDatas.Add(sceneData);
                sceneData.loadOperation = SceneManager.LoadSceneAsync(secondarySceneName, LoadSceneMode.Additive);
                sceneData.loadOperation.allowSceneActivation = allowSceneActivation;
                if (allowSceneActivation)
                {
                    yield return sceneData.loadOperation;
                }
            }
        }

        if (!allowSceneActivation)
        {
            bool notFinished = true;
            while (notFinished)
            {
                notFinished = false;
                foreach (SceneLoadingData data in sceneLoadingDatas)
                {
                    if (data.loadOperation.progress < 0.9f)
                    {
                        notFinished = true;
                    }
                    else
                    {
                        data.isPreloadCompleted = true;
                    }
                }
                totalLoading = GetSceneLoadingProgress();
                yield return new WaitForSecondsRealtime(0.5f);
            }

            if (loadingFromLoadScreen)
            {
                yield return new WaitForSecondsRealtime(1f);
                loadingFromLoadScreen = false;
            }
            foreach (SceneLoadingData data in sceneLoadingDatas)
            {
                data.isComplete = true;
                data.loadOperation.allowSceneActivation = true;
            }
        }

        bool allComplete = false;
        while (!allComplete)
        {
            allComplete = true;
            foreach (SceneLoadingData data in sceneLoadingDatas)
            {
                if (!data.loadOperation.isDone)
                {
                    allComplete = false;
                }
            }
            yield return null;
        }

        SceneLoader.SetActiveScene(primarySceneToLoad);
        isLoading = false;
        isLoadingComplete = true;
        OnFinishLoad.Invoke();
    }

    IEnumerator LoadScenesThenSetActive()
    {
        yield return StartCoroutine(LoadScenesRoutine());
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(primarySceneToLoad));
        OnActiveSceneChange.Invoke();
    }

    public static void DelayReloadCurrentScene()
    {
        instance.StartCoroutine(instance.DelayReloadRoutine(5f));
    }

    public IEnumerator DelayReloadRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReloadCurrentScene();
    }
    public void ReloadCurrentScene()
    {

        isAfterFirstLoad = false;
        allowSceneActivation = false;
        loadOnStart = false;
        shouldLoadInitScene = true;
        shouldReloadScenes = true;
        LoadWithProgressBar(SceneManager.GetActiveScene().name);
    }

    public static void LoadSceneSingle(string sceneName)
    {
        instance.LoadSceneSingleInstance(sceneName);
    }

    public void LoadSceneSingleInstance(string sceneName)
    {
        primarySceneToLoad = sceneName;
        secondaryScenesToLoad = new string[0];
        isAfterFirstLoad = false;
        allowSceneActivation = true;
        loadOnStart = false;
        shouldLoadInitScene = true;
        shouldReloadScenes = true;
        LoadScenes();
    }

    public float GetSceneLoadingProgress()
    {
        if (isLoadingComplete)
        {
            return 1f;
        }
        else if (isLoading)
        {
            float progress = 0f;
            float max = 0f;
            if (sceneLoadingDatas == null)
            {
                return 0f;
            }
            foreach (SceneLoadingData data in sceneLoadingDatas)
            {
                max += 1f;
                if (data.isComplete)
                {
                    progress += 1f;
                }
                else
                {
                    progress += data.loadOperation.progress;
                }

            }
            if (max != 0f)
            {
                return progress / max;
            }
            else
            {
                return 0f;
            }
        }
        else
        {
            return 0f;
        }
    }

    public static void EnsureScenesAreLoaded(string primary, params string[] secondaries)
    {
        if (instance.isLoading) return;
        instance.primarySceneToLoad = primary;
        instance.secondaryScenesToLoad = secondaries;
        instance.StartCoroutine(instance.LoadScenesRoutine());
    }

    public static void LoadWithProgressBar(string primary, params string[] secondaries)
    {
        if (instance.isLoading) return;
        instance.primarySceneToLoad = primary;
        instance.secondaryScenesToLoad = secondaries;
        instance.allowSceneActivation = false;
        instance.loadingFromLoadScreen = true;
        instance.StartCoroutine(instance.LoadingBarScene());
    }

    public static void LoadMainMenu()
    {
        if (instance.isLoading) return;
        instance.shouldLoadInitScene = false;
        LoadWithProgressBar("MainMenu");
    }

    IEnumerator LoadingBarScene()
    {
        yield return SceneManager.LoadSceneAsync("_LoadScene", LoadSceneMode.Single);
        yield return StartCoroutine(instance.LoadScenesRoutine());
        SceneManager.UnloadSceneAsync("_LoadScene");
    }
    public static AsyncOperation LoadSceneAdditively(string scene)
    {
        return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
    }
    public static void EnsureScenesAreUnloaded(string primary, params string[] secondaries)
    {
        if (SceneManager.GetSceneByName(primary) != null)
        {
            instance.StartCoroutine(instance.UnloadScene(primary));
        }

        foreach (string scene in secondaries)
        {
            if (SceneManager.GetSceneByName(scene) != null)
            {
                instance.StartCoroutine(instance.UnloadScene(scene));
            }
        }
    }

    IEnumerator UnloadScene(string scene)
    {
        AsyncOperation operation = null;
        try
        {
            operation = SceneManager.UnloadSceneAsync(scene);
        }
        catch (System.ArgumentException ex)
        {
            Debug.LogWarning("Scene " + scene + " is not loaded!");
            yield break;
        }

        if (operation != null)
        {
            yield return operation;
            Debug.Log("scene " + name + " unloaded");
        }
    }

    public static void SetActiveScene(string scene)
    {
        Scene ascene = SceneManager.GetSceneByName(scene);
        if (ascene.isLoaded)
        {
            SceneManager.SetActiveScene(ascene);
            instance.OnActiveSceneChange.Invoke();
        }
    }

    public static void AllowSceneActivation(bool active)
    {
        instance.allowSceneActivation = active;
    }

    public static void ShouldReloadScenes(bool reload)
    {
        instance.shouldReloadScenes = reload;
    }

    public static void ShouldLoadInitScene(bool init)
    {
        instance.shouldLoadInitScene = init;
    }

    public static bool IsSceneLoaded(string scene)
    {
#if UNITY_EDITOR
        return ifScene_CurrentlyLoaded_inEditor(scene);
#else
        return isScene_CurrentlyLoaded(scene);
#endif
    }

#if UNITY_EDITOR
    static bool ifScene_CurrentlyLoaded_inEditor(string sceneName_no_extention)
    {
        for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; ++i)
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i);

            if (scene.name == sceneName_no_extention)
            {
                return true;//the scene is already loaded
            }
        }
        //scene not currently loaded in the $$anonymous$$erarchy:
        return false;
    }
#endif


    static bool isScene_CurrentlyLoaded(string sceneName_no_extention)
    {
        for (int i = 0; i < SceneManager.sceneCount; ++i)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == sceneName_no_extention)
            {
                //the scene is already loaded
                return true;
            }
        }

        return false;//scene not currently loaded in the $$anonymous$$erarchy
    }


    public static UnityEvent GetOnActiveSceneChange()
    {
        return instance.OnActiveSceneChange;
    }

    public static UnityEvent GetOnFinishLoad()
    {
        return instance.OnFinishLoad;
    }

    public static bool IsSceneLoaderActive()
    {
        return instance != null;
    }

    public static bool IsSceneLoadingComplete()
    {
        return instance != null && instance.isLoadingComplete;
    }

    public static bool DoesSceneExist(string sceneName)
    {
        //TODO: figure out best way to do this
        return true;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            if (SceneManager.GetSceneByBuildIndex(i).name == sceneName)
            {
                return true;
            }
        }
        return false;
    }
    public void ValidateDoubleInitScenes()
    {
        bool foundInit = false;
        int countLoaded = SceneManager.sceneCount;

        for (int i = 0; i < countLoaded; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == "_InitScene")
            {
                if (!foundInit)
                {
                    foundInit = true;
                }
                else
                {
                    SceneManager.UnloadSceneAsync(scene);
                    Debug.LogWarning("InitScene was loaded multiple times. Unloading.");
                }
            }
        }
    }

    public static Scene[] GetAllOpenScenes()
    {
        int countLoaded = SceneManager.sceneCount;
        Scene[] loadedScenes = new Scene[countLoaded];

        for (int i = 0; i < countLoaded; i++)
        {
            loadedScenes[i] = SceneManager.GetSceneAt(i);
        }
        return loadedScenes;
    }
}

[SerializeField]
class SceneLoadingData
{
    public string name;
    public bool isAlreadyLoaded;
    public bool skip;
    public bool isComplete;
    public AsyncOperation loadOperation;
    public bool isLoading;
    public bool isPreloadCompleted;
    public AsyncOperation unloadOperation;
    public bool isUnloading;
    public Scene scene;
}