using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "heavyattack_single", menuName = "ScriptableObjects/Attacks/Heavy Attack - Single", order = 1)]
public class HeavyAttackSingle : HeavyAttack
{
    public AnimationClip clip;
    public float speed = 1f;

    public override void OnEquip(HumanoidActor actor)
    {
        base.OnEquip(actor);

        if (!isSpecialAttack)
        {
            actor.animator.SetFloat("HeavySpeed1", speed);
        }
        else
        {
            actor.animator.SetFloat("SpecialSpeed1", speed);
        }
    }

    public override void SetOverrides(AnimatorOverrideController controller)
    {
        overrides = new AnimationClipOverrides(controller.overridesCount);
        controller.GetOverrides(overrides);
        if (!isSpecialAttack)
        {
            if (clip != null) overrides["-replace_heavy-single"] = clip;
        }
        else
        {
            if (clip != null) overrides["-replace_special-single"] = clip;
        }

        controller.ApplyOverrides(overrides);

    }

    public override StanceHandler.HeavyStyle GetHeavyStyle()
    {
        return StanceHandler.HeavyStyle.Single;
    }
}
