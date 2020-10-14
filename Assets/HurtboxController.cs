using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

public abstract class HurtboxController : MonoBehaviour
{
    public UnityEvent OnHurt;
    public int mercyId;
    private void Awake()
    {
        OnHurt = new UnityEvent();
    }

    private void OnTriggerEnter(Collider other)
    {
        HitboxController hitbox;
        if (!other.TryGetComponent<HitboxController>(out hitbox))
        {
            return;
        }

        if (hitbox.GetSource() == this.GetSource())
        {
            return;
        }

        if (hitbox.id == mercyId)
        {
            return;
        }

        mercyId = hitbox.id;
        OnHurt.Invoke();
        OnHitboxEnter(hitbox);
    }

    protected abstract void OnHitboxEnter(HitboxController hitbox);

    public abstract bool Knockback(DamageKnockback damageKnockback);

    public abstract bool Damage(DamageKnockback damageKnockback);

    public abstract void ProcessDamageKnockback(DamageKnockback damageKnockback);

    public Transform GetSource()
    {
        return this.transform.root;
    }
}
