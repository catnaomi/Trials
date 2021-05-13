using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SimpleCrosshair : MonoBehaviour
{
    public PlayerActor player;
    public Image image;

    private void Start()
    {
        player = PlayerActor.player;
    }
    private void OnGUI()
    {
        if (player.IsAiming() && player.GetCombatTarget() == null)
        {
            image.enabled = true;
        }
        else
        {
            image.enabled = false;
        }
    }
}
