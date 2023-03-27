using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HumanoidDamageHandler : IDamageable, IDamageHandler
{
    protected Actor actor;
    protected DamageAnims damageAnims;
    protected AnimancerComponent animancer;

    public DamageKnockback lastDamageTaken;
    public float lastDamage;
    public float damageTaken;

    public float critTime = -1f;
    protected float totalCritTime;
    bool inCritCoroutine;
    float maxTime;

    public float unfreezeDamageDelay = 0.25f;
    public Queue<DamageKnockback> timeStopDamages;
    protected bool inFrozenRoutine;

    public AnimancerState hurt;
    public AnimancerState block;
    public AnimancerState fall;
    AnimancerState invuln;
    public Hitbox lastHitbox;
    protected ClipTransition blockStagger;
    protected ClipTransition guardBreak;

    public bool isFacingUp;


    protected System.Action _OnEnd;
    protected System.Action _OnBlockEnd;
    public void Recoil()
    {
        AnimancerState state = animancer.Play(damageAnims.recoil);
        state.Events.OnEnd = _OnEnd;
        isFacingUp = true;
        CheckFallContinuous(state, false);
        hurt = state;
        actor.OnHurt.Invoke();
    }

    public HumanoidDamageHandler(Actor actor, DamageAnims anims, AnimancerComponent animancer)
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

    public void SetGuardBreakClip(ClipTransition clip)
    {
        guardBreak = clip;
    }

    public virtual void TakeDamage(DamageKnockback damage)
    {
        if (!actor.IsAlive() || IsInInvulnClip()) return;
        bool isCrit = IsCritVulnerable() || damage.critData.alwaysCritical;
        damage.didCrit = isCrit;
        float damageAmount = damage.GetDamageAmount(isCrit);
        if (actor.IsTimeStopped())
        {
            if (!inFrozenRoutine)
            {
                //actor.StartCoroutine(FrozenRoutine());
            }
            //timeStopDamages.Enqueue(damage);
            TimeTravelController.time.TimeStopDamage(damage, this, damageAmount);
            return;
        }
        lastDamage = damage.healthDamage;
        lastDamageTaken = damage;
        damageTaken += lastDamage;

        //lastHitbox = damage.hitboxSource.GetComponent<Hitbox>();

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

        Debug.Log("damage before resistances = " + damageAmount);
        damageAmount = DamageKnockback.GetTotalMinusResistances(damageAmount, damage.GetTypes(), dr);
        Debug.Log("damage after resistances = " + damageAmount);

        bool isArmored = actor.IsArmored() && !damage.breaksArmor;
        bool willInjure = actor.attributes.spareable && actor.attributes.HasHealthRemaining() && damageAmount >= actor.attributes.health.current;
        bool willKill = (!willInjure) && damageAmount >= actor.attributes.health.current;
        bool isCounterhit = actor.IsAttacking();

        if (!actor.IsDodging())
        {
            if (willKill && damage.cannotKill)
            {
                actor.attributes.SetHealth(1f);
                willKill = false;
            }
            else
            {
                actor.attributes.ReduceHealth(damageAmount);
            }
        }
        



        Vector3 contactPosition = damage.originPoint;
        Vector3 contactDirection = actor.transform.right;
        if (damage.hitboxSource != null)
        {
            contactPosition = actor.GetComponent<Collider>().ClosestPoint(damage.hitboxSource.GetComponent<SphereCollider>().bounds.center);
            contactDirection = damage.hitboxSource.GetComponent<Hitbox>().GetDeltaPosition().normalized;
            if (damage.source.TryGetComponent<Actor>(out Actor sourceActor))
            {

                sourceActor.lastContactPoint = contactPosition;
                sourceActor.SetLastBlockpoint(damage.hitboxSource.GetComponent<SphereCollider>().bounds.center);
                if (blockSuccess)
                {
                    contactPosition = sourceActor.GetBlockpoint(contactPosition); 
                }
            }
        }
        else if (damage.originPoint != Vector3.zero)
        {
            contactPosition = actor.GetComponent<Collider>().ClosestPoint(damage.originPoint);
            if (damage.source.TryGetComponent<Actor>(out Actor sourceActor))
            {
                sourceActor.lastContactPoint = contactPosition;
                sourceActor.SetLastBlockpoint(damage.originPoint);
            }
        }
        actor.GetComponent<IDamageable>().SetHitParticlePosition(contactPosition, contactDirection);

        if (actor.IsDodging())
        {
            actor.OnDodgeSuccess.Invoke();
        }
        else if (blockSuccess && !willKill && !willInjure)
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
                if (damage.bouncesOffBlock && damage.source.TryGetComponent<IDamageable>(out IDamageable damageable) && !damage.cannotRecoil)
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
                CheckFallContinuous(state, willKill);
                hurt = state;
                actor.OnHurt.Invoke();
                damage.OnCrit.Invoke();
                StartCritVulnerability(clip.MaximumDuration / clip.Speed);
            }
            if (!actor.IsAttacking())
            {
                Vector3 dir = damage.source.transform.position - actor.transform.position;
                dir.y = 0f;

                actor.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            }
            damage.OnBlock.Invoke();
            actor.OnBlock.Invoke();
        }
        else
        {
            
            DamageKnockback.StaggerType stagger;
            
            // = (!isCrit) ? damage.staggers.onHit : damage.staggers.onCritical;
            maxTime = 0f;
            //bool isFlinch = (stagger == DamageKnockback.StaggerType.Flinch || actor.IsArmored());

            

            if (willKill || willInjure)
            {
                stagger = damage.staggers.onKill;

                if (stagger != DamageKnockback.StaggerType.Stumble && stagger != DamageKnockback.StaggerType.Knockdown && stagger != DamageKnockback.StaggerType.FallOver && stagger != DamageKnockback.StaggerType.SpinDeath)
                {
                    stagger = DamageKnockback.StaggerType.FallOver;
                }
            }
            else if (isCrit)
            {
                stagger = damage.staggers.onCritical;
            }
            else if (isArmored)
            {
                stagger = damage.staggers.onArmorHit;
            }
            else if (isCounterhit)
            {
                stagger = damage.staggers.onCounterHit;
                if (stagger == DamageKnockback.StaggerType.None)
                {
                    stagger = damage.staggers.onHit;
                }
            }
            else
            {
                stagger = damage.staggers.onHit;
            }

            if (actor.IsClimbing())
            {
                if (willKill)
                {
                    stagger = DamageKnockback.StaggerType.Knockdown;
                }
                else
                {
                    stagger = DamageKnockback.StaggerType.Flinch;
                }
            }
            ProcessStaggerType(damage, stagger, hitFromBehind, willKill, isCrit);

            damage.OnHit.Invoke();
            actor.OnHurt.Invoke();
        }
        
    }

    protected void ProcessStaggerType(DamageKnockback damage, DamageKnockback.StaggerType stagger, bool hitFromBehind, bool willKill, bool isCrit)
    {
        bool isFlinch = (stagger == DamageKnockback.StaggerType.Flinch);

        if (!isFlinch)
        {
            animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
        }
        if (stagger == DamageKnockback.StaggerType.Flinch)
        {
            AnimancerState state = animancer.Layers[HumanoidAnimLayers.Flinch].Play(damageAnims.flinch);
            state.Time = 0f;
            state.Events.OnEnd = () => { animancer.Layers[HumanoidAnimLayers.Flinch].Stop(); };
            maxTime = state.RemainingDuration / state.Speed;
        }
        else if (stagger == DamageKnockback.StaggerType.Knockdown)
        {
            isFacingUp = !hitFromBehind;
            Vector3 dir = actor.transform.position - damage.source.transform.position;
            dir = dir.normalized * (isFacingUp ? 1f : -1f);
            ClipTransition clip = (!isFacingUp) ? damageAnims.knockdownFaceDown : damageAnims.knockdownFaceUp;
            AnimancerState state = animancer.Play(clip);

            SetInvulnClip(state);
            state.Events.OnEnd = () =>
            {
                if (actor.IsGrounded())
                {
                    AnimancerState prone = animancer.Play((!isFacingUp) ? damageAnims.proneFaceDown : damageAnims.proneFaceUp);
                    prone.NormalizedTime = 0f;
                    SetInvulnClip(prone);
                    hurt = prone;
                    if (willKill)
                    {
                        Die();
                    }
                }
                else
                {
                    StartHurtFall(willKill);
                }
            };
            hurt = state;
            fall = hurt;
            if (!willKill)
            {
                actor.StartCoroutine(EndProne(isFacingUp));
            }
        }
        else if (stagger == DamageKnockback.StaggerType.Crumple)
        {
            AnimancerState state = animancer.Play(damageAnims.crumple);


            CheckFallContinuous(state, willKill);
            SetInvulnClip(state);
            if (willKill)
            {
                state.Events.OnEnd = Die;
            }
            else
            {
                actor.StartCoroutine(EndProne(true));
            }
            hurt = state;
        }
        else if (stagger == DamageKnockback.StaggerType.FallOver)
        {
            AnimancerState state = animancer.Play(damageAnims.fallOver);


            CheckFallContinuous(state, willKill);
            SetInvulnClip(state);
            if (willKill)
            {
                state.Events.OnEnd = Die;
            }
            else
            {
                actor.StartCoroutine(EndProne(true));
            }
            hurt = state;
        }
        else if (stagger == DamageKnockback.StaggerType.SpinDeath)
        {
            AnimancerState state = animancer.Play(damageAnims.spinDeath);


            CheckFallContinuous(state, willKill);
            SetInvulnClip(state);
            if (willKill)
            {
                state.Events.OnEnd = Die;
            }
            else
            {
                actor.StartCoroutine(EndProne(true));
            }
            hurt = state;
        }
        else if (stagger == DamageKnockback.StaggerType.StaggerSmall)
        {
            Vector3 dir = (damage.source.transform.position - actor.transform.position).normalized;
            float xdot = Vector3.Dot(actor.transform.right, dir);
            float ydot = Vector3.Dot(actor.transform.forward, dir);

            if (animancer.States.Current != hurt || damage.cannotAutoFlinch)
            {
                DirectionalMixerState state = (DirectionalMixerState)animancer.Play(damageAnims.staggerSmall);
                state.Time = 0f;
                state.ParameterX = xdot;
                state.ParameterY = ydot;
                state.Events.OnEnd = _OnEnd;
                CheckFallContinuous(state, willKill);
                hurt = state;
                maxTime = state.RemainingDuration / state.Speed;
            }
            else
            {
                isFlinch = true;
                AnimancerState state = animancer.Layers[HumanoidAnimLayers.Flinch].Play(damageAnims.flinch);
                state.Time = 0f;
                state.Events.OnEnd = () => { animancer.Layers[HumanoidAnimLayers.Flinch].Stop(); };
                maxTime = state.RemainingDuration / state.Speed;
            }
        }
        else if (stagger == DamageKnockback.StaggerType.StaggerLarge)
        {
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
            maxTime = state.RemainingDuration / state.Speed;
        }
        else if (stagger != DamageKnockback.StaggerType.None)
        {
            ClipTransition clip = damageAnims.GetClipFromStaggerType(stagger);
            AnimancerState state = animancer.Play(clip);
            state.Events.OnEnd = _OnEnd;
            CheckFallContinuous(state, willKill);
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
            animancer.Layers[HumanoidAnimLayers.Flinch].Stop();
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

    void Die()
    {
        if (hurt != null)
        {
            hurt.Speed = 0.1f;
        }
        actor.Die();
    }

    public bool IsInInvulnClip()
    {
        return animancer.States.Current == invuln;
    }

    public void SetInvulnClip(AnimancerState state)
    {
        invuln = state;
    }
    public IEnumerator EndProne(bool faceUp)
    {
        yield return new WaitForSeconds(2f);
        if (!actor.IsGrounded())
        {
            StartHurtFall(false);
            yield break;
        }
        AnimancerState state = animancer.Play((faceUp) ? damageAnims.getupFaceUp : damageAnims.getupFaceDown);
        SetInvulnClip(state);
        hurt = state;
        hurt.Events.OnEnd = _OnEnd;
    }
    IEnumerator CriticalTimeOut()
    {
        inCritCoroutine = true;
        while (critTime > 0)
        {
            yield return null;
            if (!actor.IsTimeStopped())
            {
                critTime -= Time.deltaTime;
            }
        }
        inCritCoroutine = false;
    }

    protected IEnumerator FrozenRoutine()
    {
        inFrozenRoutine = true;
        yield return new WaitWhile(actor.IsTimeStopped);
        while (timeStopDamages.Count > 0)
        {
            DamageKnockback damage = timeStopDamages.Dequeue();
            damage.breaksArmor = true;
            damage.cannotAutoFlinch = true;
            damage.bouncesOffBlock = false;
            if (timeStopDamages.Count > 0)
            {
                damage.critData.doesNotConsumeCritState = true;
                damage.critData.criticalExtensionTime = timeStopDamages.Count * unfreezeDamageDelay;
            }
            else
            {
                damage.critData.doesNotConsumeCritState = false;
            }
            
            TakeDamage(damage);
            yield return new WaitForSeconds(unfreezeDamageDelay);
        }
        inFrozenRoutine = false;
    }

    protected System.Action CheckFallOnEnd(System.Action end, bool willKill)
    {
        void _CheckFall()
        {
            if (actor.IsGrounded())
            {
                end();
            }
            else
            {
                StartHurtFall(willKill);
            }
        }
        return _CheckFall;
    }

    IEnumerator HurtFallRoutine(AnimancerState state, bool willkill)
    {
        while (state.IsActive)
        {
            if (actor.IsGrounded())
            {
                yield return new WaitForSeconds(0.1f);
                if (!actor.IsGrounded()) continue;

                AnimancerState landState = animancer.Play((isFacingUp) ? damageAnims.landFaceUp : damageAnims.landFaceDown);
                if (!willkill)
                {
                    actor.StartCoroutine(EndProne(isFacingUp));
                }
                else
                {
                    landState.Events.OnEnd = Die;
                    hurt = landState;
                }
                hurt = landState;
                yield break;
            }
            yield return null;
        }
    }

    protected void CheckFallContinuous(AnimancerState current, bool willKill)
    {
        actor.StartCoroutine(HurtFallRoutineContinuous(current, willKill));
    }

    IEnumerator HurtFallRoutineContinuous(AnimancerState state, bool willKill)
    {
        while (state.IsActive)
        {
            if (!actor.IsGrounded())
            {
                yield return new WaitForSeconds(0.1f);
                if (actor.IsGrounded()) continue;

                StartHurtFall(willKill);
                yield break;
            }
            yield return null;
        }
    }

    public void StartHurtFall(bool willKill)
    {
        hurt = animancer.Play(isFacingUp ? damageAnims.fallFaceUp : damageAnims.fallFaceDown);
        fall = hurt;
        actor.StartCoroutine(HurtFallRoutine(hurt, willKill));
    }
    public void SetCritVulnState(AnimancerState state, float time)
    {
        hurt = state;
        critTime = time;
        if (animancer.States.Current == state)
        {
            actor.OnCritVulnerable.Invoke();
        }
    }

    public bool IsCritVulnerable()
    {
        bool isCritVuln = animancer.States.Current == hurt && critTime > 0f;
        if (!isCritVuln) totalCritTime = 0f;
        return isCritVuln;
    }

    public float GetCritTime()
    {
        return critTime;
    }
    public HumanoidDamageHandler GetDamageHandler()
    {
        return this;
    }

    public void SetHitParticlePosition(Vector3 position, Vector3 direction)
    {
        throw new System.NotImplementedException();
    }

    public DamageKnockback GetLastTakenDamage()
    {
        return lastDamageTaken;
    }

    public GameObject GetGameObject()
    {
        return actor.gameObject;
    }
}