using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : Projectile
{
    public Rigidbody tip;
    public Rigidbody feather;
    public Hitbox hitbox;
    public DamageKnockback damageKnockback;
    public GameObject origin;

    bool launched;
    bool inFlight;
    bool shouldStick;
    Collider stickParent;
    Vector3 stickPos;
    
    Vector3 initScale;

    Vector3 initPos;

    private static readonly float ARROW_DURATION = 30f;

    private void Awake()
    {
        //Physics.IgnoreLayerCollision(0, 11);
        //Physics.IgnoreLayerCollision(11, 11);

        initScale = new Vector3(this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);

    }

    private void Start()
    {

        //hitbox = Hitbox.CreateHitbox(tip.position, 0.25f, tip.transform, this.damageKnockback, origin);
        
    }
    private void Update()
    {
        if (!launched)
        {
            hitbox.SetActive(true);
            hitbox.OnHitAnything.AddListener(OnArrowHit);
            launched = true;
            inFlight = true;
            tip.position = initPos;
        }
        if (tip.velocity.magnitude < 0.1f)
        {
            EndFlight();
        }
        else if (inFlight)
        {
            float mag = hitbox.damageKnockback.kbForce.magnitude;
            hitbox.damageKnockback.kbForce = tip.velocity.normalized * mag;
        }
    }

    private void EndFlight()
    {
        inFlight = false;
        hitbox.gameObject.SetActive(false);
    }

    private void OnArrowHit()
    {
        EndFlight();

        if (hitbox.didHitTerrain)
        {

            tip.isKinematic = true;
            feather.isKinematic = true;

            Vector3 stickPos = hitbox.hitTerrain.ClosestPointOnBounds(tip.transform.position);

            GameObject empty = new GameObject();
            empty.name = "Arrow [" + 0 + "] Stick Mount";
            Destroy(empty, ARROW_DURATION);
            empty.transform.SetParent(hitbox.hitTerrain.transform, false);
            empty.transform.localScale = new Vector3(1f / hitbox.hitTerrain.transform.localScale.x, 1f / hitbox.hitTerrain.transform.localScale.y, 1f / hitbox.hitTerrain.transform.localScale.z);
            tip.transform.SetParent(empty.transform, true);
            tip.position = stickPos;
        }
        else if (hitbox.victims.Count > 0)
        {
            Destroy(tip.gameObject);
        }

        return;
        Actor victim;// = hitboxController.lastHitActor;

        if (inFlight)
        {
            inFlight = false;
            Rigidbody lead = null;
            float leadDistance = Mathf.Infinity;
            foreach (Rigidbody rigidbody in victim.GetComponentsInChildren<Rigidbody>())
            {
                Vector3 pos = rigidbody.transform.GetComponent<Collider>().ClosestPoint(tip.position);
                float dist = Vector3.Distance(pos, tip.transform.position);
                if (dist < leadDistance)
                {
                    leadDistance = dist;
                    lead = rigidbody;
                }
            }
            if (lead != null) {
                shouldStick = true;
                //stickTarget = lead;
                stickPos = lead.transform.GetComponent<Collider>().bounds.center;
                tip.position = stickPos; //tip.position + tip.transform.forward * 4f;
                /*tip.position = lead.position;//.transform.GetComponent<Collider>().ClosestPoint(tip.position);//tip.transform.position + tip.transform.forward * 0.5f;
                tip.transform.SetParent(lead.transform, true);*/
                tip.isKinematic = true;
                feather.isKinematic = true;
                
                EndFlight();
            }
        }
    }

    /*
    private void DisabledUpdate()
    {
        
        if (shouldStick)
        {
            /*
            GameObject empty = new GameObject();
            empty.name = "Arrow [" + hitboxController.id + "] Stick Mount";
            Destroy(empty, ARROW_DURATION);
            empty.transform.SetParent(stickParent.transform, true);
            //empty.transform.localScale = new Vector3(1,1,1);
            this.transform.root.SetParent(empty.transform, true);
            this.transform.localScale = new Vector3(0.1f / empty.transform.localScale.x, 0.1f / empty.transform.localScale.y, 0.1f / empty.transform.localScale.z);
            //this.transform.root.    stickParent.transform, false);
            //tip.transform.localScale = stickScale;
            //tip.transform.rotation = stickRotation;
            //tip.transform.localScale = 1f / (tip.transform.parent.localScale.x);
            tip.transform.position = stickParent.GetComponent<Collider>().ClosestPoint(tip.position);

            hitboxController.Deactivate();
            shouldStick = false;
            
            GameObject empty = new GameObject();
            empty.name = "Arrow [" + 0 + "] Stick Mount";
            Destroy(empty, ARROW_DURATION);
            empty.transform.SetParent(stickParent.transform, false);
            empty.transform.localScale = new Vector3(1f / stickParent.transform.localScale.x, 1f / stickParent.transform.localScale.y, 1f / stickParent.transform.localScale.z);
            tip.transform.SetParent(empty.transform, true);
            /*GameObject clone = Instantiate(tip.gameObject, stickParent.ClosestPoint(tip.position), tip.transform.rotation, empty.transform);
            //clone.GetComponentInChildren<HitboxController>().enabled = false;
            clone.GetComponentInChildren<ArrowController>().enabled = false;
            foreach (Collider c in clone.GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }
            Destroy(tip.gameObject);
            shouldStick = false;
        }
        else if (inFlight)
        {
            RaycastHit[] hits = tip.SweepTestAll(tip.transform.forward, 0.5f);
            Collider lead = null;
            int leadChildren = 999;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider != null)
                {
                    int count = hit.collider.transform.GetComponentsInChildren<Collider>().Length;
                    if (count < leadChildren)
                    {
                        lead = hit.collider;
                        leadChildren = count;
                    }
                }
            }
            if (lead != null) {

                //this.transform.root.SetParent(lead.transform, false);
                tip.transform.position = lead.ClosestPoint(tip.position);
                shouldStick = true;
                stickParent = lead;
                tip.Sleep();
                feather.Sleep();
                tip.isKinematic = true;
                feather.isKinematic = true;

                EndFlight();

                
            }
            
            
        }
        else if (false)
        {
            if (tip.transform.parent != null)
            {
                tip.transform.position = tip.transform.parent.GetComponent<Collider>().ClosestPoint(tip.position);
            }
        }
        /*
        if (shouldStick)
        {
            
            tip.transform.SetParent(stickTarget.transform, true);
            tip.position = stickPos;
            shouldStick = false;
        }
    }

    */
    public static new ArrowController Launch(GameObject arrowPrefab, Vector3 position, Quaternion angle, Vector3 force, Transform source, DamageKnockback damageKnockback)
    {

        GameObject arrowObj = GameObject.Instantiate(arrowPrefab, position, angle);
        ArrowController arrowController = arrowObj.GetComponent<ArrowController>();

        arrowController.damageKnockback = damageKnockback;
        arrowController.origin = source.gameObject;

        arrowController.tip.position = position;

        arrowController.initPos = position;

        arrowController.tip.AddForce(force, ForceMode.VelocityChange);

        arrowController.hitbox = Hitbox.CreateHitbox(arrowController.tip.position, 0.1f, arrowController.tip.transform, damageKnockback, source.gameObject);

        //arrowController.hitbox.SetActive(true);

        //hitboxController.OnHit.AddListener(OnArrowHit);

        Destroy(arrowObj, ARROW_DURATION);

        return arrowController;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(stickPos, 0.05f);
    }
}
