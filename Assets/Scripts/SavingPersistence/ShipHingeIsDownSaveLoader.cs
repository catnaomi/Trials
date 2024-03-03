public class ShipHingeIsDownSaveLoader : SceneFlagSaveLoader
{
    public ShipHingeAnimationPlayer shipHingeAnimationPlayer;

    public override void LoadFlag(bool flag)
    {
        shipHingeAnimationPlayer.PlayHasFallen();

        // Attached to breakable target that lowers the bridge
        gameObject.SetActive(false);
    }
}
