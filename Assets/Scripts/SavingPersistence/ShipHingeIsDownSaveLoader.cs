using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipHingeIsDownSaveLoader : SceneFlagSaveLoader
{
    public GameObject target;
    public GameObject hinge;
    public uint fallenAnimationIndex;

    public override void LoadFlag(bool flag)
    {
        target.SetActive(false);
        var animationPlayers = hinge.GetComponents<AnimancerPlayEvent>();
        animationPlayers[fallenAnimationIndex].PlayClip();
    }
}
