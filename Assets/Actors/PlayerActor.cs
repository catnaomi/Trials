using UnityEngine;
using System.Collections;
using CustomUtilities;
using System;
using System.Collections.Generic;

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
    public Moveset.AttackStyle currentAttackInput;

    bool startDodge;

    Vector3 stickDirection;
    //Vector3 moveDirection;

    Vector3 airDirection;
    Vector3 lastPos;

    List<Interactable> interactables;
    public Interactable highlightedInteractable;

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
        player = this;
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
        GetInput();

        float primaryVertical = Input.GetAxis("Vertical");
        float primaryHorizontal = Input.GetAxis("Horizontal");
        float secondaryVertical = Input.GetAxisRaw("SecondaryVertical");
        float secondaryHorizontal = Input.GetAxisRaw("SecondaryHorizontal");

        //bool buttonPressed = HandleInput();

        AlignMode alignMode = AlignMode.None;

        bool canMove = CanMove();
        bool weaponDrawn = inventory.IsWeaponDrawn();
        bool isGrounded = GetGrounded();
        bool isAiming = IsAiming();
        bool lockedOn = this.GetCombatTarget() != null;//cameraController.lockedOn;

        animator.SetBool("Cam-Locked", lockedOn);
        animator.SetBool("Cam-Aiming", isAiming && Input.GetButton("Aim"));

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
        else if (IsSprinting() || IsDodging() || startDodge)
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
            if (alignMode == AlignMode.Camera || alignMode == AlignMode.None || alignMode == AlignMode.Target)
            {
                animator.SetFloat("ForwardVelocity", Mathf.Lerp(animator.GetFloat("ForwardVelocity"), forwardVel, 0.2f));
                animator.SetFloat("StrafingVelocity", Mathf.Lerp(animator.GetFloat("StrafingVelocity"), strafeVel, 0.2f));
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

        AxisUtilities.AxisDirection stickAxis = GetStickAxis();

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

    private void GetInput()
    {

        //animator.SetBool("MirrorArmedStance", true);
        //animator.SetBool("MirrorBlockingStance", false);
        //animator.SetInteger("LightAttackStyle", 1);
        animator.SetBool("Blocking", animator.GetBool("Armed") && InputHandler.main.blockHeld);
        GetInventoryInput();
        if (!animator.GetBool("Armed"))
        {
            if (CanMove() && (InputHandler.main.atk1Down || InputHandler.main.atk2Down))
            {
                //animator.SetTrigger("Unsheath-Main");
            }
        }
        else if (attributes.HasAttributeRemaining(attributes.stamina))
        {
            

            bool atk1UnderThreshold = InputHandler.main.atk1HeldTime < InputHandler.main.LONG_PRESS_THRESHOLD;
            bool atk2UnderThreshold = InputHandler.main.atk2HeldTime < InputHandler.main.LONG_PRESS_THRESHOLD;
            bool atk3UnderThreshold = InputHandler.main.atk3HeldTime < InputHandler.main.LONG_PRESS_THRESHOLD;
            bool atk4UnderThreshold = InputHandler.main.atk4HeldTime < InputHandler.main.LONG_PRESS_THRESHOLD;


            bool skill1Up = InputHandler.main.atk3Up;
            bool skill1Down = InputHandler.main.atk3Down;
            bool skill1Held = InputHandler.main.atk3Held;
            float skill1HeldTime = InputHandler.main.atk3HeldTime;
            bool skill1Long = InputHandler.main.atk3LongPress;

            bool skill2Up = InputHandler.main.atk4Up;
            bool skill2Down = InputHandler.main.atk4Down;
            bool skill2Held = InputHandler.main.atk4Held;
            float skill2HeldTime = InputHandler.main.atk4HeldTime;
            bool skill2Long = InputHandler.main.atk4LongPress;

            InputAttack atk = null;
            bool inputtedAttack = false;
            bool up = false;
            bool down = false;
            bool held = false;
            float heldTime = 0f;

            if (InputHandler.main.atk1Down)
            {
                if (IsSprinting())
                {
                    atk = stance.moveset.slashDash;
                }
                else if (!GetGrounded())
                {
                    atk = stance.moveset.slashPlunge;
                }
                else if (IsSneaking())
                {
                    atk = stance.moveset.slashSneak;
                }
                else if (inventory.IsTwoHanding())
                {
                    atk = stance.moveset.slash2H;
                }
                else
                {
                    atk = stance.moveset.slash1H;
                }
                currentAttackInput = Moveset.AttackStyle.Slash;
                inputtedAttack = true;
            }
            else if (InputHandler.main.atk2Down)
            {
                if (IsSprinting())
                {
                    atk = stance.moveset.thrustDash;
                }
                else if (!GetGrounded())
                {
                    atk = stance.moveset.thrustPlunge;
                }
                else if (IsSneaking())
                {
                    atk = stance.moveset.thrustSneak;
                }
                else if (inventory.IsTwoHanding())
                {
                    atk = stance.moveset.thrust2H;
                }
                else
                {
                    atk = stance.moveset.thrust1H;
                }
                currentAttackInput = Moveset.AttackStyle.Slash;
                inputtedAttack = true;
            }
            else if (InputHandler.main.atk3Down)
            {
                atk = stance.moveset.skill1;
                currentAttackInput = Moveset.AttackStyle.Skill1;
                inputtedAttack = true;
            }
            else if (InputHandler.main.atk4Down)
            {
                atk = stance.moveset.skill2;
                currentAttackInput = Moveset.AttackStyle.Skill2;
                inputtedAttack = true;
            }


            if (currentAttackInput == Moveset.AttackStyle.Slash)
            {
                up = InputHandler.main.atk1Up;
                down = InputHandler.main.atk1Down;
                held = InputHandler.main.atk1Held;
                heldTime = InputHandler.main.atk1HeldTime;
            }
            else if (currentAttackInput == Moveset.AttackStyle.Skill1)
            {
                up = InputHandler.main.atk3Up;
                down = InputHandler.main.atk3Down;
                held = InputHandler.main.atk3Held;
                heldTime = InputHandler.main.atk3HeldTime;
            }
            else if (currentAttackInput == Moveset.AttackStyle.Skill2)
            {
                up = InputHandler.main.atk4Up;
                down = InputHandler.main.atk4Down;
                held = InputHandler.main.atk4Held;
                heldTime = InputHandler.main.atk4HeldTime;
            }

            if (up)
            {
                animator.SetTrigger("Input-AttackUp");
            }
            if (down)
            {
                animator.SetTrigger("Input-AttackDown");
            }
            animator.SetBool("Input-AttackHeld", held);
            animator.SetFloat("Input-AttackHeldTime", heldTime);

            if (inputtedAttack)
            {
                animator.SetTrigger("Input-Attack");
            }
            
            if (atk != null)
            {
                int id = atk.GetAttackID();
                animator.SetInteger("Input-AttackID", id);
                animator.SetBool("Input-AttackBlockOkay", atk.IsBlockOkay());
                animator.SetBool("Input-AttackSprintOkay", atk.IsSprintOkay());
                animator.SetBool("Input-AttackFallingOkay", atk.IsFallingOkay());
            }
        }
        else if (false)
        {
            if (InputHandler.main.atk1Up)
            {
                animator.SetTrigger("Input-SlashUp");
                animator.SetTrigger("Input-AttackUp");
                animator.SetTrigger("Input-Attack");
                RegisterPlayerInput();
                //animator.ResetTrigger("Input-SlashDown");
            }
            if (InputHandler.main.atk1Down)
            {
                animator.SetTrigger("Input-SlashDown");
                animator.SetTrigger("Input-AttackDown");
                animator.SetTrigger("Input-Attack");
                RegisterPlayerInput();
                //animator.ResetTrigger("Input-SlashUp");
            }
            animator.SetFloat("Input-SlashHeldTime", InputHandler.main.atk1HeldTime);
            animator.SetFloat("Input-AttackHeldTime", InputHandler.main.atk1HeldTime);
            animator.SetBool("Input-SlashHeld", InputHandler.main.atk1Held);
            animator.SetBool("Input-AttackHeld", InputHandler.main.atk1Held);

            if (InputHandler.main.atk2Up)
            {
                animator.SetTrigger("Input-ThrustUp");
                RegisterPlayerInput();
                //animator.ResetTrigger("Input-ThrustDown");
            }
            if (InputHandler.main.atk2Down)
            {
                animator.SetTrigger("Input-ThrustDown");
                RegisterPlayerInput();
                //animator.ResetTrigger("Input-ThrustUp");
            }
            animator.SetFloat("Input-ThrustHeldTime", InputHandler.main.atk2HeldTime);
            animator.SetBool("Input-ThrustHeld", InputHandler.main.atk2Held);
            if (InputHandler.main.heavyDown)
            {
                animator.SetTrigger("Input-HeavyDown");
                RegisterPlayerInput();
            }
            if (InputHandler.main.heavyUp)
            {
                animator.SetTrigger("Input-HeavyUp");
                RegisterPlayerInput();
            }
            animator.SetFloat("Input-HeavyHeldTime", InputHandler.main.heavyHeldTime);
            animator.SetBool("Input-HeavyHeld", InputHandler.main.heavyHeld);

            if (InputHandler.main.atk1Held && InputHandler.main.atk2Held)
            {
                RegisterPlayerInput();
            }
        }
        else
        {
            animator.SetBool("Input-SlashHeld", false);
            animator.SetBool("Input-ThrustHeld", false);
            animator.SetBool("Input-HeavyHeld", false);

        }
            

       
        animator.SetInteger("AxisDirection", (int)InputHandler.main.PrimaryQuadrant);

        if (attributes.HasAttributeRemaining(attributes.stamina))
        {
            bool jump = Input.GetButtonDown("Jump");
            if (jump)
            {
                animator.SetBool("Input-Jump", true);
            }
            animator.SetBool("Input-JumpHeld", Input.GetButton("Jump"));
            if (InputHandler.main.jumpDown)
            {
                animator.SetTrigger("Input-DodgeDown");
                RegisterPlayerInput();
            }
            if (InputHandler.main.jumpUp)
            {
                animator.SetTrigger("Input-DodgeUp");
                RegisterPlayerInput();
                startDodge = true;
            }
        }
        animator.SetBool("Input-DodgeHeld", InputHandler.main.jumpHeld);
        animator.SetBool("Sprinting", InputHandler.main.sprintHeld && attributes.HasAttributeRemaining(attributes.stamina) && stickDirection != Vector3.zero);

        if (Input.GetButtonDown("Sneak"))
        {
            shouldSneak = !shouldSneak;
        }
        animator.SetBool("Sneaking", shouldSneak);

        if (IsHanging())
        {
            if (Input.GetButtonDown("Jump"))
            {
                animator.SetTrigger("LedgeClimb");
            }
            else if (Input.GetButtonDown("Dodge"))
            {
                if (currentClimb is Ledge)
                {
                    UnsnapFromLedge();
                }
                else if (currentClimb is Ladder)
                {
                    UnsnapFromLadder();
                }
            }
        }
        

        if (Input.GetButtonDown("Interact")) {
            if (highlightedInteractable != null) // TODO: check for interactable items here
            {
                highlightedInteractable.Interact(this);
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

    private void GetInventoryInput()
    {
        
        AxisUtilities.AxisDirection inputSlot = InputHandler.main.equipSlot;
        bool down = InputHandler.main.equipDown;
        bool held = InputHandler.main.equipHeld;
        float time = InputHandler.main.equipHeldTime;

        bool interactDown = Input.GetButtonDown("Interact");

        
        
        if (down)
        {
            EquippableWeapon weapon;

            switch (inputSlot)
            {
                case AxisUtilities.AxisDirection.Down:
                    weapon = inventory.Slot0Weapon;
                    break;
                case AxisUtilities.AxisDirection.Up:
                    weapon = inventory.Slot1Weapon;
                    break;
                case AxisUtilities.AxisDirection.Left:
                    weapon = inventory.Slot2Weapon;
                    break;
                case AxisUtilities.AxisDirection.Right:
                    weapon = inventory.Slot3Weapon;
                    break;
                default:
                    weapon = null;
                    break;
            }

            if (weapon != null)
            {
                if (inventory.GetItemHand(weapon) >= 1) // equipped in main
                {
                    /*
                    if (inventory.IsMainDrawn())
                    {
                        if (inventory.IsTwoHanding() && weapon.OneHanded)
                        {
                            inventory.UpdateTwoHand(false);
                        }
                        else if (!inventory.IsTwoHanding() && weapon.TwoHanded)
                        {
                            inventory.UpdateTwoHand(true);
                        }
                    }
                    */
                    // toggle two hand
                }
                else if (inventory.GetItemHand(weapon) <= -1 && !inventory.IsOffDrawn())
                {
                    TriggerSheath(true, inventory.OffWeapon.OffHandEquipSlot, false);
                }
                else if (inventory.GetItemHand(weapon) <= -1 && weapon.EquippableMain)
                {
                    inventory.EquipMainWeapon(weapon);
                }
                else if (false)//weapon.EquippableOff && weapon.EquippableMain && inventory.IsTwoHanding()) // current weapon is two handed and thus can't have off hand
                {
                    inventory.EquipMainWeapon(weapon);
                }
                else if (weapon.EquippableOff)
                {
                    if (!inventory.IsMainEquipped() && weapon.EquippableMain)
                    {
                        inventory.EquipMainWeapon(weapon);
                    }
                    else if (true)//inventory.GetMainWeapon().OneHanded)
                    {
                        inventory.EquipOffHandWeapon(weapon);
                    }
                }
                else if (weapon.EquippableMain)
                {
                    inventory.EquipMainWeapon(weapon);
                }
            }

            /*

            if (weapon != null)
            {
                bool equippedInMain = (inventory.GetItemHand(weapon) >= 1);
                bool equippedInOff = (inventory.GetItemHand(weapon) <= -1);
                bool mainEmpty = !inventory.IsMainEquipped();
                bool offEmpty = !inventory.IsOffEquipped();
                bool only2h = weapon.TwoHanded && !weapon.OneHanded;

                if (!equippedInMain && !equippedInOff)
                {
                    if (mainEmpty && weapon.EquippableMain)
                    {
                        inventory.EquipMainWeapon(weapon);
                    }
                    else if (weapon.EquippableOff && inventory.MainWeapon.OneHanded)
                    {
                        inventory.UpdateTwoHand(false);
                        inventory.EquipOffHandWeapon(weapon);
                    }
                    else if (weapon.EquippableMain)
                    {
                        inventory.EquipMainWeapon(weapon);
                    }
                }
                else if (equippedInOff)
                {
                    if (!inventory.IsOffDrawn())
                    {
                        TriggerSheath(true, inventory.OffWeapon.OffHandEquipSlot, false);
                    }
                    else if (weapon.EquippableMain) {
                        if (inventory.MainWeapon.EquippableOff)
                        {
                            inventory.EquipOffHandWeapon(inventory.MainWeapon);
                        }
                        inventory.EquipMainWeapon(weapon);
                    }
                }
                else if (equippedInMain)
                {
                    if (!inventory.IsMainDrawn())
                    {
                        TriggerSheath(true, inventory.MainWeapon.MainHandEquipSlot, true);
                    }
                    else if (inventory.MainWeapon.TwoHanded && inventory.IsMainDrawn() && !inventory.IsTwoHanding())
                    {
                        //inventory.UnequipOffHandWeapon();
                        // TODO: stances, equip as two handed
                        inventory.UpdateTwoHand(true);
                        Debug.Log("Try Equip Two handed!");
                    }
                    else if (inventory.MainWeapon.OneHanded && inventory.IsMainDrawn() && inventory.IsTwoHanding())
                    {
                        // TODO: stances, equip as one handed
                        inventory.UpdateTwoHand(false);
                        Debug.Log("Try Equip one handed!");
                    }
                }
            }*/
        }
        
        if (interactDown && highlightedInteractable == null)
        {
            if (inventory.IsOffDrawn())
            {
                TriggerSheath(false, inventory.OffWeapon.OffHandEquipSlot, false);
            }
            else
            {
                TriggerSheath(false, inventory.MainWeapon.MainHandEquipSlot, true);
            }
        }
        
        if (CanMove() && (InputHandler.main.atk1Down || InputHandler.main.atk2Down) && !inventory.IsWeaponDrawn())
        {
            if (inventory.IsMainEquipped())
            {
                TriggerSheath(true, inventory.MainWeapon.MainHandEquipSlot, true);
            }
            if (false)  
            {
                //TriggerSheath(true, inventory.OffWeapon.slot, false);
            }
        }
        
    }
    /*
    private bool HandleInput()
    {
        
        bool slashDown = Input.GetButtonDown("Attack1");
        bool slashHeld = Input.GetButton("Attack1");
        bool slashUp = Input.GetButtonUp("Attack1");
        bool stabDown = Input.GetButtonDown("Attack2");
        bool stabHeld = Input.GetButton("Attack2");
        bool stabUp = Input.GetButtonUp("Attack2");
        bool offHandDown = Input.GetButtonDown("Attack3");
        bool offHandHeld = Input.GetButton("Attack3");
        bool offHandUp = Input.GetButtonUp("Attack3");

        bool heavySlash = InputHandler.main.trigger1Down;
        bool heavyStab = InputHandler.main.trigger2Down;

        bool blockDown = Input.GetButtonDown("Block");
        bool blockHeld = Input.GetButton("Block");
        bool blockUp = Input.GetButtonUp("Block");
        

        bool lightDown = Input.GetButtonDown("Attack1");
        bool heavyDown = InputHandler.main.heavyTDown;

        bool blockDown = InputHandler.main.blockTDown;
        bool blockUp = InputHandler.main.blockTUp;

        bool offHandDown = Input.GetButtonDown("Attack2");

        bool rollUp = Input.GetButtonUp("Dodge");
        bool rollDown = Input.GetButtonDown("Dodge");
        bool sprint = false;//Input.GetButton("Dodge") && !rollUp && InputHandler.main.dodgeClock > 0.5f;
        bool interact = Input.GetButtonDown("Interact");

        

        InputAction ToQueue = null;

        bool canAttack = (humanoidState == HumanoidState.Actionable);

        if (inventory.OffWeapon != null && ((OffHandWeapon) inventory.OffWeapon).HandleInput(out InputAction action))
        {
            ToQueue = action;
        }
        else if ((rollUp && !IsSprinting() && CanMove()) || (rollDown && (IsSprinting() || !CanMove())))
        {
            if (cameraController.lockedOn)
            {
                AxisUtilities.AxisDirection stickAxis = GetStickAxis();

                string dodgeString;

                switch (stickAxis)
                {
                    default:
                    case AxisUtilities.AxisDirection.Zero:
                        dodgeString = "Jump Backwards";
                        break;
                    case AxisUtilities.AxisDirection.Forward:
                        dodgeString = "Roll";
                        break;
                    case AxisUtilities.AxisDirection.Backward:
                        dodgeString = "Jump Backwards";
                        break;
                    case AxisUtilities.AxisDirection.Left:
                        dodgeString = "Jump Left";
                        break;
                    case AxisUtilities.AxisDirection.Right:
                        dodgeString = "Jump Right";
                        break;
                }
                ToQueue = ActionsLibrary.GetInputAction(dodgeString);
            }
            else
            {
                ToQueue = ActionsLibrary.GetInputAction("Roll");
            }
            shouldSecondSwing = false;
        }
        else if (sprint && !IsSprinting())
        {
            ToQueue = ActionsLibrary.GetInputAction("Player Sprint");
        }
        else if ((lightDown || heavyDown) && inventory.MainWeapon != null && !inventory.IsWeaponDrawn())
        {
            ToQueue = ActionsLibrary.GetInputAction("Draw Weapon");
        }
        else if ((lightDown || heavyDown) && TrySpareSlay(this.GetCombatTarget()) && CanMove() && inventory.MainWeapon != null && inventory.IsWeaponDrawn())
        {
            ToQueue = ActionsLibrary.GetInputAction("Slay");
        }
        else if (lightDown && inventory.MainWeapon != null && inventory.IsWeaponDrawn())
        {
            ToQueue = ActionsLibrary.GetInputAction("Player Light Attack");           
        }
        //else if (stabDown && inventory.EquippedWeapon != null && inventory.IsWeaponDrawn())
        //{
        //    ToQueue = ActionsLibrary.GetInputAction("Player Thrusts");
        //}
        //else if (heavySlash && inventory.EquippedWeapon != null && inventory.IsWeaponDrawn())
        //{
        //    ToQueue = ActionsLibrary.GetInputAction("Player Charge Slash");
        //}
        else if (heavyDown && inventory.MainWeapon != null && inventory.IsWeaponDrawn())
        {
            ToQueue = ActionsLibrary.GetInputAction("Player Heavy Attack");
        }
        //else if (blockDown && CanMove() && inventory.EquippedWeapon != null && inventory.IsWeaponDrawn())
        //{
        //    ToQueue = ActionsLibrary.GetInputAction("Parry");
        //}
        else if (interact && inventory.MainWeapon != null && inventory.IsWeaponDrawn() && CanMove())
        {
            ToQueue = ActionsLibrary.GetInputAction("Sheathe Weapon");
        }

        if (canAttack && ToQueue != null)
        {
            TakeAction(ToQueue);
            chargeTime = 0f;
        }

        return (lightDown || rollUp);
    }
    */

    /*
    private bool CanMove()
    {
        string MOVABLE_TAG = "MOVABLE";
        bool ALLOW_IN_TRANSITION = true;

        return animator.GetCurrentAnimatorStateInfo(0).IsTag(MOVABLE_TAG) &&
                (ALLOW_IN_TRANSITION || !animator.IsInTransition(0));
    }
    */


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
            this.animator.SetFloat("Style-Left", (int)stance.leftHandStance);
            this.animator.SetFloat("Style-Right", (int)stance.rightHandStance);
        }
        else if (inventory.IsMainDrawn() && inventory.IsTwoHanding())
        {
            this.animator.SetFloat("Style-Left", (int)stance.twoHandStance);
            this.animator.SetFloat("Style-Right", (int)stance.twoHandStance);
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
        

        this.animator.SetFloat("BlockStyle", (int)stance.GetBlockStyle());

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

    public override bool ShouldEndContinuousAttack()
    {
        return base.ShouldEndContinuousAttack() || InputHandler.main.atk3Up;
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

    public AxisUtilities.AxisDirection GetStickAxis()
    {
        AxisUtilities.AxisDirection stickAxis = AxisUtilities.ConvertAxis(InputHandler.main.PrimaryQuadrant, "VERTICAL", "SAGGITAL");
        if (stickAxis == AxisUtilities.AxisDirection.Zero)
        {
            //stickAxis = AxisUtilities.AxisDirection.Forward;
        }
        return stickAxis;
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
