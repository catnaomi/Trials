using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class RuntimeIKTargets : MonoBehaviour
{
    Animator animator;
    [Header("Head")]
    public Transform directionTarget;
    public float weight = 1f;
    public float bodyWeight = 0f;
    public float headWeight = 1f;
    public float eyesWeight = 0f;
    public float clampWeight = 0.5f;
    [Header("Feet")]
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
        if (directionTarget != null)
        {
            animator.SetLookAtPosition(directionTarget.position);
            animator.SetLookAtWeight(weight, bodyWeight, headWeight, eyesWeight, clampWeight);
        }
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
        Gizmos.color = new Color(1f, 0f, 0f, weight);
        if (directionTarget != null)
        Gizmos.DrawLine(animator.GetBoneTransform(HumanBodyBones.Head).position, directionTarget.position);
        Gizmos.color = new Color(0f, 1f, 0f, leftFootWeight);
        if (footLeftTarget != null)
        Gizmos.DrawLine(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position, footLeftTarget.position);
        Gizmos.color = new Color(0f, 0f, 1f, rightFootWeight);
        if (footRightTarget != null)
        Gizmos.DrawLine(animator.GetBoneTransform(HumanBodyBones.RightFoot).position, footRightTarget.position);
    }
}