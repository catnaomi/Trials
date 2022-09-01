using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SimpleCrosshair : MonoBehaviour
{
    public Image image;

    private void OnGUI()
    {
        if (PlayerActor.player == null || !PlayerActor.player.gameObject.activeInHierarchy)
        {
            image.enabled = false;
            return;
        }

        if (PlayerActor.player.IsAiming() && PlayerActor.player.GetCombatTarget() == null && !PlayerActor.player.IsInDialogue())
        {
            image.enabled = true;
        }
        else
        {
            image.enabled = false;
        }
    }
}
