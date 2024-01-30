using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvenCombatantActor : CombatantActor
{
    [Header("Aven Settings")]
    public bool isFlying;
    public float riseTime = 1f;
    [Space(10)]
    public ClipTransition quickRiseAnim;
    public float quickRiseSpeed = 1f;
    [Space(10)]
    public BoxCollider flyArea;
    public InputAttack swoopAttack;
    public ClipTransition swoopEndAnim;
    public float swoopSpeed = 10f;
    public float swoopYSpeed = 1f;
    public AnimationCurve swoopYVel = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    public AnimationCurve swoopXZVel = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    public float swoopDuration = 5f;
    public float swoopTurnSpeed = 90f;
    bool swoopDescending;
    float swoopClock;

    AvenStates avenState;
    struct AvenStates
    {
        public AnimancerState rising;
        public AnimancerState swoop;
    }
    public override void ActorPostUpdate()
    {
        
        base.ActorPostUpdate();

        if (CombatTarget == null)
        {
            if (DetermineCombatTarget(out GameObject target))
            {
                CombatTarget = target;

                //StartNavigationToTarget(target);

                if (target.TryGetComponent<Actor>(out Actor actor))
                {
                    //actor.OnAttack.AddListener(BeingAttacked);
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

        if (animancer.States.Current == avenState.swoop)
        {
            ProcessSwoop();
        }
    }

    public void StartSwoop()
    {
        avenState.swoop = animancer.Play(quickRiseAnim);
        swoopDescending = false;
        isFlying = true;
        swoopClock = 0f;
        xzVel = Vector3.zero;
        yVel = 0f;
        avenState.swoop.Events.OnEnd = () =>
        {
            swoopDescending = true;
            avenState.swoop = swoopAttack.ProcessHumanoidAction(this, () =>
            {
                swoopDescending = false;
                isFlying = false;
                MoveOnEnd();
            });
        };
    }

    void ProcessSwoop()
    {
        if (swoopDescending)
        {
            swoopClock += Time.deltaTime;
            float t = Mathf.Clamp01(swoopClock / swoopDuration);
            Vector3 dir = (GetCombatTarget().transform.position - this.transform.position);
            dir.y = 0f;
            dir.Normalize();

            xzVel = Vector3.zero;

            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(dir), swoopTurnSpeed * Time.deltaTime);
            if (swoopClock >= swoopDuration)
            {
                swoopDescending = false;
                isFlying = false;
            }
        }
        else
        {
            xzVel = Vector3.zero;
        }
    }

    
    private void OnAnimatorMove()
    {
        if (animancer == null || animancer.Animator == null || nav == null) return;
        Vector3 position = animancer.Animator.rootPosition + yVel * Vector3.up;
        if (!ignoreRoot) transform.rotation = animancer.Animator.rootRotation;
        if (!isFlying)
        {
            position.y = nav.nextPosition.y;
        }
        else
        {
            if ((Mathf.Abs(position.y - nav.nextPosition.y) < 1f && Mathf.Abs(animancer.Animator.velocity.y) < 0.1f) || (position.y < nav.nextPosition.y && (nav.nextPosition.y - position.y < 2f)))
            {
                position.y = nav.nextPosition.y;
            }
        }
        Vector3 dir = position - this.transform.position;
        if (!ignoreRoot && ((animancer.States.Current != navstate.idle && !IsFalling()) || !Physics.SphereCast(this.transform.position + (Vector3.up * positionReference.eyeHeight), 0.25f, dir, out RaycastHit hit, dir.magnitude, MaskReference.Terrain)))
        {
            //cc.enabled = false;
            cc.Move(position-transform.position);
            //cc.enabled = true;
        }
        //animatorVelocity = animancer.Animator.velocity;

    }
    
    private void FixedUpdate()
    {
        Vector3 velocity = Vector3.zero;
        if (isFlying)
        {
            velocity += xzVel;
        }
        else if (GetGrounded(out RaycastHit hitCheck1))
        {
            if (yVel <= 0)
            {
                yVel = 0f;
            }
            else
            {
                yVel += Physics.gravity.y * Time.fixedDeltaTime;
            }
            airTime = 0f;
            landTime += Time.fixedDeltaTime;
            if (landTime > 1f)
            {
                landTime = 1f;
            }
            Vector3 groundY = new Vector3(this.transform.position.x, hitCheck1.point.y, this.transform.position.z);
            this.transform.position = Vector3.MoveTowards(this.transform.position, groundY, 0.2f * Time.fixedDeltaTime);
        }
        else
        {
            yVel += Physics.gravity.y * Time.fixedDeltaTime;
            if (yVel < -70f)
            {
                yVel = -70;
            }
            airTime += Time.fixedDeltaTime;
            lastAirTime = airTime;
            if (IsFalling()) velocity += xzVel;
        }
        velocity += yVel * Vector3.up * Time.fixedDeltaTime;
        this.GetComponent<CharacterController>().Move((velocity));

        // keep within bounds
        if (flyArea != null)
        {
            if (!flyArea.bounds.Contains(this.transform.position))
            {
                cc.Move(flyArea.ClosestPoint(this.transform.position)- this.transform.position);
            }
        }
    }
}
