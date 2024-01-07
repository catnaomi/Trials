using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class IceGolemMecanimActor : Actor, IAttacker, IDamageable, IAdjustRootMotion, INavigates
{
    HumanoidNPCInventory inventory;
    CapsuleCollider collider;
    CharacterController cc;
    HumanoidPositionReference positionReference;
    ActorTimeTravelHandler timeTravelHandler;
    [ReadOnly, SerializeField] NavMeshAgent nav;
    public float attackTimer = 2f;
    public float waterDashTimer = 3f;
    public float specialAttackTimer = 90f;
    public bool isHitboxActive;
    public UnityEvent OnHitboxActive;
    [Header("Navigation")]
    public bool actionsEnabled;
    public float closeRange = 4f;
    public float meleeRange = 2f;
    public float farRange = 10f;
    bool shouldRealign;
    float shouldRotateAmount;


    
    bool wasDashingLastFrame;
    [ReadOnly, SerializeField] bool isGrounded;
    [Header("Damageable")]
    public DamageAnims damageAnims;
    public float maximumHurtDuration = 2f;
    [SerializeField, ReadOnly]float hurtDuration;
    bool wasHurtLastFrame;
    SimplifiedDamageHandler damageHandler;
    [Header("Body Spin")]
    public bool isBodySpinning;
    public float bodySpinAccel = 90f;
    public float bodySpinMaxSpeed = 720f;
    public float bodySpinAngleDown = 30f;
    public bool spinFacing;
    public float bodySpinAngle;
    public float bodySpinSpeed;
    [Header("Particles")]
    public GameObject puddlePrefab;
    public GameObject puddleObject;
    public GameObject deathPrefab;
    [Header("Mecanim Values")]
    [ReadOnly, SerializeField] bool InCloseRange;
    [ReadOnly, SerializeField] bool InMeleeRange;
    [ReadOnly, SerializeField] bool InFarRange;
    [ReadOnly, SerializeField] bool ShouldAttack;
    [ReadOnly, SerializeField] bool ShouldDash;
    [ReadOnly, SerializeField] bool ShouldSpecial;
    [ReadOnly, SerializeField] bool ActionsEnabled;
    [ReadOnly, SerializeField] float Speed;
    [ReadOnly, SerializeField] bool InDamageAnim;
    [ReadOnly, SerializeField] float AngleBetween;
    [ReadOnly, SerializeField] float AngleBetweenAbs;
    [ReadOnly, SerializeField] bool Ambush;
    [Header("Events")]
    public UnityEvent StartDash;

    public override void ActorStart()
    {
        base.ActorStart();
        nav = GetComponent<NavMeshAgent>();
        cc = GetComponent<CharacterController>();
        collider = this.GetComponent<CapsuleCollider>();
        positionReference = this.GetComponent<HumanoidPositionReference>();
        damageHandler = new SimplifiedDamageHandler(this, damageAnims, animancer);
        timeTravelHandler = this.GetComponent<ActorTimeTravelHandler>();
        InitPuddle();
        if (timeTravelHandler != null)
        {
            timeTravelHandler.OnFreeze.AddListener(DeactivateHitboxes);
        }
        this.OnHurt.AddListener(DeactivateHitboxes);
        damageHandler.SetEndAction(StopDamageAnims);
        this.OnCorpseClean.AddListener(OnClean);
        if (actionsEnabled)
        {
            EnableActions();
        }
    }

    void Awake()
    {
        inventory = this.GetComponent<HumanoidNPCInventory>();
    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();
        GetGrounded();
        if (ActionsEnabled != actionsEnabled)
        {
            if (actionsEnabled)
            {
                EnableActions();
            }
            ActionsEnabled = actionsEnabled;
        }
        if (IsHurt())
        {
            UpdateMecanimValues();
            return;
        }
        else
        {
            InDamageAnim = false;
        }
        if (inventory.IsMainEquipped() && !inventory.IsMainDrawn())
        {
            inventory.SetDrawn(true, true);
        }
        if (inventory.IsOffEquipped() && !inventory.IsOffDrawn())
        {
            inventory.SetDrawn(false, true);
        }
        UpdateTarget();
        if (IsMoving())
        {
            nav.enabled = true;
            nav.updatePosition = true;
            nav.updateRotation = true;
            cc.enabled = false;
        }
        else
        {
            nav.enabled = false;
            nav.updatePosition = false;
            nav.updateRotation = false;
            cc.enabled = true;
        }
        if (shouldRealign || IsDashing() || IsStrafing())
        {
            ForceRealignToTarget();
            shouldRealign = false;
            shouldRotateAmount = 0f;
        }
        else if (shouldRotateAmount != 0f)
        {
            ForceRotateTowards(shouldRotateAmount);
            shouldRotateAmount = 0f;
        }
        if (IsDashing() && !wasDashingLastFrame)
        {
            StartDash.Invoke();
        }
        wasDashingLastFrame = IsDashing();
        if (CombatTarget != null)
        {
            float dist = Vector3.Distance(this.transform.position, CombatTarget.transform.position);
            InCloseRange = dist < closeRange;
            InMeleeRange = dist < meleeRange;
            InFarRange = dist < farRange;

            Vector3 dir = (CombatTarget.transform.position - this.transform.position);
            dir.y = 0f;
            dir.Normalize();
            AngleBetween = Vector3.SignedAngle(dir, this.transform.forward, -Vector3.up);
            AngleBetweenAbs = Mathf.Abs(AngleBetween);

        }
        
        UpdateMecanimValues();
    }

    void UpdateMecanimValues()
    {
        animator.SetBool("InCloseRange", InCloseRange);
        animator.SetBool("InMeleeRange", InMeleeRange); 
        animator.SetBool("InFarRange", InFarRange);
        animator.SetBool("ActionsEnabled", ActionsEnabled);
        animator.UpdateTrigger("ShouldAttack", ref ShouldAttack);
        Speed = nav.desiredVelocity.magnitude;
        animator.SetFloat("Speed", Speed);
        animator.UpdateTrigger("ShouldDash", ref ShouldDash);
        animator.SetBool("InDamageAnim", InDamageAnim);
        animator.SetFloat("AngleBetween", AngleBetween);
        animator.SetFloat("AngleBetweenAbs", AngleBetweenAbs);
        animator.UpdateTrigger("ShouldSpecial", ref ShouldSpecial);
        animator.UpdateTrigger("Ambush", ref Ambush);
    }
    void UpdateTarget()
    {
        if (CombatTarget == null && PlayerActor.player != null)
        {
            CombatTarget = PlayerActor.player.gameObject;
        }
    }

    void SetDestination()
    {
        if (CombatTarget != null && IsMoving() && nav.enabled)
        {
            nav.SetDestination(CombatTarget.transform.position);
        }
    }

    public bool ShouldAdjustRootMotion()
    {
        return CombatTarget != null && (IsAttacking() || IsDashing());
    }

    public Vector3 GetAdjustmentRelativePosition()
    {
        if (CombatTarget != null)
        {
            return CombatTarget.transform.position;
        }
        else
        {
            return Vector3.zero;
        }
    }
    public void BeginAmbush()
    {
        Ambush = true;
    }

    public void BeginAttack()
    {
        if (CanUpdate() && actionsEnabled)
        {
            ShouldAttack = true;
        }     
    }

    public void BeginWaterDash()
    {
        if (CanUpdate() && actionsEnabled)
        {
            ShouldDash = true;
        }
    }

    public void BeginSpecialAttack()
    {
        if (CanUpdate() && actionsEnabled)
        {
            ShouldSpecial = true;
        }
    }

    public void BeginSpin()
    {
        isBodySpinning = true;
    }

    public void EndSpin()
    {
        positionReference.Spine.localRotation = Quaternion.identity;
        isBodySpinning = false;
        bodySpinSpeed = 0f;
        bodySpinAngle = 0f;
    }
    void LateUpdate()
    {
        if (!CanUpdate())
        {
            nav.enabled = false;
            cc.enabled = false;
            collider.enabled = true;
            return;
        }
        if (isBodySpinning)
        {
            if (spinFacing)
            {
                Vector3 dir = (CombatTarget.transform.position - this.transform.position);
                dir.y = 0f;
                positionReference.Spine.rotation = Quaternion.LookRotation(dir.normalized) * Quaternion.Euler(0f, bodySpinAngle, 0f) * Quaternion.AngleAxis(bodySpinAngleDown, Vector3.Cross(Vector3.up, dir.normalized));
            }
            else
            {
                positionReference.Spine.localRotation = Quaternion.Euler(bodySpinAngleDown, bodySpinAngle, 0f);
            }
            if (CanUpdate())
            {
                bodySpinAngle += bodySpinSpeed * Time.deltaTime;
            }
            if (bodySpinAngle >= 360f)
            {
                HitboxActive(2);
            }
            bodySpinAngle %= 360f;
            bodySpinSpeed += bodySpinAccel * Time.deltaTime;
            if (bodySpinSpeed > bodySpinMaxSpeed)
            {
                bodySpinSpeed = bodySpinMaxSpeed;
            }
        }
        
    }
    /*
    * triggered by animation:
    * 0 = deactivate hitboxes
    * 1 = main weapon
    * 2 = off weapon, if applicable
    * 3 = both, if applicable
    * 4 = ranged weapon
    */
    public void HitboxActive(int active)
    {
        EquippableWeapon mainWeapon = inventory.GetMainWeapon();
        EquippableWeapon offHandWeapon = inventory.GetOffWeapon();
        EquippableWeapon rangedWeapon = inventory.GetRangedWeapon();
        bool main = (mainWeapon != null && mainWeapon is IHitboxHandler);
        bool off = (offHandWeapon != null && offHandWeapon is IHitboxHandler);
        bool ranged = (rangedWeapon != null && rangedWeapon is IHitboxHandler);
        if (active == 0)
        {
            if (main)
            {
                ((IHitboxHandler)mainWeapon).HitboxActive(false);
            }
            if (off)
            {
                ((IHitboxHandler)offHandWeapon).HitboxActive(false);
            }
            isHitboxActive = false;
        }
        else if (active == 1)
        {
            if (main)
            {
                ((IHitboxHandler)mainWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }
        else if (active == 2)
        {
            if (off)
            {
                ((IHitboxHandler)offHandWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }
        else if (active == 3)
        {
            if (main)
            {
                ((IHitboxHandler)mainWeapon).HitboxActive(true);
            }
            if (off)
            {
                ((IHitboxHandler)offHandWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }
        else if (active == 4)
        {
            if (ranged)
            {
                ((IHitboxHandler)rangedWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }

    }

    public override void RealignToTarget()
    {
        shouldRealign = true;
    }

    public void ForceRealignToTarget()
    {
        base.RealignToTarget();
    }
    public override void RotateTowardsTarget(float maxDegreesDelta)
    {
        shouldRotateAmount = maxDegreesDelta;
    }

    public void ForceRotateTowards(float maxDegreesDelta)
    {
        base.RotateTowardsTarget(maxDegreesDelta);
    }


    public override void DeactivateHitboxes()
    {
        HitboxActive(0);
    }
    public override bool IsHitboxActive()
    {
        return isHitboxActive;
    }

    public void StopDamageAnims()
    {
        InDamageAnim = false;
        animancer.Stop();
        BeginWaterDash();
    }

    public bool IsHurt()
    {
        return InDamageAnim && damageHandler.hurt != null && animancer.States.Current == damageHandler.hurt;
    }

    public bool GetGrounded()
    {
        isGrounded = ActorUtilities.GetGrounded(this.transform);
        return isGrounded;
    }

    public override bool IsGrounded()
    {
        return isGrounded;
    }
    public bool IsMoving()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("MOVE");
    }

    public bool IsStrafing() {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("STRAFE");
    }
    public bool IsDashing()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("DASH");
    }

    public override bool IsAttacking()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("ATTACK");
    }

    public void TakeDamage(DamageKnockback damage)
    {
        InDamageAnim = true;
        isBodySpinning = false;
        DeactivateHitboxes();
        ((IDamageable)damageHandler).TakeDamage(damage);
    }

    public void Recoil()
    {
        InDamageAnim = true;
        EndSpin();
        DeactivateHitboxes();
        ((IDamageable)damageHandler).Recoil();
    }

    public void StartCritVulnerability(float time)
    {
        ((IDamageable)damageHandler).StartCritVulnerability(time);
    }

    public bool IsCritVulnerable()
    {
        return ((IDamageable)damageHandler).IsCritVulnerable();
    }

    public void GetParried()
    {
        InDamageAnim = true;
        EndSpin();
        DeactivateHitboxes();
        ((IDamageable)damageHandler).GetParried();
    }

    public override void Die()
    {
        base.Die();
        InDamageAnim = true;
        DeactivateHitboxes();
        EndSpin();
    }

    public void OnClean()
    {
        Instantiate(deathPrefab, this.transform.position, Quaternion.identity);
    }
    public DamageKnockback GetLastTakenDamage()
    {
        return ((IDamageable)damageHandler).GetLastTakenDamage();
    }

    public GameObject GetGameObject()
    {
        return ((IDamageable)damageHandler).GetGameObject();
    }

    public void InitPuddle()
    {
        puddleObject = Instantiate(puddlePrefab);
        ParticleSystem particle = puddleObject.GetComponent<ParticleSystem>();
        RigidbodyFollowTransform follow = puddleObject.GetComponent<RigidbodyFollowTransform>();
        follow.target = this.transform;
        this.StartDash.AddListener(particle.Play);
        this.OnDie.AddListener(DestroyPuddle);
    }

    public void DestroyPuddle()
    {
        Destroy(puddleObject);
    }

    public void PlayPuddle()
    {
        puddleObject.GetComponent<ParticleSystem>().Play();
    }

    public void SetDestination(Vector3 position)
    {
        throw new System.NotImplementedException();
    }

    public void SetDestination(GameObject target)
    {
        throw new System.NotImplementedException();
    }

    public void ResumeNavigation()
    {
        throw new System.NotImplementedException();
    }

    public void StopNavigation()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetDestination()
    {
        throw new System.NotImplementedException();
    }

    public void EnableActions()
    {
        actionsEnabled = true;
        this.StartTimer(0.1f, true, SetDestination);
        this.StartTimer(attackTimer, true, BeginAttack);
        this.StartTimer(waterDashTimer, true, BeginWaterDash);
        this.StartTimer(specialAttackTimer, true, BeginSpecialAttack);
    }

    public void StartInvulnerability(float duration)
    {
        ((IDamageable)damageHandler).StartInvulnerability(duration);
    }

    public bool IsInvulnerable()
    {
        return ((IDamageable)damageHandler).IsInvulnerable();
    }

    public void StopCritVulnerability()
    {
        ((IDamageable)damageHandler).StopCritVulnerability();
    }
}
