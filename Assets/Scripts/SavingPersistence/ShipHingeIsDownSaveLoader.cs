public class ShipHingeIsDownSaveLoader : SceneFlagSaveLoader
{
    public ShipHingeAnimationPlayer shipHingeAnimationPlayer;

    public void SaveSceneData()
    {
        SceneSaveDataManager.instance.data.dojo.isShipHingeDown = true;
    }

    public override void LoadSceneData(AllScenesSaveData data)
    {
        if (data.dojo.isShipHingeDown)
        {
            shipHingeAnimationPlayer.PlayHasFallen();

            // Attached to breakable target that lowers the bridge
            gameObject.SetActive(false);
        }
    }
}
