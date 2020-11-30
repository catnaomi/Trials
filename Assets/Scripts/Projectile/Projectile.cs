using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
   
    public static Projectile Launch(GameObject prefab, Vector3 position, Quaternion angle, Vector3 force, Transform source, DamageKnockback damageKnockback)
    {
        
        GameObject obj = GameObject.Instantiate(prefab);
        Projectile controller = obj.GetComponent<Projectile>();

        return controller;
    }
}
