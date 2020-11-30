using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "IKHandler", menuName = "ScriptableObjects/IKHandler/Throw IK", order = 1), SerializeField]
public class ThrowIKHandler : IKHandler
{
    public override void OnIK(Animator animator)
    {
        HumanoidActor actor = animator.GetComponent<HumanoidActor>();

        
        Vector3 aimDir = actor.GetLaunchVector(actor.positionReference.Spine.transform.position);
        
        animator.SetLookAtWeight(1f, 0f, 0.5f);

        animator.SetLookAtPosition(actor.positionReference.Head.transform.position + aimDir * 100f);
    }
}
