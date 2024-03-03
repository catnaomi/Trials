using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SceneFlagSaveLoader : MonoBehaviour, IPersistentFlagLoader
{
    public void SaveFlag()
    {
        SceneSaveDataManager.instance.PersistSceneFlag(gameObject, true);
    }

    public abstract void LoadFlag(bool flag);
}
