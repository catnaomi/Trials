using UnityEngine.Events;

public class ShipHingeIsDownSaveLoader : SceneFlagSaveLoader
{
    public UnityEvent playFallenAnimation;

    public override void LoadFlag(bool flag)
    {
        playFallenAnimation.Invoke();

        // Attached to breakable target that lowers the bridge
        gameObject.SetActive(false);
    }
}
