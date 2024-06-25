using Animancer;
using CustomUtilities;
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
    public AnimancerState rise;
    AnimancerState invuln;
    public Hitbox lastHitbox;
    protected ClipTransition guardBreak;

    public bool isFacingUp;

    int lastStagger = 0;
    int lastBlockStagger = 0;
    bool isInvulnerable;
    protected System.Action _OnEnd;
    protected System.Action _OnBlockEnd;
    public void Recoil()
    {
        AnimancerState state = animancer.Play(damageAnims.recoil);
        state.Events.OnEnd = OnEnd;
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

        guardBreak = damageAnims.guardBreak;

        totalCritTime = 0f;

        animancer.Layers[HumanoidAnimLayers.Flinch].SetMask(damageAnims.flinchMask);
        animancer.Layers[HumanoidAnimLayers.Flinch].IsAdditive = true;
        animancer.Layers[HumanoidAnimLayers.Flinch].SetWeight(1f);

        timeStopDamages = new Queue<DamageKnockback>();
    }

    public void SetEndAction(System.Action action)
    {
        if (action != null)
        {
            _OnEnd = action;
        }
        else
        {
            _OnEnd = () => { };
        }
    }

    public void SetBlockEndAction(System.Action action)
    {
        if (action != null)
        {
            _OnBlockEnd = action;
        }
        else
        {
            _OnBlockEnd = () => { };
        }
    }

    protected void OnEnd()
    {
        lastStagger = -1;
        if (_OnEnd != null) _OnEnd();
    }

    protected void OnBlockEnd()
    {
        lastBlockStagger = -1;
        if (actor is PlayerActor player)
        {
            player.VerifyAccelerationAfterDelay(2f);
        }
        if (_OnBlockEnd != null) _OnBlockEnd();
    }

    public void SetGuardBreakClip(ClipTransition clip)
    {
        guardBreak = clip;
    }

    public virtual void TakeDamage(DamageKnockback damage)
    {
        if (!actor.IsAlive() || IsInvulnerable()) return;
        if (DamageKnockback.IsFriendlyFire(actor.attributes.friendlyGroup, damage.friendlyGroup)) return;
        float damageAmount = damage.GetDamageAmount();
       
        if (actor.IsTimeStopped())
        {
            TimeTravelController.time.TimeStopDamage(damage, this, damageAmount);
            return;
        }
        bool isCrit = IsCritVulnerable() || damage.critData.alwaysCritical;
        damage.result.didCrit = isCrit;
        damageAmount = damage.GetDamageAmount(isCrit);

        actor.lastDamageTaken = damage;

        bool hitFromBehind = !(Vector3.Dot(-actor.transform.forward, (damage.source.transform.position - actor.transform.position).normalized) <= 0f);

        DamageResistance dr = new DamageResistance();

        if (actor.GetResistances() != null)
        {
            dr = DamageResistance.Add(dr, actor.GetResistances());
        }
        bool blockSuccess = (actor.IsBlocking() && !hitFromBehind && !damage.unblockable);
        bool didTypedBlock = false;
        bool breaksBlock = damage.breaksBlock;

        PlayerActor player = null;
        if (actor is PlayerActor)
        {
            player = actor as PlayerActor;
            bool blockingSlash = player.IsBlockingSlash();
            bool blockingThrust = player.IsBlockingThrust();

            if (blockingSlash)
            {
                didTypedBlock = true;
                if (damage.isThrust)
                {
                    blockSuccess = true;
                    didTypedBlock = false;
                }
            }
            else if (blockingThrust)
            {
                didTypedBlock = true;
                if (damage.isSlash)
                {
                    blockSuccess = true;
                    didTypedBlock = false;
                }
            }
        }

        bool bouncesOffBlock = damage.bouncesOffBlock;
        if (didTypedBlock && damage.bouncesOffTypedBlock)
        {
            bouncesOffBlock = true;
        }
        
        if (blockSuccess && actor.GetBlockResistance() != null)
        {
            dr = DamageResistance.Add(dr, actor.GetBlockResistance());
        }

        damageAmount = DamageKnockback.GetTotalMinusResistances(damageAmount, damage.unresistedMinimum, damage.GetTypes(), dr);

        if (damage.GetTypes().HasType(dr.weaknesses))
        {
            damage.OnHitWeakness.Invoke();
            damage.result.didHitWeakness = true;
        }
        
        /*
        if (damage.GetTypes().HasType(dr.resistances))
        {
            damage.OnHitResistance.Invoke();
            damage.result.didHitResistance = true;
        }
        */

        bool isArmored = actor.IsArmored() && !damage.breaksArmor;
        bool willInjure = actor.attributes.spareable && actor.attributes.HasHealthRemaining() && damageAmount >= actor.attributes.health.current;
        bool willKill = (!willInjure) && damageAmount >= actor.attributes.health.current;
        bool isCounterhit = actor.IsAttacking();

        damage.result.didKill = willKill;

        lastDamageTaken = damage;
        lastDamage = damageAmount;
        damageTaken += lastDamage;

        damage.result.damageAmount = damageTaken;
        actor.lastDamageTaken = damage;
        actor.lastDamageAmountTaken = damageAmount;

        bool dodge = actor.IsDodging() || (damage.jumpable && actor.IsJumping());

        if (dodge)
        {
            actor.OnDodge.Invoke();
        }
        else if (willKill && (damage.cannotKill || blockSuccess))
        {
            actor.attributes.SetHealth(1f);
            willKill = false;
        }
        else
        {
            actor.attributes.ReduceHealth(damageAmount);
        }

        DamageKnockback.GetContactPoints(actor.GetComponent<IDamageable>(), damage, actor.GetComponent<Collider>(), blockSuccess);

        if (dodge)
        {
            actor.OnDodgeSuccess.Invoke();
        }
        else if (blockSuccess && !willKill && !willInjure)
        {
            DamageKnockback.BlockStaggerType blockStagger = (didTypedBlock ? damage.staggers.onTypedBlock : damage.staggers.onBlock);
            if (!breaksBlock)
            {
                if (blockStagger == DamageKnockback.BlockStaggerType.AutoParry)
                {
                    ClipTransition clip = damageAnims.GetClipFromBlockType(blockStagger);
                    animancer.Layers[HumanoidAnimLayers.Flinch].Stop();
                    animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
                    block = animancer.Layers[HumanoidAnimLayers.Base].Play(clip);
                    block.NormalizedTime = 0f;
                    block.Events.OnEnd = OnBlockEnd;
                    if (damage.source.TryGetComponent<IDamageable>(out IDamageable attacker) && !damage.cannotRecoil)
                    {
                        attacker.GetParried();
                    }
                    actor.OnParrySuccess.Invoke();
                }
                else if (!actor.IsAttacking())
                {
                    bool isBlockFlinch = blockStagger == DamageKnockback.BlockStaggerType.Flinch || blockStagger == DamageKnockback.BlockStaggerType.FlinchSlow;

                    if (!isBlockFlinch &&
                        (animancer.States.Current != block || ((int)blockStagger >= lastBlockStagger)))
                    {
                        ClipTransition clip = damageAnims.GetClipFromBlockType(blockStagger);
                        animancer.Layers[HumanoidAnimLayers.Flinch].Stop();
                        animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
                        block = animancer.Layers[HumanoidAnimLayers.Base].Play(clip);
                        block.NormalizedTime = 0f;
                        block.Events.OnEnd = OnBlockEnd;
                        SetBlockAccel(.25f + block.Length);
                    }
                    else
                    {
                        ClipTransition clip = damageAnims.GetClipFromBlockType(DamageKnockback.BlockStaggerType.Flinch);
                        AnimancerState state = animancer.Layers[HumanoidAnimLayers.Flinch].Play(clip);
                        state.Events.OnEnd = () => { 
                            animancer.Layers[HumanoidAnimLayers.Flinch].Stop();
                        };
                        if (blockStagger == DamageKnockback.BlockStaggerType.FlinchSlow)
                        {
                            SetBlockAccel(0.25f);
                        }
                        
                    }
                    lastBlockStagger = (int)damage.staggers.onBlock;
                }
                if (bouncesOffBlock && damage.source.TryGetComponent<IDamageable>(out IDamageable damageable) && !damage.cannotRecoil)
                {
                    damageable.Recoil();
                }
            }
            else
            {
                animancer.Layers[HumanoidAnimLayers.Flinch].Stop();
                animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
                ClipTransition clip = guardBreak;
                AnimancerState state = animancer.Play(clip);
                state.Events.OnEnd = OnEnd;
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
            AdjustDefendingPosition(damage.source, damage.repositionLength, damage.repositionMaxDist);
            if (actor is PlayerActor)
            {
                
                if (didTypedBlock && blockSuccess && !breaksBlock)
                {
                    player.OnTypedBlockSuccess.Invoke();
                }
            }
            damage.OnBlock.Invoke();
            actor.OnBlock.Invoke();
        }
        else
        {
            DamageKnockback.StaggerType stagger;
            maxTime = 0f;

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

        switch (stagger)
        {
            case DamageKnockback.StaggerType.None:
            {
            } break;

            case DamageKnockback.StaggerType.Flinch:
            {
                AnimancerState state = animancer.Layers[HumanoidAnimLayers.Flinch].Play(damageAnims.flinch);
                state.Time = 0f;
                state.Events.OnEnd = () => { animancer.Layers[HumanoidAnimLayers.Flinch].Stop(); };
                maxTime = state.RemainingDuration / state.Speed;
            } break;

            case DamageKnockback.StaggerType.Knockdown:
            {
                isFacingUp = !hitFromBehind;
                Vector3 dir = actor.transform.position - damage.source.transform.position;
                dir = dir.normalized * (isFacingUp ? 1f : -1f);
                ClipTransition clip = (!isFacingUp) ? damageAnims.knockdownFaceDown : damageAnims.knockdownFaceUp;
                AnimancerState state = animancer.Play(clip);

                Vector3 launchVector = dir * damage.kbForce.z + Vector3.up * damage.kbForce.y + Vector3.Cross(dir, Vector3.up) * damage.kbForce.x;

                actor.xzVel = launchVector;
                actor.xzVel.y = 0f;

                actor.yVel = launchVector.y;

                if (actor is PlayerActor player)
                {
                    player.StartGroundedLockout(0.25f);
                }

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
            } break;

            case DamageKnockback.StaggerType.Crumple:
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
            } break;

            case DamageKnockback.StaggerType.FallOver:
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
            } break;

            case DamageKnockback.StaggerType.SpinDeath:
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
            } break;

            case DamageKnockback.StaggerType.StaggerSmall:
            {
                Vector3 dir = (damage.source.transform.position - actor.transform.position).normalized;
                float xdot = Vector3.Dot(actor.transform.right, dir);
                float ydot = Vector3.Dot(actor.transform.forward, dir);

                if (hurt == null || animancer.States.Current != hurt || damage.cannotAutoFlinch)
                {
                    DirectionalMixerState state = (DirectionalMixerState)animancer.Play(damageAnims.staggerSmall);
                    state.Time = 0f;
                    state.ParameterX = xdot;
                    state.ParameterY = ydot;
                    state.Events.OnEnd = OnEnd;
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
            } break;

            case DamageKnockback.StaggerType.StaggerLarge:
            {
                Vector3 dir = (damage.source.transform.position - actor.transform.position).normalized;
                float xdot = Vector3.Dot(actor.transform.right, dir);
                float ydot = Vector3.Dot(actor.transform.forward, dir);

                isFacingUp = ydot > 0f;

                DirectionalMixerState state = (DirectionalMixerState)animancer.Play(damageAnims.staggerLarge);
                state.Time = 0f;
                state.ParameterX = xdot;
                state.ParameterY = ydot;
                state.Events.OnEnd = OnEnd;
                CheckFallContinuous(state, willKill);
                hurt = state;
                maxTime = state.RemainingDuration / state.Speed;
            } break;

            default:
            {
                ClipTransition clip = damageAnims.GetClipFromStaggerType(stagger);
                AnimancerState state = animancer.Play(clip);
                state.Events.OnEnd = OnEnd;
                CheckFallContinuous(state, willKill);
                hurt = state;
                actor.transform.rotation = Quaternion.LookRotation(-(actor.transform.position - damage.source.transform.position), Vector3.up);
                maxTime = clip.MaximumDuration / clip.Speed;
            } break;
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
            AdjustDefendingPosition(damage.source, damage.repositionLength, damage.repositionMaxDist);
            animancer.Layers[HumanoidAnimLayers.Flinch].Stop();
        }
    }

    public void GetParried()
    {
        animancer.Layers[HumanoidAnimLayers.Flinch].Stop();
        ClipTransition clip = guardBreak;
        AnimancerState state = animancer.Play(clip);
        state.Events.OnEnd = OnEnd;
        CheckFallContinuous(state, false);
        hurt = state;
        actor.OnHurt.Invoke();
        StartCritVulnerability(clip.MaximumDuration / clip.Speed);
    }

    public void AdjustDefendingPosition(GameObject attacker, float length = 1.5f, float maxAdjust = 0.25f)
    {
        if (attacker == null || !attacker.TryGetComponent<Actor>(out Actor attackerActor) || length < 0)
        {
            return;
        }

        Vector3 targetPosition = attacker.transform.position + (attacker.transform.forward * length);

        Vector3 moveVector = Vector3.MoveTowards(actor.transform.position, targetPosition, maxAdjust) - actor.transform.position;

        actor.GetComponent<CharacterController>().Move(moveVector);
    }

    public void SetBlockAccel(float duration = 2f)
    {
        if (actor is PlayerActor player)
        {
            player.SetSpeed(0);
            player.SetWalkAccel(player.blockHitAccel);
            player.VerifyAccelerationAfterDelay(duration);
        }
    }

    public void StartCritVulnerability(float time)
    {
        if (totalCritTime >= DamageKnockback.MAX_CRITVULN_TIME) return;
        if (time < critTime)
        {
            totalCritTime -= critTime - time;
        }
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
        return invuln != null && animancer.States.Current == invuln;
    }

    public void SetInvulnClip(AnimancerState state)
    {
        invuln = state;
    }

    public void StartInvulnerability(float duration)
    {
        actor.StartCoroutine(InvulnerabilityRoutine(duration));
    }
    
    public bool IsInvulnerable()
    {
        return isInvulnerable || IsInInvulnClip();
    }

    IEnumerator InvulnerabilityRoutine(float duration)
    {
        isInvulnerable = true;
        float clock = 0f;
        while (clock < duration)
        {
            if (actor.isInTimeState)
            {
                yield return new WaitWhile(() => actor.isInTimeState);
            }
            yield return new WaitForSeconds(0.25f);
            clock += 0.25f;
        }
        isInvulnerable = false;
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
        rise = hurt;
        hurt.Events.OnEnd = OnEnd;
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
            damage.bouncesOffTypedBlock = false;
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
        if (animancer.States.Current == state && state != null)
        {
            actor.OnCritVulnerable.Invoke();
        }
    }

    public bool IsCritVulnerable()
    {
        bool isCritVuln = animancer.States.Current == hurt && hurt != null && critTime > 0f;
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

    public void SetHitParticleVectors(Vector3 position, Vector3 direction)
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