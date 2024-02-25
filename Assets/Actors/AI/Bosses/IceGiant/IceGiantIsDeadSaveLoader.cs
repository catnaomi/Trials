using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceGiantIsDeadSaveLoader : MonoBehaviour, IPersistentFlagLoader
{
    public GameObject spawnTrigger;

    public void SaveDead()
    {
        SceneSaveDataManager.instance.PersistSceneFlag(gameObject, true);
    }

    public void LoadFlag(bool flag)
    {
        if (flag)
        {
            spawnTrigger.SetActive(false);
        }
    }
}
