using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// unfinished: pending new ice golem animations and mesh
public class SingleWeaknessDamageHandler : HumanoidDamageHandler
{

    ClipTransition customDeathAnim;

    public SingleWeaknessDamageHandler(Actor actor, DamageAnims anims, AnimancerComponent animancer) : base(actor, anims, animancer)
    {
        this.actor = actor;
        this.damageAnims = anims;
        this.animancer = animancer;
        blockStagger = damageAnims.blockStagger;
        guardBreak = damageAnims.guardBreak;

        totalCritTime = 0f;

        DizzyHumanoid dizzy = FXController.CreateDizzy().GetComponent<DizzyHumanoid>();
        dizzy.SetActor(actor, this);
        animancer.Layers[HumanoidAnimLayers.Flinch].SetMask(damageAnims.flinchMask);
        animancer.Layers[HumanoidAnimLayers.Flinch].IsAdditive = true;
        animancer.Layers[HumanoidAnimLayers.Flinch].SetWeight(1f);

        timeStopDamages = new Queue<DamageKnockback>();
    }
    public override void TakeDamage(DamageKnockback damage)
    {

        if (!actor.IsAlive()) return;
        bool isCrit = IsCritVulnerable();
        float normalDamageAmount = damage.GetDamageAmount(isCrit);
        bool hitFromBehind = !(Vector3.Dot(-actor.transform.forward, (damage.source.transform.position - actor.transform.position).normalized) <= 0f);

        List<DamageResistance> dr = new List<DamageResistance>();

        if (actor.GetResistances() != null)
        {
            dr.AddRange(actor.GetResistances());
        }
        bool blockSuccess = (actor.IsBlocking() && !hitFromBehind && !damage.unblockable);
        if (blockSuccess && actor.GetBlockResistance() != null)
        {
            dr.AddRange(actor.GetBlockResistance());
        }

        normalDamageAmount = DamageKnockback.GetTotalMinusResistances(normalDamageAmount, damage.GetTypes(), dr);

        float simplifiedDamageAmount = 0f;
        if ((normalDamageAmount > 0 || isCrit) && (!actor.IsDodging()))
        {
            simplifiedDamageAmount = 10f;
        }

        bool willKill = simplifiedDamageAmount >= actor.attributes.health.current || isCrit;
        bool tink = simplifiedDamageAmount <= 0f;


        actor.attributes.ReduceHealth(simplifiedDamageAmount);

        if (damage.hitboxSource != null)
        {
            Vector3 contactPosition = actor.GetComponent<Collider>().ClosestPoint(damage.hitboxSource.GetComponent<SphereCollider>().bounds.center);

            if (damage.source.TryGetComponent<Actor>(out Actor sourceActor))
            {
                sourceActor.lastContactPoint = contactPosition;
                sourceActor.SetLastBlockpoint(damage.hitboxSource.GetComponent<SphereCollider>().bounds.center);
            }
        }

        if (blockSuccess && !willKill)
        {
            if (!damage.breaksBlock)
            {
                if (!actor.IsAttacking())
                {
                    if (animancer.States.Current != block || damage.cannotAutoFlinch)
                    {
                        ClipTransition clip = blockStagger;
                        animancer.Layers[HumanoidAnimLayers.Flinch].Stop();
                        block = animancer.Play(clip);
                        block.Events.OnEnd = _OnBlockEnd;

                    }
                    else
                    {
                        ClipTransition clip = blockStagger;
                        AnimancerState state = animancer.Layers[HumanoidAnimLayers.Flinch].Play(clip);
                        state.Events.OnEnd = () => { animancer.Layers[HumanoidAnimLayers.Flinch].Stop(); };
                    }
                }
                if (damage.bouncesOffBlock && damage.source.TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    damageable.Recoil();
                }
            }
            else
            {
                animancer.Layers[HumanoidAnimLayers.Flinch].Stop();
                ClipTransition clip = guardBreak;
                AnimancerState state = animancer.Play(clip);
                state.Events.OnEnd = _OnEnd;
                hurt = state;
                actor.OnHurt.Invoke();
                damage.OnCrit.Invoke();
                StartCritVulnerability(clip.MaximumDuration / clip.Speed);
            }
            damage.OnBlock.Invoke();
            actor.OnBlock.Invoke();
        }
        else if (tink)
        {
            if (damage.bouncesOffBlock && damage.source.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.Recoil();
            }
            damage.OnBlock.Invoke();
            actor.OnBlock.Invoke();
        }
        else if (!willKill)
        {
            DamageKnockback.StaggerType stagger = DamageKnockback.StaggerType.StaggerLarge;

            Vector3 dir = (damage.source.transform.position - actor.transform.position).normalized;
            float xdot = Vector3.Dot(actor.transform.right, dir);
            float ydot = Vector3.Dot(actor.transform.forward, dir);

            isFacingUp = ydot > 0f;

            DirectionalMixerState state = (DirectionalMixerState)animancer.Play(damageAnims.staggerLarge);
            state.Time = 0f;
            state.ParameterX = xdot;
            state.ParameterY = ydot;
            state.Events.OnEnd = _OnEnd;
            CheckFallContinuous(state, willKill);
            hurt = state;

            animancer.Layers[HumanoidAnimLayers.BilayerBlend].Stop();

            damage.OnHit.Invoke();
            actor.OnHurt.Invoke();
        }
        else if (willKill)
        {
            /*
            DamageKnockback.StaggerType stagger = DamageKnockback.StaggerType.SpinDeath;

            AnimancerState state = animancer.Play(damageAnims.spinDeath);

            state.Events.OnEnd = actor.Die;
            hurt = state;

            damage.OnHit.Invoke();
            actor.OnHurt.Invoke();
            */

            damage.OnHit.Invoke();
            actor.OnHurt.Invoke();
            actor.Die();

        }
    }
}