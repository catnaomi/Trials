using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

[Serializable]
public class EffectBlockReduction : Effect
{
    float percent;
    public EffectBlockReduction(float percent)
    {
        displayName = "Block Reduction";
        this.percent = percent;
        desc = String.Format("While blocking, stamina damage dealt is reduced by {0}", this.percent);
        applied = false;
    }
    protected override bool ApplyEffect(ActorAttributes attributes)
    {
        if (percent == 0f)
        {
            return false;
        }
        attributes.BlockReduction *= percent;
        return true;
    }

    protected override bool RemoveEffect(ActorAttributes attributes)
    {
        if (percent == 0f)
        {
            return false;
        }
        attributes.BlockReduction /= percent;
        return true;
    }

}
