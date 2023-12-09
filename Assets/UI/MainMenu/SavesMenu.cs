using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavesMenu : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject saveControllerPrefab;
    public GameObject sceneLoaderPrefab;
    [Header("References")]
    public SaveDataDisplay[] displays;
    CanvasGroup group;
    

    // Start is called before the first frame update
    void Start()
    {
        group = this.GetComponent<CanvasGroup>();
        //group.alpha = 0;
        Init();
    }


    public void Init()
    {
        var saves = SaveDataController.GetSaveDatas(3);
        bool b = false;
        if (SaveDataController.instance == null)
        {
            Instantiate(saveControllerPrefab);
        }
        if (SceneLoader.instance == null)
        {
            Instantiate(sceneLoaderPrefab);
        }
        

        for (int i = 0; i < displays.Length; i++)
        {
            displays[i].SetSaveData(i, saves[i]);
        }
    }

}
