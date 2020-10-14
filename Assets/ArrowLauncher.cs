using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowLauncher : MonoBehaviour
{
    public bool Launch;
    [Space(10)]
    public GameObject arrow;
    public float force;
    public DamageKnockback damageKnockback;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Launch || Input.GetButtonDown("Debug"))
        {
            Launch = false;
            //GameObject newArrow = Instantiate(arrow, transform.position, transform.rotation);
            //Transform tip = newArrow.transform.Find("TipRoot");
            //newArrow.GetComponentInChildren<Rigidbody>().AddForce(this.transform.forward * force, ForceMode.VelocityChange);

            //newArrow.GetComponentInChildren<ArrowController>().Launch(this.transform.forward * force, this.transform.root, damageKnockback);

            ArrowController.LaunchArrow(arrow, transform.position, Quaternion.LookRotation(this.transform.forward), this.transform.forward * force, this.transform, this.damageKnockback);
            
        }
    }
}
