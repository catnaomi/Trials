using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "IKHandler", menuName = "ScriptableObjects/IKHandler/Bow IK", order = 1), SerializeField]
public class BowIKHandler : IKHandler
{

    public override void OnIK(Animator animator)
    {
        if (animator.TryGetComponent<HumanoidPositionReference>(out HumanoidPositionReference positionReference))
        {
            animator.SetLookAtWeight(1f, 0f, 0.5f);

            animator.SetLookAtPosition(positionReference.Spine.transform.position + positionReference.transform.forward * 100f);
        }
        /*
        Vector3 aimDir = actor.GetLaunchVector(actor.positionReference.Spine.transform.position);

        Quaternion rot = Quaternion.LookRotation(aimDir) * Quaternion.AngleAxis(90f, Vector3.up);

        Vector3 adjAimDir = Quaternion.AngleAxis(90f, Vector3.up) * aimDir;
        */
        
    }

    public override void OnUpdate(Actor aactor)
    {
        if (aactor is HumanoidActor actor)
        {
            Vector3 aimDir = actor.GetLaunchVector(actor.positionReference.Spine.transform.position);

            Quaternion aimRot = Quaternion.LookRotation(aimDir, Vector3.up) * Quaternion.AngleAxis(90f, Vector3.up);

            actor.positionReference.Spine.rotation = aimRot;
        }
        else if (aactor is PlayerActor player)
        {
            Vector3 aimDir = player.GetLaunchVector(player.positionReference.Spine.transform.position);

            Quaternion aimRot = Quaternion.LookRotation(aimDir, Vector3.up) * Quaternion.AngleAxis(90f, Vector3.up);

            player.positionReference.Spine.rotation = aimRot;
        }
    }
}
