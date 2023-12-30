using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericProjectile : Projectile
{
    public float radius;
    public GameObject prefabRef;
    [ReadOnly] public Hitbox hitbox;
    public DamageKnockback damageKnockback;
    public Vector3 velocity;

    private static readonly float PROJECTILE_DURATION = 30f;
    // Start is called before the first frame update
    protected virtual void FixedUpdate()
    {
        this.transform.position += velocity * Time.fixedDeltaTime;
    }

    public static new GenericProjectile Launch(GameObject arrowPrefab, Vector3 position, Quaternion angle, Vector3 force, Transform source, DamageKnockback damageKnockback)
    {

        GameObject obj = GameObject.Instantiate(arrowPrefab, position, angle);
        GenericProjectile projectile = obj.GetComponent<GenericProjectile>();


        projectile.hitbox = Hitbox.CreateHitbox(obj.transform.position, projectile.radius, obj.transform, damageKnockback, source.gameObject);

        projectile.prefabRef = arrowPrefab;

        projectile.Launch(position, angle, force, source, damageKnockback);

        return projectile;
    }

    public override void Launch(Vector3 position, Quaternion angle, Vector3 force, Transform source, DamageKnockback damageKnockback)
    {
        this.hitbox.SetActive(false);
        this.transform.position = position;
        this.transform.rotation = angle;

        this.damageKnockback = new(damageKnockback);
        this.origin = source.gameObject;

        this.transform.position = position;

        this.gameObject.SetActive(true);

        this.hitbox.SetDamage(damageKnockback);

        this.velocity = force;

        this.hitbox.SetActive(true);

        Destroy(this.gameObject, PROJECTILE_DURATION);
    }

    public override void SetHitbox(bool active)
    {
        if (hitbox != null)
        {
            hitbox.SetActive(active);
        }
    }
}
