using UnityEngine;

public class SpinnyDoomTest : MonoBehaviour
{

    public float spinSpeed = 360f;
    public float probeDistance = 5f;
    public float knockbackModifier = 0.5f;
    HitboxController hitbox;
    public Vector3 launchVelocity;
    public float launchMagnitude;
    Vector3 lastPosition;
    float clock;

    void Start()
    {
        lastPosition = transform.position + transform.forward * probeDistance;
        hitbox = GetComponentInChildren<HitboxController>();
        clock = 0f;
    }

    void LateUpdate()
    {
        Vector3 additForce = transform.forward * launchMagnitude + transform.up * launchMagnitude;
        launchVelocity = ((((transform.position + transform.forward * probeDistance) - lastPosition) / Time.deltaTime) * knockbackModifier) + additForce;
        lastPosition = transform.position + transform.forward * probeDistance;
        hitbox.damageKnockback.kbForce = launchVelocity;

        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        clock += Time.deltaTime;
        if (clock > 1f)
        {
            hitbox.GetNewID();
            clock = 0f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.forward * probeDistance, launchVelocity);
        Gizmos.DrawWireSphere(transform.position + transform.forward * probeDistance, 0.05f);
    }
}
