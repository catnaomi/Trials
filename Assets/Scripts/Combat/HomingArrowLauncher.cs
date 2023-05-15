using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HomingArrowLauncher : MonoBehaviour
{
    public bool Launch;
    [Space(10)]
    public GameObject arrow;
    public float force;
    public DamageKnockback damageKnockback;
    public Transform targetTransform;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

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
            HomingGroundProjectileController.Launch(
                arrow,
                transform.position,
                Quaternion.LookRotation(this.transform.forward),
                this.transform.forward * force,
                this.transform,
                this.damageKnockback,
                targetTransform.position
                ); 
        }
    }
}
