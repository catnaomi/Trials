using UnityEngine;
using System.Collections;
using Animancer;
using UnityEngine.Events;
using UnityEngine.AI;

[RequireComponent(typeof(HumanoidNPCInventory))]
public class TutorialMeleeCombatantActor : NavigatingHumanoidActor, IAttacker, IDamageable
{
    HumanoidNPCInventory inventory;
    [Header("Combatant Settings")]
    public float SightRange = 15f;
    public bool InSightRange;
    [Space(5)]
    public InputAttack CloseAttack;
    public float CloseAttackRange = 1.5f;
    public bool InCloseAttackRange;
    [Space(5)]
    public InputAttack FarAttack;
    public float FarAttackRange = 3.5f;
    public float FarAttackRotationSpeed = 45f;
    public bool InFarAttackRange;
    [Space(5)]
    public InputAttack OffMeshAttack;
    [Space(5)]
    public DamageAnims damageAnims;
    HumanoidDamageHandler damageHandler;
    [Space(5)]
    public GameObject deathParticle;
    [Space(10)]
    public float clock;
    public float ActionDelayMinimum = 2f;
    public float ActionDelayMaximum = 5f;
    
    public float LowHealthThreshold = 50f;
    public bool isLowHealth;

    bool isHitboxActive;
    public InputAttack bodySpin;
    public bool bodySpinning;
    float bodySpinAngle;
    public float bodySpinAccel = 90f;
    public float bodySpinMaxSpeed = 360f;
    float bodySpinSpeed = 0f;
    public float bodySpinAngleDown = 30f;
    protected CombatState cstate;
    protected struct CombatState
    {
        public AnimancerState attack;
        public AnimancerState approach;
        public AnimancerState dodge;
        public AnimancerState sheathe;
        public AnimancerState spinAttack;
    }

    System.Action _MoveOnEnd;
    public override void ActorStart()
    {
        base.ActorStart();
        _MoveOnEnd = () =>
        {
            animancer.Play(navstate.move, 0.1f);
        };

        damageHandler = new SimplifiedDamageHandler(this, damageAnims, animancer);
        damageHandler.SetEndAction(_MoveOnEnd);

        OnHurt.AddListener(() => {
            HitboxActive(0);
        });
    }

    void Awake()
    {
        inventory = this.GetComponent<HumanoidNPCInventory>();
    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();
        
        if (clock > -1)
        {
            clock -= Time.deltaTime;
        }
        bool shouldAct = (clock <= 0f);

        if (CombatTarget == null)
        {
            if (DetermineCombatTarget(out GameObject target))
            {
                CombatTarget = target;

                StartNavigationToTarget(target);

                if (target.TryGetComponent<Actor>(out Actor actor))
                {
                    actor.OnAttack.AddListener(BeingAttacked);
                }
            }
        }
        else if (CombatTarget.tag == "Corpse")
        {
            CombatTarget = null;
        }

        if (inventory.IsMainEquipped() && !inventory.IsMainDrawn())
        {
            inventory.SetDrawn(true, true);
        }
        if (shouldAct && CanAct())
        {
            clock = Random.Range(ActionDelayMinimum, ActionDelayMaximum);
            if (CombatTarget != null)
            {
                float navdist = GetDistanceToTarget();
                float realdist = Vector3.Distance(this.transform.position, GetCombatTarget().transform.position);

                InSightRange = realdist <= SightRange;
                InCloseAttackRange = realdist <= CloseAttackRange && nav.hasPath;//navdist <= CloseAttackRange && nav.hasPath;
                InFarAttackRange = realdist <= FarAttackRange && nav.hasPath;//navdist <= FarAttackRange && nav.hasPath;

                if (InSightRange)
                {
                    if (!IsStrafing())
                    {
                        SetDestination(CombatTarget);
                    }
                    else
                    {
                        SetStrafeDestination(CheckStrafe());
                    }
                    ResumeNavigation();
                }
                else
                {
                    StopNavigation();
                }
                if (InCloseAttackRange)
                {
                    StartCloseAttack();
                }
                else if (InFarAttackRange)
                {
                    StartFarAttack();
                }
            }
        }
        if (animancer.States.Current == cstate.attack)
        {
            if (!IsHitboxActive())
            {
                Vector3 dir = (destination - this.transform.position).normalized;
                this.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(this.transform.forward, dir, FarAttackRotationSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f));
            }
            if (!GetGrounded() && airTime > 1f)
            {
                navstate.fall = animancer.Play(fallAnim, 1f);
                HitboxActive(0);
            }
        }
    }

    void LateUpdate()
    {
        if (bodySpinning)
        {
            positionReference.Spine.localRotation = Quaternion.Euler(bodySpinAngleDown, bodySpinAngle, 0f);
            bodySpinAngle += bodySpinSpeed * Time.deltaTime;
            bodySpinAngle %= 360f;
            bodySpinSpeed += bodySpinAccel * Time.deltaTime;
            if (bodySpinSpeed > bodySpinMaxSpeed)
            {
                bodySpinSpeed = bodySpinMaxSpeed;
            }
        }
    }
    public void StartFarAttack()
    {
        if (FarAttack == null) return;
        RealignToTarget();
        cstate.attack = FarAttack.ProcessHumanoidAction(this, _MoveOnEnd);
        OnAttack.Invoke();
    }

    public void StartSpinAttack()
    {
        if (bodySpin == null) return;
        cstate.spinAttack = cstate.attack = bodySpin.ProcessHumanoidAction(this, () =>
        {
            _MoveOnEnd();
            bodySpinning = false;
            HitboxActive(0);
            DeactivateHitboxes();
        });
        bodySpinning = true;
        bodySpinSpeed = 90f;
        OnHurt.AddListener(StopSpin);
        OnAttack.Invoke();
    }

    public void StopSpin()
    {
        bodySpinning = false;
        animancer.Layers[HumanoidAnimLayers.BilayerBlend].Stop();
        DeactivateHitboxes();
        //HitboxActive(0);
    }
    public void StartCloseAttack()
    {
        if (CloseAttack == null) return;
        RealignToTarget();
        cstate.attack = CloseAttack.ProcessHumanoidAction(this, _MoveOnEnd);
        OnAttack.Invoke();
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

    public override void DeactivateHitboxes()
    {
        HitboxActive(0);
    }
    public override bool IsHitboxActive()
    {
        return isHitboxActive;
    }

    public void AnimDrawWeapon(int slot)
    {
        if (inventory.IsMainEquipped()) inventory.SetDrawn(0, true);
        if (inventory.IsOffEquipped()) inventory.SetDrawn(1, true);
        if (inventory.IsRangedEquipped()) inventory.SetDrawn(2, true);
    }

    public void AnimSheathWeapon(int slot)
    {
        if (inventory.IsMainEquipped()) inventory.SetDrawn(0, false);
        if (inventory.IsOffEquipped()) inventory.SetDrawn(1, false);
        if (inventory.IsRangedEquipped()) inventory.SetDrawn(2, false);
    }

    public DamageKnockback GetLastDamage()
    {
        return currentDamage;
    }

    public bool DetermineCombatTarget(out GameObject target)
    {
        if (PlayerActor.player == null)
        {
            target = null;
            return false;
        }
        target = PlayerActor.player.gameObject;
        return PlayerActor.player.gameObject.tag != "Corpse";
    }


    public override void HandleCustomOffMeshLink()
    {
        Drop();
        return;
        if (OffMeshAttack == null)
        {
            base.HandleCustomOffMeshLink();
        }
        else
        {
            OffMeshLinkData data = nav.currentOffMeshLinkData;
            Vector3 dir = data.endPos - this.transform.position;
            dir.y = 0f;
            this.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            cstate.attack = OffMeshAttack.ProcessHumanoidAction(this, () =>
            {
                Vector3 pos = animancer.Animator.rootPosition;
                this.transform.position = pos;
                nav.nextPosition = pos;
                offMeshInProgress = false;
                _MoveOnEnd();
            });
        }
    }
    public override bool IsArmored()
    {
        return true;
    }
    public override bool IsDodging()
    {
        return animancer.States.Current == cstate.dodge;
    }

    public override bool IsAttacking()
    {
        return animancer.States.Current == cstate.attack;
    }

    public override bool IsFalling()
    {
        return animancer.States.Current == navstate.fall || animancer.States.Current == damageHandler.fall;
    }

    public override void ProcessDamageKnockback(DamageKnockback damageKnockback)
    {
        damageHandler.TakeDamage(damageKnockback);
    }

    public void BeingAttacked()
    {
        if (CombatTarget != null && currentDistance < bufferRange && CanAct())
        {
            //TryDodge();
        }
    }

    public void TakeDamage(DamageKnockback damage)
    {
        ((IDamageable)damageHandler).TakeDamage(damage);
    }

    public void Recoil()
    {
        StopSpin();
        HitboxActive(0);
        ((IDamageable)damageHandler).Recoil();
        
    }

    public void StartCritVulnerability(float time)
    {
        ((IDamageable)damageHandler).StartCritVulnerability(time);
    }
    public override void Die()
    {
        if (dead) return;
        OnDie.Invoke();
        GameObject particle = Instantiate(deathParticle);
        particle.transform.position = this.GetComponent<Collider>().bounds.center;
        Destroy(particle, 5f);
        Destroy(this.gameObject);
        return;
        /*
        foreach (Renderer r in this.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
        foreach (Collider c in this.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
        this.GetComponent<Collider>().enabled = false;
        
        actionsEnabled = false;
        */
    }

    public override void FlashWarning(int hand)
    {
        EquippableWeapon mainWeapon = inventory.GetMainWeapon();
        EquippableWeapon offHandWeapon = inventory.GetOffWeapon();
        EquippableWeapon rangedWeapon = inventory.GetRangedWeapon();
        bool main = (mainWeapon != null && mainWeapon is IHitboxHandler);
        bool off = (offHandWeapon != null && offHandWeapon is IHitboxHandler);
        bool ranged = (rangedWeapon != null && rangedWeapon is IHitboxHandler);
        if (hand == 1 && main)
        {
            mainWeapon.FlashWarning();
        }
        else if (hand == 2 && off)
        {
            offHandWeapon.FlashWarning();
        }
        else if (hand == 3)
        {
            if (main)
            {
                mainWeapon.FlashWarning();
            }
            if (off)
            {
                offHandWeapon.FlashWarning();
            }
        }
        else if (hand == 4 && ranged)
        {
            rangedWeapon.FlashWarning();
        }
    }

    public DamageKnockback GetLastTakenDamage()
    {
        return ((IDamageable)damageHandler).GetLastTakenDamage();
    }

    public GameObject GetGameObject()
    {
        return ((IDamageable)damageHandler).GetGameObject();
    }

    public void GetParried()
    {
        ((IDamageable)damageHandler).GetParried();
    }

    public bool IsCritVulnerable()
    {
        return ((IDamageable)damageHandler).IsCritVulnerable();
    }
}
