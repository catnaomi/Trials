using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using CustomUtilities;
using Yarn.Unity;

public class SaveDataController : MonoBehaviour
{
    public static SaveDataController instance;
    public string savePath;
    [Header("Inspector Slots")]
    public int slot = 1;
    public bool write;
    public bool read;
    public bool apply;

    [Header("Save Data")]
    [ReadOnly, SerializeField] SaveData data;
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
    }

    public void Read()
    {
        string path = GetPath() + $"savedata{slot}.json";
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
            Debug.LogError(ex);
            return;
        }

        SaveData readData = JsonConvert.DeserializeObject<SaveData>(json);
        if (readData != null)
        {
            data = readData;
        }

    }

    public void Apply()
    {
        if (data != null)
        {
            StartCoroutine(LoadSaveDataRoutine());
        }
    }

    IEnumerator LoadSaveDataRoutine()
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

        if (VariableStorageHelper.variableStorage != null)
        {
            VariableStorageHelper.variableStorage.SetAllVariables(data.yarnData.floats, data.yarnData.strings, data.yarnData.bools);
        }

        PlayerSaveDataManager.SetAttributeData(data.playerAttributeData);


        // load the next scene and the loading screen

        SceneLoader.LoadWithProgressBar(data.playerWorldData.activeScene);


        yield return new WaitUntil(SceneLoader.IsSceneLoadingComplete);

        // after loading

    }
    public void SetSlot(int s)
    {
        slot = s;
    }
    public string GetPath()
    {
        string actualPath = savePath.Replace("%persistentDataPath%", Application.persistentDataPath);
        return actualPath;
    }
}
