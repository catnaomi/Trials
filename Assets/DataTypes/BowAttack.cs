﻿using Animancer;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "atk0000_name", menuName = "ScriptableObjects/Attacks/Aim Attacks/Bow Attack", order = 1)]
public class BowAttack : AimAttack
{
    public override void ProcessAimAttack(PlayerActor player, bool aimHeld, bool attackDown, bool attackHeld)
    {
        if (!player.inventory.IsRangedEquipped())
        {
            return;
        }
        AnimancerComponent animancer = player.animancer;
        PlayerInventory inventory = player.inventory;
        IRangedWeapon rwep = (IRangedWeapon)player.inventory.GetRangedWeapon();
        bool anyPlaying = animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying();

        if (!anyPlaying)
        {
            player.astate.idle = animancer.Layers[HumanoidAnimLayers.UpperBody].Play(this.GetIdleClip());
        }
        else if (animancer.Layers[HumanoidAnimLayers.UpperBody].CurrentState == player.astate.idle)
        {
            if (!inventory.IsRangedDrawn())
            {
                inventory.SetDrawn(Inventory.MainType, false);
                inventory.SetDrawn(Inventory.OffType, false);
                player.astate.sheathe = player.TriggerSheath(true, inventory.GetRangedWeapon().RangedEquipSlot, Inventory.RangedType);
            }
            else if (attackHeld)
            {
                ClipTransition clip = this.GetStartClip();

                player.astate.start = animancer.Layers[HumanoidAnimLayers.UpperBody].Play(clip);
                player.astate.start.Events.OnEnd = () => { player.astate.hold = animancer.Layers[HumanoidAnimLayers.UpperBody].Play(this.GetHoldClip()); };
            }
        }
        else if (animancer.Layers[HumanoidAnimLayers.UpperBody].CurrentState == player.astate.start || animancer.Layers[HumanoidAnimLayers.UpperBody].CurrentState == player.astate.hold)
        {
            if (!attackHeld && rwep.CanFire())
            {
                ClipTransition clip = this.GetFireClip();

                player.astate.fire = animancer.Layers[HumanoidAnimLayers.UpperBody].Play(clip);
                player.astate.fire.Events.OnEnd = () => { animancer.Layers[HumanoidAnimLayers.UpperBody].Stop(); };
                player.ResetInputs();
            }
        }
    }
}