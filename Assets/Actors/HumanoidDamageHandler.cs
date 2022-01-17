using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HumanoidDamageHandler : IDamageable
{
    Actor actor;
    DamageAnims damageAnims;
    AnimancerComponent animancer;

    public float lastDamage;
    public float damageTaken;

    public float critTime = -1f;
    float totalCritTime;
    bool inCritCoroutine;

    public AnimancerState hurt;
    public AnimancerState block;

    public Hitbox lastHitbox;
    ClipTransition blockStagger;

    System.Action _OnEnd;
    System.Action _OnBlockEnd;
    public void Recoil()
    {
        AnimancerState state = animancer.Play(damageAnims.recoil);
        state.Events.OnEnd = _OnEnd;
        hurt = state;
        actor.OnHurt.Invoke();
    }

    public HumanoidDamageHandler(Actor actor, DamageAnims anims, AnimancerComponent animancer)
    {
        this.actor = actor;
        this.damageAnims = anims;
        this.animancer = animancer;
        blockStagger = damageAnims.blockStagger;

        totalCritTime = 0f;

        DizzyHumanoid dizzy = FXController.CreateDizzy().GetComponent<DizzyHumanoid>();
        dizzy.SetActor(actor, this);
        animancer.Layers[(int)HumanoidPositionReference.AnimLayer.Flinch].SetMask(damageAnims.flinchMask);
        animancer.Layers[(int)HumanoidPositionReference.AnimLayer.Flinch].IsAdditive = true;
        animancer.Layers[(int)HumanoidPositionReference.AnimLayer.Flinch].SetWeight(1f);
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

        lastHitbox = damage.hitboxSource.GetComponent<Hitbox>();

        bool hitFromBehind = !(Vector3.Dot(-actor.transform.forward, (damage.source.transform.position - actor.transform.position).normalized) <= 0f);

        List <DamageResistance> dr = new List<DamageResistance>();

        if (actor.GetResistances() != null)
        {
            dr.AddRange(actor.GetResistances());
        }
        bool blockSuccess = (actor.IsBlocking() && !hitFromBehind && !damage.unblockable);
        if (blockSuccess && actor.GetBlockResistance() != null)
        {
            dr.AddRange(actor.GetBlockResistance());
        }
        bool isCrit = IsCritVulnerable();
        float damageAmount = damage.healthDamage * (isCrit ? damage.critData.criticalMultiplier : 1f);
        Debug.Log("damage before resistances = " + damageAmount);
        damageAmount = DamageKnockback.GetTotalMinusResistances(damageAmount, damage.GetTypes(), dr);
        Debug.Log("damage after resistances = " + damageAmount);

        bool isArmored = actor.IsArmored() && !damage.breaksArmor;
        bool willInjure = actor.attributes.spareable && actor.attributes.HasHealthRemaining() && damageAmount >= actor.attributes.health.current;
        bool willKill = (!willInjure) && damageAmount >= actor.attributes.health.current;

        actor.attributes.ReduceHealth(damageAmount);

        if (damage.hitboxSource != null)
        {
            Vector3 contactPosition = actor.GetComponent<Collider>().ClosestPoint(damage.hitboxSource.GetComponent<SphereCollider>().bounds.center);

            if (damage.source.TryGetComponent<Actor>(out Actor sourceActor))
            {
                sourceActor.lastContactPoint = contactPosition;
                sourceActor.SetLastBlockpoint(damage.hitboxSource.GetComponent<SphereCollider>().bounds.center);
            }
        }

        if (actor.IsDodging())
        {
            actor.OnDodge.Invoke();
        }
        else if (blockSuccess && !willKill && !willInjure)
        {
            

            if (!damage.breaksBlock)
            {
                if (animancer.States.Current != block)
                {
                    ClipTransition clip = blockStagger;
                    animancer.Layers[(int)HumanoidPositionReference.AnimLayer.Flinch].Stop();
                    block = animancer.Play(clip);
                    block.Events.OnEnd = _OnBlockEnd;
                    
                }
                else
                {
                    ClipTransition clip = blockStagger;
                    AnimancerState state = animancer.Layers[(int)HumanoidPositionReference.AnimLayer.Flinch].Play(clip);
                    state.Events.OnEnd = () => { animancer.Layers[(int)HumanoidPositionReference.AnimLayer.Flinch].Stop(); };
                }
                if (damage.bouncesOffBlock && damage.source.TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    damageable.Recoil();
                }
            }
            else
            {
                animancer.Layers[(int)HumanoidPositionReference.AnimLayer.Flinch].Stop();
                ClipTransition clip = damageAnims.guardBreak;
                AnimancerState state = animancer.Play(clip);
                state.Events.OnEnd = _OnEnd;
                hurt = state;
                actor.OnHurt.Invoke();
                StartCritVulnerability(clip.MaximumDuration / clip.Speed);
            }
            actor.transform.rotation = Quaternion.LookRotation(-(actor.transform.position - damage.source.transform.position), Vector3.up);
            damage.OnBlock.Invoke();
            actor.OnBlock.Invoke();
        }
        else
        {
            
            DamageKnockback.StaggerType stagger;
            
            // = (!isCrit) ? damage.staggers.onHit : damage.staggers.onCritical;
            float maxTime = 0f;
            //bool isFlinch = (stagger == DamageKnockback.StaggerType.Flinch || actor.IsArmored());

            

            if (willKill)
            {
                stagger = damage.staggers.onKill;
            }
            else if (willInjure)
            {
                stagger = damage.staggers.onInjure;
            }
            else if (isCrit)
            {
                stagger = damage.staggers.onCritical;
            }
            else if (isArmored)
            {
                stagger = damage.staggers.onArmorHit;
            }
            else
            {
                stagger = damage.staggers.onHit;
            }

            bool isFlinch = (stagger == DamageKnockback.StaggerType.Flinch);
            if (stagger == DamageKnockback.StaggerType.Flinch)
            {
                AnimancerState state = animancer.Layers[(int)HumanoidPositionReference.AnimLayer.Flinch].Play(damageAnims.flinch);
                state.Time = 0f;
                state.Events.OnEnd = () => { animancer.Layers[(int)HumanoidPositionReference.AnimLayer.Flinch].Stop(); };
                maxTime = state.RemainingDuration / state.Speed;
            }
            else if (stagger == DamageKnockback.StaggerType.StaggerSmall)
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
                    maxTime = state.RemainingDuration / state.Speed;
                }
                else
                {
                    isFlinch = true;
                    AnimancerState state = animancer.Layers[(int)HumanoidPositionReference.AnimLayer.Flinch].Play(damageAnims.flinch);
                    state.Time = 0f;
                    state.Events.OnEnd = () => { animancer.Layers[(int)HumanoidPositionReference.AnimLayer.Flinch].Stop(); };
                    maxTime = state.RemainingDuration / state.Speed;
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
                maxTime = state.RemainingDuration / state.Speed;
            }
            else if (stagger != DamageKnockback.StaggerType.None)
            {

                Debug.Log("look");
                ClipTransition clip = damageAnims.GetClipFromStaggerType(stagger);
                AnimancerState state = animancer.Play(clip);
                state.Events.OnEnd = _OnEnd;
                hurt = state;
                actor.transform.rotation = Quaternion.LookRotation(-(actor.transform.position - damage.source.transform.position), Vector3.up);
                maxTime = clip.MaximumDuration / clip.Speed;
            }

            

            if (isCrit)
            {
                if (!damage.critData.doesNotConsumeCritState)
                {
                    StopCritVulnerability();
                }
                else
                {
                    StartCritVulnerability(Mathf.Min(maxTime, damage.critData.criticalExtensionTime));
                }
                damage.OnCrit.Invoke();
            }

            

           
            if (!isFlinch)
            {
                AdjustDefendingPosition(damage.source);
                animancer.Layers[(int)HumanoidPositionReference.AnimLayer.Flinch].Stop();
            }
            damage.OnHit.Invoke();
            actor.OnHurt.Invoke();
        }
        
    }
    public void AdjustDefendingPosition(GameObject attacker)
    {
        if (attacker == null || !attacker.TryGetComponent<Actor>(out Actor attackerActor))
        {
            return;
        }

        float MAX_ADJUST = 0.1f;

        Vector3 targetPosition = attacker.transform.position + (attacker.transform.forward * 0.5f);

        Vector3 moveVector = Vector3.MoveTowards(actor.transform.position, targetPosition, MAX_ADJUST) - actor.transform.position;

        actor.GetComponent<CharacterController>().Move(moveVector);
    }

    public void StartCritVulnerability(float time)
    {
        if (totalCritTime >= DamageKnockback.MAX_CRITVULN_TIME) return;
        critTime = time;
        totalCritTime += time;
        if (!inCritCoroutine)
        {
            actor.StartCoroutine(CriticalTimeOut());
        }
        actor.OnCritVulnerable.Invoke();
    }

    public void StopCritVulnerability()
    {
        critTime = -1f;
        totalCritTime = 0f;
    }

    IEnumerator CriticalTimeOut()
    {
        inCritCoroutine = true;
        while (critTime > 0)
        {
            yield return new WaitForEndOfFrame();
            critTime -= Time.deltaTime;
        }
        inCritCoroutine = false;
    }

    public bool IsCritVulnerable()
    {
        bool isCritVuln = animancer.States.Current == hurt && critTime > 0f;
        if (!isCritVuln) totalCritTime = 0f;
        return isCritVuln;
    }

    public HumanoidDamageHandler GetDamageHandler()
    {
        return this;
    }
}