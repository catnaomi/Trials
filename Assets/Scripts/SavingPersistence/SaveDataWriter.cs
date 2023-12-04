using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class SaveDataWriter : MonoBehaviour
{
    public static SaveDataWriter instance;
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
    }

    public void CollectData()
    {
        data.playerInventoryData = PlayerSaveDataManager.GetInventoryData();
        data.playerAttributeData = PlayerSaveDataManager.GetAttributeData();
        data.yarnData = YarnSaveDataManager.GetSaveDataStatic();
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
