using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class HeadIKTarget : MonoBehaviour
{
    Animator animator;
    public Transform directionTarget;
    public float weight = 1f;
    public float bodyWeight = 0f;
    public float headWeight = 1f;
    public float eyesWeight = 0f;
    public float clampWeight = 0.5f;
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
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, weight);
        Gizmos.DrawLine(animator.GetBoneTransform(HumanBodyBones.Head).position, directionTarget.position);
    }
}