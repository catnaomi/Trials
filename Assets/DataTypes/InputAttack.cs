using UnityEngine;
using System.Collections;
using Animancer;

[CreateAssetMenu(fileName = "atk0000_name", menuName = "ScriptableObjects/Attacks/Basic Attack", order = 1)]
public class InputAttack : InputAction
{
    public int attackId;
    public bool isBlockOK; // can attack be initiated from block
    public bool isSprintOK; // can attack be initiated from sprint
    public bool isFallingOK; // can attack be initiated while falling
    public bool isParryOK; // is attack a riposte or disarm?
    [SerializeField] protected ClipTransition anim;
    [Header("Attack Data")]
    public DamageKnockback attackData = DamageKnockback.GetDefaultDamage();
    public float staminaCost;
    public float exitTime = -1f;
    public float cancelStartTime = 0f;
    [Space(5)]
    public float mainHandAngleAdjust = 0f;
    public float offHandAngleAdjust = 0f;
    public int GetAttackID()
    {
        return attackId;
    }

    public bool IsBlockOkay()
    {
        return isBlockOK;
    }

    public bool IsSprintOkay()
    {
        return isSprintOK;
    }

    public bool IsFallingOkay()
    {
        return isFallingOK;
    }

    public bool IsParryOkay()
    {
        return isParryOK;
    }
    public virtual ClipTransition GetClip()
    {
        return anim;
    }

    public virtual DamageKnockback GetDamage()
    {
        return attackData;
    }
    public void GetDefaultAttack()
    {
        attackData = DamageKnockback.GetDefaultDamage();
    }
    public float GetExitTime()
    {
        return exitTime;
    }

    public float GetCancelStartTime()
    {
        return cancelStartTime;
    }

    public override AnimancerState ProcessHumanoidAction(NavigatingHumanoidActor actor, System.Action endEvent)
    {
        AnimancerState state = actor.animancer.Play(this.GetClip());
        actor.SetCurrentDamage(this.GetDamage());
        state.Events.OnEnd = endEvent;
        return state;
    }

    public override AnimancerState ProcessPlayerAction(PlayerActor player, out float cancelTime, System.Action endEvent)
    {
        
        AnimancerState state = player.animancer.Play(this.GetClip());
        cancelTime = this.GetExitTime();
        player.SetCurrentDamage(this.GetDamage());
        cancelTime = this.GetExitTime();
        state.Events.OnEnd = endEvent;
        if (mainHandAngleAdjust != 0f)
        {
            player.ResetMainRotation();
            player.RotateMainWeapon(mainHandAngleAdjust);
        }
        if (offHandAngleAdjust != 0f)
        {
            player.ResetOffRotation();
            player.RotateOffWeapon(offHandAngleAdjust);
        }
        return state;
    }
}
