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

    private bool shouldRagdoll;

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

    [HideInInspector] public UnityEvent OnSheathe;
    [HideInInspector] public UnityEvent OnOffhandAttack;
    [HideInInspector] public UnityEvent OnAttack;
    [HideInInspector] public UnityEvent OnBlock;
    [HideInInspector] public UnityEvent OnDodge;
    [HideInInspector] public UnityEvent OnInjure;
    [HideInInspector] public UnityEvent OnHitboxActive;
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

    public float tempValue1;
    public float tempValue2;

    public GameObject damageDisplay;

    public Vector3 gravity;
    [Serializable]
    public struct PositionReference
    {
        [Header("Body & Joint Positions")]
        public Rigidbody Hips;
        public Transform Spine;
        public Transform Head;
        [Header("Weapon Positions")]
        public GameObject MainHand;
        public GameObject OffHand;
        [Space(5)]
        public GameObject rHip;
        public GameObject rBack;
        public GameObject lHip;
        public GameObject lBack;
        public GameObject cBack;
    }

    [Header("Humanoid Settings")]
    public HumanoidState humanoidState;
    public PositionReference positionReference;
    public Inventory inventory;
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
        
    }
    public override void ActorStart()
    {
        base.ActorStart();

        LocateSlotsByName();

        if (inventory == null)
        {
            inventory = this.GetComponent<Inventory>();
        }

        inventory.Init();

        //inventory.OnChange.AddListener(GetStance);

        //GetStance();

        cc = GetComponent<CharacterController>();

        boundingCollider = GetComponent<Collider>();
        joints = GetComponentsInChildren<Collider>();

        UpdateColliders(false);

        SetHeft(1f);

        //animator.SetFloat("Agility", 1f);

        

        attributes.Reset();

        if (damageDisplay != null)
        {
            damageDisplay = Instantiate(damageDisplay);
            damageDisplay.GetComponent<DamageDisplay>().source = this.transform;
        }

        OnSheathe = new UnityEvent();
        OnOffhandAttack = new UnityEvent();
        OnAttack = new UnityEvent();
        OnBlock = new UnityEvent();
        OnDodge = new UnityEvent();
        OnInjure = new UnityEvent();
        OnHitboxActive = new UnityEvent();
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

        animator.SetBool("Armed", inventory.IsWeaponDrawn());

        inventory.UpdateWeapon();

        wasAttackingLastFrame = IsAttacking();        
        
    }

    void LocateSlotsByName()
    {
        string HAND_R_NAME = "_equipHandR";
        string HAND_L_NAME = "_equipHandL";


        Dictionary<Inventory.EquipSlot, string> SLOT_NAMES = new Dictionary<Inventory.EquipSlot, string> {
            {Inventory.EquipSlot.rHip, "_equipSheathR" },
            {Inventory.EquipSlot.lHip, "_equipSheathL" },
            {Inventory.EquipSlot.rBack, "_equipBackR" },
            {Inventory.EquipSlot.lBack, "_equipBackL" },
            {Inventory.EquipSlot.cBack, "_equipBackC" }
        };

        Transform current;
        if (positionReference.MainHand == null)
        {
            current = LocateSlotsRecursive(this.transform, HAND_R_NAME);
            if (current != null)
            {
                positionReference.MainHand = current.gameObject;
            }            
        }
        if (positionReference.OffHand == null)
        {
            current = LocateSlotsRecursive(this.transform, HAND_L_NAME);
            if (current != null)
            {
                positionReference.OffHand = current.gameObject;
            }
        }

        foreach (Inventory.EquipSlot slot in SLOT_NAMES.Keys)
        {
            if (this.GetPositionRefSlot(slot) == null)
            {
                current = LocateSlotsRecursive(this.transform, SLOT_NAMES[slot]);
                if (current != null)
                {
                    this.SetPositionRefSlot(slot, current.gameObject);
                }
            }
        }
    }

    Transform LocateSlotsRecursive(Transform t, string n)
    {
        Transform s = t.Find(n);
        if (s == null)
        {
            foreach(Transform c in t)
            {
                Transform found = LocateSlotsRecursive(c, n);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }
        else
        {
            return s;
        }
    }

    public GameObject GetPositionRefSlot(Inventory.EquipSlot slot)
    {
        switch (slot)
        {
            case Inventory.EquipSlot.rHip:
                return positionReference.rHip;
            case Inventory.EquipSlot.lHip:
                return positionReference.lHip;
            case Inventory.EquipSlot.rBack:
                return positionReference.rBack;
            case Inventory.EquipSlot.lBack:
                return positionReference.lBack;
            case Inventory.EquipSlot.cBack:
                return positionReference.cBack;
            default:
                return null;
        }
    }

    public void SetPositionRefSlot(Inventory.EquipSlot slot, GameObject newSlot)
    {
        switch (slot)
        {
            case Inventory.EquipSlot.rHip:
                positionReference.rHip = newSlot;
                break;
            case Inventory.EquipSlot.lHip:
                positionReference.lHip = newSlot;
                break;
            case Inventory.EquipSlot.rBack:
                positionReference.rBack = newSlot;
                break;
            case Inventory.EquipSlot.lBack:
                positionReference.lBack = newSlot;
                break;
            case Inventory.EquipSlot.cBack:
                positionReference.cBack = newSlot;
                break;
        }
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
        return Physics.Raycast(transform.position, Vector3.down, 0.1f, LayerMask.GetMask("Terrain"));
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

    public override void TakeAction(InputAction action)
    {
        // lmao this no longer does anything
        return;
    }
    public void TryGetup()
    {
        ragdollClock += Time.deltaTime;

        if (positionReference.Hips.velocity.magnitude < autoGetupVelocity)
        {
            ragdollStillClock += Time.deltaTime;
        }
        else
        {
            ragdollStillClock = 0f;
        }

        if (
            ragdollClock > forceGetupDelay || // force getup
            ragdollStillClock > autoGetupDelay || // auto getup
            (!attributes.HasHealthRemaining() && (ragdollStillClock > helplessGetupDelay)) // get up faster when helpless
            )
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
    }

    public void Getup()
    {
        animator.enabled = true;
        cc.enabled = true;

        Vector3 spineUp;
        spineUp = -positionReference.Hips.transform.up;
        spineUp.Scale(new Vector3(1, 0, 1));
        spineUp.Normalize();

        bool isFacingUp;
        float upDist = Vector3.Distance(positionReference.Hips.transform.forward, Vector3.up);
        float downDist = Vector3.Distance(-positionReference.Hips.transform.forward, Vector3.up);
        isFacingUp = upDist < downDist;


        cc.Move((positionReference.Hips.transform.position - transform.position) + spineUp);
        transform.rotation = Quaternion.LookRotation(spineUp * (isFacingUp ? 1 : -1));
        positionReference.Hips.transform.position = Vector3.zero;
        animator.SetBool("FacingUp", isFacingUp);
        animator.SetTrigger("GetUp");
        //animator.SetTrigger(isFacingUp ? "GetUpFaceUp" : "GetUpFaceDown");
        UpdateColliders(false);

        attributes.RecoverAttribute(attributes.stamina, 50f);

        humanoidState = HumanoidState.Actionable;
    }

    public void StartHelpless()
    {
        if (!animator.enabled)
        {
            animator.enabled = true;
            cc.enabled = true;

            Vector3 spineUp;
            spineUp = -positionReference.Hips.transform.up;
            spineUp.Scale(new Vector3(1, 0, 1));
            spineUp.Normalize();

            bool isFacingUp;
            float upDist = Vector3.Distance(positionReference.Hips.transform.forward, Vector3.up);
            float downDist = Vector3.Distance(-positionReference.Hips.transform.forward, Vector3.up);
            isFacingUp = upDist < downDist;


            cc.Move((positionReference.Hips.transform.position - transform.position) + spineUp);
            transform.rotation = Quaternion.LookRotation(spineUp * (isFacingUp ? 1 : -1));
            positionReference.Hips.transform.position = Vector3.zero;

            animator.SetBool("FacingUp", isFacingUp);
        }
        
        animator.SetBool("Helpless", true);
        //animator.SetTrigger("ForceHelpless");
        //animator.SetTrigger(isFacingUp ? "GetUpFaceUp" : "GetUpFaceDown");
        //UpdateColliders(false);

        humanoidState = HumanoidState.Helpless;
    }

    public void StopHelpless()
    {
        reviveClock = 0f;
        animator.SetBool("Helpless", false);
        humanoidState = HumanoidState.Actionable;
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
        Ragdoll();
        humanoidState = HumanoidState.Dead;
    }

    public void Ragdoll()
    {
        transform.position += Vector3.up * 0.1f;

        animator.enabled = false;
        cc.enabled = false;
        UpdateColliders(true);

        foreach (Rigidbody rigidbody in GetComponentsInChildren<Rigidbody>())
        {
            rigidbody.velocity = Vector3.zero;
        }

        humanoidState = HumanoidState.Ragdolled;

        ragdollClock = 0f;
        ragdollStillClock = 0f;
    }

    // on false: use bounding collider. on true: use joint colliders.
    private void UpdateColliders(bool isRagdolled)
    {
        foreach (Collider collider in joints)
        {
            if (collider != null)
            {
                //collider.enabled = isRagdolled;
                if (collider.TryGetComponent<Rigidbody>(out Rigidbody rigid))
                {
                    rigid.isKinematic = !isRagdolled;
                }
            }
        }
        boundingCollider.enabled = true;//!isRagdolled;
        positionReference.Hips.GetComponent<Collider>().enabled = true;
    }


    public bool Vulnerable()
    {
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
        float totalDamage = damageKnockback.damage.GetTotalMinusResistances(this.attributes.resistances).GetTotal();

        if (this.IsDodging() || isInvulnerable)
        {
            // do nothing
            // slowdown effect on player dodge!
            OnDodge.Invoke();
        }
        else if (this.IsParrying() && attributes.HasAttributeRemaining(attributes.stamina) && !damageKnockback.unblockable) // is actor parrying with stamina remaining
        {
            // take no damage / stamina damage, and stagger human opponents
            Parry(damageKnockback);
        }
        else if (this.IsBlocking() && !damageKnockback.unblockable) // is actor blocking
        {
            // blocking deals stamina damage
            attributes.ReduceAttribute(attributes.stamina, damageKnockback.staminaDamage * attributes.BlockReduction);
            Damage(damageKnockback, true);
            if (attributes.HasAttributeRemaining(attributes.stamina) && attributes.HasAttributeRemaining(attributes.health)) // does actor have stamina remaining?
            {
                Block(damageKnockback); // block stagger
            }
            else if (!attributes.HasAttributeRemaining(attributes.health))
            {
                Stun(damageKnockback); // injure
                this.OnInjure.Invoke();
            }
            else // no stamina remaining
            {
                GuardBreak(damageKnockback); // guard break
            }
        }
        else
        {
            if (!attributes.HasHealthRemaining()) // die
            {
                // send flying on death
                Knockback(damageKnockback);
                Die();
            }
            else if (totalDamage >= attributes.health.current) // injure
            {
                Damage(damageKnockback);
                if (damageKnockback.staggerType == DamageKnockback.StaggerType.Knockdown)
                {
                    Knockback(damageKnockback);
                }
                else if (damageKnockback.staggerType == DamageKnockback.StaggerType.FallDamage)
                {
                    Flinch(damageKnockback);
                }
                else
                {
                    Stun(damageKnockback);
                }
                this.OnInjure.Invoke();
            }
            else // get hit normally
            {
                Damage(damageKnockback);

                DamageKnockback.StaggerType stagger = DamageKnockback.StaggerType.None;

                if (damageKnockback.staggerType == DamageKnockback.StaggerType.FallDamage)
                {
                    //Flinch(damageKnockback);
                    stagger = DamageKnockback.StaggerType.Flinch;
                }
                else if (!attributes.GetOffBalance() && (!IsArmored() || damageKnockback.breaksArmor)) // is not off balance, but not armored
                {
                    stagger = damageKnockback.minStaggerType;
                }
                else if (attributes.GetOffBalance() && IsArmored() && !damageKnockback.breaksArmor) // is off balance but is armored
                {
                    switch (damageKnockback.staggerType)
                    {
                        case DamageKnockback.StaggerType.Stun:
                            stagger = DamageKnockback.StaggerType.Stun;
                            break;
                        default:
                            stagger = DamageKnockback.StaggerType.Flinch;
                            break;
                    }
                }
                else // if off balance and is not armored
                {
                    stagger = damageKnockback.staggerType;
                }
 
                switch (stagger)
                {
                    case DamageKnockback.StaggerType.Stun:
                        Stun(damageKnockback);
                        break;
                    case DamageKnockback.StaggerType.Flinch:
                        Flinch(damageKnockback);
                        break;
                    case DamageKnockback.StaggerType.Stagger:
                        LightStagger(damageKnockback);
                        break;
                    case DamageKnockback.StaggerType.HeavyStagger:
                        HeavyStagger(damageKnockback);
                        break;
                    case DamageKnockback.StaggerType.Knockdown:
                        Knockback(damageKnockback);
                        break;
                }
            }
        }
        
    }

    public void Block(DamageKnockback damageKnockback)
    {
        Vector3 turnTowards = new Vector3(damageKnockback.kbForce.x, 0, damageKnockback.kbForce.z);
        turnTowards = -(turnTowards.normalized);

        slideAmount = damageKnockback.kbForce.magnitude / 20f;

        transform.LookAt(transform.position + turnTowards);

        //OnHurt.Invoke();
        OnBlock.Invoke();
        if (damageKnockback.source != null && damageKnockback.source.GetComponentInChildren<Actor>() != null)
        {
            damageKnockback.source.GetComponentInChildren<Actor>().OnHit.Invoke();
        }

        FXController.CreateFX(FXController.FX.FX_Block, GetFXPosition(damageKnockback), Quaternion.LookRotation(turnTowards), 2f);

        AnimatorImpact(DamageKnockback.StaggerType.BlockStagger);

        if (damageKnockback.source != null && damageKnockback.source.TryGetComponent<HumanoidActor>(out HumanoidActor humanoid))
        {
            //humanoid.BlockRecoil(this.inventory.GetBlockPoiseDamage(stance.BlockWithMain()));
            // TODO: implement block resistance stat
            humanoid.BlockRecoil(this.inventory.GetBlockPoiseDamage(true));
            //humanoid.HeavyStagger(parryDamage);
        }
    }

    public void BlockRecoil(float poiseDamage)
    {
        attributes.ReducePoise(poiseDamage);
        if (!IsArmored() && !IsAiming() && !attributes.HasPoiseRemaining())
        {
            animator.SetTrigger("AttackBlocked");
        }
    }
    public void Knockback(DamageKnockback damageKnockback)
    {
        Vector3 force = damageKnockback.kbForce;// + Vector3.up * 2f;


        OnHurt.Invoke();
        if (damageKnockback.source != null && damageKnockback.source.GetComponentInChildren<Actor>() != null)
        {
            damageKnockback.source.GetComponentInChildren<Actor>().OnHit.Invoke();
        }

        FXController.CreateFX(FXController.FX.FX_Stagger, GetFXPosition(damageKnockback), Quaternion.LookRotation(-NumberUtilities.FlattenVector(force).normalized), 2f, damageKnockback.hitClip);

        AnimatorImpact(DamageKnockback.StaggerType.Knockdown);
        Ragdoll();

        positionReference.Hips.AddForce(force, ForceMode.Impulse);

        lastForce = damageKnockback.kbForce;
    }

    public bool LightStagger(DamageKnockback damageKnockback)
    {
        Vector3 turnTowards = new Vector3(damageKnockback.kbForce.x, 0, damageKnockback.kbForce.z);
        turnTowards = -(turnTowards.normalized);
        

        OnHurt.Invoke();
        if (damageKnockback.source != null && damageKnockback.source.GetComponentInChildren<Actor>() != null)
        {
            damageKnockback.source.GetComponentInChildren<Actor>().OnHit.Invoke();
        }
        FXController.CreateFX(FXController.FX.FX_Hit, GetFXPosition(damageKnockback), Quaternion.LookRotation(turnTowards), 2f, damageKnockback.hitClip);


        transform.LookAt(transform.position + turnTowards);

        AnimatorImpact(DamageKnockback.StaggerType.Stagger);

        return true;
    }

    public bool HeavyStagger(DamageKnockback damageKnockback)
    {
        Vector3 turnTowards = new Vector3(damageKnockback.kbForce.x, 0, damageKnockback.kbForce.z);
        turnTowards = -(turnTowards.normalized);
        transform.LookAt(transform.position + turnTowards);

        OnHurt.Invoke();

        if (damageKnockback.source != null && damageKnockback.source.GetComponentInChildren<Actor>() != null)
        {
            damageKnockback.source.GetComponentInChildren<Actor>().OnHit.Invoke();
        }
        FXController.CreateFX(FXController.FX.FX_Hit, GetFXPosition(damageKnockback), Quaternion.LookRotation(turnTowards), 2f, damageKnockback.hitClip);

        AxisUtilities.AxisDirection axis = AxisUtilities.AxisDirection.Backward;//AxisUtilities.DirectionToAxisDirection(damageKnockback.kbForce, this.transform, "HORIZONTAL", "SAGGITAL");


        animator.SetInteger("AxisDirection", (int)axis);

        AnimatorImpact(DamageKnockback.StaggerType.HeavyStagger);

        return true;
    }

    public bool GuardBreak(DamageKnockback damageKnockback)
    {
        Vector3 turnTowards = new Vector3(damageKnockback.kbForce.x, 0, damageKnockback.kbForce.z);
        turnTowards = -(turnTowards.normalized);
        transform.LookAt(transform.position + turnTowards);

        OnHurt.Invoke();
        if (damageKnockback.source != null && damageKnockback.source.GetComponentInChildren<Actor>() != null)
        {
            damageKnockback.source.GetComponentInChildren<Actor>().OnHit.Invoke();
        }
        FXController.CreateFX(FXController.FX.FX_Block, GetFXPosition(damageKnockback), Quaternion.LookRotation(turnTowards), 2f);


        AnimatorImpact(DamageKnockback.StaggerType.GuardBreak);

        //attributes.ReduceAttribute(attributes.poise, 100f);
        attributes.SetPoise(0f);

        return true;
    }

    public void Parry(DamageKnockback damageKnockback)
    {

        FXController.CreateFX(FXController.FX.FX_Block, GetFXPosition(damageKnockback), Quaternion.LookRotation(-NumberUtilities.FlattenVector(damageKnockback.kbForce).normalized), 2f);

        DamageKnockback parryDamage = new DamageKnockback(damageKnockback);
        parryDamage.kbForce = -NumberUtilities.FlattenVector(damageKnockback.kbForce).normalized;
        parryDamage.source = this.gameObject;

        if (damageKnockback.source != null && damageKnockback.source.TryGetComponent<HumanoidActor>(out HumanoidActor humanoid))
        {
            humanoid.Stun(parryDamage);
            humanoid.attributes.SetPoise(0f);
            //humanoid.HeavyStagger(parryDamage);
        }

    }

    public void Flinch(DamageKnockback damageKnockback)
    {
        Vector3 turnTowards = new Vector3(damageKnockback.kbForce.x, 0, damageKnockback.kbForce.z);
        turnTowards = -(turnTowards.normalized);
        transform.LookAt(transform.position + turnTowards);

        OnHurt.Invoke();

        if (damageKnockback.source != null && damageKnockback.source.GetComponentInChildren<Actor>() != null)
        {
            damageKnockback.source.GetComponentInChildren<Actor>().OnHit.Invoke();
        }

        FXController.CreateFX(FXController.FX.FX_Hit, GetFXPosition(damageKnockback), Quaternion.LookRotation(turnTowards), 2f, damageKnockback.hitClip);

        AnimatorImpact(DamageKnockback.StaggerType.Flinch);
    }

    public void Stun(DamageKnockback damageKnockback)
    {
        Vector3 turnTowards = new Vector3(damageKnockback.kbForce.x, 0, damageKnockback.kbForce.z);
        turnTowards = -(turnTowards.normalized);
        transform.LookAt(transform.position + turnTowards);

        OnHurt.Invoke();

        if (damageKnockback.source != null && damageKnockback.source.GetComponentInChildren<Actor>() != null)
        {
            damageKnockback.source.GetComponentInChildren<Actor>().OnHit.Invoke();
        }

        FXController.CreateFX(FXController.FX.FX_Stagger, GetFXPosition(damageKnockback), Quaternion.LookRotation(turnTowards), 2f, damageKnockback.hitClip);

        AnimatorImpact(DamageKnockback.StaggerType.Stun);
    }

    public void Recoil(DamageKnockback damageKnockback)
    {
        Vector3 turnTowards = new Vector3(damageKnockback.kbForce.x, 0, damageKnockback.kbForce.z);
        turnTowards = -(turnTowards.normalized);
        transform.LookAt(transform.position + turnTowards);

        OnHurt.Invoke();

        if (damageKnockback.source != null && damageKnockback.source.GetComponentInChildren<Actor>() != null)
        {
            damageKnockback.source.GetComponentInChildren<Actor>().OnHit.Invoke();
        }

        FXController.CreateFX(FXController.FX.FX_Stagger, GetFXPosition(damageKnockback), Quaternion.LookRotation(turnTowards), 2f, damageKnockback.hitClip);

        AnimatorImpact(DamageKnockback.StaggerType.Recoil);
    }

    public void AnimatorImpact(DamageKnockback.StaggerType type)
    {
        if (false)//type == DamageKnockback.StaggerType.Knockdown)
        {
            animator.SetTrigger("ForceKnockdown");
        }
        animator.SetInteger("ImpactType", (int)type);
        animator.SetTrigger("Impact");
    }
    public bool Damage(DamageKnockback damageKnockback)
    {
        return Damage(damageKnockback, 1, false);
    }

    public bool Damage(DamageKnockback damageKnockback, bool blocking)
    {
        return Damage(damageKnockback, 1, blocking);
    }

    public bool Damage(DamageKnockback damageKnockback, int multiplier, bool blocking)
    {
        // account for resistances
        float totalDamage;
        if (!blocking)
        {
            totalDamage = damageKnockback.damage.GetTotalMinusResistances(this.attributes.resistances).GetTotal();
        }
        else
        {
            //totalDamage = damageKnockback.damage.GetTotalMinusResistances(this.attributes.resistances, inventory.GetBlockResistance(stance.BlockWithMain())).GetTotal();
            totalDamage = damageKnockback.damage.GetTotalMinusResistances(this.attributes.resistances, inventory.GetBlockResistance(true)).GetTotal();
        }

        if (damageDisplay != null)
        {
            damageDisplay.GetComponent<DamageDisplay>().AddDamage(totalDamage * multiplier, damageKnockback.damage.GetHighestType(DamageType.Slashing, DamageType.Piercing));
        }

        if (totalDamage <= 0)
        {
            return false;
        }

        attributes.ReducePoise(damageKnockback.poiseDamage);
        attributes.ReduceAttribute(attributes.health, totalDamage * multiplier);
        
        

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

    public void HandleBlockIK()
    {
        /*
        if (stance.GetBlockStyle() == StanceHandler.BlockStyle.TwoHand && !animator.IsInTransition(animator.GetLayerIndex("Base Movement")))
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, tempValue1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, positionReference.MainHand.transform.position + positionReference.MainHand.transform.forward * tempValue2);
        }
        */
    }

    public void SetNextAttackType(BladeWeapon.AttackType type, bool adjustPoise)
    {
        nextAttackType = type;
        if (adjustPoise)
        {
            attributes.IncreasePoiseByWeight(inventory.GetPoiseFromAttack(type));
        }
    }

    private void OnDrawGizmosSelected()
    {
        try
        {
            string staminaText = "stamina: [";

            for (int i = 0; i < attributes.stamina.max; i += 10)
            {
                char c;
                if (i < attributes.stamina.current)
                {
                    c = '=';
                }
                else if (i < attributes.smoothedStamina)
                {
                    c = '+';
                }
                else
                {
                    c = '–';
                }
                staminaText += c;
            }
            staminaText += "] " + (int)attributes.stamina.current;

            InterfaceUtilities.GizmosDrawText(transform.position + Vector3.up * 2f, new Color(0, 0.5f, 0), staminaText);

            string stunText = "stun: [";

            for (float f = 0; f < stunAmount; f += 0.1f)
            {
                stunText += "|";
            }

            stunText += "] " + (int)(stunAmount * 100f);

            if (stunAmount > 0)
            {
                InterfaceUtilities.GizmosDrawText(transform.position + Vector3.up * 2.1f, new Color(0.5f, 0.5f, 0.5f), stunText);
            }

            string hpText = "hp : [";
            for (int h = 1; h <= attributes.health.current; h++)
            {
                hpText += "|";
            }
            hpText += "] " + attributes.health.current;

            InterfaceUtilities.GizmosDrawText(transform.position + Vector3.up * 2.2f, new Color(0.5f, 0, 0), hpText);
        }
        catch (Exception ex)
        {

        }
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

    public bool IsHelpless()
    {
        return humanoidState == HumanoidState.Helpless;
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
            damage = new Damage(fallDamage * mult, DamageType.TrueDamage),
            poiseDamage = 999f,
            kbForce = Vector3.zero,
            staggerType = ragdoll ? DamageKnockback.StaggerType.Knockdown : DamageKnockback.StaggerType.FallDamage,
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
     */
    public void HitboxActive(int active)
    {
        EquippableWeapon mainWeapon = inventory.GetMainWeapon();
        EquippableWeapon offHandWeapon = inventory.GetOffHand();
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
        EquippableWeapon weapon = inventory.GetOffHand();
        GameObject weaponModel = weapon.model;
        GameObject mount = this.positionReference.OffHand;

        float angleDiff = offWeaponAngle - angle;

        //Quaternion rotation = Quaternion.AngleAxis(angle, mount.transform.up);

        //weaponModel.transform.rotation = rotation;

        weaponModel.transform.RotateAround(mount.transform.position, mount.transform.up, angleDiff);
        offWeaponAngle = angle;
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

    public bool IsWeaponEquipped()
    {
        return inventory.MainWeapon != null;
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

    public bool IsBlocking()
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

    public bool IsParrying()
    {
        /*string MOVABLE_TAG = "MOVABLE";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(0).IsTag(MOVABLE_TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(0));*/


        string[] MOVEABLE_STATES = new string[]
        {
            "PARRY",
            "PARRY_MOVABLE",
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


    public bool IsHitboxActive()
    {
        return isHitboxActive;
    }
}
