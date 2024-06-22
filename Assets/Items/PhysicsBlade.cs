using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PhysicsBlade", menuName = "ScriptableObjects/Weapons/Debug/Create Physics Blade", order = 1)]
public class PhysicsBlade : BladeWeapon
{
    public float force = 1000;
    HashSet<Rigidbody> victims;
    public override void EquipWeapon(Actor actor)
    {
        base.EquipWeapon(actor);
        victims = new();
        hitboxes.events.OnHitActor.AddListener(HitActor);
        hitboxes.events.OnHitTerrain.AddListener(HitCollider);
    }

    void HitCollider(Hitbox hitbox, Collider collider) {

        if (collider.transform.root.TryGetComponent(out Rigidbody rb) && !rb.isKinematic)
        {
            victims.Add(rb);
        }
    }

    void HitActor(Hitbox hitbox, IDamageable actor)
    {
        if (actor.GetGameObject().transform.root.TryGetComponent(out Rigidbody rb) && !rb.isKinematic)
        {
            victims.Add(rb);
        }
    }

    public override void FixedUpdateWeapon(Actor actor)
    {
        base.FixedUpdateWeapon(actor);
        if (victims.Count > 0)
        {
            foreach (Rigidbody rb in victims)
            {
                rb.AddExplosionForce(force, top.position, 5f);
            }
            victims.Clear();
        }
    }
}
