using UnityEngine;
using System.Collections;
using CustomUtilities;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

[RequireComponent(typeof(Inventory))]
public class HumanoidActor : Actor
{

    protected CharacterController cc;
    protected Rigidbody rigidbody;

    [HideInInspector]
    public Collider boundingCollider;
    [HideInInspector]
    public Collider[] joints;


    
    protected float stunLength;
    protected float stunAmount;
    private float slideAmount;
    protected float slideAccel;


    private float autoGetupVelocity = 0.1f;
    private float helplessGetupDelay = 1f;
    private float autoGetupDelay = 3f;
    private float forceGetupDelay = 10f;
    private float ragdollClock;
    private float ragdollStillClock;


    private float reviveDelay = 30f;
    private float reviveClock;

    protected float airTime = 0f;
    private float fallDamage = 0f;

    [HideInInspector] public Vector3 lastForce;

    [HideInInspector] public bool isInvulnerable;

    public UnityEvent OnSheathe;
    public UnityEvent OnOffhandAttack;
    public UnityEvent OnAttack;
    public UnityEvent OnBlock;
    public UnityEvent OnDodge;
    public UnityEvent OnInjure;
    public UnityEvent OnHitboxActive;
    public UnityEvent OnParry;
    //public StanceHandler stance;

    [ReadOnly] public IKHandler aimIKHandler;
    public float heft;

    public float mainWeaponAngle;
    public float offWeaponAngle;

    public bool wasAttackingLastFrame;
    public bool isGrounded;

    public bool shouldSneak;

    float fallClock;

    public bool shouldForceLeft;

    bool isHitboxActive;

    [Range(-1f,1f)]
    public float tempValue1;
    [Range(-1f, 1f)]
    public float tempValue2;
    [Range(-1f, 1f)]
    public float tempValue3;

    public Vector3 gravity;

    [Header("Humanoid Settings")]
    public HumanoidState humanoidState;
    public HumanoidPositionReference positionReference;
    public HumanoidNPCInventory inventory;
    public bool canRevive = true;

    public BladeWeapon.AttackType nextAttackType;

    //public ActionsLibrary.BlockType blockType;
    [Header("Movement Settings")]
    public float BaseMovementSpeed = 5f;
    public float ForwardMultiplier = 1f;
    public float StrafeMultiplier = 0.5f;
    public float BackwardsMultiplier = 1f;
    public float WeaponDrawnMultiplier = 0.9f;
    public float BlockMultiplier = 0.5f;
    public float SprintMultiplier = 4f;
    public float SneakMultiplier = 0.5f;
    public float AimMultipler = 0.5f;

    public enum HumanoidState
    {
        Actionable,
        Ragdolled,
        Helpless,
        Dead
    }

    private void Awake()
    {
        positionReference.LocateSlotsByName();
    }
    public override void ActorStart()
    {
        base.ActorStart();

        

        if (inventory == null)
        {
            inventory = this.GetComponent<HumanoidNPCInventory>();
        }

        //inventory.Init();

        //inventory.OnChange.AddListener(GetStance);

        //GetStance();

        cc = GetComponent<CharacterController>();
        rigidbody = GetComponent<Rigidbody>();
        boundingCollider = GetComponent<Collider>();

        SetHeft(1f);

        //animator.SetFloat("Agility", 1f);

        

        attributes.ResetAttributes();

        /*
        OnSheathe = new UnityEvent();
        OnOffhandAttack = new UnityEvent();
        OnAttack = new UnityEvent();
        OnBlock = new UnityEvent();
        OnDodge = new UnityEvent();
        OnInjure = new UnityEvent();
        OnHitboxActive = new UnityEvent();
        */
    }

    public override void ActorPreUpdate()
    {
        if (humanoidState != HumanoidState.Ragdolled && cc.enabled && !IsJumping())
        {
            cc.Move(gravity);
        }
    }
    public override void ActorPostUpdate()
    {
        if (humanoidState == HumanoidState.Ragdolled)
        {
            TryGetup();
            //staminaClock = 0f;
        }
        else if (humanoidState == HumanoidState.Helpless)
        {
            if (canRevive)
            {
                TryRevive();
            }
        }

        if (humanoidState != HumanoidState.Ragdolled && Vector3.Distance(positionReference.Hips.transform.position, this.transform.position) > 2f)
        {
            positionReference.Hips.transform.position = this.transform.position;
        }

        if (IsSprinting())
        {
            //staminaClock = 0f;
            /*
            if (!attributes.HasstaminaRemaining())
            {
                animator.SetBool("Sprinting", false);
            }
            */
            attributes.ReduceAttribute(attributes.stamina, 10f * Time.deltaTime);
            //attributes.ReduceAttributeToMin(attributes.poise, 50f * Time.deltaTime, 50f);
        }

        /*
        if (IsDodging())
        {
            isInvulnerable = true;
        }
        else
        {
            isInvulnerable = false;
        }
        */
        /*
        animator.SetBool("WeaponDrawn", inventory.IsWeaponDrawn());
        animator.SetInteger("BlockType", (int)blockType);
        animator.SetBool("Aiming", IsAiming());
        animator.SetBool("Injured", IsInjured());
        */
        //animator.SetBool("Grounded", GetGrounded());
        //animator.SetBool("BlendMovement", animator.GetCurrentAnimatorStateInfo(0).IsTag("BLEND_MOVE"));

        animator.SetBool("InImpactState", IsInImpactState());
        animator.SetBool("Armed", inventory.IsWeaponDrawn());

        inventory.UpdateWeapon();

        wasAttackingLastFrame = IsAttacking();        
        
    }

    
    
    protected void LateUpdate()
    {
        if (!GetGrounded())
        {
            airTime += Time.deltaTime;
            fallDamage = (100f / 3f) * airTime;
            animator.SetFloat("AirTime", airTime);
        }
        else
        {
            airTime = 0f;
        }

        if (IsHanging())
        {
            airTime = 0f;
            animator.SetFloat("AirTime", 0f);
            animator.SetFloat("AirTimeReal", 0f);
        }
        animator.SetFloat("AirTimeReal", airTime);


        this.isGrounded = GetGrounded();
        animator.SetBool("Grounded", GetGrounded());

        if (IsAiming() && aimIKHandler != null)
        {
            aimIKHandler.OnUpdate(this);
        }
    }

    public bool GetGrounded()
    {
        // return cc.isGrounded;
        Collider c = this.GetComponent<Collider>();
        Vector3 bottom = c.bounds.center + c.bounds.extents.y * Vector3.down;
        return Physics.Raycast(bottom, Vector3.down, 0.2f, LayerMask.GetMask("Terrain"));
    }

    protected void OnAnimatorIK(int layerIndex)
    {
        if (IsAiming() && aimIKHandler != null)
        {
            aimIKHandler.OnIK(this.animator);
        }
    }
    protected void FixedUpdate()
    {
        inventory.FixedUpdateWeapon();
        if (!GetGrounded() && !IsJumping() && !IsHanging())
        {
            gravity += Physics.gravity * Time.fixedDeltaTime;
        }
        else
        {
            gravity = Vector3.zero;
        }
    }

    public void TryGetup()
    {
        if (attributes.HasHealthRemaining() || this is PlayerActor)
        {
            Getup();
        }
        else
        {
            StartHelpless();
        }
    }

    public void Getup()
    {
        animator.SetTrigger("GetUp");

        attributes.RecoverAttribute(attributes.stamina, 50f);

        humanoidState = HumanoidState.Actionable;
    }

    public void StartHelpless()
    {
        animator.SetBool("Helpless", true);
        humanoidState = HumanoidState.Helpless;
    }

    public void StopHelpless()
    {
        reviveClock = 0f;
        animator.SetBool("Helpless", false);
        humanoidState = HumanoidState.Actionable;
    }

    public virtual bool ShouldHelpless()
    {
        return false;
    }

    public void TryRevive()
    {
        reviveClock += Time.deltaTime;


        if (reviveClock > reviveDelay)
        {
            {
                StopHelpless();
            }
        }
    }
    public void StandingDeath()
    {
        animator.SetTrigger("StandingDeath");
        animator.SetBool("Helpless", true);
        animator.SetBool("FacingUp", false);

        humanoidState = HumanoidState.Helpless;
    }

    public void Kneel()
    {
        //TakeAction(ActionsLibrary.GetInputAction("Kneel"));
        animator.SetBool("Helpless", true);
        animator.SetBool("FacingUp", false);

        humanoidState = HumanoidState.Helpless;
    }
    public void Die()
    {
        this.gameObject.tag = "Corpse";
        //animator.SetBool("Helpless", true);
        animator.SetBool("Dead", true);
        humanoidState = HumanoidState.Dead;
        OnDie.Invoke();
        StartCleanUp();
    }

    public bool Vulnerable()
    {
        return !IsProne();
        switch (humanoidState)
        {
            case HumanoidState.Ragdolled:
            //case HumanoidState.Helpless:
                return false;

            default:
                return true;
        }
    }

    public override void ProcessDamageKnockback(DamageKnockback damageKnockback)
    {
        
        if (!Vulnerable())
        {
            return;
        }
        
        // as flowchart

        AdjustDefendingPosition(damageKnockback.source);

        //  implement resistances
        float totalDamage = DamageKnockback.GetTotalMinusResistances(damageKnockback.healthDamage, damageKnockback.GetTypes(), this.attributes.resistances);

        bool willKill = attributes.HasHealthRemaining() && (totalDamage >= attributes.health.current);

        if (this.IsDodging() || isInvulnerable)
        {
            // do nothing
            // slowdown effect on player dodge!
            OnDodge.Invoke();
        }
        else if (this.IsParrying() && attributes.HasAttributeRemaining(attributes.stamina) && !damageKnockback.unblockable) // is actor parrying with stamina remaining
        {
            // take no damage / stamina damage, and stagger human opponents
            // todo: Reimplement parrying
            ParryAtk(damageKnockback);
        }
        else if (this.IsBlocking() && !damageKnockback.unblockable) // is actor blocking. cannot die through block.
        {
            // blocking deals stamina damage
            attributes.ReduceAttribute(attributes.stamina, damageKnockback.staminaDamage);
            //attributes.ReducePoise(damageKnockback.poiseDamage);
            //Damage(damageKnockback, true); don't take health damage through blocks

            /*
            if (!attributes.HasAttributeRemaining(attributes.health))
            { // injure!
                this.OnInjure.Invoke();
            }
            */
            if (attributes.HasAttributeRemaining(attributes.stamina))
            {
                ProcessStagger(DamageKnockback.StaggerType.BlockStagger, damageKnockback);
            }
            else
            {
                ProcessStagger(DamageKnockback.StaggerType.GuardBreak, damageKnockback);
            }
        }
        else // get hit
        {
            Damage(damageKnockback, this.IsCritVulnerable());
            if (willKill)
            {
                if (!animator.GetBool("Helpless"))
                {
                    ProcessStagger(damageKnockback.staggers.onKill, damageKnockback);
                }
                Die();
            }
            else if (IsArmored() && !damageKnockback.breaksArmor)
            {
                ProcessStagger(damageKnockback.staggers.onArmorHit, damageKnockback);
            }
            else if (IsCritVulnerable() || damageKnockback.forceCritical)
            {
                ProcessStagger(damageKnockback.staggers.onCritical, damageKnockback);
            }
            else
            {
                ProcessStagger(damageKnockback.staggers.onHit, damageKnockback);
            }
            if (ShouldHelpless())
            {
                animator.SetBool("Helpless", true);
            }

            if (damageKnockback.disarm)
            {
                Disarm(damageKnockback);
            }
        }
    }

    public void ProcessStagger(DamageKnockback.StaggerType type, DamageKnockback damageKnockback)
    {
        bool turn = false;
        bool isBlock = type == DamageKnockback.StaggerType.BlockStagger || type == DamageKnockback.StaggerType.GuardBreak;
        if (isBlock)
        {
            OnBlock.Invoke();

        }
        else
        {
            OnHurt.Invoke();
        }

        Vector3 turnTowards = new Vector3(damageKnockback.kbForce.x, 0, damageKnockback.kbForce.z);

        float sideStagger = -Vector3.Dot(turnTowards, this.transform.right);
        float forwardStagger = Vector3.Dot(turnTowards, this.transform.forward);
        animator.SetFloat("Impact-SideStagger", sideStagger);
        animator.SetFloat("Impact-ForwardStagger", forwardStagger);
        turnTowards = -(turnTowards.normalized);
        if (turn)
        { 
            transform.LookAt(transform.position + turnTowards);
        }

        // TODO: fx flexibility
        FXController.FX fx = (!isBlock) ? FXController.FX.FX_Hit : FXController.FX.FX_Block;
        FXController.CreateFX(fx, GetFXPosition(damageKnockback), Quaternion.LookRotation(turnTowards), 2f, damageKnockback.hitClip);

        if (!IsProne())
        {
            AnimatorImpact(type);
        }
        else
        {
            AnimatorImpact(DamageKnockback.StaggerType.Flinch);
        }

        if (type == DamageKnockback.StaggerType.BlockStagger)
        {
            if (damageKnockback.source != null && damageKnockback.source.TryGetComponent<HumanoidActor>(out HumanoidActor humanoid))
            {
                humanoid.BlockRecoil();
            }
        }
    }

    public void Disarm(DamageKnockback damageKnockback)
    {
        if (this.inventory.IsMainDrawn())
        {
            EquippableWeapon weapon = inventory.GetMainWeapon();
            inventory.UnequipMainWeapon();
            inventory.Remove(weapon);
            LooseItem li = LooseItem.CreateLooseItem(weapon);
            li.gameObject.transform.position = this.transform.position + Vector3.up;
            li.GetComponent<Rigidbody>().AddForce(this.transform.up * 2.5f, ForceMode.Impulse);
        }
    }
    public void AnimatorImpact(DamageKnockback.StaggerType type)
    {
        animator.SetInteger("ImpactType", (int)type);
        animator.SetTrigger("Impact");
    }

    public void BlockRecoil()
    {
        if (!IsArmored() && !IsAiming())
        {
            AnimatorImpact(DamageKnockback.StaggerType.Recoil);
        }
    }

    public bool Damage(DamageKnockback damageKnockback, bool isCritical)
    {
        // account for resistances
        float critMult = (isCritical) ? damageKnockback.criticalMultiplier : 1f;
        float totalDamage = (!isCritical) ? DamageKnockback.GetTotalMinusResistances(damageKnockback.healthDamage, damageKnockback.GetTypes(), this.attributes.resistances) : this.attributes.health.max;

        lastDamageTaken = totalDamage;
        OnHurt.Invoke();


        if (totalDamage <= 0)
        {
            return false;
        }

        //attributes.ReducePoise(damageKnockback.poiseDamage);
        if (attributes.HasHealthRemaining())
        {
            attributes.ReduceAttribute(attributes.health, totalDamage);
        }
        
        

        return true;
    }

    public bool ParryAtk(DamageKnockback damageKnockback)
    {
        OnParry.Invoke();
        if (damageKnockback.source != null && damageKnockback.source.TryGetComponent<HumanoidActor>(out HumanoidActor humanoid))
        {
            humanoid.BlockRecoil();
        }
        return true;
    }

    private Vector3 GetFXPosition(DamageKnockback damageKnockback)
    {


        float MAX_DIST = 1f;

        Vector3 OFFSET = Vector3.up;

        Vector3 origin = this.transform.position;

        Vector3 endpoint = origin - damageKnockback.kbForce.normalized;

        if (damageKnockback.source != null)
        {
            endpoint = damageKnockback.source.transform.position;
        }



        float dist = Vector3.Distance(origin, endpoint);

        if (dist < MAX_DIST)
        {
            return Vector3.Lerp(origin, endpoint, 0.5f) + OFFSET;
        }
        else
        {
            return Vector3.MoveTowards(origin, endpoint, MAX_DIST) + OFFSET;
        }
    }
    public virtual void DeductStaminaFromDodge()
    {
        attributes.ReduceAttribute(attributes.stamina, 10f);
        //staminaClock = 0f;
    }
    public virtual void DeductStaminaFromAttack()
    {
        attributes.ReduceAttribute(attributes.stamina, 10f);
        //staminaClock = 0f;
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

        cc.Move(moveVector);
    }

    public void ForceLeftHand(bool left)
    {
        shouldForceLeft = left;
    }

    public virtual bool ShouldEndContinuousAttack()
    {
        // remove all references to this
        return false;
    }

    public void SetNextAttackType(BladeWeapon.AttackType type, bool adjustPoise)
    {
        nextAttackType = type;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(this.transform.position + this.transform.up * this.positionReference.eyeHeight, new Vector3(1, 0.025f, 1));
    }

    protected AxisUtilities.AxisDirection GetDodgeDirection()
    {
        Dictionary<string, AxisUtilities.AxisDirection> DIRECTION_MAP = new Dictionary<string, AxisUtilities.AxisDirection>()
        {
            {"dodge-forward", AxisUtilities.AxisDirection.Forward},
            {"dodge-backward", AxisUtilities.AxisDirection.Backward},
            {"dodge-left", AxisUtilities.AxisDirection.Left},
            {"dodge-right", AxisUtilities.AxisDirection.Right},
            {"roll", AxisUtilities.AxisDirection.Forward},
            {"backflip", AxisUtilities.AxisDirection.Backward}
        };

        foreach (string direction in DIRECTION_MAP.Keys)
        {
            if (animator.GetCurrentAnimatorStateInfo(1).IsName(direction))
            {
                return DIRECTION_MAP[direction];
            }
        }

        // if not found?
        return AxisUtilities.AxisDirection.Zero;
    }

    public void Slay()
    {
        GameObject victim = this.GetCombatTarget();

        if (TrySpareSlay(victim, out HumanoidActor humanoidVictim))
        {
            humanoidVictim.Die();
        }
    }

    public void Spare()
    {
        GameObject victim = this.GetCombatTarget();

        if (TrySpareSlay(victim, out HumanoidActor humanoidVictim))
        {
            humanoidVictim.StopHelpless();
        }
    }

    protected bool TrySpareSlay(GameObject victim, out HumanoidActor humanoidVictim)
    {
        humanoidVictim = null;

        if (victim == null)
        {
            return false;
        }


        if (Vector3.Distance(victim.transform.position, this.transform.position) > 5f)
        {
            return false;
        }

        if (!victim.TryGetComponent<HumanoidActor>(out humanoidVictim))
        {
            return false;
        }

        if (!humanoidVictim.IsHelpless())
        {
            return false;
        }

        return true;
    }

    public void TakeFallDamage(float mult, bool ragdoll)
    {
        DamageKnockback dk = new DamageKnockback()
        {
            healthDamage = 100f,//new Damage(fallDamage * mult, DamageType.TrueDamage),
            kbForce = Vector3.zero,
            unblockable = true,
            breaksArmor = true,
            hitClip = FXController.clipDictionary["shield_bash_hit"]
        };
        this.ProcessDamageKnockback(dk);
    }

    protected bool TrySpareSlay(GameObject victim)
    {
        return TrySpareSlay(victim, out HumanoidActor humanoid);
    }

    // anim events
    bool equipToMain;
    public void TriggerSheath(bool draw, Inventory.EquipSlot slot, bool targetMain)
    {
        animator.SetInteger("EquipSlot", (int)slot);
        if (draw)
        {
            animator.SetTrigger("Draw");
        }
        else
        {
            animator.SetTrigger("Sheath");
        }
        equipToMain = targetMain;
    }

    public void AnimSheathWeapon(int slot)
    {
        //Inventory.EquipSlot equipSlot = (Inventory.EquipSlot)slot;

        /*
        if (slot == 1 || slot == 3)
        { // main hand
            inventory.SetDrawn(true, false);
        }
        else if (slot == 2 || slot == 4)s
        {
            inventory.SetDrawn(false, false);
        }
        */
        inventory.SetDrawn(equipToMain, false);
    }

    public float GetCurrentSpeed()
    {
        float speed = BaseMovementSpeed;
        if (IsBlocking())
        {
            speed *= BlockMultiplier;
        }
        if (inventory.IsMainDrawn() || inventory.IsOffDrawn())
        {
            speed *= WeaponDrawnMultiplier;
        }
        if (IsSprinting())
        {
            speed *= SprintMultiplier;
        }
        if (IsAiming())
        {
            speed *= AimMultipler;
        }
        if (IsSneaking())
        {
            speed *= SneakMultiplier;
        }
        return speed;
    }

    public float GetSlowMultiplier()
    {
        float speed = 1f;
        if (IsBlocking())
        {
            speed *= BlockMultiplier;
        }
        if (inventory.IsMainDrawn() || inventory.IsOffDrawn())
        {
            speed *= WeaponDrawnMultiplier;
        }
        //if (IsSprinting())
        //{
            //speed *= SprintMultiplier;
        //}
        if (IsAiming())
        {
            speed *= AimMultipler;
        }
        if (IsSneaking())
        {
            speed *= SneakMultiplier;
        }
        return speed;
    }

    public void AnimDrawWeapon(int slot)
    {
        //Inventory.EquipSlot equipSlot = (Inventory.EquipSlot)slot;

        inventory.SetDrawn(equipToMain, true);
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
        bool main = (mainWeapon != null && mainWeapon is HitboxHandler);
        bool off = (offHandWeapon != null && offHandWeapon is HitboxHandler);
        if (active == 0)
        {
            if (main)
            {
                ((HitboxHandler)mainWeapon).HitboxActive(false);
            }
            if (off)
            {
                ((HitboxHandler)offHandWeapon).HitboxActive(false);
            }
            isHitboxActive = false;
        }
        else if (active == 1 && !shouldForceLeft)
        {
            if (main)
            {
                ((HitboxHandler)mainWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }
        else if (active == 2 || (active == 1 && shouldForceLeft))
        {
            if (off)
            {
                ((HitboxHandler)offHandWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }
        else if (active == 3)
        {
            if (main)
            {
                ((HitboxHandler)mainWeapon).HitboxActive(true);
            }
            if (off)
            {
                ((HitboxHandler)offHandWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }
        
    }

    // rotates the main weapon model around the upwards axis of the main hand mount
    public void RotateMainWeapon(float angle)
    {
        if (!inventory.IsMainDrawn())
        {
            return;
        }
        EquippableWeapon weapon = inventory.GetMainWeapon();
        GameObject weaponModel = weapon.model;
        GameObject mount = this.positionReference.MainHand;

        float angleDiff = mainWeaponAngle - angle;

        //Quaternion rotation = Quaternion.AngleAxis(angle, mount.transform.up);

        //weaponModel.transform.rotation = rotation;

        weaponModel.transform.RotateAround(mount.transform.position, mount.transform.up, angleDiff);

        if (weapon is BladeWeapon blade)
        {
            blade.GetHitboxes().root.transform.RotateAround(mount.transform.position, mount.transform.up, angleDiff);
        }
        mainWeaponAngle = angle;
    }

    // rotates the off hand weapon model around the upwards axis of the off hand mount
    public void RotateOffWeapon(float angle)
    {
        if (!inventory.IsOffDrawn())
        {
            return;
        }
        EquippableWeapon weapon = inventory.GetOffWeapon();
        GameObject weaponModel = weapon.model;
        GameObject mount = this.positionReference.OffHand;

        float angleDiff = offWeaponAngle - angle;

        //Quaternion rotation = Quaternion.AngleAxis(angle, mount.transform.up);

        //weaponModel.transform.rotation = rotation;

        weaponModel.transform.RotateAround(mount.transform.position, mount.transform.up, angleDiff);

        if (weapon is BladeWeapon blade)
        {
            blade.GetHitboxes().root.transform.RotateAround(mount.transform.position, mount.transform.up, angleDiff);
        }
        offWeaponAngle = angle;
    }

    public void ResetMainRotation()
    {
        RotateMainWeapon(0f);
        /*
        if (!inventory.IsMainDrawn())
        {
            return;
        }
        EquippableWeapon weapon = inventory.GetMainWeapon();
        GameObject weaponModel = weapon.model;
        GameObject mount = this.positionReference.MainHand;

        //Quaternion rotation = Quaternion.AngleAxis(angle, mount.transform.up);

        //weaponModel.transform.rotation = rotation;

        //weaponModel.transform.localRotation = Quaternion.identity;
        */
    }

    public void ResetOffRotation()
    {
        RotateOffWeapon(0f);
    }
    public void SetHeft(float heft)
    {
        this.heft = heft;
        //animator.SetFloat("Heft", heft);
    }
    
    public void HitWall()
    {
        if (!IsArmored())
        {
            AnimatorImpact(DamageKnockback.StaggerType.Recoil);
        }
    }

    public virtual Vector3 GetLaunchVector(Vector3 origin)
    {
        return this.transform.forward;
    }

    public override bool IsAlive()
    {
        return humanoidState != HumanoidState.Dead;
    }

    public void BecomeInvulnerable(float duration)
    {
        var coroutine = InvulnCoroutine(duration);
        StartCoroutine(coroutine);
    }

    IEnumerator InvulnCoroutine(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }

    public bool IsAerial()
    {
        return !GetGrounded() && airTime > 0.25f;
    }
    public bool CanMove()
    {
        /*string MOVABLE_TAG = "MOVABLE";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(0).IsTag(MOVABLE_TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(0));*/

        /*
        string TAG = "IMMOBILE";
        bool ALLOW_IN_TRANSITION = true;

        bool immobile = animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Base Movement")).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(0));
                */

        if (IsAiming() && !IsAttacking())
        {
            return true;
        }
        if (humanoidState == HumanoidState.Ragdolled)
        {
            return false;
        }

        bool actLayer = false;

        string[] MOVEABLE_STATES = new string[]
        {
            "EMPTY",
            "MOVABLE",
            "BLEND_MOVE",
            "BLOCK_MOVABLE",
            "SPRINTING",
            "PARRY_MOVABLE",
            "NEUTRAL_ACTION",
            "ATTACK_MOVABLE"
        };

        foreach (string state in MOVEABLE_STATES)
        {
            if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(state))
            {
                actLayer = true;
            }
        }

        string TAG = "EMPTY";
        bool ALLOW_IN_TRANSITION = true;

        /*
        bool impactLayer = animator.GetCurrentAnimatorStateInfo(StanceHandler.ImpactLayer).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(StanceHandler.ImpactLayer));
                */

        return actLayer;
    }

    public bool CanBuffer()
    {
        string TAG = "BUFFERABLE";
        bool ALLOW_IN_TRANSITION = true;

        bool bufferable = animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(animator.GetLayerIndex("Actions")));

        return bufferable || CanMove();
    }


    public bool IsFalling()
    {
        string TAG = "FALLING";
        bool ALLOW_IN_TRANSITION = true;

        bool falling = animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(animator.GetLayerIndex("Actions")));

        return falling;
    }

    public bool IsAttacking()
    {
        /*string MOVABLE_TAG = "MOVABLE";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(0).IsTag(MOVABLE_TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(0));*/

        if (IsHeavyAttacking() || IsSpecialAttacking())
        {
            if (!wasAttackingLastFrame)
            {
                OnAttack.Invoke();
                wasAttackingLastFrame = true;
            }
            return true;
        }
 
        string[] MOVEABLE_STATES = new string[]
        {
            "ATTACKING",
            "ARMOR_ATTACK",
            "BLOCK_ATTACK",
            "ATTACK_MOVABLE",
            "JUMP_ATTACK",
            "AIM_ATTACK"
        };

        foreach (string state in MOVEABLE_STATES)
        {
            if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(state))
            {
                if (!wasAttackingLastFrame)
                {
                    OnAttack.Invoke();
                    wasAttackingLastFrame = true;
                }
                return true;
            }
        }
        return false;
    }

    public bool IsDodging()
    {
        string[] STATES = new string[]
        {
            "DODGING",
            "DODGE_ATTACK",
        };

        foreach (string state in STATES)
        {
            if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(state))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsAiming()
    {
        string[] STATES = new string[]
        {
            "AIMING",
            "AIM_ATTACK",
        };

        foreach (string state in STATES)
        {
            if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(state))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsHeavyStaggered()
    {
        string TAG = "HEAVY_STAGGER";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(animator.GetLayerIndex("Actions")));
    }

    public bool IsProne()
    {
        string TAG = "PRONE";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(animator.GetLayerIndex("Actions")));
    }

    public bool IsSneaking()
    {
        string TAG = "SNEAKING";
        bool ALLOW_IN_TRANSITION = true;

        return !IsSprinting() && !IsAttacking() && animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Base Movement")).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(animator.GetLayerIndex("Base Movement")));
    }
    public bool IsHeavyAttacking()
    {
        /*string MOVABLE_TAG = "MOVABLE";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(0).IsTag(MOVABLE_TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(0));*/


        string[] MOVEABLE_STATES = new string[]
        {            
            "HEAVY_ATTACK",
            "HEAVY_START",
            "HEAVY_LOOP",
            "HEAVY_END"
        };

        foreach (string state in MOVEABLE_STATES)
        {
            if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(state))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsSpecialAttacking()
    {
        /*string MOVABLE_TAG = "MOVABLE";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(0).IsTag(MOVABLE_TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(0));*/


        string[] MOVEABLE_STATES = new string[]
        {
            "SPECIAL_ATTACK",
            "SPECIAL_START",
            "SPECIAL_LOOP",
            "SPECIAL_END"
        };

        foreach (string state in MOVEABLE_STATES)
        {
            if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(state))
            {
                return true;
            }
        }
        return false;
    }

    public virtual bool IsBlocking()
    {
        bool baseLayer = false;
        bool actionLayer = false;

        string[] BLOCKABLE_STATES = new string[]
        {
            "EMPTY",
            "BLOCKING",
            "BLOCK_MOVABLE",
        };

        if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Base Movement")).IsTag("BLOCKING"))
        {
            baseLayer = true;
        }
        foreach (string state in BLOCKABLE_STATES)
        {
            
            if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(state))
            {
                actionLayer = true;
            }
        }

        return baseLayer && actionLayer;
    }

    public bool IsEmptyState()
    {
        string TAG = "EMPTY";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(animator.GetLayerIndex("Actions")));
    }

    public bool IsHelpless()
    {
        string TAG = "HELPLESS";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(animator.GetLayerIndex("Actions")));
    }

    public bool IsBlockStaggered()
    {
        string TAG = "BLOCK_STAGGER";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(animator.GetLayerIndex("Actions")));
    }
    bool sprint;
    public bool IsSprinting()
    {
        string[] STATES = new string[]
        {
            "SPRINTING",
            "LEAPING"
        };

        foreach (string state in STATES)
        {
            if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(state))
            {
                sprint = true;
                return sprint;
            }
        }

        if (isGrounded)
        {
            sprint = false;
        }
        return sprint;
    }

    public bool IsArmored()
    {
        string TAG = "ARMOR_ATTACK";
        bool ALLOW_IN_TRANSITION = true;

        if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(animator.GetLayerIndex("Actions"))))
        {
            return true;
        }

        if (IsHeavyAttacking())// && stance != null && stance.heavyAttack != null && stance.heavyAttack.IsArmored())
        {
            return true;
        }

        if (IsSpecialAttacking())// && stance != null && stance.specialAttack != null && stance.specialAttack.IsArmored())
        {
            return true;
        }

        return false;
    }

    public bool IsJumping()
    {
        string[] STATES = new string[]
        {
            "JUMPING",
            "JUMP_ATTACK",
            "LEAPING"
        };

        foreach (string state in STATES)
        {
            if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(state))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsHanging()
    {
        string[] STATES = new string[]
        {
            "LEDGE",
            "LADDER"
        };

        foreach (string state in STATES)
        {
            if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(state))
            {
                return true;
            }
        }
        return false;
    }

    public virtual bool IsParrying()
    {
        string TAG = "PARRY";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(animator.GetLayerIndex("Actions")));
    }

    public bool IsCritVulnerable()
    {
        string[] STATES = new string[]
        {
            "CRITICAL",
        };

        foreach (string state in STATES)
        {
            if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Actions")).IsTag(state))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsInImpactState()
    {
        string TAG = "EMPTY";

        return !(animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Impacts")).IsTag(TAG));
    }
    public bool IsHitboxActive()
    {
        return isHitboxActive;
    }
}
