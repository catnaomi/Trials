using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceGiantIsDeadSaveLoader : SceneDataLoader
{
    public GameObject spawnTrigger;

    public void SaveSceneData()
    {
        SceneSaveDataManager.instance.data.dojo.isIceGiantDead = true;
    }

    public override void LoadSceneData(AllScenesSaveData data)
    {
        if (data.dojo.isIceGiantDead)
        {
            spawnTrigger.SetActive(false);
        }
    }
}
