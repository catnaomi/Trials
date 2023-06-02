using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetGroupPopulateWithPlayer : MonoBehaviour
{
    CinemachineTargetGroup cmtg;
    // Start is called before the first frame update
    void Start()
    {
        cmtg = this.GetComponent<CinemachineTargetGroup>();
        if (cmtg != null)
        {
            cmtg.m_Targets[cmtg.m_Targets.Length - 1].target = PlayerActor.player.positionReference.eyeTarget;
        }
    }
}
