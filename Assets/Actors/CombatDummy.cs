using Animancer;
using System.Collections;
using UnityEngine;
using CustomUtilities;

[RequireComponent(typeof(HumanoidPositionReference), typeof(AnimancerComponent), typeof(CharacterController))]
public class CombatDummy : Actor, IDamageable
{
    [Header("Combat Dummy Settings")]
    public bool isBlocking;
    bool blockanim;
    public bool isArmored;
    public ClipTransition idle;
    public ClipTransition block;

    public DamageAnims damageAnims;
    HumanoidDamageHandler damageHandler;
    public float damageTaken;
    public float lastDamage;
    public float lastStaminaDamage;
    public float stamina = 100f;

    HumanoidPositionReference positionReference;
    AnimancerState hurt;
    AnimancerState idleState;
    AnimancerState blockState;
    bool hitAlternator;
    System.Action _OnEnd;
    public override void ActorStart()
    {
        base.ActorStart();
        animancer = this.GetComponent<AnimancerComponent>();
        positionReference = this.GetComponent<HumanoidPositionReference>();
        idleState = animancer.States.GetOrCreate(idle);
        blockState = animancer.States.GetOrCreate(block);
        _OnEnd = () =>
        {
            animancer.Play((!blockanim)? animancer.States.GetOrCreate(idle) : animancer.States.GetOrCreate(block));
        };
        animancer.Play((!blockanim)? idleState : blockState);
        animancer.Layers[1].IsAdditive = true;
        animancer.Layers[1].Weight = 0.5f;

        damageHandler = new HumanoidDamageHandler(this, damageAnims, animancer);
        damageHandler.SetEndAction(_OnEnd);
        damageHandler.SetBlockEndAction(_OnEnd);
    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();
        if (isBlocking && !blockanim)
        {
            Debug.Log("startblock");
            animancer.Play(blockState);
            blockanim = true;
        }
        if (!isBlocking && blockanim)
        {
            Debug.Log("endblock");
            animancer.Play(idleState);
            blockanim = false;
        }
    }
    public override void ProcessDamageKnockback(DamageKnockback damage)
    {
        TakeDamage(damage);
    }
    public void TakeDamage(DamageKnockback damage)
    {
        damageHandler.TakeDamage(damage);
        Debug.Log($"Dummy took {damageHandler.lastDamage} damage with type: {DamageKnockback.FlagsToString(damageHandler.lastDamageTaken.GetTypes())}");
    }
    public void AdjustDefendingPosition(GameObject attacker)
    {
        damageHandler.AdjustDefendingPosition(attacker);
    }

    public void Recoil()
    {
        damageHandler.Recoil();
    }

    public void StartCritVulnerability(float time)
    {
        ((IDamageable)damageHandler).StartCritVulnerability(time);
    }
    public override bool IsBlocking ()
    {
        return isBlocking && animancer.States.Current != damageHandler.hurt;
    }

    public DamageKnockback GetLastTakenDamage()
    {
        return ((IDamageable)damageHandler).GetLastTakenDamage();
    }

    public GameObject GetGameObject()
    {
        return ((IDamageable)damageHandler).GetGameObject();
    }

    public void GetParried()
    {
        ((IDamageable)damageHandler).GetParried();
    }

    public bool IsCritVulnerable()
    {
        return ((IDamageable)damageHandler).IsCritVulnerable();
    }

    public void StartInvulnerability(float duration)
    {
        ((IDamageable)damageHandler).StartInvulnerability(duration);
    }

    public bool IsInvulnerable()
    {
        return ((IDamageable)damageHandler).IsInvulnerable();
    }
}