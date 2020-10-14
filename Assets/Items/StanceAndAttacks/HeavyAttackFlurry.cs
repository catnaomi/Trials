using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "heavyattack_flurry", menuName = "ScriptableObjects/Attacks/Heavy Attack - Flurry", order = 1)]
public class HeavyAttackFlurry : HeavyAttackSingle
{

    public BladeWeapon.AttackType[] attacks;

    int index;
    public override void OnEquip(HumanoidActor actor)
    {
        base.OnEquip(actor);
        actor.OnHitboxActive.RemoveListener(NextAttack);
        actor.OnHitboxActive.AddListener(NextAttack);
    }

    public override void OnUnequip(HumanoidActor actor)
    {
        actor.OnHitboxActive.RemoveListener(NextAttack);
    }

    public override void OnHeavyEnter()
    {
        if (attacks.Length == 0)
        {
            return;
        }
        if (actor.IsHeavyAttacking())
        {
            index = 0;
            actor.nextAttackType = attacks[index];
        }
    }

    private void NextAttack()
    {
        if (attacks.Length == 0 || !actor.IsHeavyAttacking())
        {
            return;
        }
        actor.nextAttackType = attacks[index];
        index = (index + 1) % attacks.Length;
    }
}
