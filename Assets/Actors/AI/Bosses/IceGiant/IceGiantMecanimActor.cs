using Cinemachine;
using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class IceGiantMecanimActor : Actor, IAttacker, IDamageable
{
    [Header("Position Reference")]
    public Transform RightHand;
    public Transform LeftHand;
    public GameObject leftLeg;
    public DamageablePoint leftLegWeakPoint;
    public GameObject rightLeg;
    public DamageablePoint rightLegWeakPoint;
    public DamageablePoint weakPoint;
    [SerializeField,ReadOnly] Collider leftLegCollider;
    [SerializeField,ReadOnly] Collider rightLegCollider;
    Vector3 rootDelta;
    Quaternion animatorRotation;
    [Header("Navigation & AI")]
    public bool actionsEnabled = true;
    [SerializeField, ReadOnly] NavMeshAgent nav;
    public float closeRange = 8f;
    public float meleeRange = 3f;
    public float stompTimer = 15f;
    Coroutine stompCoroutine;
    public float maxRealignAngle = 30f;
    public float behindAngle = 90f;
    public float rotationSpeed = 90f;
    [Header("Weapons")]
    public float RightWeaponLength = 1f;
    public float RightWeaponRadius = 1f;
    [Space(15)]
    public float LeftWeaponLength = 1f;
    public float LeftWeaponRadius = 1f;
    [Header("Attacks")]
    public DamageKnockback tempDamage;
    public InputAttack stepShockwave;
    public float stepShockwaveRadius = 2f;
    public InputAttack harmlessShockwave;
    public InputAttack smallShockwave;
    public InputAttack largeShockwave;
    public float shockwaveRadius = 2f;
    public InputAttack groundShockwave;
    public float groundShockwaveRadius = 25f;
    public float spinAccel = 360f;
    public float spinAttackSpeed = 360f;
    public float spinVelocity;
    public bool spinning;
    public float maxIKHandDistance = 1f;
    public float ikHandOffset = -1;
    [Space(10)]
    public float nonActorGroundedThreshold = 1f;
    [Space(20)]
    public float getupDelay = 5f;
    float getupClock = 0f;
    HitboxGroup rightHitboxes;
    DamageKnockback lastTakenDamage;
    HitboxGroup leftHitboxes;
    public UnityEvent OnHitboxActive;
    bool isHitboxActive;
    [Header("Particles")]
    public IceGiantFXHelper fx;
    [Header("Mecanim Values")]
    [ReadOnly, SerializeField] bool Dead;
    [ReadOnly, SerializeField] bool IsFallen;
    [ReadOnly, SerializeField] bool Fall;
    [ReadOnly, SerializeField] bool InCloseRange;
    [ReadOnly, SerializeField] bool InMeleeRange;
    [ReadOnly, SerializeField] bool ShouldStomp;
    [ReadOnly, SerializeField] bool ActionsEnabled;
    [ReadOnly, SerializeField] bool StompLeft;
    [ReadOnly, SerializeField] bool TargetBehind;
    [ReadOnly, SerializeField] float AngleBetween;
    [ReadOnly, SerializeField] float AngleBetweenAbs;
    [ReadOnly, SerializeField] bool DeadHand;
    public float animated_TrackingHandIKWeight;
    Vector3 handIKPosition;
    public override void ActorStart()
    {
        base.ActorStart();
        animator = this.GetComponent<Animator>();
        GenerateWeapons();
        leftLegWeakPoint.OnHurt.AddListener(() => TakeDamageFromDamagePoint(leftLegWeakPoint));
        rightLegWeakPoint.OnHurt.AddListener(() => TakeDamageFromDamagePoint(rightLegWeakPoint));
        leftLegCollider = leftLegWeakPoint.GetComponent<Collider>();
        rightLegCollider = rightLegWeakPoint.GetComponent<Collider>();
        weakPoint.OnHurt.AddListener(() => TakeDamageFromDamagePoint(weakPoint));
        if (TryGetComponent<AnimationFXHandler>(out AnimationFXHandler fxHandler))
        {
            fxHandler.OnStepL.AddListener(StepShockwaveLeft);
            fxHandler.OnStepR.AddListener(StepShockwaveRight);
        }
        nav = GetComponent<NavMeshAgent>();
        nav.updatePosition = false;
        nav.updateRotation = false;
        StartCoroutine(DestinationCoroutine());
        fx = this.GetComponent<IceGiantFXHelper>();
        //EnableWeakPoint(false);
    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();

        if (IsFallen)
        {
            getupClock -= Time.deltaTime;
            if (getupClock <= 0f && !dead)
            {
                GetUp();
            }
        }

        UpdateTarget();

        if (CombatTarget != null)
        {
            InCloseRange = Vector3.Distance(this.transform.position, CombatTarget.transform.position) <= closeRange;
            InMeleeRange = Vector3.Distance(this.transform.position, CombatTarget.transform.position) <= meleeRange;
        }


        AngleBetween = Vector3.SignedAngle(nav.desiredVelocity.normalized, this.transform.forward, -Vector3.up);
        AngleBetweenAbs = Mathf.Abs(AngleBetween);
        UpdateMecanimValues();
    }

    void UpdateMecanimValues()
    {
        animator.SetBool("IsFallen", IsFallen);
        animator.UpdateTrigger("Fall", ref Fall);
        animator.SetBool("InCloseRange", InCloseRange);
        animator.SetBool("InMeleeRange", InMeleeRange);
        animator.UpdateTrigger("ShouldStomp", ref ShouldStomp);

        ActionsEnabled = actionsEnabled;
        animator.SetBool("ActionsEnabled", ActionsEnabled);
        animator.SetBool("StompLeft", StompLeft);
        TargetBehind = IsTargetBehind();
        animator.SetBool("TargetBehind", TargetBehind);
        animator.SetFloat("AngleBetween", AngleBetween);
        animator.SetFloat("AngleBetweenAbs", AngleBetweenAbs);

        animator.SetBool("Dead", Dead);
        animator.UpdateTrigger("DeadHand", ref DeadHand);
    }

    void UpdateTarget()
    {
        // TODO: allow targets that aren't the player
        if (CombatTarget == null && PlayerActor.player != null)
        {
            CombatTarget = PlayerActor.player.gameObject;
            if (stompCoroutine == null)
            {
                stompCoroutine = StartCoroutine(StompTimer());
            }
        } 
    }

    IEnumerator StompTimer()
    {
        float clock;
        while (actionsEnabled && !dead)
        {
            clock = stompTimer;
            while (clock > 0)
            {
                yield return new WaitForSeconds(1f);
                if (!isInTimeState)
                {
                    clock -= 1f;
                }
            }
            BeginStomp();
        }
    }

    IEnumerator DestinationCoroutine()
    {
        while (true)
        {
            if (CombatTarget != null && IsMoving() && nav.enabled)
            {
                nav.SetDestination(CombatTarget.transform.position);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }


    void OnAnimatorMove()
    {
        Vector3 diff = animator.rootPosition - this.transform.position;
        rootDelta = diff;
        
        animatorRotation = animator.rootRotation;
    }

    void FixedUpdate()
    {
        this.transform.position += rootDelta;
        this.transform.rotation = animatorRotation;
        if (spinning)
        {
            spinVelocity = Mathf.MoveTowards(spinVelocity, spinAttackSpeed, spinAccel * Time.fixedDeltaTime);
        }
        else
        {
            spinVelocity = Mathf.MoveTowards(spinVelocity, 0f, spinAccel * Time.fixedDeltaTime);
        }
        if (spinVelocity != 0f)
        {
            this.transform.Rotate(-Vector3.up, spinVelocity * Time.fixedDeltaTime);
        }
        if (IsRotating())
        {
            Vector3 dir = CombatTarget.transform.position - this.transform.position;
            dir.y = 0f;
            dir.Normalize();
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(dir), rotationSpeed * Time.fixedDeltaTime);
        }
        nav.nextPosition = transform.position;
    }

    void OnAnimatorIK(int layer)
    {
        Transform bone = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        Vector3 bonePosition = bone.position;

        if (isHitboxActive && CombatTarget != null)
        {
            handIKPosition = Vector3.MoveTowards(bonePosition, CombatTarget.transform.position, maxIKHandDistance);
            
        }
        handIKPosition.y = bonePosition.y + ikHandOffset;
        Debug.DrawLine(bonePosition, handIKPosition, Color.magenta, 1f);

        animator.SetIKPosition(AvatarIKGoal.LeftHand, handIKPosition);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, animated_TrackingHandIKWeight);
    }

    public void BeginStomp()
    {
        ShouldStomp = true;
        if (leftLegWeakPoint.health.current < rightLegWeakPoint.health.current)
        {
            StompLeft = true;
        }
        else
        {
            StompLeft = false;
        }
    }


    void GenerateWeapons()
    {
        if (rightHitboxes != null)
        {
            rightHitboxes.DestroyAll();
        }
        if (leftHitboxes != null)
        {
            leftHitboxes.DestroyAll();
        }
        rightHitboxes = Hitbox.CreateHitboxLine(RightHand.position, RightHand.up, RightWeaponLength, RightWeaponRadius, RightHand, new DamageKnockback(tempDamage), this.gameObject);
        leftHitboxes = Hitbox.CreateHitboxLine(LeftHand.position, LeftHand.up, LeftWeaponLength, LeftWeaponRadius, LeftHand, new DamageKnockback(tempDamage), this.gameObject);
    }
    public override void RealignToTarget()
    {
        if (CombatTarget != null)
        {
            Vector3 dir = CombatTarget.transform.position - this.transform.position;
            dir.y = 0f;
            dir.Normalize();
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(dir), maxRealignAngle);
        }
    }

    bool IsTargetBehind()
    {
        Vector3 dir = CombatTarget.transform.position - this.transform.position;
        dir.y = 0f;
        dir.Normalize();
        return Vector3.Angle(dir, this.transform.forward) > behindAngle;
    }
    public DamageKnockback GetLastDamage()
    {
        return currentDamage;
    }

    public override void SetCurrentDamage(DamageKnockback damageKnockback)
    {
        base.SetCurrentDamage(damageKnockback);
        rightHitboxes.SetDamage(currentDamage);
        leftHitboxes.SetDamage(currentDamage);
    }
    public void TakeDamageFromDamagePoint(DamageablePoint point)
    {
        float damageTaken = point.GetLastAmountTaken();
        bool willKill = damageTaken >= attributes.health.current;
        attributes.health.current -= point.GetLastAmountTaken();
        if (willKill)
        {
            Die();
        }
        if (point.hasHealth && (point.health.current <= 0f || willKill))
        {
            BreakDamageablePoint(point);
        }
        lastDamageTaken = point.GetLastTakenDamage();
        SetHitParticleVectors(point.GetHitPosition(), point.GetHitDirection());
        OnHurt.Invoke();
    }

    void BreakDamageablePoint(DamageablePoint point)
    {
        point.gameObject.SetActive(false);
        if (point == rightLegWeakPoint)
        {
            rightLeg.SetActive(false);
        }
        else if (point == leftLegWeakPoint)
        {
            leftLeg.SetActive(false);
        }
        FallOver();
    }

    void FixDamageablePoint(DamageablePoint point)
    {
        point.gameObject.SetActive(true);
        if (point == rightLegWeakPoint)
        {
            rightLeg.SetActive(true);
        }
        else if (point == leftLegWeakPoint)
        {
            leftLeg.SetActive(true);
        }
        point.health.current = point.health.max;
    }

    public void FallOver()
    {
        spinning = false;
        if (dead) return;
        getupClock = getupDelay;
        //EnableWeakPoint(true);
        weakPoint.StartCritVulnerability(getupDelay);
        IsFallen = true;
        Fall = true;
        spinning = false;
    }

    public void GetUp()
    {
        IsFallen = false;
        //EnableWeakPoint(false);
    }
    public void EnableWeakPoint(bool active)
    {
        weakPoint.gameObject.SetActive(active);
    }

    public override void Die()
    {
        if (dead) return;
        Dead = dead = true;
        OnDie.Invoke();
        StartCleanUp(15f);
        DeadHand = true;
    }
    public void HitboxActive(int active)
    {
        if (active == 1)
        {
            isHitboxActive = true;
            rightHitboxes.SetActive(true);
            leftHitboxes.SetActive(false);
        }
        else if (active == 2)
        {
            isHitboxActive = true;
            rightHitboxes.SetActive(false);
            leftHitboxes.SetActive(true);
        }
        else if (active == 0)
        {
            isHitboxActive = false;
            rightHitboxes.SetActive(false);
            leftHitboxes.SetActive(false);
        }
        if (isHitboxActive)
        OnHitboxActive.Invoke();
    }

    public void StartReformFoot()
    {
        bool isLeft = animator.GetCurrentAnimatorStateInfo(0).IsTag("STOMP_LEFT");
        fx.PlayReformFoot(isLeft);
        
    }

    public void ReformFoot()
    {
        bool isLeft = animator.GetCurrentAnimatorStateInfo(0).IsTag("STOMP_LEFT");
        Transform foot = (isLeft) ? leftLeg.transform : rightLeg.transform;
        DamageablePoint point = (isLeft) ? leftLegWeakPoint : rightLegWeakPoint;
        FixDamageablePoint(point);
    }
    public void Stomp(int left)
    {
        bool isLeft = animator.GetCurrentAnimatorStateInfo(0).IsTag("STOMP_LEFT");
        Transform foot = (isLeft) ? leftLeg.transform : rightLeg.transform;

        Vector3 position = foot.position;
        position.y = this.transform.position.y;

        StartCoroutine(StompShockwaveRoutine(position));

        fx.StompFX(isLeft);
        
    }
    IEnumerator StompShockwaveRoutine(Vector3 position)
    {
        Shockwave(position, shockwaveRadius, new DamageKnockback(largeShockwave.GetDamage()), true);
        yield return null;
        Shockwave(position, groundShockwaveRadius, new DamageKnockback(groundShockwave.GetDamage()), true);
    }
    public void HandShockwaveIn()
    {
        fx.HandShockwaveInFX();

    }
    public void HandShockwaveOut()
    {
        Vector3 position = LeftHand.position;
        position.y = this.transform.position.y;
        Shockwave(position, shockwaveRadius, new DamageKnockback(harmlessShockwave.GetDamage()), false);

        fx.HandShockwaveOutFX();

    }
    public void StepShockwaveLeft()
    {
        StepShockwave(-1);
    }

    public void StepShockwaveRight()
    {
        StepShockwave(1);
    }

    public void StepShockwave(int left)
    {
        bool isLeft = left == -1;
        Transform foot = (isLeft) ? leftLeg.transform : rightLeg.transform;
        Vector3 position = foot.position;
        position.y = this.transform.position.y;

        DamageablePoint point = (isLeft) ? leftLegWeakPoint : rightLegWeakPoint;

        if (point.health.current > 0)
        {
            Shockwave(position, stepShockwaveRadius, new DamageKnockback(stepShockwave.GetDamage()), false);
            fx.StepFX(position, false);
        }
        else
        {
            fx.StepFX(position, true);
        }       
    }

    void Shockwave(Vector3 position, float radius, DamageKnockback damage, bool groundedOnly)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius, Hitbox.GetHitboxMask());
        List<IDamageable> victims = new List<IDamageable>();

        damage.source = this.gameObject;
        foreach (Collider c in colliders)
        {
            if (c.transform.root == this.transform) continue;
            if (c.TryGetComponent<IDamageable>(out IDamageable victim))
            {
                if (victims.Contains(victim))
                {
                    continue;
                }
                else
                {
                    victims.Add(victim);
                }
            }
        }

        foreach (IDamageable v in victims)
        {
            // check if they are grounded
            if (groundedOnly)
            {
                if (v.GetGameObject().TryGetComponent<Actor>(out Actor actor) && !actor.IsGrounded())
                {
                    continue;
                }
                else if (v.GetGameObject().transform.position.y - this.transform.position.y > nonActorGroundedThreshold)
                {
                    continue;
                }
            }
            v.TakeDamage(damage);
        }
        Debug.DrawRay(position, Vector3.forward * radius, Color.red, 5f);
        Debug.DrawRay(position, Vector3.back * radius, Color.red, 5f);
        Debug.DrawRay(position, Vector3.right * radius, Color.red, 5f);
        Debug.DrawRay(position, Vector3.left * radius, Color.red, 5f);
    }

    public void Spin(int active)
    {
        spinning = active > 0;
        if (spinning)
        {
            fx.SpinFXStart();
            leftLegCollider.enabled = false;
            rightLegCollider.enabled = false;
        }
        else
        {
            leftLegCollider.enabled = true;
            rightLegCollider.enabled = true;
            fx.SpinFXStop();
        }
        
    }

    public void TakeDamage(DamageKnockback damage)
    {
        // do nothing, never takes damage directly
    }

    public void Recoil()
    {
        
    }

    public void StartCritVulnerability(float time)
    {
        
    }

    public bool IsCritVulnerable()
    {
        return false;
    }

    public void GetParried()
    {
        
    }

    public DamageKnockback GetLastTakenDamage()
    {
        return lastDamageTaken;
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }
    public override bool IsAttacking()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("ATTACK");
    }

    public bool IsMoving()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("MOVE") || IsRotating();
    }

    public bool IsRotating()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("ROTATE");
    }
}
