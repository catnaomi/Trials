using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// makes the target group point towards the cross product of both targets
// so that they're both framed profile.
public class CrossProductTargetGroup : MonoBehaviour
{
    public Vector3 leftVector = Vector3.up;
    CinemachineTargetGroup cmtg;
    Transform target1;
    Transform target2;
    // Start is called before the first frame update
    void Start()
    {
        cmtg = this.GetComponent<CinemachineTargetGroup>();
        cmtg.m_RotationMode = CinemachineTargetGroup.RotationMode.Manual;
    }

    // Update is called once per frame
    void Update()
    {
        if (cmtg.m_Targets.Length >= 2)
        {
            target1 = cmtg.m_Targets[0].target;
            target2 = cmtg.m_Targets[1].target;

            if (target1 != null && target2 != null)
            {
                Vector3 dir = target2.position - target1.position;

                this.transform.rotation = Quaternion.LookRotation(Vector3.Cross(leftVector, dir.normalized));
            }
            
        }
    }
}
