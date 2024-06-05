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

    public UnityEvent OnSaveComplete;

    [Header("Inspector Slots")]
    public int slot = 1; // Inspector only, not used for normal save/load operations
    public bool save;
    public bool load;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        // inspector method triggers
        if (save)
        {
            save = false;
            Save(slot);
        }
        if (load)
        {
            load = false;
            Load(slot);
        }
    }

    public static void EnsureSaveDirectoryExists()
    {
        Directory.CreateDirectory(saveDirectory);
    }

    public void Save(int slot)
    {
        EnsureSaveDirectoryExists();
        SaveData data = new SaveData();

        PlayerSaveDataManager.EnsureData();
        data.playerInventoryData = PlayerSaveDataManager.GetInventoryData();
        data.playerAttributeData = PlayerSaveDataManager.GetAttributeData();
        data.yarnData = YarnSaveDataManager.GetSaveDataStatic();
        data.playerWorldData = PlayerSaveDataManager.GetWorldData();
        data.allScenesSaveData = SceneSaveDataManager.instance.data;

        string saveSlotPath = GetSaveSlotPath(slot);
        string json = JsonConvert.SerializeObject(data);

        using (StreamWriter sw = new StreamWriter(saveSlotPath, false, new UTF8Encoding()))
        {
            sw.Write(json);
        }
        Debug.Log($"succesfully saved to {saveSlotPath}");
        OnSaveComplete.Invoke();
    }

    public void Load(int slot)
    {
        EnsureSaveDirectoryExists();

        SaveData readData = Read(slot);
        if (readData != null)
        {
            // Need to resume the game or unity will actually die
            // but verify we have the controller, it won't exist on the main menu
            if (TimeScaleController.instance != null)
            {
                TimeScaleController.instance.paused = false;
            }
            StartCoroutine(LoadSaveDataRoutine(readData));
        }
    }

    static SaveData Read(int slot)
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
        catch (FileNotFoundException)
        {
            Debug.Log($"Slot {slot} is empty.");
            return null;
        }

        SaveData read_save = JsonConvert.DeserializeObject<SaveData>(json);

        // TODO: properly handle corrupted save data
        if (read_save == null) {
            Debug.LogError($"Corrupt save data in slot {slot}, json string is {json}");
        }

        return read_save;
    }

    // Co-routines to apply data to game state
    IEnumerator LoadSaveDataRoutine(SaveData data) {
        // fade screen to black
        bool fadedToBlack = false;
        FadeToBlackController.FadeOut(1f, () => fadedToBlack = true, Color.black);
        yield return new WaitUntil(() => { return fadedToBlack; });

        // set the spawn location before we load the scene
        Vector3 position = NumberUtilities.ArrayToVector3(data.playerWorldData.position);
        Quaternion rotation = NumberUtilities.ArraytoQuaternion(data.playerWorldData.rotation);

        PlayerPositioner.SetNextOverridePosition(position, rotation);

        // load the next scene and the loading screen
        SceneLoader.LoadWithProgressBar(data.playerWorldData.activeScene);
        yield return new WaitUntil(SceneLoader.IsSceneLoadingComplete);
        
        // after scene load apply remaining data
        YarnSaveDataManager.ApplyDataToMemory(data.yarnData);
        PlayerSaveDataManager.SetAttributeData(data.playerAttributeData);
        PlayerSaveDataManager.SetInventoryData(data.playerInventoryData);
        SceneSaveDataManager.LoadData(data.allScenesSaveData);
        yield return new WaitForEndOfFrame();
    }

    public void NewGame() {
        ClearSaveData();
        StartCoroutine(NewGameRoutine());
    }

    IEnumerator NewGameRoutine() {
        // fade screen to black
        bool fadedToBlack = false;
        FadeToBlackController.FadeOut(1f, () => fadedToBlack = true, Color.black);
        yield return new WaitUntil(() => { return fadedToBlack; });

        // make sure we don't spawn in a special place

        PlayerPositioner.ClearOverride();

        PlayerSaveDataManager.SetAttributesToDefault();

        SceneLoader.LoadWithProgressBar(SceneLoader.FIRST_SCENE);
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

    public void ClearSaveData()
    {
        PlayerSaveDataManager.Clear();
        YarnSaveDataManager.ClearMemory();
    }

    public static SaveData[] GetSaveDatas(int amount)
    {
        EnsureSaveDirectoryExists();

        SaveData[] saves = new SaveData[amount];
        for (int i = 0; i < amount; i++)
        {
            saves[i] = Read(i);
        }
        return saves;
    }
}
