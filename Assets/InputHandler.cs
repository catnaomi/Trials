using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomUtilities;
using System;
using UnityEngine.Events;

public class InputHandler : MonoBehaviour
{

    public float STICK_EDGE_THRESHOLD = 0.75f;
    public float FLICK_VELOCITY_THRESHOLD = 0.5f;
    public float STICK_STILLNESS_THRESHOLD = 0.1f;

    public AxisUtilities.AxisDirection PrimaryQuadrant;
    public AxisUtilities.AxisDirection SecondaryQuadrant;
    public Vector2 PrimaryInput;
    public Vector2 SecondaryInput;
    public AxisUtilities.AxisDirection PrimaryFlickDirection;
    public AxisUtilities.AxisDirection SecondaryFlickDirection;
    public UnityEvent PrimaryStickFlick;
    public UnityEvent SecondaryStickFlick;
    public float PrimaryStickSpeed;
    public float SecondaryStickSpeed;
    public bool canPrimaryFlick;
    public bool canSecondaryFlick;
    public static InputHandler main;

    public float targetClock;

    public bool trigger1Down;
    bool trigger1Lock;
    public bool trigger2Down;
    bool trigger2Lock;

    public bool trigger1Up;
    bool trigger1LockUp;
    public bool trigger2Up;
    bool trigger2LockUp;

    [Header("Inputs")]
    public bool slashDown;
    public bool slashHeld;
    public bool slashUp;
    public float slashHeldTime;
    bool slashReset;
    [Space(5)]
    public bool thrustDown;
    public bool thrustHeld;
    public bool thrustUp;
    public float thrustHeldTime;
    bool thrustReset;
    [Space(5)]
    public bool blockDown;
    public bool blockHeld;
    public bool blockUp;
    public float blockHeldTime;
    bool blockReset;
    [Space(5)]
    public bool heavyDown;
    public bool heavyHeld;
    public bool heavyUp;
    public float heavyHeldTime;
    bool heavyReset;
    [Space(5)]
    public bool jumpDown;
    public bool jumpHeld;
    public bool jumpUp;
    public float jumpHeldTime;
    bool jumpReset;
    [Space(5)]
    public bool sprintDown;
    public bool sprintHeld;
    public bool sprintUp;
    public float sprintHeldTime;
    bool sprintReset;
    [Space(5)]
    public bool sheathDown;
    [Space(5)]
    public AxisUtilities.AxisDirection equipSlot;
    public bool equipDown;
    public bool equipHeld;
    public float equipHeldTime;

    bool equipReleased;
    private void Awake()
    {
        PrimaryInput = new Vector2();
        SecondaryInput = new Vector2();
        PrimaryStickFlick = new UnityEvent();
        SecondaryStickFlick = new UnityEvent();

        //PrimaryStickFlick.AddListener(() => Debug.Log(String.Format("primary stick flicked to {0}",Enum.GetName(typeof(AxisUtilities.AxisDirection), PrimaryFlickDirection))));
        //SecondaryStickFlick.AddListener(() => Debug.Log(String.Format("secondary stick flicked to {0}", Enum.GetName(typeof(AxisUtilities.AxisDirection), SecondaryFlickDirection))));

        canPrimaryFlick = true;
        canSecondaryFlick = true;
        main = this;
    }

    private void Update()
    {
        Vector2 currentPrimaryInput = new Vector2(Mathf.Clamp(Input.GetAxisRaw("Horizontal"), -1f, 1f), Mathf.Clamp(Input.GetAxisRaw("Vertical"), -1f, 1f));
        Vector2 currentSecondaryInput = new Vector2(Mathf.Clamp(Input.GetAxisRaw("SecondaryHorizontal"), -1f, 1f), Mathf.Clamp(Input.GetAxisRaw("SecondaryVertical"), -1f, 1f));
        AxisUtilities.AxisDirection currentPrimaryQuadrant = AxisUtilities.DirectionToAxisDirection(currentPrimaryInput, "HORIZONTAL", "VERTICAL", "NONE");
        AxisUtilities.AxisDirection currentSecondaryQuadrant = AxisUtilities.DirectionToAxisDirection(currentSecondaryInput, "HORIZONTAL", "VERTICAL", "NONE");

        float currentPrimarySpeed = Vector2.Distance(currentPrimaryInput, PrimaryInput) / Time.deltaTime;
        float currentSecondarySpeed = Vector2.Distance(currentSecondaryInput, SecondaryInput) / Time.deltaTime;

        AxisUtilities.AxisDirection currentPrimaryDirection = AxisUtilities.DirectionToAxisDirection(currentPrimaryInput - PrimaryInput, "HORIZONTAL", "VERTICAL");
        AxisUtilities.AxisDirection currentSecondaryDirection = AxisUtilities.DirectionToAxisDirection(currentSecondaryInput - SecondaryInput, "HORIZONTAL", "VERTICAL");

        if (currentPrimaryDirection == currentPrimaryQuadrant && currentPrimarySpeed > STICK_STILLNESS_THRESHOLD && canPrimaryFlick)
        {
            canPrimaryFlick = false;
            PrimaryFlickDirection = currentPrimaryDirection;
            PrimaryStickFlick.Invoke();
        } else if (currentPrimarySpeed < STICK_STILLNESS_THRESHOLD)
        {
            canPrimaryFlick = true;
        }

        if(currentSecondaryDirection == currentSecondaryQuadrant && currentSecondarySpeed > STICK_STILLNESS_THRESHOLD && canSecondaryFlick)
        {
            canSecondaryFlick = false;
            SecondaryFlickDirection = currentSecondaryDirection;
            SecondaryStickFlick.Invoke();
        } else if (currentSecondarySpeed < STICK_STILLNESS_THRESHOLD)
        {
            canSecondaryFlick = true;
        }

        PrimaryInput = currentPrimaryInput;
        SecondaryInput = currentSecondaryInput;

        PrimaryQuadrant = currentPrimaryQuadrant;
        SecondaryQuadrant = currentSecondaryQuadrant;

        PrimaryStickSpeed = currentPrimarySpeed;
        SecondaryStickSpeed = currentSecondarySpeed;

        if (Input.GetButton("Target"))
        {
            targetClock += Time.deltaTime;
        }
        else
        {
            targetClock = 0;
        }

        // handle press trigger input 

        if (Input.GetAxis("Attack3") >= 0.9f && !trigger1Lock)
        {
            trigger1Down = true;
            trigger1Lock = true;
        }
        else
        {
            trigger1Down = false;
        }

        if (Input.GetAxis("Trigger1") <= 0.1f && trigger1Lock)
        {
            trigger1Lock = false;
        }

        if (Input.GetAxis("Trigger2") >= 0.9f && !trigger2Lock)
        {
            trigger2Down = true;
            trigger2Lock = true;
        }
        else
        {
            trigger2Down = false;
        }

        if (Input.GetAxis("Trigger2") <= 0.1f && trigger2Lock)
        {
            trigger2Lock = false;
        }

        // handle release trigger input

        if (Input.GetAxis("Trigger1") <= 0.1f && !trigger1LockUp)
        {
            trigger1Up = true;
            trigger1LockUp = true;
        }
        else
        {
            trigger1Up = false;
        }

        if (Input.GetAxis("Trigger1") >= 0.9f && trigger1LockUp)
        {
            trigger1LockUp = false;
        }

        if (Input.GetAxis("Trigger2") <= 0.1f && !trigger2LockUp)
        {
            trigger2Up = true;
            trigger2LockUp = true;
        }
        else
        {
            trigger2Up = false;
        }

        if (Input.GetAxis("Trigger2") >= 0.9f && trigger2LockUp)
        {
            trigger2LockUp = false;
        }

        // inputs redone
        if (slashHeld)
        {
            if (slashReset)
            {
                slashHeldTime = 0f;
                slashReset = false;
            }
            slashHeldTime += Time.deltaTime;
        }
        else
        {
            slashReset = true;
        }

        slashHeld = Input.GetButton("Attack1");       
        slashDown = Input.GetButtonDown("Attack1");
        slashUp = Input.GetButtonUp("Attack1");

        if (thrustHeld)
        {
            if (thrustReset)
            {
                thrustHeldTime = 0f;
                thrustReset = false;
            }
            thrustHeldTime += Time.deltaTime;
        }
        else
        {
            thrustReset = true;
        }

        thrustHeld = Input.GetButton("Attack2");
        thrustDown = Input.GetButtonDown("Attack2");
        thrustUp = Input.GetButtonUp("Attack2");

        if (heavyHeld)
        {
            if (heavyReset)
            {
                heavyHeldTime = 0f;
                heavyReset = false;
            }
            heavyHeldTime += Time.deltaTime;
        }
        else
        {
            heavyReset = true;
        }

        heavyHeld = Input.GetAxis("Trigger1") >= 0.9f;
        heavyDown = trigger1Down;
        heavyUp = trigger1Up;

        if (blockHeld)
        {
            if (blockReset)
            {
                blockHeldTime = 0f;
                blockReset = false;
            }
            blockHeldTime += Time.deltaTime;
        }
        else
        {
            blockReset = true;
        }

        blockHeld = Input.GetAxis("Trigger2") >= 0.9f;
        blockDown = trigger2Down;
        blockUp = trigger2Up;

        if (jumpHeld)
        {
            if (jumpReset)
            {
                jumpHeldTime = 0f;
                jumpReset = false;
            }
            jumpHeldTime += Time.deltaTime;
        }
        else
        {
            jumpReset = true;
        }

        jumpHeld = Input.GetButton("Dodge");
        jumpDown = Input.GetButtonDown("Dodge");
        jumpUp = Input.GetButtonUp("Dodge");

        if (sprintHeld)
        {
            if (sprintReset)
            {
                sprintHeldTime = 0f;
                sprintReset = false;
            }
            sprintHeldTime += Time.deltaTime;
        }
        else
        {
            sprintReset = true;
        }

        sprintHeld = Input.GetButton("Sprint");
        sprintDown = Input.GetButtonDown("Sprint");
        sprintUp = Input.GetButtonUp("Sprint");

        float sens = 0.75f;
        if (Input.GetAxisRaw("PadHorizontal") > sens)
        {
            equipSlot = AxisUtilities.AxisDirection.Right;
        }
        else if (Input.GetAxisRaw("PadHorizontal") < -sens)
        {
            equipSlot = AxisUtilities.AxisDirection.Left;
        }
        else if (Input.GetAxisRaw("PadVertical") > sens)
        {
            equipSlot = AxisUtilities.AxisDirection.Up;
        }
        else if (Input.GetAxisRaw("PadVertical") < -sens)
        {
            equipSlot = AxisUtilities.AxisDirection.Down;
        }
        else
        {
            equipSlot = AxisUtilities.AxisDirection.Zero;
        }

        equipHeld = equipSlot != AxisUtilities.AxisDirection.Zero;
        if (equipHeld)
        {
            if (equipReleased)
            {
                equipDown = true;
            }
            else
            {
                equipDown = false;
            }
            equipReleased = false;
            equipHeldTime += Time.deltaTime;
        }
        else
        {
            equipDown = false;
            equipReleased = true;
            equipHeldTime = 0f;
        }
    }

    public bool GetTargetDown()
    {
        return Input.GetButtonDown("Target");
    }

    public bool GetTargetHeld()
    {
        return Input.GetButton("Target");
    }
}
