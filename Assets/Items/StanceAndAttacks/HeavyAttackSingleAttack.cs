using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "heavyattack_singleattack", menuName = "ScriptableObjects/Attacks/Heavy Attack - Single Attack", order = 1)]
public class HeavyAttackSingleAttack : HeavyAttackSingle
{
    public BladeWeapon.AttackType attack;

    public override void OnHeavyEnter()
    {
        actor.SetNextAttackType(attack, true);
    }
}
