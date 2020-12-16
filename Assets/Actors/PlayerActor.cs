using UnityEngine;
using System.Collections;
using CustomUtilities;
using System;

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
    public override void ActorStart()
    {
        base.ActorStart();

        cam = Camera.main;
        //cameraController =  cam.GetComponent<CursorCam>();

        camRotation = transform.forward;

        GetStance();

        OnSheathe.AddListener(Spare);

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
        bool isGrounded = cc.isGrounded;
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

        if (isGrounded && moveDirection != Vector3.zero)
        {
            airDirection = moveDirection;
        }

        if (isAiming)
        {
            alignMode = AlignMode.Camera;
        }
        else if (!cc.isGrounded && airTime > 0.25f)
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
        else if (!IsDodging() && !IsJumping())
        {
            animator.SetFloat("ForwardVelocity", 0f);
            animator.SetFloat("StrafingVelocity", 0f);
        }

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
        if (CanMove() && humanoidState == HumanoidState.Actionable)
        {
            if (IsAerial())
            {
                finalMov += airDirection;
            }
            else
            {
                finalMov += moveDirection;
            }
        }
        finalMov += moveAdditional;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.5f, LayerMask.GetMask("Terrain")))
        {
            Vector3 temp = Vector3.Cross(hit.normal, finalMov);
            cc.Move(Vector3.Cross(temp, hit.normal) * Time.fixedDeltaTime);
            //transform.Translate(0, (hit.point - transform.position).y * Time.fixedDeltaTime * GetCurrentSpeed() * 2f, 0);
        }
        else if (!IsJumping())
        {
            cc.Move((finalMov + gravity) * Time.fixedDeltaTime);
        }
        else
        {
            cc.Move(finalMov * Time.fixedDeltaTime);
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
            bool mainStrongUp = InputHandler.main.atk3Up;
            bool mainStrongDown = InputHandler.main.atk3Down;
            bool mainStrongHeld = InputHandler.main.atk3Held;
            float mainStrongHeldTime = InputHandler.main.atk3HeldTime;
            bool mainStrongLong = InputHandler.main.atk3LongPress;
            bool mainStrongUnderThreshold = mainStrongHeldTime < InputHandler.main.LONG_PRESS_THRESHOLD;

            bool mainQuickUp = InputHandler.main.atk1Up;
            bool mainQuickDown = InputHandler.main.atk1Down;
            bool mainQuickHeld = InputHandler.main.atk1Held;
            float mainQuickHeldTime = InputHandler.main.atk1HeldTime;
            bool mainQuickLong = InputHandler.main.atk1LongPress;
            bool mainQuickUnderThreshold = mainQuickHeldTime < InputHandler.main.LONG_PRESS_THRESHOLD;

            bool offStrongUp = InputHandler.main.atk4Up;
            bool offStrongDown = InputHandler.main.atk4Down;
            bool offStrongHeld = InputHandler.main.atk4Held;
            float offStrongHeldTime = InputHandler.main.atk4HeldTime;
            bool offStrongLong = InputHandler.main.atk4LongPress;
            bool offStrongUnderThreshold = offStrongHeldTime < InputHandler.main.LONG_PRESS_THRESHOLD;

            bool offQuickUp = InputHandler.main.atk2Up;
            bool offQuickDown = InputHandler.main.atk2Down;
            bool offQuickHeld = InputHandler.main.atk2Held;
            float offQuickHeldTime = InputHandler.main.atk2HeldTime;
            bool offQuickLong = InputHandler.main.atk2LongPress;
            bool offQuickUnderThreshold = offQuickHeldTime < InputHandler.main.LONG_PRESS_THRESHOLD;

            bool bothStrongDown =  (mainStrongHeld && offStrongDown) || (mainStrongDown && offStrongHeld);
            bool bothStrongHeld = (mainStrongHeld && offStrongHeld);

            bool bothQuickDown = (mainQuickHeld && offQuickDown) || (mainQuickDown && offQuickHeld);
            bool bothQuickHeld = (mainQuickHeld && offQuickHeld);

            bool inputtedAttack = false;

            /*
            if (bothStrongDown && stance.moveset.GetAttackFromInput(Moveset.AttackStyle.BothStrong) != null)
            {
                currentAttackInput = Moveset.AttackStyle.BothStrong;
                inputtedAttack = true;
            }
            else if (bothQuickDown && stance.moveset.GetAttackFromInput(Moveset.AttackStyle.BothQuick) != null)
            {
                currentAttackInput = Moveset.AttackStyle.BothQuick;
                inputtedAttack = true;
            }
            */
            //if ((mainQuickUp || (mainQuickLong && !offQuickHeld)) && stance.moveset.GetAttackFromInput(Moveset.AttackStyle.MainQuick) != null)
            if ((mainQuickDown) && stance.moveset.GetAttackFromInput(Moveset.AttackStyle.MainQuick) != null)
            {
                currentAttackInput = Moveset.AttackStyle.MainQuick;
                inputtedAttack = true;
            }
            else if ((mainStrongDown) && stance.moveset.GetAttackFromInput(Moveset.AttackStyle.MainStrong) != null)
            {
                currentAttackInput = Moveset.AttackStyle.MainStrong;
                inputtedAttack = true;
            }
            //else if ((offQuickUp && offQuickUnderThreshold) || (offQuickLong && !mainQuickHeld))
            else if ((offQuickDown))
            {
                if (!inventory.IsTwoHanding() && stance.moveset.GetAttackFromInput(Moveset.AttackStyle.OffQuick) != null)
                {
                    currentAttackInput = Moveset.AttackStyle.OffQuick;
                    inputtedAttack = true;
                }
                else if (inventory.IsTwoHanding() && stance.moveset.GetAttackFromInput(Moveset.AttackStyle.TwoQuick) != null)
                {
                    currentAttackInput = Moveset.AttackStyle.TwoQuick;
                    inputtedAttack = true;
                }
            }
            else if ((offStrongDown))
            {
                if (!inventory.IsTwoHanding() && stance.moveset.GetAttackFromInput(Moveset.AttackStyle.OffStrong) != null)
                {
                    currentAttackInput = Moveset.AttackStyle.OffStrong;
                    inputtedAttack = true;
                }
                else if (inventory.IsTwoHanding() && stance.moveset.GetAttackFromInput(Moveset.AttackStyle.TwoStrong) != null)
                {
                    currentAttackInput = Moveset.AttackStyle.TwoStrong;
                    inputtedAttack = true;
                }
            }

            bool up = false;
            bool down = false;
            bool held = false;
            float heldTime = 0f;


            if (currentAttackInput == Moveset.AttackStyle.BothStrong)
            {
                down = bothStrongDown;
                held = bothStrongHeld;
                heldTime = Mathf.Min(mainStrongHeldTime, offStrongHeldTime);
            }
            else if (currentAttackInput == Moveset.AttackStyle.BothQuick)
            {
                down = bothQuickDown;
                held = bothQuickHeld;
                heldTime = Mathf.Min(mainQuickHeldTime, offQuickHeldTime);
            }
            else if (currentAttackInput == Moveset.AttackStyle.MainStrong)
            {
                up = mainStrongUp;
                down = mainStrongDown;
                held = mainStrongHeld;
                heldTime = mainStrongHeldTime;
            }
            else if (currentAttackInput == Moveset.AttackStyle.MainQuick)
            {
                up = mainQuickUp;
                down = mainQuickDown;
                held = mainQuickHeld;
                heldTime = mainQuickHeldTime;
            }
            else if (currentAttackInput == Moveset.AttackStyle.OffQuick || currentAttackInput == Moveset.AttackStyle.TwoQuick)
            {
                up = offQuickUp;
                down = offQuickDown;
                held = offQuickHeld;
                heldTime = offQuickHeldTime;
            }
            else if (currentAttackInput == Moveset.AttackStyle.OffStrong || currentAttackInput == Moveset.AttackStyle.TwoStrong)
            {
                up = offStrongUp;
                down = offStrongDown;
                held = offStrongHeld;
                heldTime = offStrongHeldTime;
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
            InputAttack atk = stance.moveset.GetAttackFromInput(currentAttackInput);
            if (atk != null)
            {
                int id = atk.GetAttackID();
                animator.SetInteger("Input-AttackID", id);
                animator.SetBool("Input-AttackBlockOkay", atk.IsBlockOkay());
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

        if (Input.GetButtonDown("Interact")) {
            if (false) // TODO: check for interactable items here
            {
                if (inventory.IsWeaponDrawn())
                {
                    animator.SetTrigger("Sheath-Main");
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
        
        if (interactDown)
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
        
    }
    private void OnDrawGizmosSelected()
    {
        try
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(positionReference.Head.transform.position, positionReference.Head.transform.position + GetLaunchVector(positionReference.Head.transform.position) * 100f);
        }
        catch (Exception ex)
        {
            //Debug.Log(ex.Message);
        }
    }
}
