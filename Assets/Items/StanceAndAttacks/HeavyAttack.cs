using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeavyAttack : ScriptableObject
{
    [ReadOnly] public HumanoidActor actor;

    public bool armored;
    /*
    [Header("Single Attack, Charge Start, & Aim Start")]
    public AnimationClip attack1;
    [Header("Charge Loop & Aim Loop")]
    public AnimationClip attack2;
    [Header("Charge Release & Aim Release")]
    public AnimationClip attack3;
    */

    protected AnimationClipOverrides overrides;

    public virtual void OnEquip(HumanoidActor actor)
    {
        this.actor = actor;
    }

    public virtual void OnUnequip(HumanoidActor actor)
    {
        return;
    }
    public virtual void OnHeavyEnter()
    {
        return;
    }

    public virtual void OnHeavyUpdate()
    {
        return;
    }

    public virtual void OnHeavyExit()
    {
        return;
    }

    public virtual void OnHeavyIK()
    {
        return;
    }

    public virtual void SetOverrides(AnimatorOverrideController controller)
    {
        return;
    }
    
    public virtual StanceHandler.HeavyStyle GetHeavyStyle()
    {
        return StanceHandler.HeavyStyle.None;
    }

    public bool IsArmored()
    {
        return armored;
    }
}
