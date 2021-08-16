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
    IKHandMatch ikHand;
    float ikHandWeight;
    float showRight;
    float showLeft;
    float show2h;
    Transform offGrip;
    Vector3 offGripLast;
    PlayerInput inputs;

    public bool isMenuOpen;
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


    [Space(20)]
    [Range(0f, 1f)]
    public float tempVal;
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

        inventory.OnChange.AddListener(GetStance);
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
        bool lockedOn = IsLockedOn();

        animator.SetBool("Cam-Locked", lockedOn);
        //animator.SetBool("Cam-Aiming", isAiming && Input.GetButton("Aim"));

        float slowMultiplier = 1f;

        if (animator.GetBool("Input-AttackHeld"))
        {
            animator.SetFloat("Input-AttackHeldTime", animator.GetFloat("Input-AttackHeldTime") + Time.deltaTime);
        }
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

        ApplyStance();
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

        if (offGrip != null)
        {
            offGripLast = offGrip.transform.position;
        }
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
        if (!CanPlayerInput()) return;
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
                animator.SetBool("Input-AttackHeld", true);
                animator.SetFloat("Input-AttackHeldTime", 0f);
            }
        }
    }

    public void OnAttackRelease()
    {
        if (!CanPlayerInput()) return;
        animator.SetBool("Input-AttackHeld", false);
    }

    public void Jump()
    {
        if (!CanPlayerInput()) return;
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
        if (!CanPlayerInput()) return;
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
        if (!CanPlayerInput()) return;
        animator.SetBool("Blocking", block);
    }

    public void Sheathe()
    {
        if (!CanPlayerInput()) return;
        if (inventory.IsOffDrawn())
        {
            TriggerSheath(false, inventory.GetOffWeapon().OffHandEquipSlot, false);
        }
        else if (inventory.IsMainDrawn())
        {
            TriggerSheath(false, inventory.GetMainWeapon().MainHandEquipSlot, true);
        }
        InventoryUI2.invUI.FlareSlot(3);
    }
    private void SetupInput()
    {
        inputs = GetComponent<PlayerInput>();

        inputs.actions["Atk_ThrustMain"].performed += (context) =>
        {
            if (!context.performed) return;
            if (this.moveset == null) return;
            if (context.interaction is TapInteraction)
            {
                InputAttack atk = (!IsTwoHanding()) ? this.moveset.thrust1h : this.moveset.thrust2h;
                if (atk != null)
                {
                    OnInputAttack(atk);
                }
            }
            else if (context.interaction is HoldInteraction)
            {
                InputAttack atk = (!IsTwoHanding()) ? this.moveset.thrustHeavy1h : this.moveset.thrustHeavy2h;
                if (atk != null)
                {
                    OnInputAttack(atk);
                }
            }
        };

        inputs.actions["Atk_ThrustMain"].canceled += (context) =>
        {
            OnAttackRelease();
        };

        inputs.actions["Atk_SlashMain"].performed += (context) =>
        {
            if (this.moveset == null) return;
            if (context.interaction is TapInteraction)
            {
                InputAttack atk = (!IsTwoHanding()) ? this.moveset.slash1h : this.moveset.slash2h;
                if (atk != null)
                {
                    OnInputAttack(atk);
                }
            }
            else if (context.interaction is HoldInteraction)
            {
                InputAttack atk = (!IsTwoHanding()) ? this.moveset.slashHeavy1h : this.moveset.slashHeavy2h;
                if (atk != null)
                {
                    OnInputAttack(atk);
                }
            }
        };

        inputs.actions["Atk_SlashMain"].canceled += (context) =>
        {
            OnAttackRelease();
        };

        /*
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
        */

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

        inputs.actions["Menu"].started += (context) =>
        {
            ToggleMenu();
        };

        inputs.actions["Interact"].started += (context) =>
        {
            Interact();
        };

        inputs.actions["QuickSlot - 0"].performed += (context) =>
        {
            if (CanPlayerInput() || (InventoryUI2.invUI != null && InventoryUI2.invUI.awaitingQuickSlotEquipInput))
            {
                inventory.InputOnSlot(0);
                InventoryUI2.invUI.FlareSlot(0);
            }
        };

        inputs.actions["QuickSlot - 1"].performed += (context) =>
        {
            if (CanPlayerInput() || (InventoryUI2.invUI != null && InventoryUI2.invUI.awaitingQuickSlotEquipInput))
            {
                inventory.InputOnSlot(1);
                InventoryUI2.invUI.FlareSlot(1);
            }
        };

        inputs.actions["QuickSlot - 2"].performed += (context) =>
        {
            if (CanPlayerInput() || (InventoryUI2.invUI != null && InventoryUI2.invUI.awaitingQuickSlotEquipInput))
            {
                inventory.InputOnSlot(2);
                InventoryUI2.invUI.FlareSlot(2);
            }
        };

        inputs.actions["Sheathe"].performed += (context) =>
        {
            Sheathe();
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

    // checks to see if player input is accepted. used for inventory menu
    public bool CanPlayerInput()
    {
        return !isMenuOpen;
    }
    public void ToggleMenu()
    {
        if (!isMenuOpen)
        {
            MenuController.menu.ShowMenu();
            isMenuOpen = true;
        }
        else
        {
            MenuController.menu.HideMenu();
            isMenuOpen = false;
        }
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

    public bool IsTwoHanding()
    {
        return inventory.IsTwoHanding();
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

        ResetMainRotation();
        float HEAVY_WEIGHT_THRESHOLD = 5f;
        float HEAVY_LENGTH_THRESHOLD = 3f;

        bool twohand = IsTwoHanding();
        bool mainDrawn = inventory.IsMainDrawn();
        bool offDrawn = inventory.IsOffDrawn();
        
        bool mainHeavy = mainDrawn && inventory.GetMainWeapon() is BladeWeapon mb && ((mb.GetWeight() >= HEAVY_WEIGHT_THRESHOLD) || (mb.GetLength() > HEAVY_LENGTH_THRESHOLD));
        bool offHeavy = offDrawn && inventory.GetOffWeapon() is BladeWeapon ob && ((ob.GetWeight() >= HEAVY_WEIGHT_THRESHOLD) || (ob.GetLength() > HEAVY_LENGTH_THRESHOLD));

        /*
        bool leftEquipped = inventory.IsOffDrawn() || (inventory.IsMainDrawn() && inventory.GetMainWeapon().ParentLeftAsMain);
        bool rightEquipped = inventory.IsMainDrawn() || (inventory.IsOffDrawn() && inventory.GetOffWeapon().ParentRightAsOff);
        animator.SetLayerWeight(animator.GetLayerIndex("Right Arm Override"), rightEquipped ? 1f : 0f);
        animator.SetLayerWeight(animator.GetLayerIndex("Left Arm Override"), leftEquipped ? 1f : 0f);
        */
        // stance: 14, 1 or 12, maybe different when locked on?

        

        if (!mainDrawn && !offDrawn) // unarmed
        {
            // unarmed
            stance.stanceStyle = StanceHandler.StanceStyle.None;
            stance.leftGrip = StanceHandler.GripStyle.None;
            stance.rightGrip = StanceHandler.GripStyle.None;
            stance.twohandGrip = StanceHandler.GripStyle.None;

            DefaultLayerWeights();
        }
        else if (mainDrawn && !offDrawn) // main hand only
        {
            if (!mainHeavy)
            {
                stance.twohandGrip = StanceHandler.GripStyle.Greatsword;
                stance.rightGrip = StanceHandler.GripStyle.Unarmed;
                stance.leftGrip = StanceHandler.GripStyle.None;
                stance.stanceStyle = StanceHandler.StanceStyle.Casual_OffF;

                DefaultLayerWeights();
            }
            else
            {  
                stance.twohandGrip = StanceHandler.GripStyle.TwoHandPoise;
                stance.rightGrip = StanceHandler.GripStyle.Shoulder;
                stance.leftGrip = StanceHandler.GripStyle.None;
                stance.stanceStyle = StanceHandler.StanceStyle.Shoulder_OffF;//(twohand) ? StanceHandler.StanceStyle.Shield_OffF : StanceHandler.StanceStyle.Shoulder_OffF;


                DefaultLayerWeights();
                if (twohand)
                {
                    //RotateMainWeapon(tempValue1 * 90f);
                    SetIKHands(IKHandMatch.Down, 0.1f);
                }
            }
            
        }
        else if (!mainDrawn && offDrawn) // off hand only
        {
            // dunno yet
            stance.stanceStyle = StanceHandler.StanceStyle.None;
            stance.leftGrip = StanceHandler.GripStyle.None;
            stance.rightGrip = StanceHandler.GripStyle.None;
            stance.twohandGrip = StanceHandler.GripStyle.None;

            DefaultLayerWeights();
        }
        else if (mainDrawn && offDrawn) // dual wield
        {
            if (inventory.GetOffWeapon() is OffHandShield)
            {
                stance.twohandGrip = StanceHandler.GripStyle.Greatsword;
                stance.rightGrip = (!mainHeavy) ? StanceHandler.GripStyle.Unarmed : StanceHandler.GripStyle.Shoulder;
                stance.leftGrip = StanceHandler.GripStyle.None;
                stance.stanceStyle = (!mainHeavy) ? StanceHandler.StanceStyle.Casual_OffF : StanceHandler.StanceStyle.Shoulder_OffF;

                DefaultLayerWeights();
            }
            else
            {
                // dunno yet, but account for shield
                stance.stanceStyle = StanceHandler.StanceStyle.None;
                stance.leftGrip = StanceHandler.GripStyle.None;
                stance.rightGrip = StanceHandler.GripStyle.None;
                stance.twohandGrip = StanceHandler.GripStyle.None;

                DefaultLayerWeights();
            }
            
        }

        if (inventory.IsMainEquipped())
        {
            Transform grpO = InterfaceUtilities.FindRecursively(inventory.GetWeaponModel().transform, "GripOff");
            if (grpO != null)
            {
                offGrip = grpO;
            }
            else
            {
                offGrip = null;
            }
        }
           

        ApplyStance();
    }

    /*
    public void UpdateStance(StanceHandler stance)
    {
        StanceHandler mainStance = null;
        StanceHandler offStance = null;

        if (IsMainEquipped() && IsMainDrawn())
        {
            //stance = StanceHandler.MergeStances(stance, GetMainWeapon().PrfMainHandStance);
            mainStance = GetMainWeapon().stance;
            //stance.Merge(mainStance, true);
            stance.stanceStyle = mainStance.stanceStyle;
            stance.rightGrip = mainStance.rightGrip;
            stance.leftGrip = mainStance.rightGrip;
            stance.twohandGrip = mainStance.twohandGrip;
            stance.blockStyle = mainStance.blockStyle;
        }

        if (IsOffEquipped() && IsOffDrawn())
        {
            //stance = StanceHandler.MergeStances(stance, GetOffHand().PrfOffHandStance);
            offStance = GetOffWeapon().stance;
            stance.Merge(offStance, false);
            stance.leftGrip = offStance.leftGrip;
            if (offStance.blockStyle != StanceHandler.BlockStyle.Shield)
            {
                stance.blockStyle = StanceHandler.BlockStyle.Bracing; // cross block?
            }
            else
            {
                stance.blockStyle = StanceHandler.BlockStyle.Shield;
            }
        }


    }
    */

    private void DefaultLayerWeights()
    {
        bool twohand = IsTwoHanding();
        showRight = (!twohand && stance.rightGrip != StanceHandler.GripStyle.None) ? 1f : 0f;
        showLeft = (!twohand && stance.leftGrip != StanceHandler.GripStyle.None) ? 1f : 0f;
        show2h = (twohand && stance.twohandGrip != StanceHandler.GripStyle.None) ? 1f : 0f;
        SetIKHands(IKHandMatch.None, 0f);
        RotateMainWeapon(0f);
        RotateOffWeapon(0f);
    }
    public void ApplyStance()
    {
        //this.animator.runtimeAnimatorController = stance.GetController();



        this.animator.SetFloat("Style-Left", (int)stance.leftGrip);
        this.animator.SetFloat("Style-Right", (int)stance.rightGrip);
        this.animator.SetFloat("Style-2H", (int)stance.twohandGrip);

        bool twohand = IsTwoHanding();

        float rw = animator.GetLayerWeight(animator.GetLayerIndex("Right Arm Override"));
        float lw = animator.GetLayerWeight(animator.GetLayerIndex("Left Arm Override"));
        float tw = animator.GetLayerWeight(animator.GetLayerIndex("Two Arm Override"));
        animator.SetLayerWeight(animator.GetLayerIndex("Right Arm Override"), Mathf.MoveTowards(rw,  showRight, 7.5f * Time.deltaTime));
        animator.SetLayerWeight(animator.GetLayerIndex("Left Arm Override"), Mathf.MoveTowards(lw, showLeft, 7.5f * Time.deltaTime));
        animator.SetLayerWeight(animator.GetLayerIndex("Two Arm Override"), Mathf.MoveTowards(tw, show2h, 7.5f * Time.deltaTime));
        /*
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
        */

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

    private void Interact()
    {
        Interactable interactable = GetHighlightedInteractable();
        if (interactable != null)
        {
            interactable.Interact(this);
        }
        
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
        else if (ikHand == IKHandMatch.LeftToRight)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikHandWeight);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.5f);
            animator.SetIKPosition(AvatarIKGoal.RightHand, this.transform.forward + this.transform.up * 0.5f);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, positionReference.MainHand.transform.position);

            animator.SetIKHintPosition(AvatarIKHint.LeftElbow, positionReference.MainHand.transform.position + this.transform.forward);
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0.9f);

        }
        else if (ikHand == IKHandMatch.RightToLeft)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikHandWeight);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
            animator.SetIKPosition(AvatarIKGoal.RightHand, positionReference.OffHand.transform.position);
        }
        else if (ikHand == IKHandMatch.Down)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikHandWeight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, this.transform.position + Vector3.down);

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.9f);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, positionReference.MainHand.transform.position + positionReference.MainHand.transform.forward * -0.25f);

            //animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0.5f);
            //animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, tempVal);
            //animator.SetIKHintPosition(AvatarIKHint.RightElbow, this.transform.position + Vector3.down);
            //animator.SetIKHintPosition(AvatarIKHint.LeftElbow, this.transform.position + Vector3.down * 2f);

            if (offGrip != null)
            {
                animator.SetIKPosition(AvatarIKGoal.LeftHand, offGrip.position + positionReference.MainHand.transform.up * -0.07f + positionReference.MainHand.transform.forward * -0.08f + positionReference.MainHand.transform.right * 0.05f);
            }
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
        }
        
    }

    public void SetIKHands(IKHandMatch ik, float w)
    {
        ikHand = ik;
        ikHandWeight = w;
    }
    public enum IKHandMatch
    {
        None,
        LeftToRight,
        RightToLeft,
        Down
    }

    public bool IsLockedOn()
    {
        return this.GetCombatTarget() != null;
    }
    public override void SetCombatTarget(GameObject target)
    {
        base.SetCombatTarget(target);
        GetStance();
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
