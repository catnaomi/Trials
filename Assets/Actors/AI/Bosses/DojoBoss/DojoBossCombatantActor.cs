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
    public float MeleeCombo1StartDistance = 10f;
    public InputAttack MeleeCombo2; // 3x stab left hand - > slash R -(transform to gs)> slash R
    public InputAttack MeleeComboApproach; // (jump into position) walk slash 1h -> 2h slash -> stab
    [Space(10)]
    public InputAttack Plunge;
    public ClipTransition PlungeJump;
    public ClipTransition PlungeFall;
    public AnimationCurve heightPlungeCurve;
    public AnimationCurve heightPlungePillarCurve;
    public AnimationCurve horizPlungeCurve;
    public AnimationCurve horizPlungePillarCurve;
    public float PlungeJumpHeight = -950f;
    public float StopAdjustPoint = 0.75f;
    public float DescentPoint = 0.75f;
    public float AttackPoint = 0.9f;
    public float PlungeTime = 5f;
    float plungeClock;
    bool plunging;
    Vector3 plungeTarget;
    Vector3 plungeStart;
    [Space(10)]
    public ClipTransition CrouchDown;
    public ClipTransition Crouch;
    public float CrouchTime = 1f;
    bool crouching;
    float crouchClock;
    public CrouchAction actionAfterCrouch;
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
    public float AttackRotationSpeed = 720f;
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
    public Transform center;
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
    Vector3 lastTargetPos;
    Vector3 targetSpeed;
    protected CombatState cstate;
    protected struct CombatState
    {
        public AnimancerState attack;
        public AnimancerState jump;
        public AnimancerState plunge_rise;
        public AnimancerState plunge_fall;
        public AnimancerState plunge_attack;
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

    public enum CrouchAction
    {
        Plunge,
        JumpTo_Pillar1,
        JumpTo_Pillar2,
        JumpTo_Pillar3,
        JumpTo_Center
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
        OnHitboxActive.AddListener(RealignToTarget);
        OnHurt.AddListener(() => {
            HitboxActive(0);
        });

        animancer.Play(navstate.idle);
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
        if (inventory.IsOffEquipped() && !inventory.IsOffDrawn())
        {
            inventory.SetDrawn(false, true);
        }
        if (shouldAct && CanAct())
        {
            clock = Random.Range(ActionDelayMinimum, ActionDelayMaximum);
            if (CombatTarget != null)
            {
                float navdist = GetDistanceToTarget();
                float realdist = Vector3.Distance(this.transform.position, GetCombatTarget().transform.position);

                //StartMeleeCombo1();

                //StartMeleeCombo2();

                //StartCrouch();

                /*
                CrouchAction action = CrouchAction.Plunge;
                int r = Random.Range(1, 10);
                if (r == 1)
                {
                    action = CrouchAction.JumpTo_Pillar1;
                }
                else if (r == 2)
                {
                    action = CrouchAction.JumpTo_Pillar2;
                }
                else if (r == 3)
                {
                    action = CrouchAction.JumpTo_Pillar3;
                }*/
                StartCrouch(actionAfterCrouch);
                /*
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
                }
                */
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
        if (animancer.States.Current == cstate.attack)
        {
            if (!IsHitboxActive())
            {
                Vector3 dir = (destination - this.transform.position).normalized;
                this.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(this.transform.forward, dir, AttackRotationSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f));
            }
            if (!GetGrounded() && airTime > 1f)
            {
                navstate.fall = animancer.Play(fallAnim, 1f);
                HitboxActive(0);
            }
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
        if (plunging)
        {
            ProcessPlunge();
        }
        else if (crouching)
        {
            ProcessCrouch();
        }
        if (CombatTarget != null)
        {
            Vector3 delta = CombatTarget.transform.position - lastTargetPos;
            if (Time.deltaTime > 0f)
            {
                targetSpeed = delta / Time.deltaTime;
            }
            else
            {
                targetSpeed = Vector3.zero;
            }
            lastTargetPos = CombatTarget.transform.position;
        }
        
    }

    public void StartMeleeCombo1()
    {
        RealignToTarget();
        //cstate.attack = CloseAttack.ProcessHumanoidAttack(this, _MoveOnEnd);
        nav.enabled = true;

        Vector3 pos = Vector3.zero;

        Vector3 centerPoint = CombatTarget.transform.position + (center.position - CombatTarget.transform.position).normalized * MeleeCombo1StartDistance;
        bool centerOnNav = NavMesh.SamplePosition(centerPoint, out NavMeshHit hit, 10f, 0);
        if (centerOnNav)
        {
            centerPoint = hit.position;
        }
        
        Vector3 farPoint = this.transform.position + (this.transform.position - CombatTarget.transform.position).normalized * MeleeCombo1StartDistance;
        bool farOnNav = NavMesh.SamplePosition(farPoint, out NavMeshHit hit2, 10f, 0);
        if (farOnNav)
        {
            farPoint = hit.position;
        }

        float centerDist = Vector3.Distance(CombatTarget.transform.position, centerPoint);
        float farDist = Vector3.Distance(CombatTarget.transform.position, farPoint);

        if (centerDist < MeleeCombo1StartDistance && farDist >= MeleeCombo1StartDistance)
        {
            pos = farPoint;
        }
        else if (farDist < MeleeCombo1StartDistance && centerDist >= MeleeCombo1StartDistance)
        {
            pos = centerPoint;
        }
        else if (farDist < MeleeCombo1StartDistance && centerDist < MeleeCombo1StartDistance)
        {
            if (centerDist > farDist)
            {
                pos = centerPoint;
            }
            else
            {
                pos = farPoint;
            }
        }
        else
        {
            pos = centerPoint;
        }

        void _Then()
        {
            RealignToTarget();
            nav.enabled = true;
            cstate.attack = MeleeCombo1.ProcessHumanoidAttack(this, () => { });
            OnAttack.Invoke();
        }

        DodgeJumpThen(pos, _Then, 1.25f);
    }

    // basic attack template
    /*
    public void StartFarAttack()
    {
        RealignToTarget();
        cstate.attack = FarAttack.ProcessHumanoidAttack(this, _MoveOnEnd);
        OnAttack.Invoke();
    }
    */

    public void StartMeleeCombo2()
    {
        RealignToTarget();
        cstate.attack = MeleeCombo2.ProcessHumanoidAttack(this, _MoveOnEnd);
        OnAttack.Invoke();
    }

    public void StartCrouch(CrouchAction action)
    {
        RealignToTarget();

        crouchClock = CrouchTime;
        animancer.Play(CrouchDown).Events.OnEnd = () => { animancer.Play(Crouch); };
        crouching = true;
        actionAfterCrouch = action;
    }
    public void StartPlungeAttack()
    {
        Vector3 position = GetProjectedPosition(PlungeTime);
        Vector3 offset = (this.transform.position - CombatTarget.transform.position);
        offset.y = 0f;
        offset = offset.normalized * 1.5f;
        plungeTarget = CombatTarget.transform.position + Vector3.ClampMagnitude((position + offset) - CombatTarget.transform.position, 1.5f);

        if (NavMesh.SamplePosition(plungeTarget, out NavMeshHit hit, Vector3.Distance(CombatTarget.transform.position, position) + 10f, NavMesh.AllAreas))
        {
            plungeTarget = hit.position;
        }
        else
        {
            _MoveOnEnd(); // cancel attack if destination isn't on mesh
            return;
        }

        Vector3 dir = plungeTarget - this.transform.position;
        dir.y = 0f;
        this.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        plungeClock = PlungeTime;
        cstate.plunge_rise = animancer.Play(PlungeJump);
        cstate.plunge_rise.Events.OnEnd = null;
        plungeStart = this.transform.position;
        OnAttack.Invoke();
        plunging = true;
    }

    public void ProcessCrouch()
    {
        crouchClock -= Time.deltaTime;
        if (crouchClock <= 0f)
        {
            crouching = false;
            switch (actionAfterCrouch)
            {
                case CrouchAction.Plunge:
                    StartPlungeAttack();
                    return;
                case CrouchAction.JumpTo_Pillar1:
                    onPillar = true;
                    DodgeJump(pillar1.position);
                    return;
                case CrouchAction.JumpTo_Pillar2:
                    onPillar = true;
                    DodgeJump(pillar2.position);
                    return;
                case CrouchAction.JumpTo_Pillar3:
                    onPillar = true;
                    DodgeJump(pillar3.position);
                    return;
            }
            //StartPlungeAttack();
        }
    }
    public void ProcessPlunge()
    {

        

        float t = 1f - Mathf.Clamp01(plungeClock/PlungeTime);
        nav.enabled = false;

        Vector3 pos;

        if (!onPillar)
        {
            pos = Vector3.Lerp(plungeStart, plungeTarget, horizPlungeCurve.Evaluate(t));
        }
        else
        {
            pos = Vector3.Lerp(plungeStart, plungeTarget, horizPlungePillarCurve.Evaluate(t));
        }

        float vert = 0f;
        if (!onPillar)
        {
            vert = Mathf.Lerp(plungeStart.y, PlungeJumpHeight, heightPlungeCurve.Evaluate(t));
        }
        else
        {
            vert = Mathf.Lerp(plungeTarget.y, plungeStart.y, heightPlungePillarCurve.Evaluate(t));
        }
       

        pos.y = vert;

        this.transform.position = pos;

        
        if (t >= 1f)
        {
            this.transform.position = plungeTarget;
            //EndPlunge();
        }
        else if (t >= AttackPoint)
        {
            if (animancer.States.Current == cstate.plunge_fall)
            {
                cstate.plunge_attack = Plunge.ProcessHumanoidAttack(this, EndPlunge);
            }
        }
        else if (t >= DescentPoint)
        {
            if (animancer.States.Current == cstate.plunge_rise)
            {
                cstate.plunge_fall = animancer.Play(PlungeFall);//Plunge.ProcessHumanoidAttack(this, EndPlunge);
            }
        }


        plungeClock -= Time.deltaTime;

        
    }

    void EndPlunge()
    {
        nav.enabled = true;
        plunging = false;
        onPillar = false;
        _MoveOnEnd();
    }
    public void DodgeJump(Vector3 position)
    {
        DodgeJumpThen(position, JumpEnd, 1f);
    }

    public void DodgeJumpThen(Vector3 position, System.Action endingAction, float speed)
    {
        AnimancerState jump = animancer.Play(JumpDodge);
        jump.Events.OnEnd = endingAction;
        jump.Speed *= speed;
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

    public Vector3 GetProjectedPosition(float timeOut)
    {
        Debug.DrawLine(CombatTarget.transform.position, CombatTarget.transform.position + targetSpeed * timeOut, Color.blue, 5f);
        return CombatTarget.transform.position + targetSpeed * timeOut;
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
        return animancer.States.Current == cstate.attack || animancer.States.Current == cstate.plunge_attack;
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

    public void EndAnim()
    {
        _MoveOnEnd();
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
