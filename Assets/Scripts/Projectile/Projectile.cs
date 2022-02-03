using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public GameObject origin;
    [ReadOnly] public bool inFlight;
    public static Projectile Launch(GameObject prefab, Vector3 position, Quaternion angle, Vector3 force, Transform source, DamageKnockback damageKnockback)
    {
        
        GameObject obj = GameObject.Instantiate(prefab);
        Projectile controller = obj.GetComponent<Projectile>();

        return controller;
    }

    public abstract void Launch(Vector3 position, Quaternion angle, Vector3 force, Transform source, DamageKnockback damageKnockback);
    public static Projectile Spawn(GameObject prefab, Transform source)
    {
        GameObject obj = GameObject.Instantiate(prefab);
        Projectile controller = obj.GetComponent<Projectile>();
        obj.SetActive(false);
        return controller;
    }

    public abstract void SetHitbox(bool active);
}
