using Animancer;
using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UIElements;

[RequireComponent(typeof(NavMeshAgent))]
public class WyrmActor : Actor, INavigates, IDamageable, IAttacker, IHitboxHandler
{
    [HideInInspector]
    public NavMeshAgent nav;
    CharacterController cc;
    public SphereCollider collider;

    public string statePreview;
    [Header("Movement & Navigation")]
    public bool actionsEnabled;
    bool wereActionsEnabled;
    public float updateSpeed = 0.1f;
    [ReadOnly, SerializeField] Vector3 destination;
    public float targetDistance = 10f;
    [Header("Combat")]
    public Transform hitboxMount;
    public GameObject targetObject;
    public float hitboxRadius;

    [SerializeField, ReadOnly] Hitbox hitbox;
    public UnityEvent OnHitboxActive;
    public bool hitboxActive;
    [Space(15)]
    public InputAttack chargeAttack;
    public float attackDuration = 2f;
    public float attackSpeed = 15f;
    public float attackAccel = 50f;
    public float attackDecel = 20f;
    public float attackCooldown = 5f;
    bool attackOnCooldown;
    [Header("Animancer")]
    public ClipTransition idleAnim;
    public LinearMixerTransition moveAnim;
    public ClipTransition hurtAnim;
    public ClipTransition deathAnim;
    public ClipTransition fallAnim;
    WyrmAnimState state;

    struct WyrmAnimState
    {
        public AnimancerState idle;
        public LinearMixerState move;
        public AnimancerState hurt;
        public AnimancerState attack;
        public AnimancerState fall;
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

        if (actionsEnabled)
        {
            EnableActions();
        }
        else
        {
            state.idle = animancer.Play(idleAnim);
        }
    }

    public void EnableActions()
    {
        actionsEnabled = true;
        this.StartTimer(updateSpeed, true, SetDestination);
    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();

        if (actionsEnabled && !wereActionsEnabled)
        {
            EnableActions();
        }
        wereActionsEnabled = actionsEnabled;

        UpdateStates();
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
        if (cc.enabled) cc.Move(velocity);


        if (nav.enabled)
        {
            if (cc.enabled)
            {
                cc.Move(nav.nextPosition - this.transform.position);
            }
            else
            {
                this.transform.Translate(nav.nextPosition - this.transform.position);
            }
            nav.nextPosition = this.transform.position;
            xzVel = nav.velocity;
        }

    }

    #region State Machine
    void UpdateStates()
    {
        if (animancer.States.Current == state.idle)
        {
            IdleState();
        }
        else if (animancer.States.Current == state.move)
        {
            MoveState();
        }
        else if (animancer.States.Current == state.attack)
        {
            AttackState();
        }
        else if (animancer.States.Current == state.fall)
        {
            FallState();
        }
    }

    void IdleState()
    {
        cc.enabled = true;
        collider.enabled = true;
        nav.enabled = false;
        nav.updatePosition = false;
        nav.updateRotation = false;

        if (actionsEnabled)
        {
            animancer.Play(state.move);
        }
        statePreview = "idle";
    }
    void MoveState()
    {
        cc.enabled = true;
        collider.enabled = true;
        nav.enabled = true;
        nav.updatePosition = false;
        nav.updateRotation = true;

        float dist = Vector3.Distance(this.transform.position, CombatTarget.transform.position);
        if (!attackOnCooldown && dist < targetDistance)
        {
            Vector3 origin = hitboxMount.position;
            Vector3 dir = GetDirectionToTarget();
            bool hit = Physics.SphereCast(origin, hitboxRadius, GetDirectionToTarget(), out RaycastHit rayhit, targetDistance, MaskReference.Terrain | MaskReference.Actors);
            bool hitTarget = (hit) && rayhit.collider.transform.root == CombatTarget.transform.root;
            if (!hit || hitTarget)
            {
                StartChargeAttack();
            }
            Debug.DrawRay(origin, GetDirectionToTarget() * targetDistance, (!hit || hitTarget) ? Color.green : Color.red);
        }
        statePreview = "move";
    }

    void AttackState()
    {
        cc.enabled = false;
        collider.enabled = false;
        nav.enabled = false;
        nav.updatePosition = false;
        nav.updateRotation = false;

        statePreview = "attack";
    }
    void FallState()
    {
        cc.enabled = true;
        collider.enabled = true;
        nav.enabled = false;
        nav.updatePosition = false;
        nav.updateRotation = false;

        if (GetGrounded(out RaycastHit hitCheck1))
        {
            this.transform.position = hitCheck1.point;
            yVel = 0f;
            animancer.Play(state.move);
        }
        statePreview = "fall";
    }

   #endregion

    public void StartChargeAttack()
    {
        state.attack = chargeAttack.ProcessGenericAction(this, _MoveOnEnd);
        StartCoroutine(ChargeAttackRoutine());
    }

    IEnumerator ChargeAttackRoutine()
    {
        while (!hitboxActive)
        {
            RotateTowardsTarget(nav.angularSpeed * Time.deltaTime);
            yield return null;
        }
        float speed = 0f;
        float clock = 0f;
        Vector3 dir = GetDirectionToTarget();
        RealignToTarget();
        // accelerate in dash
        while (clock < attackDuration)
        {
            yield return new WaitForFixedUpdate();
            clock += Time.fixedDeltaTime;
            speed = Mathf.MoveTowards(speed, attackSpeed, attackAccel * Time.fixedDeltaTime);
            Vector3 move = dir * speed * Time.fixedDeltaTime;
            // check collision

            move = ChargeAttackCheckCollision(move);
            this.transform.position += move;
            
            xzVel = dir * speed;
        }

        HitboxActive(false);
        attackOnCooldown = true;
        yield return new WaitForFixedUpdate();
        // check grounded when we're done dashing
        bool grounded = GetGrounded(out RaycastHit hitCheck1);
        
        if (grounded)
        {
            // decel if grounded
            while (speed > 0f)
            {
                yield return new WaitForFixedUpdate();
                speed = Mathf.MoveTowards(speed, 0f, attackDecel * Time.fixedDeltaTime);
                Vector3 move = dir * speed * Time.fixedDeltaTime;
                move = ChargeAttackCheckCollision(move);
                this.transform.position += move;
                xzVel = dir * speed;
            }
            animancer.Play(state.move);
        }
        else
        {
            state.fall = animancer.Play(fallAnim);
        }

        yield return new WaitForSeconds(attackCooldown);
        attackOnCooldown = false;
    }

    Vector3 ChargeAttackCheckCollision(Vector3 inMove)
    {
        Vector3 move = inMove;
        bool hit = Physics.SphereCast(collider.transform.position, collider.radius, move.normalized, out RaycastHit rayhit, move.magnitude, MaskReference.Terrain);
        if (hit)
        {
            if (rayhit.distance > collider.radius)
            {
                move = move.normalized * (rayhit.distance - collider.radius);
            }
            else
            {
                move = Vector3.zero;
            }
        }
        return move;
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

    void SetDestination()
    {
        if (CombatTarget != null && IsMoving() && nav.enabled)
        {
            SetDestination(CombatTarget.transform.position);
        }
    }
    public void SetDestination(Vector3 position)
    {
        nav.SetDestination(CombatTarget.transform.position);
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
        damage.result.didCrit = isCrit;
        float damageAmount = DamageKnockback.GetTotalMinusResistances(damage.GetDamageAmount(isCrit), damage.unresistedMinimum, damage.GetTypes(), this.attributes.resistances);
        if (this.IsTimeStopped())
        {
            TimeTravelController.time.TimeStopDamage(damage, this, damageAmount);
            return;
        }
        if (damage.source != null)
        {
            this.transform.LookAt(damage.source.transform, Vector3.up);
            CombatTarget = damage.source;
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
        hitboxActive = active;
    }


    public override bool IsAttacking()
    {
        return animancer.States.Current == state.attack;
    }

    public override bool IsFalling()
    {
        return animancer.States.Current == state.fall;
    }
    public void GetParried()
    {
        state.hurt = animancer.Play(hurtAnim);
        state.hurt.Events.OnEnd = _MoveOnEnd;
        OnHurt.Invoke();
    }

    public bool IsMoving()
    {
        return animancer.States.Current == state.move;
    }

    public bool IsCritVulnerable()
    {
        return false;
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
