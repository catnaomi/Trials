using UnityEngine;
using System.Collections;
using CustomUtilities;

[CreateAssetMenu(fileName = "heavyattack_holdmove", menuName = "ScriptableObjects/Attacks/Heavy Attack - Hold Attack & Movement (Spins)", order = 1)]
public class HeavyAttackMovementHold : HeavyAttackCharge
{
    public BladeWeapon.AttackType holdAttack;
    public BladeWeapon.AttackType releaseAttack;
    public AxisUtilities.AxisDirection direction = AxisUtilities.AxisDirection.Forward;
    public float speed = 1f;
    public override void OnHeavyEnter()
    {
        actor.SetNextAttackType(holdAttack, true);
        //actor.nextAttackType = holdAttack;
    }

    public override void OnHeavyUpdate()
    {
        CharacterController cc = actor.GetComponent<CharacterController>();
        AxisUtilities.AxisDirection adjustedDir = direction;
        if (direction == AxisUtilities.AxisDirection.Zero)
        {
            adjustedDir = AxisUtilities.AxisDirection.Forward;
        }
        Vector3 dirVector = AxisUtilities.AxisDirectionToTransformDirection(actor.transform, adjustedDir);
        cc.Move(dirVector * speed * Time.deltaTime);
    }

    public override void OnHeavyExit()
    {
        actor.SetNextAttackType(releaseAttack, true);
    }
}
