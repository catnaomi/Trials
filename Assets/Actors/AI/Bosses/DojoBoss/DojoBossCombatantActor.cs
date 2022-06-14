using UnityEngine;
using System.Collections;
using Animancer;
using UnityEngine.Events;
using UnityEngine.AI;

[RequireComponent(typeof(HumanoidNPCInventory))]
public class DojoBossCombatantActor : NavigatingHumanoidActor, IAttacker, IDamageable
{
    HumanoidNPCInventory inventory;
    /*
     * attacks:
     * 3 hit melee combo
     * triple stab -> swipe
     * shoot from ice pillars
     * jump shot
     * jump dodge (may include shot)
     * jumping plunge (from ground)
     * jumping plunge (from pillar)
     * cross parry
     * circle parry
     * summon
     */
    [Header("Combatant Settings")]
    public InputAttack MeleeCombo1; // 1h slash -> 2h slash -> stab
    public InputAttack MeleeCombo2; // 3x stab left hand - > slash R -(transform to gs)> slash R
    public InputAttack MeleeComboApproach; // (jump into position) walk slash 1h -> 2h slash -> stab
    [Space(10)]
    public InputAttack GroundPlunge;
    public InputAttack PillarPlunge;
    [Space(10)]
    public InputAttack CrossParry;
    public InputAttack CircleParry;
    [Space(10)]
    public ClipTransition JumpDodge;
    public ClipTransition JumpLand;
    public InputAttack JumpShot;
    [Space(10)]
    public InputAttack RangedShot;
    public InputAttack PillarShot;
    [Space(10)]
    public InputAttack Summon;
    [Space(5)]
    public DamageAnims damageAnims;
    HumanoidDamageHandler damageHandler;
    [Space(10)]
    public float clock;
    public float ActionDelayMinimum = 2f;
    public float ActionDelayMaximum = 5f;
    
    public float LowHealthThreshold = 50f;
    public bool isLowHealth;
    bool isHitboxActive;
    [Header("Map Information")]
    public Transform pillar1;
    public Transform pillar2;
    public Transform pillar3;
    [ReadOnly]public bool onPillar;
    public float nonPillarHeight = -1000f;
    [Space(10)]
    [SerializeField] AnimationCurve jumpHorizCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] AnimationCurve jumpVertCurve = AnimationCurve.Constant(0f, 1f, 0f);
    [SerializeField] float jumpVertMult = 1f;
    Vector3 startJumpPosition;
    Vector3 endJumpPosition;
    [Header("Enumerated States")]
    public WeaponState weaponState;
    public UnityEvent OnWeaponTransform;
    CharacterController cc;
    protected CombatState cstate;
    protected struct CombatState
    {
        public AnimancerState attack;
        public AnimancerState jump;
    }

    public enum WeaponState
    {
        None,           // 0
        Quarterstaff,   // 1
        Scimitar,       // 2
        Greatsword,     // 3
        Rapier,         // 4
        Bow,            // 5
        Hammer,         // 6
        Daox2,          // 7
        MagicStaff,     // 8
        Spear           // 9
    }
    System.Action _MoveOnEnd;
    public override void ActorStart()
    {
        base.ActorStart();
        _MoveOnEnd = () =>
        {
            animancer.Play(navstate.move, 0.1f);
        };

        damageHandler = new SingleWeaknessDamageHandler(this, damageAnims, animancer);
        damageHandler.SetEndAction(_MoveOnEnd);

        cc = this.GetComponent<CharacterController>();
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

                StartMeleeCombo1();
                /*
                Transform pillar = pillar1;
                int r = Random.Range(1, 4);
                if (r == 1)
                {
                    pillar = pillar1;
                }
                else if (r == 2)
                {
                    pillar = pillar2;
                }
                else if (r == 3)
                {
                    pillar = pillar3;
                }
                if (!onPillar)
                {
                    DodgeJump(pillar.position);
                    onPillar = true;
                }
                else
                {
                    if (Random.value > 0.5f)
                    {
                        DodgeJump(new Vector3(-1.49f, -1060.6f, -122.42f));
                        onPillar = false;
                    }
                    else
                    {
                        DodgeJump(pillar.position);
                        onPillar = true;
                    }
                }*/
            }
        }
        if (animancer.States.Current == cstate.jump)
        {
            float t = cstate.jump.NormalizedTime;
            cc.enabled = false;
            shouldNavigate = false;

            Vector3 targetPosition = Vector3.Lerp(startJumpPosition, endJumpPosition, jumpHorizCurve.Evaluate(t)) + Vector3.up * jumpVertCurve.Evaluate(t) * jumpVertMult;

            this.transform.position = targetPosition;

            cc.enabled = true;
            yVel = 0f;
        }
        if (Vector3.Distance(CombatTarget.transform.position, this.transform.position) < 10f)
        {
            //animancer.Layers[0].ApplyAnimatorIK = true;
            //animancer.Animator.SetLookAtPosition(headPoint);
        }
        else
        {
            //animancer.Layers[0].ApplyAnimatorIK = false;
        }
    }

    public void StartMeleeCombo1()
    {
        RealignToTarget();
        //cstate.attack = CloseAttack.ProcessHumanoidAttack(this, _MoveOnEnd);
        cstate.attack = MeleeCombo1.ProcessHumanoidAttack(this, _MoveOnEnd);
        OnAttack.Invoke();
    }

    public void DodgeJump(Vector3 position)
    {
        AnimancerState jump = animancer.Play(JumpDodge);
        jump.Events.OnEnd = JumpEnd;
        cstate.jump = jump;
        endJumpPosition = position;
        startJumpPosition = this.transform.position;
        nav.enabled = false;
    }

    void JumpEnd()
    {
        shouldNavigate = !onPillar;
        AnimancerState land = animancer.Play(JumpLand);
        land.Events.OnEnd = _MoveOnEnd;
        nav.enabled = true;
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

    public override bool IsHitboxActive()
    {
        return isHitboxActive;
    }

    public void AnimTransWep(int wep)
    {
        weaponState = (WeaponState)wep;
        OnWeaponTransform.Invoke();
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

    public DamageKnockback GetCurrentDamage()
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

    public override bool IsArmored()
    {
        return true;
    }
    public override bool IsDodging()
    {
        return animancer.States.Current == cstate.jump;
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

    public bool CanAct()
    {
        return (animancer.States.Current == navstate.move || animancer.States.Current == navstate.idle) && actionsEnabled;
    }

    public void TakeDamage(DamageKnockback damage)
    {
        ((IDamageable)damageHandler).TakeDamage(damage);
    }

    public void Recoil()
    {
        ((IDamageable)damageHandler).Recoil();
    }

    public override void Die()
    {
        if (dead) return;
        base.Die();
        
        foreach(Renderer r in this.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
        foreach (Collider c in this.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
        this.GetComponent<Collider>().enabled = false;

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

    private void OnAnimatorIK(int layerIndex)
    {
        animancer.Animator.SetLookAtPosition(CombatTarget.transform.position + Vector3.up);
        animancer.Animator.SetLookAtWeight(1f);
    }
}
