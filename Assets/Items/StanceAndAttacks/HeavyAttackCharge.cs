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

        actor.animator.SetFloat("HeavySpeed1", startSpeed);
        actor.animator.SetFloat("HeavySpeed2", loopSpeed);
        actor.animator.SetFloat("HeavySpeed3", releaseSpeed);
    }
    public override void SetOverrides(AnimatorOverrideController controller)
    {
        overrides = new AnimationClipOverrides(controller.overridesCount);
        controller.GetOverrides(overrides);
        if (start != null) overrides["-replace_heavy-charge-start"] = start;
        if (loop != null) overrides["-replace_heavy-charge-loop"] = loop;
        if (release != null) overrides["-replace_heavy-charge-release"] = release;

        controller.ApplyOverrides(overrides);
    }

    public override StanceHandler.HeavyStyle GetHeavyStyle()
    {
        return StanceHandler.HeavyStyle.Charge_Release;
    }
}
