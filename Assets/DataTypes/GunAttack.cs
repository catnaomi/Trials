using Animancer;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "atk0000_name", menuName = "ScriptableObjects/Attacks/Aim Attacks/Gun Attack", order = 1)]
public class GunAttack : AimAttack
{
    public override void ProcessAimAttack(PlayerActor player, bool aimHeld, bool attackDown, bool attackHeld)
    {
        if (!player.inventory.IsRangedEquipped())
        {
            return;
        }
        AnimancerComponent animancer = player.animancer;
        PlayerInventory inventory = player.inventory;
        RangedWeapon rwep = (RangedWeapon)player.inventory.GetRangedWeapon();
        bool anyPlaying = animancer.Layers[HumanoidAnimLayers.UpperBody].IsAnyStatePlaying();

        if (!anyPlaying)
        {
            player.astate.idle = animancer.Layers[HumanoidAnimLayers.UpperBody].Play(this.GetIdleClip());
            rwep.SetCanFire(true);
        }
        else if (animancer.Layers[HumanoidAnimLayers.UpperBody].CurrentState == player.astate.idle)
        {
            if (!inventory.IsRangedDrawn())
            {
                inventory.SetDrawn(Inventory.MainType, false);
                inventory.SetDrawn(Inventory.OffType, false);
                player.astate.sheathe = player.TriggerSheath(true, inventory.GetRangedWeapon().RangedEquipSlot, Inventory.RangedType);
            }
            else if (attackDown)
            {
                if (rwep is RangedGun gun && gun.ShouldReload())
                {
                    ClipTransition clip = this.GetHoldClip();

                    player.astate.hold = animancer.Layers[HumanoidAnimLayers.UpperBody].Play(clip);
                    player.astate.hold.Events.OnEnd = () => { animancer.Layers[HumanoidAnimLayers.UpperBody].Stop(); };
                    player.ResetInputs();
                }
                else if (rwep.CanFire())
                {
                    ClipTransition clip = this.GetFireClip();

                    player.astate.fire = animancer.Layers[HumanoidAnimLayers.UpperBody].Play(clip);
                    player.astate.fire.Events.OnEnd = () => {
                        animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
                        rwep.SetCanFire(true);
                    };
                    player.ResetInputs();
                }
            }
        }
    }
}