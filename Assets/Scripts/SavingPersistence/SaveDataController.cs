using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using CustomUtilities;
using Yarn.Unity;
using UnityEngine.Events;

public class SaveDataController : MonoBehaviour
{
    public static string SAVE_PATH = "%persistentDataPath%/Saves/";
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
        PlayerSaveDataManager.EnsureData();
        data.playerInventoryData = PlayerSaveDataManager.GetInventoryData();
        data.playerAttributeData = PlayerSaveDataManager.GetAttributeData();
        data.yarnData = YarnSaveDataManager.GetSaveDataStatic();
        data.playerWorldData = PlayerSaveDataManager.GetWorldData();
    }

    public void Write()
    {
        string path = GetPath() + $"savedata{slot}.json";
        string json = JsonConvert.SerializeObject(data);

        Directory.CreateDirectory(GetPath());
        using (StreamWriter sw = new StreamWriter(path, false, new UTF8Encoding()))
        {
            sw.Write(json);
        }
        Debug.Log($"succesfully saved to {path}");
        OnSaveComplete.Invoke();
    }

    public static void SaveToSlot(int slot)
    {
        if (instance == null) return;
        SetSlotStatic(slot);
        instance.CollectData();
        instance.Write();
    }

    public static void LoadFromSlot(int slot)
    {
        if (instance == null) return;
        SetSlotStatic(slot);
        instance.Read();
        instance.Apply();
    }
    public void Read()
    {
        data = ReadSlot(slot);

    }

    public void Clear()
    {
        data = null;

    }

    public static void ClearSaveData()
    {
        if (instance != null)
            instance.Clear();

        PlayerSaveDataManager.Clear();
        YarnSaveDataManager.ClearMemory();
    }
    public static SaveData ReadSlot(int slotNum)
    {
        string path = GetPath() + $"savedata{slotNum}.json";
        string json = "";
        try
        {
            using (StreamReader sr = new StreamReader(path))
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

    public void Apply()
    {
        if (data != null)
        {
            StartCoroutine(LoadSaveDataRoutine(data));
        }
    }

    IEnumerator LoadSaveDataRoutine(SaveData data)
    {
        // fade screen to black
        bool fadedToBlack = false;
        FadeToBlackController.FadeOut(1f,() => fadedToBlack = true,Color.black);
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
    }

    IEnumerator NewGameRoutine()
    {
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
    public static void LoadSaveDataStatic(SaveData data)
    {
        if (instance != null)
        instance.StartCoroutine(instance.LoadSaveDataRoutine(data));
    }

    public void SetSlot(int s)
    {
        slot = Mathf.Abs(s);
    }

    public static void SetSlotStatic(int slot)
    {
        if (instance != null)
            instance.SetSlot(slot);
    }
    public static string GetPath()
    {
        string actualPath = SAVE_PATH.Replace("%persistentDataPath%", Application.persistentDataPath);
        return actualPath;
    }

    public static void NewGameStatic()
    {
        if (instance != null)
            instance.NewGame();
    }

    void NewGame()
    {
        ClearSaveData();
        StartCoroutine(NewGameRoutine());
    }

    public static SaveData[] GetSaveDatas(int amount)
    {
        SaveData[] saves = new SaveData[amount];

        for (int i = 0; i < amount; i++)
        {
            saves[i] = ReadSlot(i);
        }
        return saves;
    }
}
