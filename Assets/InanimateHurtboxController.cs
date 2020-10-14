using UnityEngine;
using System.Collections;
using System;

public class InanimateHurtboxController : HurtboxController
{
    Rigidbody body;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
    }
    protected override void OnHitboxEnter(HitboxController hitbox)
    {

        Knockback(hitbox.damageKnockback);

    }

    public override bool Damage(DamageKnockback damageKnockback)
    {
        return false;
        // do nothing
    }

    public override bool Knockback(DamageKnockback knockback)
    {
        Vector3 force = knockback.kbForce;
        foreach (Rigidbody rigidbody in GetComponentsInChildren<Rigidbody>())
        {
            rigidbody.velocity = Vector3.zero;
        }
        if (knockback.hitboxSource != null && knockback.hitboxSource.TryGetComponent<Collider>(out Collider collider))
        {
            body.AddForceAtPosition(force, body.ClosestPointOnBounds(collider.bounds.center), ForceMode.Impulse);
        }
        else
        {
            body.AddForce(force, ForceMode.Impulse);
        }

        return true;
    }

    public override void ProcessDamageKnockback(DamageKnockback damageKnockback)
    {
        throw new NotImplementedException();
    }
}
