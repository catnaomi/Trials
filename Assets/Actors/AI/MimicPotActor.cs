using Animancer;
using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(HumanoidPositionReference)), RequireComponent(typeof(NavMeshAgent))]
public class MimicPotActor : Actor, INavigates, IDamageable, IAttacker, IHitboxHandler
{
    [HideInInspector]
    public NavMeshAgent nav;
    CharacterController cc;
    Collider collider;

    public GameObject currentTarget;
    
    [Header("Movement & Navigation")]
    public bool runningAway;
    public float updateSpeed;
    public Vector3 destination;
    public float minimumRunDistance = 1f;
    public float corneredTargetDistance = 5f;
    public float safeDistance = 10f;
    public bool isCornered;
    public bool isSafeDistance;
    public float corneredTime = 2f;
    public float retaliationTime = 3f;
    public float targetDistance;
    float corneredClock;
    [Header("Carryable")]
    public bool isCarried;
    public MimicCarryable carryable;
    public Interactable carryInteract;
    bool thrown;
    public float throwYVelocity = 10f;
    [Header("Combat")]
    public Transform hitboxMount;
    public GameObject targetObject;
    public float hitboxRadius;
    [SerializeField, ReadOnly] Hitbox hitbox;
    public UnityEvent OnHitboxActive;
    public UnityEvent OnBiteActive;
    public InputAttack rollAttack;
    public InputAttack biteAttack;
    public InputAttack carryBiteAttack;
    public InputAttack standAttack;
    [Header("Animancer")]
    public ClipTransition idleAnim;
    public ClipTransition walkAnim;
    public LinearMixerTransition moveAnim;
    [Space(20)]
    public ClipTransition hidingAnim;
    public ClipTransition startHideAnim;
    public ClipTransition endHideAnim;
    [Space(20)]
    public ClipTransition hopAnim;
    public ClipTransition hurtAnim;
    public ClipTransition deathAnim;
    [Space(20)]
    public ClipTransition standAttackAnim;
    public ClipTransition rollAttackAnim;
    public ClipTransition biteAttackAnim;
    [Space(20)]
    public ClipTransition carryWiggleAnim;
    public ClipTransition thrownAnim;
    MimicPotAnimState state;

    DamageKnockback lastDamageTaken;

    struct MimicPotAnimState
    {
        public LinearMixerState move;
        public AnimancerState hidden;
        public AnimancerState hurt;
        public AnimancerState attack;
        public AnimancerState jump;
        public AnimancerState cornered;
    }
    public override void ActorStart()
    {
        base.ActorStart();
        nav = GetComponent<NavMeshAgent>();
        nav.updatePosition = false;
        nav.updateRotation = false;
        nav.autoTraverseOffMeshLink = false;
        animancer = GetComponent<AnimancerComponent>();
        cc = GetComponent<CharacterController>();
        collider = GetComponent<SphereCollider>();
        state = new MimicPotAnimState();
        state.hidden = animancer.States.GetOrCreate(hidingAnim);
        state.move = (LinearMixerState)animancer.States.GetOrCreate(moveAnim);
        animancer.Play(state.hidden);

        hitbox = Hitbox.CreateHitbox(hitboxMount.position, hitboxRadius, hitboxMount, new DamageKnockback(), this.gameObject);
        hitbox.SetActive(false);

        carryable.OnStartCarry.AddListener(StartCarry);
        carryable.OnStopCarry.AddListener(StopCarry);
        carryable.OnThrow.AddListener(OnCarryThrow);
        StartCoroutine(UpdateDestination());
    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();

        if (currentTarget != null)
        {
            targetDistance = Vector3.Distance(this.transform.position, currentTarget.transform.position);
        }

        if (animancer.States.Current == state.hidden)
        { //hiding
            // do nothing
            //nav.updateRotation = false;
            nav.updateRotation = false;
            nav.isStopped = true;
            nav.nextPosition = this.transform.position;
            if (thrown)
            {
                thrown = false;
                EndHide();
            }
        }
        else if (animancer.States.Current == state.move)
        {
            nav.updateRotation = true;

            state.move.Parameter = Mathf.Clamp01(nav.desiredVelocity.magnitude / nav.speed);
            nav.isStopped = false;
            if (corneredClock > corneredTime)
            {
                if (isSafeDistance)
                {
                    StartHide();
                }
                else
                {
                    state.cornered = animancer.Play(idleAnim);
                }       
            }
        }
        else if (animancer.States.Current == state.cornered)
        {
            nav.updateRotation = false;
            nav.isStopped = true;
            Vector3 lookDirection = (currentTarget.transform.position- this.transform.position).normalized;
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(lookDirection), nav.angularSpeed * Time.deltaTime);

            if (!isCornered)
            {
                animancer.Play(state.move);
            }
            else
            {
                if (corneredClock > corneredTime + retaliationTime)
                {
                    float r = Random.value;

                    if (r > 0.9f)
                    {
                        // do nothing
                        corneredClock = corneredTime;
                    }
                    else if (r > 0.6f)
                    {
                        // roll attack
                        StartRollAttack();
                    }
                    else if (r > 0.3f)
                    {
                        // bite attack
                        StartBiteAttack();
                    }
                    else
                    {
                        // stand attack
                        StartStandingAttack();
                    }
                    
                }
            }
        }
        else
        {
            nav.isStopped = true;
            nav.nextPosition = this.transform.position;
        }
        if (isCornered || isSafeDistance)
        {
            corneredClock += Time.deltaTime;
        }
        else
        {
            corneredClock = 0f;
        }

        if (isCarried)
        {

            this.collider.enabled = false;
            cc.enabled = false;
            if (!IsJumping() && carryable.carryPosition != Vector3.zero)
            {
                this.transform.position = carryable.carryPosition;
                nav.isStopped = true;
                nav.nextPosition = this.transform.position;
            }

        }
        else
        {
            cc.enabled = true;
            this.collider.enabled = true;
        }

        if (animancer.States.Current == state.attack)
        {
            nav.updateRotation = false;
        }

        if ((targetObject.activeSelf && IsHidden()) || isCarried)
        {
            targetObject.SetActive(false);
        }
        else if ((!targetObject.activeSelf && !IsHidden()) && !isCarried)
        {
            targetObject.SetActive(true);
        }

        carryInteract.canInteract = IsHidden() && !isCarried;
    }
    public void StartHide()
    {
        AnimancerState startHideState = animancer.Play(startHideAnim);
        startHideState.Events.OnEnd = () =>
        {
            animancer.Play(state.hidden);
        };
    }

    public void EndHide()
    {
        AnimancerState endHide = animancer.Play(endHideAnim);
        endHide.Events.OnEnd = () =>
        {
            animancer.Play(state.move);
        };
    }

    public void StartRollAttack()
    {
        this.transform.LookAt(currentTarget.transform, Vector3.up);
        state.attack = rollAttack.ProcessGenericAction(this, _MoveOnEnd);
    }

    public void StartBiteAttack()
    {
        
        state.jump = animancer.Play(hopAnim);
        this.transform.LookAt(currentTarget.transform, Vector3.up);
        state.jump.Events.OnEnd = () =>
            {
                Vector3 targetPosition = currentTarget.transform.position;// + (Vector3.up * 0.5f);
                transform.rotation = Quaternion.LookRotation(targetPosition - this.transform.position);
                state.jump = state.attack = biteAttack.ProcessGenericAction(this, _OnBiteEnd);
            };
    }

    public void StartCarryBite()
    {
        state.jump = state.attack = carryBiteAttack.ProcessGenericAction(this, _OnBiteEnd);
        if (currentTarget.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.Recoil();
        }
    }

    public void StartStandingAttack()
    {
        this.transform.LookAt(currentTarget.transform, Vector3.up);
        state.attack = standAttack.ProcessGenericAction(this, _MoveOnEnd);
    }

    public void StartCarryAttack()
    {
        thrown = false;
        AnimancerState warning = animancer.Play(carryWiggleAnim);
        warning.Events.OnEnd = () => {
            if (isCarried && !thrown)
            {
                StartCarryBite();
            }
            else
            {
                animancer.Play(state.hidden);
                isCarried = false;
            }
        };
    }

    public void OnCarryThrow()
    {
        thrown = true;
        this.transform.rotation = Quaternion.LookRotation((this.transform.position - currentTarget.transform.position).normalized, Vector3.up);
        AnimancerState throwState = animancer.Play(thrownAnim);
        yVel = -throwYVelocity;
        state.jump = throwState;
        throwState.Events.OnEnd = _MoveOnEnd;
    }

    public void StartCarry()
    {
        isCarried = true;
        currentTarget = PlayerActor.player.gameObject;
        StartCarryAttack();
    }
    public void StopCarry()
    {
        isCarried = false;
    }
    public bool CalculateRunningAwayDestination(Vector3 runningFromPosition)
    {
        Vector3 direction = (runningFromPosition - this.transform.position).normalized;
        bool isPointAvailable = NavMesh.SamplePosition(this.transform.position - direction * 10f, out NavMeshHit hit, 10f, 1);
        if (isPointAvailable && Vector3.Distance(this.transform.position,hit.position) > minimumRunDistance)
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
            state.hurt.Events.OnEnd = EndHide;
        }
        damage.OnHit.Invoke();
        OnHurt.Invoke();
    }

    void _MoveOnEnd()
    {
        animancer.Play(state.move);
        isCarried = false;
    }

    void _OnBiteEnd()
    {
        Vector3 forward = this.transform.forward;
        forward = Vector3.ProjectOnPlane(forward, Vector3.up);
        //this.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        if (PlayerActor.player.isCarrying && PlayerActor.player.carryable == carryable)
        {
            PlayerActor.player.StopCarrying();
        }
        _MoveOnEnd();
        //_UnhideOnEnd();
    }
    public void Recoil()
    {
        state.hurt = animancer.Play(hurtAnim);
        state.hurt.Events.OnEnd = EndHide;
    }

    public void StartCritVulnerability(float time)
    {
        throw new System.NotImplementedException();
    }

    public DamageKnockback GetLastTakenDamage()
    {
        return lastDamageTaken;
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }

    public DamageKnockback GetLastDamage()
    {
        return currentDamage;
    }

    IEnumerator UpdateDestination()
    {
        while (this.enabled)
        {
            yield return new WaitForSecondsRealtime(updateSpeed);
            if (currentTarget != null)
            {
                bool canFindDestination = CalculateRunningAwayDestination(currentTarget.transform.position);
                bool withinCorneredRange = Vector3.Distance(currentTarget.transform.position, this.transform.position) < corneredTargetDistance;
                bool withinSafeRange = Vector3.Distance(currentTarget.transform.position, this.transform.position) >= safeDistance;
                isCornered = withinCorneredRange && !canFindDestination;
                isSafeDistance = withinSafeRange;
            }
            if (nav.enabled) nav.SetDestination(destination);
        }
    }

    private void FixedUpdate()
    {
        Vector3 velocity = Vector3.zero;

        if (GetGrounded(out RaycastHit hitCheck1))
        {
            if (yVel <= 0)
            {
                yVel = 0f;
            }
            else
            {
                yVel += Physics.gravity.y * Time.fixedDeltaTime;
            }
            Vector3 groundY = new Vector3(this.transform.position.x, hitCheck1.point.y, this.transform.position.z);
            this.transform.position = Vector3.MoveTowards(this.transform.position, groundY, 0.2f * Time.fixedDeltaTime);
        }
        else if (IsJumping() || isCarried)
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

    void OnAnimatorMove()
    {
        // Update position based on animation movement using navigation surface height
        if (animancer == null || animancer.Animator == null || nav == null) return;
        Vector3 position = animancer.Animator.rootPosition + yVel * Vector3.up;
        transform.rotation = animancer.Animator.rootRotation;
        if (!IsJumping() && !isCarried)
        {
            position.y = nav.nextPosition.y;
        }
        
        Vector3 dir = position - this.transform.position;
        if (!Physics.SphereCast(this.transform.position + (Vector3.up), 0.25f, dir, out RaycastHit hit, dir.magnitude, MaskReference.Terrain))
        {
            if (cc.enabled)
            {
                cc.Move(position - transform.position);
            }
            else
            {
                this.transform.position = position;
            }
        }
        //animatorVelocity = animancer.Animator.velocity;

    }

    public bool GetGrounded(out RaycastHit hitInfo)
    {
        Collider c = this.GetComponent<Collider>();
        Vector3 bottom = c.bounds.center + c.bounds.extents.y * Vector3.down * 0.9f;
        Vector3 top = c.bounds.center + c.bounds.extents.y * Vector3.up;
        bool hit = Physics.Raycast(top, Vector3.down, out hitInfo, c.bounds.extents.y * 2f + 0.5f, MaskReference.Terrain);
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
            if (IsJumping())
            {
                OnBiteActive.Invoke();
            }
        }
        else
        {
            hitbox.SetActive(false);
        }
    }

    public bool IsJumping()
    {
        return animancer.States.Current == state.jump;
    }

    public bool IsHidden()
    {
        return animancer.States.Current == state.hidden;
    }

    public override bool IsAttacking()
    {
        return animancer.States.Current == state.attack;
    }

    public void GetParried()
    {
        state.hurt = animancer.Play(hurtAnim);
        state.hurt.Events.OnEnd = EndHide;
        OnHurt.Invoke();
    }

    public bool IsCritVulnerable()
    {
        return false;
    }
}
