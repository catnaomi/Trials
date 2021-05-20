using UnityEngine;
using System.Collections;
using CustomUtilities;
using System;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.InputSystem.Interactions;

public class PlayerActor : HumanoidActor
{
    public static PlayerActor player;

    //CursorCam cameraController;
    Camera cam;
    bool shouldSecondSwing = false;

    public float rotateSpeed = 180f;
    public float BowAimRotation = 90f;

    public float unlockTime = 1f;
    public float lockDistance = 20f;

    public float minRollSpeed = 3f;
    public float maxRollSpeed = 7f;

    AxisUtilities.AxisDirection dodgeDirection;
    [HideInInspector] public Vector3 camRotation;

    public float chargeTime;
    private bool buttonHasBeenReleased;

    public Transform centerTransform;

    [Space(5)]
    public StanceHandler stance;
    [ReadOnly]
    public InputAttack currentAttackInput;
    public Moveset moveset;
    PlayerInput inputs;

    bool startDodge;

    Vector3 stickDirection;
    //Vector3 moveDirection;

    Vector3 airDirection;
    Vector3 lastPos;

    List<Interactable> interactables;
    public Interactable highlightedInteractable;
    [Header("Controls")]
    public Vector2 move;
    public Vector2 look;
    [Space(5)]
    public bool jump;
    public bool dodge;
    //public bool sprint;
    [Space(5)]
    public UnityEvent toggleTarget;
    public UnityEvent secondaryStickFlick;
    [Space(10)]
    public bool stanceMain;
    public bool stanceOff;
    public bool stanceBlock;
    public Vector2 attackDir;
    

    [Header("Ledge Hang Settings")]
    public Collider hangCollider;
    public Collider hangBar;
    public Transform hangMountR;
    public Transform hangMountL;
    public ClimbDetector currentClimb; // current ladder, ledge, wall, rope, etc.
    Vector3 hangOffset;
    public bool ledgeSnap;
    public bool ledgeHanging;
    public bool ladderSnap;
    public bool ladderLockout;

    private void OnEnable()
    {
        player = this;
        bool b = false;
    }
    public override void ActorStart()
    {
        base.ActorStart();

        cam = Camera.main;
        //cameraController =  cam.GetComponent<CursorCam>();

        camRotation = transform.forward;

        GetStance();

        OnSheathe.AddListener(Spare);

        interactables = new List<Interactable>();

        this.OnInjure.AddListener(() => { FXController.SlowMo(0.5f, 3f); });
        this.OnDodge.AddListener(() => { FXController.SlowMo(0.1f, 0.5f); });
        //OnHit.AddListener(() => { FXController.Hitpause(1f); });

        SetupInput();
    }

    private enum AlignMode
    {
        None,
        Camera,
        Stick,
        Target
    }

    public override void ActorPreUpdate()
    {
        //GetInput();
        //SetupInput();

        float primaryVertical = move.y;
        float primaryHorizontal = move.x;
        float secondaryVertical = look.y;
        float secondaryHorizontal = look.x;

        //bool buttonPressed = HandleInput();

        AlignMode alignMode = AlignMode.None;

        bool canMove = CanMove();
        bool weaponDrawn = inventory.IsWeaponDrawn();
        bool isGrounded = GetGrounded();
        bool isAiming = IsAiming();
        bool lockedOn = this.GetCombatTarget() != null;//cameraController.lockedOn;

        animator.SetBool("Cam-Locked", lockedOn);
        //animator.SetBool("Cam-Aiming", isAiming && Input.GetButton("Aim"));

        float slowMultiplier = 1f;

        /*
         *  public float BaseMovementSpeed = 5f;
    public float ForwardMultiplier = 1f;
    public float StrafeMultiplier = 0.5f;
    public float BackwardsMultiplier = 1f;
    public float WeaponDrawnMultiplier = 0.75f;
    public float SpringMultiplier = 4f;
         */

        slowMultiplier = GetSlowMultiplier();

        /*
        if (lockedOn)
        {
            slowMultiplier = WeaponDrawnMultiplier;
        }
        if (isAiming || IsBlocking())
        {
            slowMultiplier = OffHandMultiplier;
        }
        if (IsSprinting())
        {
            slowMultiplier = weaponDrawn ? SprintMultiplierArmed : SprintMultiplierUnarmed;
        }
        */

        Vector3 forward = cam.transform.forward;
        forward.y = 0;
        forward = forward.normalized;

        Vector3 right = cam.transform.right;
        right.y = 0;
        right = right.normalized;

        stickDirection = Vector3.ClampMagnitude(primaryVertical * forward + primaryHorizontal * right, 1f);

        if (lockedOn && !IsSprinting())
        {
            moveDirection = Vector3.ClampMagnitude(
                (
                    Mathf.Clamp(primaryVertical, -BackwardsMultiplier, ForwardMultiplier) * forward * BaseMovementSpeed +
                    Mathf.Clamp(primaryHorizontal, -StrafeMultiplier, StrafeMultiplier) * right * BaseMovementSpeed
                ),
                BaseMovementSpeed * slowMultiplier
            );
        }
        else if (true)
        {
            moveDirection = stickDirection * BaseMovementSpeed * slowMultiplier;
        }

        //animator.SetLayerWeight(StanceHandler.ActionLowerLayer, (stickDirection == Vector3.zero) ? 1f : 0f);

        if (IsSprinting())
        {
            moveDirection *= SprintMultiplier;
        }

        if (GetGrounded()) //CanMove() && !IsJumping())// && moveDirection != Vector3.zero)
        {
            airDirection = moveDirection;
        }

        if (IsJumping())
        {
            //airDirection = Vector3.Project(airDirection, this.transform.forward);
        }
        lastPos = transform.position;

        if (isAiming)
        {
            alignMode = AlignMode.Camera;
        }
        else if (!GetGrounded() && airTime > 0.25f)
        {
            alignMode = AlignMode.None;
        }
        else if (IsSprinting())// || IsDodging() || startDodge)
        {
            if (stickDirection.magnitude > 0f)
            {
                alignMode = AlignMode.Stick;
            }
            else
            {
                alignMode = AlignMode.None;
            }
        }
        else if (IsDodging())
        {
            alignMode = AlignMode.None;
        }
        else if (lockedOn && Vector3.Distance(this.transform.position, this.GetCombatTarget().transform.position) > 0.25f)
        {
            alignMode = AlignMode.Target;
        }
        else if (stickDirection.magnitude > 0 && CanMove())
        {
            alignMode = AlignMode.Stick;
        }

        float forwardVel = Mathf.Clamp(Mathf.Clamp(primaryVertical, -BackwardsMultiplier, ForwardMultiplier), -slowMultiplier, slowMultiplier);
        float strafeVel = Mathf.Clamp(Mathf.Clamp(primaryHorizontal, -StrafeMultiplier, StrafeMultiplier), -slowMultiplier, slowMultiplier);
        float stickVel = Mathf.Min(stickDirection.magnitude, slowMultiplier);

        float descendClamp = 0f;
        float ascendClamp = 0f;
        bool dismountLadder = false;
        if (IsHanging())
        {
            if (currentClimb != null && currentClimb is Ladder ladder)
            {
                descendClamp = (ladder.canDescend) ? -1 : 0;
                ascendClamp = (ladder.canAscend) ? 1 : 0;

                dismountLadder = ladder.SetCurrentHeight(this.transform.position);
            }
        }

        if (CanMove())
        {
            if (alignMode == AlignMode.Camera || alignMode == AlignMode.None)
            {
                animator.SetFloat("ForwardVelocity", Mathf.Lerp(animator.GetFloat("ForwardVelocity"), forwardVel, 0.2f));
                animator.SetFloat("StrafingVelocity", Mathf.Lerp(animator.GetFloat("StrafingVelocity"), strafeVel, 0.2f));
            }
            else if (alignMode == AlignMode.Target)
            {
                float fv = Vector3.Dot(moveDirection,this.transform.forward);
                float sv = Vector3.Dot(moveDirection, this.transform.right);

                Vector2 targetStrafing = new Vector2(fv, sv);
                targetStrafing.Normalize();

                animator.SetFloat("ForwardVelocity", Mathf.Lerp(animator.GetFloat("ForwardVelocity"), targetStrafing.x, 0.2f));
                animator.SetFloat("StrafingVelocity", Mathf.Lerp(animator.GetFloat("StrafingVelocity"), targetStrafing.y, 0.2f));
            }
            else
            {
                animator.SetFloat("ForwardVelocity", Mathf.Lerp(animator.GetFloat("ForwardVelocity"), stickVel, 0.1f));
                animator.SetFloat("StrafingVelocity", 0f);
            }
        }
        else if (!IsDodging() && !IsJumping() && isGrounded)
        {
            animator.SetFloat("ForwardVelocity", 0f);
            animator.SetFloat("StrafingVelocity", 0f);
        }
        else if (IsHanging())
        {
            animator.SetFloat("ForwardVelocity", Mathf.Lerp(animator.GetFloat("ForwardVelocity"), Mathf.Clamp(primaryVertical, descendClamp, ascendClamp), 0.2f));
        }

        animator.SetBool("LedgeSnap", ledgeSnap);
        animator.SetBool("LadderSnap", ladderSnap);
        animator.SetBool("LadderClimb", dismountLadder);

        if (ladderLockout && isGrounded)
        {
            ladderLockout = false;
        }
        animator.SetBool("LadderLockout", ladderLockout);

        animator.SetLayerWeight(animator.GetLayerIndex("Right Arm Override"), inventory.IsMainDrawn() ? 1f : 0f);
        animator.SetLayerWeight(animator.GetLayerIndex("Left Arm Override"), inventory.IsOffDrawn() ? 1f : 0f);
        //animator.SetFloat("StickVelocity", stickDirection.magnitude);
        //animator.SetFloat("ForwardVelocity", primaryVertical);
        //animator.SetFloat("StrafingVelocity", primaryHorizontal);
        //animator.SetBool("LockedOn", cameraController.lockedOn);

        /*
        float heft = 1f;
        if (inventory.GetWeapon() != null)
        {
            heft = inventory.EquippedWeapon.GetHeft();
        }
        animator.SetFloat("Heft", heft);
        */

        camRotation = Quaternion.AngleAxis(secondaryHorizontal * rotateSpeed * Time.deltaTime, transform.up) * camRotation;

        SetCrosshairMode(isAiming);

        
        if (alignMode == AlignMode.Camera)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(forward), 720f * Time.fixedDeltaTime);
        }
        else if (alignMode == AlignMode.Stick)
        {
            transform.rotation = Quaternion.LookRotation(stickDirection.normalized);//Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(stickDirection.normalized), 720f * 5f * Time.fixedDeltaTime);
        }
        else if (alignMode == AlignMode.Target)
        {
            Vector3 tv = GetCombatTarget().transform.position - player.positionReference.Hips.position;
            tv.y = 0;
            tv.Normalize();
            transform.rotation = Quaternion.LookRotation(tv, Vector3.up);
            //transform.LookAt(this.GetCombatTarget().transform.position, Vector3.up);
        }

        startDodge = false;
        //jump = false;
        dodge = false;
    }

    protected new void FixedUpdate()
    {
        base.FixedUpdate();
        //transform.Rotate(Vector3.up, horizontal * rotateSpeed * Time.deltaTime);

        Vector3 finalMov = Vector3.zero;
        if (!IsHanging() && (IsAerial() || IsFalling() || IsJumping()))
        {
            finalMov += airDirection;
        }
        else if ((CanMove() && humanoidState == HumanoidState.Actionable))
        {
            finalMov += moveDirection;
            /*
            if (IsAerial())
            {
                finalMov += airDirection;
            }
            else
            {
                finalMov += moveDirection;
            }*/
        }
        finalMov += moveAdditional;

        if (cc.enabled)
        {
            if (IsJumping())
            {
                cc.Move(finalMov * Time.fixedDeltaTime);
            }
            else if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.5f, LayerMask.GetMask("Terrain")))
            {
                Vector3 temp = Vector3.Cross(hit.normal, finalMov);
                cc.Move((Vector3.Cross(temp, hit.normal) + gravity) * Time.fixedDeltaTime);
                //transform.Translate(0, (hit.point - transform.position).y * Time.fixedDeltaTime * GetCurrentSpeed() * 2f, 0);
            }
            else if (!IsJumping() && !IsHanging())
            {
                cc.Move((finalMov + gravity) * Time.fixedDeltaTime);
            }
            else
            {
                cc.Move(finalMov * Time.fixedDeltaTime);
            }
        }

        /*
        if (CanMove() && humanoidState == HumanoidState.Actionable)
        {
            // TODO: find a way to keep grounded when walking down slopes and stairs
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.5f, LayerMask.GetMask("Terrain")))
            {
                Vector3 temp = Vector3.Cross(hit.normal, moveDirection);
                cc.Move(Vector3.Cross(temp, hit.normal) * Time.fixedDeltaTime);
                //transform.Translate(0, (hit.point - transform.position).y * Time.fixedDeltaTime * GetCurrentSpeed() * 2f, 0);
            }
            else if (IsAerial())
            {
                cc.Move((airDirection + gravity) * Time.fixedDeltaTime);
                Debug.Log(airDirection);
            }
            else
            {

                cc.Move((moveDirection + gravity) * Time.fixedDeltaTime);
            }
        }
        else if (!IsJumping())
        {
            cc.Move(gravity * Time.fixedDeltaTime);
        }
        */
    }

    // TODO: redo camera controller and make aiming more flexible for different weapons
    // use VTMB style crosshair on ground for aiming, and press a bumper to move camera to over the shoulder position?
    protected void LateUpdate()
    {
        base.LateUpdate();

        /*
        if (IsAiming() && stance != null && stance.heavyAttack is HeavyAttackAim heavyAttackAim && heavyAttackAim.ikHandler != null)
        {
            heavyAttackAim.ikHandler.OnUpdate(this);
        }
        */
        /*
        if (IsAiming() && //cameraController.crosshairMode)
        {
            Vector3 eyePos = transform.position + cameraController.playerEyeHeight * Vector3.up;
            Vector3 aimDir = (cameraController.focusPosition - eyePos).normalized;
            Quaternion aimRot = Quaternion.LookRotation(aimDir, Vector3.up) * Quaternion.AngleAxis(BowAimRotation, Vector3.up);
            positionReference.Spine.rotation = aimRot;
        }
        */
    }

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        look = value.Get<Vector2>();
    }

    /*
    public void OnAtk_ThrustMain(InputValue value)
    {
        Debug.Log("Main Thrust Press");

    }

    public void OnAtk_SlashMain(InputValue value)
    {
        Debug.Log("Main Slash Press");

    }

    public void OnAtk_ThrustOff(InputValue value)
    {
        Debug.Log("Off Thrust Press");

    }

    public void OnAtk_SlashOff(InputValue value)
    {
        Debug.Log("Off Slash Press");

    }*/

    public void OnInputAttack(InputAttack atk)
    {
        currentAttackInput = atk;
        if (true)
        {
            if (!inventory.IsMainDrawn())
            {
                TriggerSheath(true, Inventory.EquipSlot.lHip, true);
            }
            else
            {
                animator.SetTrigger("Input-Attack");
                animator.SetInteger("Input-AttackID", atk.attackId);
            }
        }
    }

    public void Jump()
    {
        if (!IsHanging())
        {
            animator.SetTrigger("Input-Jump");
        }
        else
        {
            animator.SetTrigger("LedgeClimb");
        }   
    }

    public void Dodge()
    {
        if (!IsHanging())
        {
            animator.SetTrigger("Input-Dodge");
            Vector2 dodgeDir;
            if (move == Vector2.zero)
            {
                dodgeDir = new Vector2(0, -1);
            }
            else if (this.GetCombatTarget() != null)
            {
                dodgeDir = move;
            }
            else
            {
                dodgeDir = new Vector2(0, 1);
            }
            dodgeDir.Normalize();
            animator.SetFloat("DodgeForward", dodgeDir.y);
            animator.SetFloat("DodgeStrafe", dodgeDir.x);
        }
        else
        {
            if (currentClimb is Ledge)
            {
                UnsnapFromLedge();
            }
            else if (currentClimb is Ladder)
            {
                animator.SetTrigger("Sheath-Main");
                UnsnapFromLadder();
            }
        }
    }

    public void Block(bool block)
    {
        animator.SetBool("Blocking", block);
    }
    private void SetupInput()
    {
        inputs = GetComponent<PlayerInput>();

        inputs.actions["Atk_ThrustMain"].performed += (context) =>
        {
            if (!context.performed) return;
            if (context.interaction is TapInteraction)
            {
                InputAttack atk = this.moveset.thrustMain;
                if (atk != null)
                {
                    OnInputAttack(atk);
                }
            }
            else if (context.interaction is HoldInteraction)
            {
                InputAttack atk = this.moveset.thrustMainHeavy;
                if (atk != null)
                {
                    OnInputAttack(atk);
                }
            }
        };

        inputs.actions["Atk_SlashMain"].performed += (context) =>
        {
            if (context.interaction is TapInteraction)
            {
                InputAttack atk = this.moveset.slashMain;
                if (atk != null)
                {
                    OnInputAttack(atk);
                }
            }
            else if (context.interaction is HoldInteraction)
            {
                InputAttack atk = this.moveset.slashMainHeavy;
                if (atk != null)
                {
                    OnInputAttack(atk);
                }
            }
        };

        inputs.actions["Atk_ThrustOff"].performed += (context) =>
        {
            if (!context.performed) return;
            if (context.interaction is TapInteraction)
            {
                InputAttack atk = this.moveset.thrustOff;
                if (atk != null)
                {
                    OnInputAttack(atk);
                }
            }
            else if (context.interaction is HoldInteraction)
            {
                InputAttack atk = this.moveset.thrustOffHeavy;
                if (atk != null)
                {
                    OnInputAttack(atk);
                }
            }
        };

        inputs.actions["Atk_SlashOff"].performed += (context) =>
        {
            if (context.interaction is TapInteraction)
            {
                InputAttack atk = this.moveset.slashOff;
                if (atk != null)
                {
                    OnInputAttack(atk);
                }
            }
            else if (context.interaction is HoldInteraction)
            {
                InputAttack atk = this.moveset.slashOffHeavy;
                if (atk != null)
                {
                    OnInputAttack(atk);
                }
            }
        };

        inputs.actions["ChangeTarget"].performed += (context) =>
        {
            if (context.interaction is PressInteraction)
            {
                secondaryStickFlick.Invoke();
            }
        };

        inputs.actions["Target"].performed += (context) =>
        {
            if (true)
            {
                toggleTarget.Invoke();
            }
        };

        inputs.actions["Jump"].performed += (context) =>
        {
            jump = true;
            Jump();
        };

        inputs.actions["Dodge"].performed += (context) =>
        {
            dodge = true;
            Dodge();
        };

        inputs.actions["Block"].started += (context) =>
        {
            Block(true);
        };

        inputs.actions["Block"].canceled += (context) =>
        {
            Block(false);
        };

        /*
        mainThrustPress.AddListener(() => {
            Debug.Log("Main Thrust Press");
        });
        mainThrustHold.AddListener(() => {
            Debug.Log("Main Thrust Hold");
        });
        mainSlashPress.AddListener(() => {
            Debug.Log("Main Slash Press");
        });
        mainSlashHold.AddListener(() => {
            Debug.Log("Main Slash Hold");
        });
        offThrustPress.AddListener(() => {
            Debug.Log("Off Thrust Press");
        });
        offThrustHold.AddListener(() => {
            Debug.Log("Off Thrust Hold");
        });
        offSlashPress.AddListener(() => {
            Debug.Log("Off Thrust Press");
        });
        offSlashHold.AddListener(() => {
            Debug.Log("off Thrust Hold");
        });*/
    }

    private void GetInput()
    {
        if (IsHanging())
        {
            if (jump)
            {
                animator.SetTrigger("LedgeClimb");
            }
            else if (dodge)
            {
                if (inventory.IsWeaponDrawn())
                    if (currentClimb is Ledge)
                    {
                        UnsnapFromLedge();
                    }
                    else if (currentClimb is Ladder)
                    {
                        animator.SetTrigger("Sheath-Main");
                        UnsnapFromLadder();
                    }
            }
        }
    }

    public new void TriggerSheath(bool draw, Inventory.EquipSlot slot, bool targetMain)
    {
        base.TriggerSheath(draw, slot, targetMain);
        RegisterPlayerInput();
    }

    private void RegisterPlayerInput()
    {
        animator.SetBool("Input-Player", true);
    }

    public void GetStance()
    {
        if (inventory.IsMainEquipped())
        {
            if (this.stance != null)
            {
                //stance.RemoveHeavyAttack(this);
                //stance.RemoveSpecialAttack(this);
            }
            inventory.UpdateStance(stance);
            ApplyStance();
        }
    }
    public void ApplyStance()
    {
        //this.animator.runtimeAnimatorController = stance.GetController();

        if (inventory.IsMainDrawn() && inventory.IsOffDrawn())
        {
            this.animator.SetFloat("Style-Left", (int)stance.leftGrip);
            this.animator.SetFloat("Style-Right", (int)stance.rightGrip);
        }
        else if (inventory.IsMainDrawn() && inventory.IsTwoHanding())
        {
            this.animator.SetFloat("Style-Left", (int)stance.twohandGrip);
            this.animator.SetFloat("Style-Right", (int)stance.twohandGrip);
        }
        else if (inventory.IsOffDrawn())
        {
            // this shouldn't happen?
        }
        else
        {
            this.animator.SetFloat("Style-Left", 0);
            this.animator.SetFloat("Style-Right", 0);
        }

        this.animator.SetFloat("Style-Stance", (int)stance.stanceStyle);
        this.animator.SetFloat("Style-Block", (int)stance.blockStyle);

        //stance.ApplyHeavyAttack(this);
        //stance.ApplySpecialAttack(this);

        //this.animator.SetBool("HeavyBackStep", stance.ShouldHeavyBackStep());

        this.animator.SetFloat("AttackSpeed1", inventory.GetAttackSpeed());
        this.animator.SetFloat("AttackSpeed2", inventory.GetOffAttackSpeed());
    }
    public void StartHoldCheck()
    {
        buttonHasBeenReleased = false;
    }

    public bool WasLastAttackCharged()
    {
        if (buttonHasBeenReleased == false && chargeTime > 1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool GetShouldRelease(params string[] buttons)
    {
        bool allReleased = true;
        foreach (string button in buttons)
        {
            if (Input.GetButton(button))
            {
                allReleased = false;
            }
        }
        if (allReleased)
        {
            buttonHasBeenReleased = true;
        }
        if (buttonHasBeenReleased || chargeTime > 1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    /*
    public override void DeductPoiseFromAttack()
    {
        attributes.ReducePoise(attributes.GetPoiseCost(WasLastAttackCharged()));
    }
    */
    public void SetCrosshairMode(bool mode)
    {
        //cameraController.SetCrosshairMode(mode);
    }

    public override Vector3 GetLaunchVector(Vector3 origin)
    {
        
        GameObject target = this.GetCombatTarget();
        if (target != null)
        {
            if (Vector3.Distance(target.transform.position, origin) > 2)
            {
                return (target.transform.position - origin).normalized;
            }
            else
            {
                return this.transform.forward;
            }
        }
        else if (IsAiming())
        {
            Vector3 aimPos = cam.transform.position + cam.transform.forward * 100f;

            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 100f) && !hit.transform.IsChildOf(this.transform.root))
            {
                aimPos = hit.point;
            }
            return (aimPos - origin).normalized;
        }
        else
        {
            return this.transform.forward;
        }
    }

    #region climbing

    // ledge
    public void SetLedge(Ledge ledge)
    {
        if (CanMove()|| IsJumping() || IsFalling())
        {
            ledgeSnap = true;
            currentClimb = ledge;
        }
    }

    public void UnsnapLedge(ClimbDetector ledge)
    {
        ledgeSnap = false;
    }

    public void SnapToCurrentLedge()
    {

        StartCoroutine(DelayedSnapToLedge());
    }
    
    IEnumerator DelayedSnapToLedge()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        if (ledgeSnap == true && currentClimb != null)
        {


            this.transform.rotation = Quaternion.LookRotation(currentClimb.collider.transform.forward);

            Vector3 offset = currentClimb.collider.transform.position - this.hangCollider.bounds.center;

            Vector3 verticalOffset = Vector3.up * offset.y;
             
            Vector3 horizontalOffset = Vector3.Project(offset, currentClimb.collider.transform.forward);

            boundingCollider.enabled = false;

            transform.position = this.transform.position + (verticalOffset + horizontalOffset);

            Debug.Log("movement! horiz" + horizontalOffset.magnitude);

            ledgeHanging = true;

        }
    }
    public void UnsnapHandsFromLedge()
    {
        ledgeHanging = false;
    }

    public void UnsnapFromLedge()
    {
        ledgeHanging = false;
        ledgeSnap = false;
        boundingCollider.enabled = true;
    }

    // ladder
    public void SetLadder(Ladder ladder)
    {
        if (CanMove() || IsJumping() || IsFalling())
        {
            ladderSnap = true;
            currentClimb = ladder;
        }
    }

    public void SnapToCurrentLadder()
    {
        StartCoroutine(DelayedSnapToLadder());
    }

    IEnumerator DelayedSnapToLadder()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        if (ladderSnap == true && currentClimb != null)
        {


            this.transform.rotation = Quaternion.LookRotation(currentClimb.collider.transform.forward);

            Vector3 offset = currentClimb.collider.transform.position - this.transform.position;

            Vector3 forwardOffset = Vector3.Project(offset, currentClimb.collider.transform.forward);

            Vector3 rightOffset = Vector3.Project(offset, currentClimb.collider.transform.right);

            boundingCollider.enabled = false;

            transform.position = this.transform.position + (forwardOffset + rightOffset);

            //ledgeHanging = true;

        }
    }

    public void UnsnapFromLadder()
    {
        ladderSnap = false;
        ladderLockout = true;
        boundingCollider.enabled = true;
    }
    #endregion

    public void AddInteractable(Interactable interactable)
    {
        if (!interactables.Contains(interactable))
        {
            interactables.Add(interactable);
            GetHighlightedInteractable();
        }
    }

    public void RemoveInteractable(Interactable interactable)
    {
        interactables.Remove(interactable);
        GetHighlightedInteractable();
    }

    public void ClearInteractables()
    {
        interactables.Clear();
        GetHighlightedInteractable();
    }

    public Interactable GetHighlightedInteractable()
    {
        highlightedInteractable = null;
        float leadDist = Mathf.Infinity;

        foreach (Interactable interactable in interactables)
        {
            float dist = Vector3.Distance(this.transform.position, interactable.transform.position);
            if (dist < leadDist)
            {
                leadDist = dist;
                highlightedInteractable = interactable;
            }
            interactable.SetIconVisiblity(false);
        }
        if (highlightedInteractable != null)
        {
            highlightedInteractable.SetIconVisiblity(true);
        }
        return highlightedInteractable;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (IsAiming())
        {
            animator.SetLookAtWeight(0f);
            //animator.bodyRotation = cam.transform.rotation;
            //animator.SetLookAtPosition(cameraController.playerIKPosition);
        }
        else
        {
            animator.SetLookAtWeight(0);
        }
        if (ledgeHanging)
        {

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, hangMountL.position);

            animator.SetIKPosition(AvatarIKGoal.RightHand, hangMountR.position);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
        }
        
    }
    private void OnDrawGizmos()
    {
        try
        {
            //Gizmos.DrawLine(positionReference.Head.transform.position, positionReference.Head.transform.position + GetLaunchVector(positionReference.Head.transform.position) * 100f);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(this.positionReference.MainHand.transform.position, this.positionReference.MainHand.transform.position + this.positionReference.MainHand.transform.forward * 0.25f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.positionReference.MainHand.transform.position, this.positionReference.MainHand.transform.position + this.positionReference.MainHand.transform.right * 0.25f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(this.positionReference.MainHand.transform.position, this.positionReference.MainHand.transform.position + this.positionReference.MainHand.transform.up * 0.25f);
        }
        catch (Exception ex)
        {
            //Debug.Log(ex.Message);
        }
    }
}
