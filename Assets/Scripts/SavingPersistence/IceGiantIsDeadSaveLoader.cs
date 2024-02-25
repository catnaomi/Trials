using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceGiantIsDeadSaveLoader : SceneFlagSaveLoader
{
    public GameObject spawnTrigger;

    public override void LoadFlag(bool flag)
    {
        if (flag)
        {
            spawnTrigger.SetActive(false);
        }
    }
}
