using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "IKHandler", menuName = "ScriptableObjects/IKHandler/Bow IK", order = 1), SerializeField]
public class BowIKHandler : IKHandler
{

    public override void OnIK(Animator animator)
    {
        HumanoidActor actor = animator.GetComponent<HumanoidActor>();

        /*
        Vector3 aimDir = actor.GetLaunchVector(actor.positionReference.Spine.transform.position);

        Quaternion rot = Quaternion.LookRotation(aimDir) * Quaternion.AngleAxis(90f, Vector3.up);

        Vector3 adjAimDir = Quaternion.AngleAxis(90f, Vector3.up) * aimDir;
        */
        animator.SetLookAtWeight(1f, 0f, 0.5f);

        animator.SetLookAtPosition(actor.positionReference.Spine.transform.position + actor.transform.forward * 100f);
    }

    public override void OnUpdate(HumanoidActor actor)
    {
        Vector3 aimDir = actor.GetLaunchVector(actor.positionReference.Spine.transform.position);

        Quaternion aimRot = Quaternion.LookRotation(aimDir, Vector3.up) * Quaternion.AngleAxis(90f, Vector3.up);

        actor.positionReference.Spine.rotation = aimRot;
    }
}
