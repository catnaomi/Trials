using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// unfinished: pending new ice golem animations and mesh
public class SimplifiedDamageHandler : HumanoidDamageHandler
{

    ClipTransition customDeathAnim;

    public SimplifiedDamageHandler(Actor actor, DamageAnims anims, AnimancerComponent animancer) : base(actor, anims, animancer)
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

        DamageResistance dr = new DamageResistance();
        
        if (actor.GetResistances() != null)
        {
            dr = DamageResistance.Add(dr, actor.GetResistances());
        }
        bool blockSuccess = (actor.IsBlocking() && !hitFromBehind && !damage.unblockable);
        if (blockSuccess && actor.GetBlockResistance() != null)
        {
            dr = DamageResistance.Add(dr, actor.GetBlockResistance());
        }
        
        normalDamageAmount = DamageKnockback.GetTotalMinusResistances(normalDamageAmount, damage.GetTypes(), dr);

        bool willKill = normalDamageAmount >= actor.attributes.health.current || isCrit;
        bool tink = normalDamageAmount <= 0f;
        bool weak = (dr.weaknesses & damage.GetTypes()) != 0;

        actor.attributes.ReduceHealth(normalDamageAmount);

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
        else if (!willKill)
        {

            
           


            DamageKnockback.StaggerType stagger = DamageKnockback.StaggerType.None;

            if (weak && damage.stagger == DamageKnockback.StaggerStrength.Light)
            {
                stagger = damageAnims.lightWeak;
            }
            else if (!weak && damage.stagger == DamageKnockback.StaggerStrength.Light)
            {
                stagger = damageAnims.lightNeutral;
            }
            else if (weak && damage.stagger == DamageKnockback.StaggerStrength.Heavy)
            {
                stagger = damageAnims.heavyWeak;
            }
            else if (!weak && damage.stagger == DamageKnockback.StaggerStrength.Heavy)
            {
                stagger = damageAnims.heavyNeutral;
            }

            ProcessStaggerType(damage, stagger, hitFromBehind, willKill, isCrit);
            //return;
            if (tink)
            {
                if (!damage.isRanged && damage.source.TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    damageable.Recoil();
                }
                damage.OnBlock.Invoke();
                actor.OnBlock.Invoke();
            }
            else
            {
                damage.OnHit.Invoke();
                actor.OnHurt.Invoke();
            }
        }
        else if (willKill)
        {

            DamageKnockback.StaggerType stagger = DamageKnockback.StaggerType.None;

            if (weak)
            {
                stagger = damageAnims.deathWeak;
            }
            else
            {
                stagger = damageAnims.deathNeutral;
            }

            if (stagger == DamageKnockback.StaggerType.None)
            {
                damage.OnHit.Invoke();
                actor.OnHurt.Invoke();
                actor.Die();
            }
            else
            {
                ProcessStaggerType(damage, stagger, hitFromBehind, willKill, isCrit);
                hurt.Events.OnEnd = actor.Die;
                damage.OnHit.Invoke();
                actor.OnHurt.Invoke();

            }
        }
    }
}