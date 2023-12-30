using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GenericLauncher : MonoBehaviour
{
    public bool Launch;
    [Space(10)]
    public GameObject arrow;
    public float force;
    public DamageKnockback damageKnockback;
    
    // Update is called once per frame
    void Update()
    {
        if (Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame)
        {
            Launch = true;
        }
        if (Launch)
        {
            Launch = false;

            GenericProjectile.Launch(arrow, transform.position, Quaternion.LookRotation(this.transform.forward), this.transform.forward * force, this.transform, this.damageKnockback);
            
        }
    }
}
