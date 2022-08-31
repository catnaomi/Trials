using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTester : NavigatingHumanoidActor, IAttacker
{
    public InputAttack attack;

    public bool go;

    HumanoidNPCInventory inventory;
    public override void ActorStart()
    {
        base.ActorStart();
        inventory = this.GetComponent<HumanoidNPCInventory>();
    }
    public override void ActorPostUpdate()
    {
        if (!inventory.IsMainDrawn() && inventory.IsMainEquipped())
        {
            inventory.SetDrawn(true, true);
        }
        if (go)
        {
            go = false;
            attack.ProcessHumanoidAction(this, () => { animancer.Play(navstate.move); });
        }
    }
    public DamageKnockback GetLastDamage()
    {
        return currentDamage;
    }
}