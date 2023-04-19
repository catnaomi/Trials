using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class FootIKTarget : MonoBehaviour
{
    Animator animator;
    public Transform footLeftTarget;
    public Transform footRightTarget;
    public float leftFootWeight = 0f;
    public float rightFootWeight = 0f;
    // Use this for initialization
    void Start()
    {
        animator = this.GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (footLeftTarget != null)
        {
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, footLeftTarget.position);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootWeight);
        }
        if (footRightTarget != null)
        {
            animator.SetIKPosition(AvatarIKGoal.RightFoot, footRightTarget.position);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootWeight);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, leftFootWeight);
        Gizmos.DrawLine(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position, footLeftTarget.position);
        Gizmos.color = new Color(0f, 0f, 1f, rightFootWeight);
        Gizmos.DrawLine(animator.GetBoneTransform(HumanBodyBones.RightFoot).position, footRightTarget.position);
    }
}