using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPlayerTargetAndDialogue : MonoBehaviour
{
    public GameObject target;

    public void SetTarget()
    {
        if (PlayerActor.player != null)
        {
            PlayerActor.player.SetCombatTarget(target);
            PlayerActor.player.StartDialogue();
        }
    }

    public void EndDialogue()
    {
        if (PlayerActor.player != null)
        {
            PlayerActor.player.StopDialogue();
        }
    }
}
