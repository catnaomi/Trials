using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
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

    [Header("Player Settings")]
    public GameObject player;
    public bool shouldSetPlayerPosition;
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    [Header("Disable These GameObjects While Loading")]
    public GameObject[] objectsToDisable;
    public static SceneLoader instance;

    public UnityEvent OnFinishLoad;
    public UnityEvent OnActiveSceneChange;
    private void Awake()
    {
        instance = this;
        isAfterFirstLoad = false;

        DontDestroyOnLoad(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        if (loadOnStart)
        {
            LoadScenes();
        }
    }

    private void Update()
    {
        isAfterFirstLoad = true;
    }
    public void LoadScenes()
    {
        isLoadingComplete = false;
        isPreloadingComplete = false;
        StartCoroutine(DisableObjectsRoutine());
        StartCoroutine(LoadScenesThenSetActive());
    }
    IEnumerator LoadScenesRoutine()
    {
        isLoading = true;
        isLoadingComplete = false;
        isPreloadingComplete = false;
        didSceneLoadFail = false;
        if (primarySceneToLoad == "")
        {
            didSceneLoadFail = true;
            Debug.LogError(new System.ArgumentException("Primary Scene Name cannot be blank.", "primarySceneToLoad"));
            yield break;
        }
        int indicesCount = secondaryScenesToLoad.Length + 1;
        
        // create scene loading objects

        List<SceneLoadingData> sceneLoadingDatas = new List<SceneLoadingData>();
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
            SceneManager.LoadScene(initSceneName, LoadSceneMode.Single);
        }

        if (!primarySceneData.isAlreadyLoaded || shouldReloadScenes)
        {
            sceneLoadingDatas.Add(primarySceneData);
            primarySceneData.loadOperation = SceneManager.LoadSceneAsync(primarySceneToLoad, LoadSceneMode.Additive);
            yield return primarySceneData.loadOperation;
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
                yield return sceneData.loadOperation;
            }

            

        }
        isLoading = false;
        OnFinishLoad.Invoke();
        
        /*
        float unloadingProgress;
        float loadingProgress;
        int preloadsCompleted = 0;
        int loadsCompleted = 0;
        int loadsToFinish = 0;
        int skips;
        bool primarySceneLoaded = false;

        
        while (!isLoadingComplete)
        {
            unloadingProgress = 1f;
            loadingProgress = 1f;
            skips = 0;
            foreach (SceneLoadingData loadData in sceneLoadingDatas)
            {
                bool isPrimaryScene = (loadData.name == primarySceneToLoad);
                if (loadData.skip)
                {
                    skips++;
                    continue;
                }
                else if (loadData.isComplete)
                {
                    continue;
                }
                
                if (loadData.isAlreadyLoaded && shouldReloadScenes && !loadData.isUnloading)
                {
                    // create unloading operations
                    loadData.unloadOperation = SceneManager.UnloadSceneAsync(loadData.name, UnloadSceneOptions.None);
                    if (loadData.unloadOperation == null)
                    {
                        Debug.LogError("Scene " + loadData.name + " failed to unload. Skipping.");
                        loadData.skip = true;
                    }
                    else
                    {
                        loadData.isUnloading = true;
                    }
                    loadData.isAlreadyLoaded = false;
                }
                else if (loadData.isUnloading) {
                    unloadingProgress *= loadData.unloadOperation.progress;
                    if (loadData.unloadOperation.isDone)
                    {
                        loadData.isUnloading = false;
                    }
                }
                else if (!loadData.isLoading)
                {
                    // create loading operations
                    try
                    {
                        loadData.loadOperation = SceneManager.LoadSceneAsync(loadData.name, LoadSceneMode.Additive);
                        loadData.loadOperation.allowSceneActivation = false;
                    }
                    catch (System.NullReferenceException ex)
                    {
                        didSceneLoadFail = true;
                        Debug.LogError(ex);
                        loadData.skip = true;
                    }
                    finally
                    {
                        loadsToFinish++;
                        loadData.isLoading = true;
                    }
                }
                else if (loadData.isLoading)
                {
                    loadingProgress *= loadData.loadOperation.progress;
                    if (loadData.loadOperation.isDone)
                    {
                        loadData.isLoading = false;
                        loadData.isComplete = true;
                        loadsCompleted++;
                        if (isPrimaryScene)
                        {
                            primarySceneLoaded = true;
                            SceneManager.SetActiveScene(SceneManager.GetSceneByName(primarySceneToLoad));
                        }
                    }
                    else if (loadData.loadOperation.progress >= 0.9f && !loadData.isPreloadCompleted)
                    {
                        loadData.isPreloadCompleted = true;
                        preloadsCompleted++;
                    }
                    else if (isPreloadingComplete && allowSceneActivation && !loadData.loadOperation.allowSceneActivation)
                    {
                        if (primarySceneLoaded || isPrimaryScene)
                        {
                            loadData.loadOperation.allowSceneActivation = true;
                        }
                        
                    }
                }
            }
            if (loadsToFinish > 0)
            {
                if (preloadsCompleted == loadsToFinish)
                {
                    isPreloadingComplete = true;
                }
                if (loadsCompleted == loadsToFinish)
                {
                    isLoadingComplete = true;
                }
            }
            else if (skips >= sceneLoadingDatas.Count)
            {
                Debug.LogError("All Scenes Failed To Load. Aborting.");
                yield break;
            }
            totalLoading = 0.5f * unloadingProgress + 0.5f * loadingProgress;
            yield return null;
        }
        */
        //SceneManager.UnloadSceneAsync("_LoadScene");
    }

    IEnumerator LoadScenesThenSetActive()
    {
        yield return StartCoroutine(LoadScenesRoutine());
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(primarySceneToLoad));
        OnActiveSceneChange.Invoke();
    }
    IEnumerator DisableObjectsRoutine()
    {
        if (player != null)
        {
            player.SetActive(false);
            foreach (GameObject obj in objectsToDisable)
            {
                obj.SetActive(false);
            }
        }
        yield return new WaitUntil(() => { return isLoadingComplete; });
        if (player == null)
        {
            player = FindObjectOfType<PlayerActor>().gameObject;
        }
        if (player != null)
        {
            player.SetActive(true);
            if (shouldSetPlayerPosition)
            {
                PositionPlayer();
            }
        }  
        foreach (GameObject obj in objectsToDisable)
        {
            obj.SetActive(true);
        }
        
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
        primarySceneToLoad = SceneManager.GetActiveScene().name;
        isAfterFirstLoad = false;
        allowSceneActivation = true;
        loadOnStart = false;
        shouldReloadScenes = true;
        LoadScenes();
    }
    void PositionPlayer()
    {
        if (player == null) return;
        player.transform.SetPositionAndRotation(playerPosition, playerRotation);
    }

    public static bool IsMovingPlayer()
    {
        return instance != null && instance.shouldSetPlayerPosition && instance.playerPosition != Vector3.zero;
    }

    public static void EnsureScenesAreLoaded(string primary, params string[] secondaries)
    {
        if (instance.isLoading) return;
        instance.primarySceneToLoad = primary;
        instance.secondaryScenesToLoad = secondaries;
        instance.StartCoroutine(instance.LoadScenesRoutine());
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
        return SceneManager.GetSceneByName(scene) != null;
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