﻿using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "heavyattack_aim", menuName = "ScriptableObjects/Attacks/Heavy Attack - Aim", order = 1)]
public class HeavyAttackAim : HeavyAttack
{
    public AnimationClip ready;
    public float readySpeed = 1f;
    public AnimationClip aim;
    public float aimSpeed = 1f;
    public AnimationClip fire;
    public float fireSpeed = 1f;

    public AnimationClip walkForward;
    public AnimationClip walkBackward;
    public AnimationClip walkLeft;
    public AnimationClip walkRight;

    public override void OnEquip(HumanoidActor actor)
    {
        base.OnEquip(actor);

        actor.animator.SetFloat("HeavySpeed1", readySpeed);
        actor.animator.SetFloat("HeavySpeed2", aimSpeed);
        actor.animator.SetFloat("HeavySpeed3", fireSpeed);
    }
    public override void SetOverrides(AnimatorOverrideController controller)
    {
        overrides = new AnimationClipOverrides(controller.overridesCount);
        controller.GetOverrides(overrides);

        if (ready != null) overrides["-replace_heavy-aim-start"] = ready;

        if (aim != null) overrides["-replace_heavy-aim-idle"] = aim;
        if (fire != null) overrides["-replace_heavy-aim-fire"] = fire;

        if (walkForward != null) overrides["-replace_heavy-aim-walk-forward"] = walkForward;
        if (walkBackward != null) overrides["-replace_heavy-aim-walk-back"] = walkBackward;
        if (walkLeft != null) overrides["-replace_heavy-aim-walk-left"] = walkLeft;
        if (walkRight != null) overrides["-replace_heavy-aim-walk-right"] = walkRight;

        controller.ApplyOverrides(overrides);
    }

    public override StanceHandler.HeavyStyle GetHeavyStyle()
    {
        return StanceHandler.HeavyStyle.Aim;
    }
}
