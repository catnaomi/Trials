using UnityEngine;

public class GolemEncounterCompleteSaveLoader : SceneDataLoader
{
    public GameObject[] disableList;

    public void SaveSceneData()
    {
        SceneSaveDataManager.instance.data.dojo.isGolemEncounterComplete = true;
    }

    public override void LoadSceneData(AllScenesSaveData data)
    {
        if (data.dojo.isGolemEncounterComplete)
        {
            foreach (var objectToDisable in disableList)
            {
                objectToDisable.SetActive(false);
            }
        }
    }
}
