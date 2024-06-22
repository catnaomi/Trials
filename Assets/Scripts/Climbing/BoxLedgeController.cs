using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class BoxLedgeController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How far from directly up the box can be angled before it is considered a ledge.")]
    public float maximumAngleTolerance = 30f;
    public float wakeDuration = 1f;
    [ReadOnly, SerializeField] float wakeTimer;
    public float sleepDuration = 1f;
    [ReadOnly, SerializeField] float sleepTimer;
    public float kinematicDuration = 1f;
    [Header("References")]
    [SerializeField] Ledge[] ledges;
    [SerializeField] Transform ledgeParent;
    BoxCollider c;
    Rigidbody rb;
    Vector3 lastPosition;
    bool wasKinematic;
    Coroutine kinematicRoutine;
    [SerializeField, ReadOnly] bool sleep;
    Vector3[] axes;
    // Start is called before the first frame update
    void Start()
    {
        c = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        axes = new Vector3[6];
    }

    private void FixedUpdate()
    {
        bool currentlySleep;
        bool fellAsleep = false;
        if (IsAnyLedgeInUse())
        {
            currentlySleep = false;
            sleepTimer = 0f;
            wakeTimer = 0f;
            rb.isKinematic = true;
        }
        else if (sleep)
        {
            if (!IsSleeping() && wakeTimer >= wakeDuration)
            {
                currentlySleep = false;
                if (rb.isKinematic)
                {
                    rb.isKinematic = false;
                }
            }
            else
            {
                currentlySleep = true;
            }
        }
        else
        {
            if (IsSleeping() && sleepTimer >= sleepDuration)
            {
                currentlySleep = true;
                fellAsleep = true;
            }
            else
            {
                currentlySleep = false;
            }
        }
        sleep = currentlySleep;
        if (fellAsleep)
        {
            axes[0] = transform.right; // x positive
            axes[1] = -transform.right; // x negative
            axes[2] = transform.up; // y positive
            axes[3] = -transform.up; // y negative
            axes[4] = transform.forward; // z positive
            axes[5] = -transform.forward; // z negative

            Vector3 closest = Vector3.zero;
            float closestAngle = Mathf.Infinity;
            foreach (Vector3 axis in axes)
            {
                float theta = Vector3.Angle(axis, Vector3.up);
                if (theta < closestAngle)
                {
                    closest = axis;
                    closestAngle = theta;
                }
            }

            bool withinTolerance = (closestAngle <= maximumAngleTolerance);
            if (withinTolerance)
            {

                Vector3 forward = closest;
                Vector3 up = Vector3.up;
                // get first orthogonal axis for up

                foreach (Vector3 axis in axes)
                {
                    if (Vector3.Dot(axis, forward) < Mathf.Epsilon)
                    {
                        up = axis;
                        break;
                    }
                }


                Debug.DrawRay(rb.position, ledgeParent.up * 3, Color.magenta, 15f);
                DrawCircle.DrawArc(rb.position, forward * 3, ledgeParent.up * 3, Color.magenta, 15f);
                Debug.DrawRay(rb.transform.position, forward * 3, Color.blue, 15f);
                Debug.DrawRay(rb.transform.position, up * 3, Color.green, 15f);
                ledgeParent.rotation = Quaternion.LookRotation(forward, up);
            }
            foreach (Ledge ledge in ledges)
            {
                ledge.IsDisabled = !withinTolerance;
            }
        }

        foreach (Ledge ledge in ledges)
        {
            ledge.IsDisabled = !sleep;
        }

        if (rb.isKinematic && (sleepTimer > kinematicDuration || wakeTimer > kinematicDuration) && !IsAnyLedgeInUse())
        {
            rb.isKinematic = false;
        }

        if (!IsSleeping())
        {
            if (wakeTimer < wakeDuration * 10)
            {
                wakeTimer += Time.fixedDeltaTime;
            }
            sleepTimer = 0;
        }
        else
        {
            if (sleepTimer < sleepDuration * 10)
            {
                sleepTimer += Time.fixedDeltaTime;
            }
            wakeTimer = 0;
        }
    }

    void StopKinematic()
    {
        if (!IsAnyLedgeInUse())
        {
            rb.isKinematic = false;
        }
    }

    bool IsSleeping()
    {
        return rb.IsSleeping() || (rb.velocity.magnitude <= Mathf.Epsilon && rb.angularVelocity.magnitude <= Mathf.Epsilon);
    }
    bool IsAnyLedgeInUse()
    {
        if (PlayerActor.player == null) return false;
        if (!PlayerActor.player.IsClimbing()) return false;
        foreach (Ledge ledge in ledges)
        {
            if (ledge.InUse && PlayerActor.player.currentClimb == ledge) return true;
        }
        return false;
    }
}
