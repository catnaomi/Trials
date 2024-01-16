using Animancer;
using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class WyrmActor : Actor, INavigates, IDamageable, IAttacker, IHitboxHandler
{
    [HideInInspector]
    public NavMeshAgent nav;
    CharacterController cc;
    public Collider collider;

    public GameObject currentTarget;

    [Header("Movement & Navigation")]
    public float updateSpeed = 0.1f;
    [ReadOnly, SerializeField] Vector3 destination;
    public float targetDistance = 10f;
    [Header("Combat")]
    public Transform hitboxMount;
    public GameObject targetObject;
    public float hitboxRadius;

    [SerializeField, ReadOnly] Hitbox hitbox;
    public UnityEvent OnHitboxActive;
    public InputAttack chargeAttack;
    [Header("Animancer")]
    public LinearMixerTransition moveAnim;
    public ClipTransition hurtAnim;
    public ClipTransition deathAnim;
    WyrmAnimState state;

    struct WyrmAnimState
    {
        public LinearMixerState move;
        public AnimancerState hurt;
        public AnimancerState attack;
    }
    public override void ActorStart()
    {
        base.ActorStart();
        nav = GetComponent<NavMeshAgent>();
        nav.updatePosition = false;
        nav.updateRotation = false;
        nav.autoTraverseOffMeshLink = false;
        cc = GetComponent<CharacterController>();
        state = new WyrmAnimState();

        state.move = animancer.Play(moveAnim) as LinearMixerState;
        hitbox = Hitbox.CreateHitbox(hitboxMount.position, hitboxRadius, hitboxMount, new DamageKnockback(), this.gameObject);
        hitbox.SetActive(false);

        StartCoroutine(UpdateDestination());
    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();

        if (currentTarget != null)
        {
            targetDistance = Vector3.Distance(this.transform.position, currentTarget.transform.position);
        }

        UpdateStates();
    }

    #region State Machine
    void UpdateStates()
    {

    }
    #endregion
    
    public void StartChargeAttack()
    {
        this.transform.LookAt(currentTarget.transform, Vector3.up);
        state.attack = chargeAttack.ProcessGenericAction(this, _MoveOnEnd);
    }

    
    public bool CalculateRunningAwayDestination(Vector3 runningFromPosition)
    {
        Vector3 direction = (runningFromPosition - this.transform.position).normalized;
        bool isPointAvailable = NavMesh.SamplePosition(this.transform.position - direction * 10f, out NavMeshHit hit, 10f, 1);
        if (isPointAvailable && Vector3.Distance(this.transform.position, hit.position) > 10f)
        {
            destination = hit.position;
            return true;
        }
        else
        {
            return false;
        }
    }
    public Vector3 GetDestination()
    {
        return destination;
    }

    public void ResumeNavigation()
    {
        throw new System.NotImplementedException();
    }

    public void SetDestination(Vector3 position)
    {
        throw new System.NotImplementedException();
    }

    public void SetDestination(GameObject target)
    {
        throw new System.NotImplementedException();
    }

    public void StopNavigation()
    {
        throw new System.NotImplementedException();
    }

    public override void ProcessDamageKnockback(DamageKnockback damageKnockback)
    {
        TakeDamage(damageKnockback);
    }
    public void TakeDamage(DamageKnockback damage)
    {
        lastDamageTaken = damage;
        bool isCrit = (IsAttacking() && damage.GetTypes().HasType(DamageType.Piercing));
        damage.didCrit = isCrit;
        float damageAmount = DamageKnockback.GetTotalMinusResistances(damage.GetDamageAmount(isCrit), damage.unresistedMinimum, damage.GetTypes(), this.attributes.resistances);
        if (this.IsTimeStopped())
        {
            TimeTravelController.time.TimeStopDamage(damage, this, damageAmount);
            return;
        }
        if (damage.source != null)
        {
            this.transform.LookAt(damage.source.transform, Vector3.up);
            currentTarget = damage.source;
        }

        bool willKill = damageAmount >= attributes.health.current || isCrit;

        attributes.ReduceHealth(damageAmount);

        if (damage.hitboxSource != null)
        {
            Vector3 contactPosition = this.GetComponent<Collider>().ClosestPoint(damage.hitboxSource.GetComponent<SphereCollider>().bounds.center);

            if (damage.source.TryGetComponent<Actor>(out Actor sourceActor))
            {
                sourceActor.lastContactPoint = contactPosition;
                sourceActor.SetLastBlockpoint(damage.hitboxSource.GetComponent<SphereCollider>().bounds.center);
            }
        }


        if (willKill)
        {
            state.hurt = animancer.Play(deathAnim);
            state.hurt.Events.OnEnd = () =>
            {
                Die();
            };
        }
        else if (animancer.States.Current != state.hurt)
        {
            state.hurt = animancer.Play(hurtAnim);
            state.hurt.Events.OnEnd = _MoveOnEnd;
        }
        damage.OnHit.Invoke();
        OnHurt.Invoke();
    }

    void _MoveOnEnd()
    {
        animancer.Play(state.move);
    }

    
    public void Recoil()
    {
        state.hurt = animancer.Play(hurtAnim);
        state.hurt.Events.OnEnd = _MoveOnEnd;
    }

    public void StartCritVulnerability(float time)
    {

    }

    public void StopCritVulnerability()
    {

    }
    public DamageKnockback GetLastTakenDamage()
    {
        return lastDamageTaken;
    }

    IEnumerator UpdateDestination()
    {
        while (this.enabled)
        {
            yield return new WaitForSecondsRealtime(updateSpeed);
            if (nav.enabled) nav.SetDestination(destination);
        }
    }

    private void FixedUpdate()
    {
        Vector3 velocity = Vector3.zero;

        float gravity = (IsAttacking()) ? 0f : Physics.gravity.y;
        if (GetGrounded(out RaycastHit hitCheck1))
        {
            if (yVel <= 0)
            {
                yVel = 0f;
            }
            else
            {
                yVel += gravity * Time.fixedDeltaTime;
            }
            Vector3 groundY = new Vector3(this.transform.position.x, hitCheck1.point.y, this.transform.position.z);
            this.transform.position = Vector3.MoveTowards(this.transform.position, groundY, 0.2f * Time.fixedDeltaTime);
        }
        else if (IsAttacking())
        {
            yVel = 0f;
        }
        else
        {
            yVel += Physics.gravity.y * Time.fixedDeltaTime;
            if (yVel < -70f)
            {
                yVel = -70;
            }
            if (IsFalling()) velocity += xzVel;
        }
        velocity += yVel * Vector3.up * Time.fixedDeltaTime;
        cc.Move(velocity);
        if (!nav.isStopped)
        {
            cc.Move(nav.nextPosition - this.transform.position);
            nav.nextPosition = this.transform.position;
        }
    }

    public bool GetGrounded(out RaycastHit hitInfo)
    {
        Vector3 bottom = cc.bounds.center + cc.bounds.extents.y * Vector3.down * 0.9f;
        Vector3 top = cc.bounds.center + cc.bounds.extents.y * Vector3.up;
        bool hit = Physics.Raycast(top, Vector3.down, out hitInfo, cc.bounds.extents.y * 2f + 0.5f, MaskReference.Terrain);
        Debug.DrawLine(top, bottom + Vector3.down * 0.2f, hit ? Color.green : Color.red);
        return hit;
    }

    public void HitboxActive(int active)
    {
        HitboxActive(active > 0);
    }
    public void HitboxActive(bool active)
    {
        if (active)
        {
            hitbox.SetDamage(GetLastDamage());
            hitbox.SetActive(true);
            OnHitboxActive.Invoke();
        }
        else
        {
            hitbox.SetActive(false);
        }
    }


    public override bool IsAttacking()
    {
        return animancer.States.Current == state.attack;
    }

    public void GetParried()
    {
        state.hurt = animancer.Play(hurtAnim);
        state.hurt.Events.OnEnd = _MoveOnEnd;
        OnHurt.Invoke();
    }

    public bool IsCritVulnerable()
    {
        return false;
    }

    public void EnableActions()
    {
        // do nothing on mimics
    }

    public void StartInvulnerability(float duration)
    {
        throw new System.NotImplementedException();
    }

    public bool IsInvulnerable()
    {
        return false; //TODO: implement invulnerability?
    }
}
