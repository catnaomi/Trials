using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "IKHandler", menuName = "ScriptableObjects/IKHandler/Throw IK", order = 1), SerializeField]
public class ThrowIKHandler : IKHandler
{
    public override void OnIK(Animator animator)
    {
        Actor actor = animator.GetComponent<Actor>();

        if (actor.TryGetComponent<HumanoidPositionReference>(out var positionReference))
        {
            Vector3 aimDir = actor.GetLaunchVector(positionReference.Spine.transform.position);

            animator.SetLookAtWeight(1f, 0f, 0.5f);

            animator.SetLookAtPosition(positionReference.Head.transform.position + aimDir * 100f);
        }
       
    }
}
