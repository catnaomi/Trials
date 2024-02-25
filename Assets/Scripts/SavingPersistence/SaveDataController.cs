using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using CustomUtilities;
using UnityEngine.Events;
using Newtonsoft.Json;

public class SaveDataController : MonoBehaviour
{
    public static string saveDirectory => Path.Combine(Application.persistentDataPath, "Saves");
    public static string GetSaveSlotPath(int slot) => Path.Combine(saveDirectory, $"savedata{slot}.json");

    public static SaveDataController instance;

    [Header("Inspector Slots")]
    public int slot = 1;
    public bool write;
    public bool read;
    public bool apply;

    [Header("Save Data")]
    [ReadOnly, SerializeField] SaveData data;
    public UnityEvent OnSaveComplete;
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        data = new SaveData();
    }

    // Update is called once per frame
    void Update()
    {
        // Check inspector debug flags
        if (write)
        {
            write = false;
            CollectData();
            Write();
        }
        if (read)
        {
            read = false;
            Read();
        }
        if (apply)
        {
            apply = false;
            Apply();
        }
    }

    public void CollectData()
    {
        if (data == null)
        {
            data = new SaveData();
        }
        PlayerSaveDataManager.EnsureData();
        data.playerInventoryData = PlayerSaveDataManager.GetInventoryData();
        data.playerAttributeData = PlayerSaveDataManager.GetAttributeData();
        data.yarnData = YarnSaveDataManager.GetSaveDataStatic();
        data.playerWorldData = PlayerSaveDataManager.GetWorldData();
        data.sceneSaveData = SceneSaveDataManager.instance.data;
    }

    public static void EnsureSaveDirectoryExists()
    {
        Directory.CreateDirectory(saveDirectory);
    }

    public void Write()
    {
        EnsureSaveDirectoryExists();

        string saveSlotPath = GetSaveSlotPath(slot);
        string json = JsonConvert.SerializeObject(data);

        using (StreamWriter sw = new StreamWriter(saveSlotPath, false, new UTF8Encoding()))
        {
            sw.Write(json);
        }
        Debug.Log($"succesfully saved to {saveSlotPath}");
        OnSaveComplete.Invoke();
    }

    public void Read() {
        EnsureSaveDirectoryExists();
        data = ReadSlot(slot);
    }

    public void Clear() {
        data = null;
    }

    public void SetSlot(int s) {
        slot = Mathf.Abs(s);
    }

    void NewGame() {
        ClearSaveData();
        StartCoroutine(NewGameRoutine());
    }

    public void Apply() {
        if (data != null) {
            StartCoroutine(LoadSaveDataRoutine(data));
        }
    }

    public static void SaveToSlot(int slot)
    {
        if (instance == null) return;
        instance.SetSlot(slot);
        instance.CollectData();
        instance.Write();
    }

    public static void LoadFromSlot(int slot)
    {
        if (instance == null) return;
        instance.SetSlot(slot);
        instance.Read();
        instance.Apply();
    }

    public static void DeleteSlot(int slot)
    {
        EnsureSaveDirectoryExists();
        string path = GetSaveSlotPath(slot);

        try
        {
            File.Delete(path);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    public static void ClearSaveData()
    {
        if (instance != null)
            instance.Clear();

        PlayerSaveDataManager.Clear();
        YarnSaveDataManager.ClearMemory();
    }

    public static SaveData ReadSlot(int slot)
    {
        string savePath = GetSaveSlotPath(slot);
        string json = "";
        try
        {
            using (StreamReader sr = new StreamReader(savePath))
            {
                json = sr.ReadToEnd();
            }
        }
        catch (FileNotFoundException ex)
        {
            Debug.LogWarning(ex);
            return null;
        }

        SaveData readData = JsonConvert.DeserializeObject<SaveData>(json);
        if (readData != null)
        {
            return readData;
        }
        return null;
    }

    public static void LoadSaveDataStatic(SaveData data)
    {
        if (instance != null)
            instance.StartCoroutine(instance.LoadSaveDataRoutine(data));
    }

    public static void SetSlotStatic(int slot)
    {
        if (instance != null)
            instance.SetSlot(slot);
    }

    public static void NewGameStatic()
    {
        if (instance != null)
            instance.NewGame();
    }

    public static SaveData[] GetSaveDatas(int amount)
    {
        EnsureSaveDirectoryExists();
        SaveData[] saves = new SaveData[amount];

        for (int i = 0; i < amount; i++)
        {
            saves[i] = ReadSlot(i);
        }

        return saves;
    }

    // Co-routines to apply data to game state
    IEnumerator LoadSaveDataRoutine(SaveData data) {
        // fade screen to black
        bool fadedToBlack = false;
        FadeToBlackController.FadeOut(1f, () => fadedToBlack = true, Color.black);
        FadeToBlackController.OverrideNextFadeInOnStart(true);
        yield return new WaitUntil(() => { return fadedToBlack; });

        // set the spawn location before we load the scene
        Vector3 position = NumberUtilities.ArrayToVector3(data.playerWorldData.position);
        Quaternion rotation = NumberUtilities.ArraytoQuaternion(data.playerWorldData.rotation);

        PlayerPositioner.SetNextOverridePosition(position, rotation);

        // set all variables and attributes
        YarnSaveDataManager.ApplyDataToMemory(data.yarnData);
        PlayerSaveDataManager.SetAttributeData(data.playerAttributeData);
        PlayerSaveDataManager.SetInventoryData(data.playerInventoryData);
     
        // load the next scene and the loading screen
        SceneLoader.LoadWithProgressBar(data.playerWorldData.activeScene);

        yield return new WaitUntil(SceneLoader.IsSceneLoadingComplete);

        // After scene load load our per scene data
        SceneSaveDataManager.LoadData(data.sceneSaveData);
    }

    IEnumerator NewGameRoutine() {
        // fade screen to black
        bool fadedToBlack = false;
        FadeToBlackController.FadeOut(1f, () => fadedToBlack = true, Color.black);
        FadeToBlackController.OverrideNextFadeInOnStart(true);
        yield return new WaitUntil(() => { return fadedToBlack; });

        // make sure we don't spawn in a special place

        PlayerPositioner.ClearOverride();

        PlayerSaveDataManager.SetAttributesToDefault();

        SceneLoader.LoadWithProgressBar(SceneLoader.FIRST_SCENE);
    }
}
