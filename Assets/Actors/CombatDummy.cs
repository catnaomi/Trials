using Animancer;
using System.Collections;
using UnityEngine;

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
    public void Start()
    {
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

    public void Update()
    {
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
        /*
        lastDamage = damage.healthDamage;
        damageTaken += lastDamage;
        lastStaminaDamage = damage.staminaDamage;
        

        bool hitFromBehind = Vector3.Dot(-this.transform.forward, (damage.source.transform.position - this.transform.position).normalized) <= 0f;
        if (isBlocking && hitFromBehind)
        {
            stamina -= lastStaminaDamage;
            if (!damage.breaksBlock)
            {
                if (animancer.States.Current != hurt)
                {
                    ClipTransition clip = damageAnims.blockStagger;
                    hurt = animancer.Play(clip);
                    hurt.Events.OnEnd = _OnEnd;
                }
                else
                {
                    ClipTransition clip = damageAnims.blockStagger;
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
                stamina = 100f;
                ClipTransition clip = damageAnims.guardBreak;
                AnimancerState state = animancer.Play(clip);
                state.Events.OnEnd = _OnEnd;
                hurt = state;
            }
            this.transform.rotation = Quaternion.LookRotation(-(this.transform.position - damage.source.transform.position), Vector3.up);
        }
        else
        {
            AdjustDefendingPosition(damage.source);
            DamageKnockback.StaggerType stagger = damage.staggers.onHit;
            if (stagger == DamageKnockback.StaggerType.StaggerSmall)
            {
                Vector3 dir = (damage.source.transform.position - this.transform.position).normalized;
                float xdot = Vector3.Dot(this.transform.right, dir);
                float ydot = Vector3.Dot(this.transform.forward, dir);

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
                Vector3 dir = (damage.source.transform.position - this.transform.position).normalized;
                float xdot = Vector3.Dot(this.transform.right, dir);
                float ydot = Vector3.Dot(this.transform.forward, dir);

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
                this.transform.rotation = Quaternion.LookRotation(-(this.transform.position - damage.source.transform.position), Vector3.up);
            }

            Vector3 contactPosition = this.GetComponent<Collider>().ClosestPointOnBounds(damage.hitboxSource.GetComponent<SphereCollider>().bounds.center);

            if (damage.source.TryGetComponent<Actor>(out Actor actor))
            {
                actor.lastContactPoint = contactPosition;
            }
            damage.OnHit.Invoke();
            //FXController.CreateFX(FXController.FX.FX_BleedSword, contactPosition, Quaternion.identity, 1f);
        }
        */
    }
    public void AdjustDefendingPosition(GameObject attacker)
    {
        if (attacker == null || !attacker.TryGetComponent<Actor>(out Actor actor))
        {
            return;
        }

        float MAX_ADJUST = 0.25f;

        Vector3 targetPosition = attacker.transform.position + (attacker.transform.forward * 0.3f);

        Vector3 moveVector = Vector3.MoveTowards(this.transform.position, targetPosition, MAX_ADJUST) - this.transform.position;

        this.GetComponent<CharacterController>().Move(moveVector);
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
}