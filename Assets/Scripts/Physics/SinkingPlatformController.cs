using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkingPlatformController : MonoBehaviour
{
    Rigidbody rigidbody;
    public Transform platform;
    [Header("Sinking Controls")]
    public float sinkDistance;
    public float sinkDuration = 1f;
    public AnimationCurve sinkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public bool riseOnLeave;
    public float riseDelay;
    public float riseDuration;
    public bool detachOnBottom;
    public float detachDelay;
    public Vector3 additDetachForce;
    public float destroyOnDetachDelay = -1f;
    
    [Header("Delay & Shaking")]
    public float sinkDelay;
    public bool shakeDuringDelay;
    public float shakeSpeed = 100f;
    public float shakeAngle = 15f;
    bool sinking;
    bool wasSinkingLastFrame;
    float sinkClock;
    bool usingTimeTravel;
    Vector3 initialPosition;
    Quaternion initialRotation;
    RigidbodyTimeTravelHandler timeTravelHandler;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = platform.GetComponent<Rigidbody>();
        timeTravelHandler = platform.GetComponent<RigidbodyTimeTravelHandler>();
        usingTimeTravel = timeTravelHandler != null;
        initialPosition = rigidbody.position;
        initialRotation = rigidbody.rotation;
        if (sinkDuration == 0)
        {
            Debug.LogWarning("Sink Duration Cannot be Zero!");
            this.enabled = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (sinking)
        {
            if (sinkClock >= sinkDuration + detachDelay)
            {
                sinkClock = sinkDuration + riseDelay + detachDelay;
            }
            else
            {
                sinkClock += GetFixedDeltaTime();
            }
        }
        else
        {
            if (sinkClock <= 0)
            {
                sinkClock = -sinkDelay;
            }
            else
            {
                if (riseOnLeave && riseDuration != 0f)
                {
                    sinkClock -= GetFixedDeltaTime() * (sinkDuration/riseDuration);
                }
                else if (riseOnLeave && riseDuration == 0f)
                {
                    sinkClock = -sinkDelay;
                }
                
            }
        }

        float t = Mathf.Clamp01(sinkClock / sinkDuration);
        if (sinking && sinkClock < 0f && shakeDuringDelay)
        {
            Vector3 shakeVector = Camera.main.transform.forward;
            shakeVector.y = 0f;
            rigidbody.MoveRotation(initialRotation * Quaternion.AngleAxis(Mathf.Sin(GetFixedTime() * shakeSpeed) * shakeAngle,shakeVector));
            //rigidbody.MovePosition(initialPosition + shakeVector.normalized * Mathf.Sin(GetFixedTime() * shakeSpeed) * shakeDistance);
        }
        else
        {
            rigidbody.MoveRotation(initialRotation);
            rigidbody.MovePosition(initialPosition + sinkCurve.Evaluate(t) * sinkDistance * Vector3.down);
        }
            
        if (sinking && t >= 1 && detachOnBottom && sinkClock >= sinkDuration + riseDelay)
        {
            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;
            this.gameObject.SetActive(false);
            //rigidbody.Sleep();
            rigidbody.AddForceAtPosition(additDetachForce, PlayerActor.player.transform.position, ForceMode.Impulse);
            if (destroyOnDetachDelay > 0f)
            {
                GameObject.Destroy(platform.gameObject, destroyOnDetachDelay);
            }
        }

        wasSinkingLastFrame = sinking;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.gameObject == PlayerActor.player.gameObject)
        {
            sinking = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.root.gameObject == PlayerActor.player.gameObject)
        {
            sinking = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = sinking ? Color.red : Color.green;
        if (Application.isPlaying)
        {
            Gizmos.DrawLine(initialPosition, initialPosition + Vector3.down * sinkDistance);
        }
        else
        {
            Gizmos.DrawLine(platform.transform.position, platform.transform.position + Vector3.down * sinkDistance);
        }
    }

    float GetFixedDeltaTime()
    {
        return !usingTimeTravel ? Time.fixedDeltaTime : TimeTravelController.GetTimeAffectedFixedDeltaTime();
    }

    float GetFixedTime()
    {
        return !usingTimeTravel ? Time.fixedTime : timeTravelHandler.GetFixedTime();
    }
}
