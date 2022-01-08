using Animancer;
using System.Collections;
using UnityEngine;


public class HumanoidDamageHandler : IDamageable
{
    Actor actor;
    DamageAnims damageAnims;
    AnimancerComponent animancer;

    public float lastDamage;
    public float damageTaken;

    public AnimancerState hurt;
    public AnimancerState block;

    ClipTransition blockStagger;

    System.Action _OnEnd;
    System.Action _OnBlockEnd;
    public void Recoil()
    {
        AnimancerState state = animancer.Play(damageAnims.recoil);
        state.Events.OnEnd = _OnEnd;
        hurt = state;
    }

    public HumanoidDamageHandler(Actor actor, DamageAnims anims, AnimancerComponent animancer)
    {
        this.actor = actor;
        this.damageAnims = anims;
        this.animancer = animancer;
        blockStagger = damageAnims.blockStagger;
    }

    public void SetEndAction(System.Action action)
    {
        _OnEnd = action;
    }

    public void SetBlockEndAction(System.Action action)
    {
        _OnBlockEnd = action;
    }

    public void SetBlockClip(ClipTransition clip)
    {
        blockStagger = clip;
    }
    public void TakeDamage(DamageKnockback damage)
    {
        lastDamage = damage.healthDamage;
        damageTaken += lastDamage;



        bool hitFromBehind = !(Vector3.Dot(-actor.transform.forward, (damage.source.transform.position - actor.transform.position).normalized) <= 0f);
        Debug.Log(actor.name + "from behind?" + hitFromBehind);
        if (actor.IsBlocking() && !hitFromBehind)
        {
            if (!damage.breaksBlock)
            {
                if (animancer.States.Current != block)
                {
                    ClipTransition clip = blockStagger;
                    animancer.Layers[1].Stop();
                    block = animancer.Play(clip);
                    block.Events.OnEnd = _OnBlockEnd;
                    
                }
                else
                {
                    ClipTransition clip = blockStagger;
                    AnimancerState state = animancer.Layers[1].Play(clip);
                    state.Events.OnEnd = () => { animancer.Layers[1].Stop(); };
                }
                if (damage.bouncesOffBlock && damage.source.TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    damageable.Recoil();
                }
            }
            else
            {
                ClipTransition clip = damageAnims.guardBreak;
                AnimancerState state = animancer.Play(clip);
                state.Events.OnEnd = _OnEnd;
                hurt = state;
            }
            actor.transform.rotation = Quaternion.LookRotation(-(actor.transform.position - damage.source.transform.position), Vector3.up);
        }
        else
        {
            AdjustDefendingPosition(damage.source);
            DamageKnockback.StaggerType stagger = damage.staggers.onHit;
            if (stagger == DamageKnockback.StaggerType.StaggerSmall)
            {
                Vector3 dir = (damage.source.transform.position - actor.transform.position).normalized;
                float xdot = Vector3.Dot(actor.transform.right, dir);
                float ydot = Vector3.Dot(actor.transform.forward, dir);

                if (animancer.States.Current != hurt)
                {
                    DirectionalMixerState state = (DirectionalMixerState)animancer.Play(damageAnims.staggerSmall);
                    state.Time = 0f;
                    state.ParameterX = xdot;
                    state.ParameterY = ydot;
                    state.Events.OnEnd = _OnEnd;
                    hurt = state;
                }
                else
                {
                    DirectionalMixerState state = (DirectionalMixerState)animancer.Layers[1].Play(damageAnims.staggerSmall);
                    state.Time = 0f;
                    state.ParameterX = xdot;
                    state.ParameterY = ydot;
                    state.Events.OnEnd = () => { animancer.Layers[1].Stop(); };
                }
            }
            else if (stagger == DamageKnockback.StaggerType.StaggerLarge)
            {
                Vector3 dir = (damage.source.transform.position - actor.transform.position).normalized;
                float xdot = Vector3.Dot(actor.transform.right, dir);
                float ydot = Vector3.Dot(actor.transform.forward, dir);

                DirectionalMixerState state = (DirectionalMixerState)animancer.Play(damageAnims.staggerLarge);
                state.Time = 0f;
                state.ParameterX = xdot;
                state.ParameterY = ydot;
                state.Events.OnEnd = _OnEnd;
                hurt = state;
            }
            else if (stagger != DamageKnockback.StaggerType.None)
            {

                Debug.Log("look");
                ClipTransition clip = damageAnims.GetClipFromStaggerType(stagger);
                AnimancerState state = animancer.Play(clip);
                state.Events.OnEnd = _OnEnd;
                hurt = state;
                actor.transform.rotation = Quaternion.LookRotation(-(actor.transform.position - damage.source.transform.position), Vector3.up);
            }

            if (damage.hitboxSource != null)
            {
                Vector3 contactPosition = actor.GetComponent<Collider>().ClosestPointOnBounds(damage.hitboxSource.GetComponent<SphereCollider>().bounds.center);

                if (damage.source.TryGetComponent<Actor>(out Actor sourceActor))
                {
                    sourceActor.lastContactPoint = contactPosition;
                }
            }
           
            damage.OnHit.Invoke();
        }
    }
    public void AdjustDefendingPosition(GameObject attacker)
    {
        if (attacker == null || !attacker.TryGetComponent<Actor>(out Actor actor))
        {
            return;
        }

        float MAX_ADJUST = 0.25f;

        Vector3 targetPosition = attacker.transform.position + (attacker.transform.forward * 0.2f);

        Vector3 moveVector = Vector3.MoveTowards(actor.transform.position, targetPosition, MAX_ADJUST) - actor.transform.position;

        actor.GetComponent<CharacterController>().Move(moveVector);
    }
}