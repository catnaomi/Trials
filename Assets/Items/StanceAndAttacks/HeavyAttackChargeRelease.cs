using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "heavyattack_hold", menuName = "ScriptableObjects/Attacks/Heavy Attack - Hold & Release", order = 1)]
public class HeavyAttackChargeRelease : HeavyAttackCharge
{
    public BladeWeapon.AttackType shortAttack;
    public BladeWeapon.AttackType longAttack;
    public float chargeTime = 1f;
    public override void OnHeavyExit()
    {
        if (actor is PlayerActor)
        {
            float time = isSpecialAttack ?
                Mathf.Min(actor.animator.GetFloat("Input-SlashHeldTime"), actor.animator.GetFloat("Input-ThrustHeldTime")) :
                actor.animator.GetFloat("Input-HeavyHeldTime");
            if (time > chargeTime)
            {
                actor.nextAttackType = longAttack;
            }
            else
            {
                actor.nextAttackType = shortAttack;
            }
        }
        else
        {
            actor.nextAttackType = longAttack;
        }
    }
}
