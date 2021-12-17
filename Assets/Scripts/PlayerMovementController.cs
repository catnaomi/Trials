using Animancer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    CharacterController cc;

    public Vector2 move;
    Vector2 moveSmoothed;
    [SerializeField]
    private Vector2 look;

    public CameraState camState = CameraState.Free;
    [Header("Movement Settings")]
    public float walkSpeedMax = 5f;
    public AnimationCurve walkSpeedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public float sprintSpeed = 10f;
    public float sprintTurnSpeed = 90f;
    public float skidAngle = 160f;
    public float gravity = 9.81f;
    public float terminalVel = 70f;
    public float fallBufferTime = 0.25f;
    public float hardLandingTime = 2f;
    public float rollSpeed = 5f;
    public float jumpVel = 10f;
    public float yVel;
    public bool isGrounded;
    public Vector3 xzVel;
    public float airTime = 0f;
    float lastAirTime;
    float landTime = 0f;
    bool dashed;
    bool jump;
    public bool sprinting;
    public bool shouldDodge;
    [Header("Animancer")]
    public AnimancerComponent animancer;

    public MixerTransition2DAsset moveAnim;
    public ClipTransition dashAnim;
    public ClipTransition sprintAnim;
    public ClipTransition skidAnim;
    public ClipTransition fallAnim;
    public ClipTransition landAnim;
    public ClipTransition rollAnim;
    public ClipTransition standJumpAnim;
    public ClipTransition runJumpAnim;

    PlayerMovementController movementController;
    AnimState state;

    public float angle1;
    struct AnimState
    {
        public DirectionalMixerState move;
        public AnimancerState dash;
        public AnimancerState sprint;
        public AnimancerState skid;
        public AnimancerState fall;
        public AnimancerState roll;
        public AnimancerState jump;
    }
    // Start is called before the first frame update
    void Start()
    {
        cc = this.GetComponent<CharacterController>();
        movementController = this.GetComponent<PlayerMovementController>();
        state.move = (DirectionalMixerState)animancer.States.GetOrCreate(moveAnim);
        animancer.Play(state.move);

        this.GetComponent<PlayerInput>().actions["Sprint"].started += (context) =>
        {
            SprintStart();
        };

        this.GetComponent<PlayerInput>().actions["Sprint"].canceled += (context) =>
        {
            SprintEnd();
        };

        skidAnim.Events.OnEnd += () => { state.sprint = animancer.Play(sprintAnim); };
        landAnim.Events.OnEnd += () => { animancer.Play(state.move, 1f); };
        rollAnim.Events.OnEnd += () => { animancer.Play(state.move); };
        standJumpAnim.Events.OnEnd += () => { state.fall = animancer.Play(fallAnim); };
        runJumpAnim.Events.OnEnd += () => { state.fall = animancer.Play(fallAnim); };
    }


    // Update is called once per frame
    void Update()
    {
        isGrounded = GetGrounded();
        moveSmoothed = Vector2.MoveTowards(moveSmoothed, move, Time.deltaTime);
        Vector3 camForward = Camera.main.transform.forward;
        camForward.Scale(new Vector3(1f, 0f, 1f));
        Vector3 camRight = Camera.main.transform.right;
        camRight.Scale(new Vector3(1f, 0f, 1f));
        Vector3 stickDirection = Vector3.zero;
        Vector3 lookDirection = this.transform.forward;
        Vector3 moveDirection = Vector3.zero;
        float speed = 0f;

        if (camState == CameraState.Free)
        {

            stickDirection = camForward * move.y + camRight * move.x;
        }

        if (animancer.States.Current == state.move)
        {
            speed = walkSpeedCurve.Evaluate(move.magnitude) * walkSpeedMax;
            if (stickDirection.sqrMagnitude > 0)
            {
                lookDirection = stickDirection.normalized;
            }
            moveDirection = stickDirection;
            if (!GetGrounded() && lastAirTime > fallBufferTime)
            {
                state.fall = animancer.Play(fallAnim);
            }
            if (sprinting && move.magnitude >= 0.5f && landTime >= 1f)
            {
                if (!dashed)
                {
                    state.dash = animancer.Play(dashAnim);
                }
                else
                {
                    state.sprint = animancer.Play(sprintAnim, 1f);
                }
                
            }
            if (shouldDodge)
            {
                shouldDodge = false;
                state.roll = animancer.Play(rollAnim);
            }
            if (jump)
            {
                jump = false;
                state.jump = animancer.Play((move.magnitude < 0.5f) ? standJumpAnim : runJumpAnim);
            }

            
        }
        else if (animancer.States.Current == state.dash)
        {
            dashed = true;
            speed = sprintSpeed;
            moveDirection = this.transform.forward;
            if (shouldDodge)
            {
                state.roll = animancer.Play(rollAnim);
            }
            else if (state.dash.NormalizedTime >= 0.8f)
            {
                if (sprinting)
                {
                    state.sprint = animancer.Play(sprintAnim);
                }
                else
                {
                    animancer.Play(state.move, 0.5f);
                }
            }
            if (!GetGrounded() && lastAirTime > fallBufferTime)
            {
                state.fall = animancer.Play(fallAnim);
            }

        }
        else if (animancer.States.Current == state.sprint)
        {
            speed = sprintSpeed;
            angle1 = Vector3.Angle(lookDirection, stickDirection);
            if (shouldDodge)
            {
                state.roll = animancer.Play(rollAnim);
            }
            else if (jump)
            {
                jump = false;
                state.jump = animancer.Play(runJumpAnim);
            }
            else if (move.magnitude > 0.75f && Vector3.Angle(lookDirection, stickDirection) >= skidAngle)
            {
                state.skid = animancer.Play(skidAnim);
                lookDirection = -stickDirection;
                

            }
            else if (move.magnitude <= 0f || !sprinting)
            {
                animancer.Play(state.move, 0.5f);
                Debug.Log("sprint release");
            }
            else
            {
                lookDirection = Vector3.RotateTowards(lookDirection, stickDirection.normalized, Mathf.Deg2Rad * sprintTurnSpeed * Time.deltaTime, 1f);
                moveDirection = lookDirection;
            }
            if (!GetGrounded() && lastAirTime > fallBufferTime)
            {
                state.fall = animancer.Play(fallAnim);
            }
        }
        else if (animancer.States.Current == state.fall)
        {
            airTime += Time.deltaTime;
            if (GetGrounded() && yVel <= 0)
            {
                if (lastAirTime >= hardLandingTime)
                {
                    animancer.Play(landAnim);
                }
                else
                {
                    animancer.Play(state.move);
                }
                
            }

        }
        else if (animancer.States.Current == state.roll)
        {
            shouldDodge = false;
            speed = rollSpeed;
            moveDirection = this.transform.forward;
        }
        else if (animancer.States.Current == state.jump)
        {
            jump = false;
            //speed = sprintSpeed;
            moveDirection = this.transform.forward;
        }




        if (GetGrounded())
        {
            if (yVel <= 0)
            {
                yVel = 0f;
            }
            else
            {
                yVel -= gravity * Time.deltaTime;
            }
            airTime = 0f;
            landTime += Time.deltaTime;
            if (landTime > 1f)
            {
                landTime = 1f;
            }
        }
        else
        {
            yVel -= gravity * Time.deltaTime;
            if (yVel < -terminalVel)
            {
                yVel = -terminalVel;
            }
            airTime += Time.deltaTime;
            lastAirTime = airTime;
        }

        this.transform.rotation = Quaternion.LookRotation(lookDirection);
        Vector3 downwardsVelocity = this.transform.up * yVel;
        state.move.Parameter = movementController.GetMovementVector();
        Vector3 finalMov = (moveDirection * speed + downwardsVelocity);

        if (!GetGrounded() || yVel > 0 || animancer.States.Current == state.jump)
        {
            /*
            if (Physics.SphereCast(this.transform.position, cc.radius, Vector3.down, out RaycastHit sphereHit, 1f, LayerMask.GetMask("Terrain")))
            {
                Vector3 dir = -(this.transform.position - sphereHit.point);
                dir.Scale(new Vector3(1f, 0f, 1f));
                cc.Move(dir.normalized * 10f);
                Debug.Log("slide");
            }*/
            cc.Move((xzVel + downwardsVelocity) * Time.deltaTime);
            Debug.Log("fall!!!");
        }
        else if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.5f, LayerMask.GetMask("Terrain")) && yVel <= 0)
        {
            Vector3 temp = Vector3.Cross(hit.normal, finalMov);
            cc.Move((Vector3.Cross(temp, hit.normal) + gravity * Vector3.down) * Time.deltaTime);
        }
        else
        {
            cc.Move(finalMov * Time.deltaTime);
        }
        if (animancer.States.Current == state.move || animancer.States.Current == state.sprint || animancer.States.Current == state.dash)
        {
            xzVel = finalMov;
            xzVel.Scale(new Vector3(1f, 0f, 1f));
        }
    }

    public void OnDodge(InputValue value)
    {
        shouldDodge = true;
    }

    public void OnJump(InputValue value)
    {
        jump = true;
    }

    public void ApplyJump()
    {
        yVel = jumpVel;
    }

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        look = value.Get<Vector2>();
    }

    void SprintStart()
    {
        sprinting = true;
        dashed = false;
    }

    void SprintEnd()
    {
        sprinting = false;
    }

    public void DashEnd()
    {
        Debug.Log("dash-end");
        animancer.Play(state.sprint);
    }
    public Vector2 GetMovementVector()
    {
        if (camState == CameraState.Free)
        {
            return new Vector2(0f, move.magnitude);
        }
        return Vector2.zero;
    }

    public enum CameraState
    {
        Free,
        Lock,
        Aim
    }

    public bool GetGrounded()
    {
        // return cc.isGrounded;
        Collider c = this.GetComponent<Collider>();
        Vector3 bottom = c.bounds.center + c.bounds.extents.y * Vector3.down;
        Debug.DrawLine(bottom, bottom + Vector3.down * 0.2f, Color.red);
        return Physics.Raycast(bottom, Vector3.down, 0.2f, LayerMask.GetMask("Terrain")) || cc.isGrounded;
    }
}
