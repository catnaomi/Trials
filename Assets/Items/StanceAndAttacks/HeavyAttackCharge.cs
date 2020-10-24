using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "heavyattack_charge", menuName = "ScriptableObjects/Attacks/Heavy Attack - Charge", order = 1)]
public class HeavyAttackCharge : HeavyAttack
{
    public AnimationClip start;
    public float startSpeed = 1f;
    public AnimationClip loop;
    public float loopSpeed = 1f;
    public AnimationClip release;
    public float releaseSpeed = 1f;

    public override void OnEquip(HumanoidActor actor)
    {
        base.OnEquip(actor);

        if (!isSpecialAttack)
        {
            actor.animator.SetFloat("HeavySpeed1", startSpeed);
            actor.animator.SetFloat("HeavySpeed2", loopSpeed);
            actor.animator.SetFloat("HeavySpeed3", releaseSpeed);
        }
        else
        {
            actor.animator.SetFloat("SpecialSpeed1", startSpeed);
            actor.animator.SetFloat("SpecialSpeed2", loopSpeed);
            actor.animator.SetFloat("SpecialSpeed3", releaseSpeed);
        }
    }
    public override void SetOverrides(AnimatorOverrideController controller)
    {
        overrides = new AnimationClipOverrides(controller.overridesCount);
        controller.GetOverrides(overrides);
        if (!isSpecialAttack)
        {
            if (start != null) overrides["-replace_heavy-charge-start"] = start;
            if (loop != null) overrides["-replace_heavy-charge-loop"] = loop;
            if (release != null) overrides["-replace_heavy-charge-release"] = release;
        }
        else {
            if (start != null) overrides["-replace_special-charge-start"] = start;
            if (loop != null) overrides["-replace_special-charge-loop"] = loop;
            if (release != null) overrides["-replace_special-charge-release"] = release;
        }

        controller.ApplyOverrides(overrides);
    }

    public override StanceHandler.HeavyStyle GetHeavyStyle()
    {
        return StanceHandler.HeavyStyle.Charge_Release;
    }
}
