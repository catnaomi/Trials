using System;

[Serializable]
public class SaveData
{
    public PlayerInventoryData playerInventoryData;
    public PlayerAttributeData playerAttributeData;
    public YarnSaveData yarnData;
    public PlayerWorldData playerWorldData;
    public AllScenesSaveData allScenesSaveData;

    public bool IsDataValid()
    {
        if (this == null) return false;
        if (playerAttributeData == null || playerWorldData == null) return false;
        if (playerWorldData.activeScene == "") return false;
        return true;
    }
}
