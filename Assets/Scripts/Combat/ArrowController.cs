using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class ArrowController : Projectile
{
    public Rigidbody tip;
    public Rigidbody feather;
    [ReadOnly] public Hitbox hitbox;
    public DamageKnockback damageKnockback;
    public GameObject interactable;
    public GameObject prefabRef;
    public GameObject[] dontDestroy;
    bool launched;
    bool ignoreStick;
    
    Vector3 stickPos;
    
    Vector3 initScale;

    Vector3 initPos;
    [ReadOnly]public Vector3 initForce;
    float timeRemaining;

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
        //hitbox.SetActive(false);
    }
    public override void SetHitbox(bool active)
    {
        hitbox.SetActive(active);
    }
    private void OnArrowHit()
    {
        bool allowInteract = true;
        EndFlight();

        if (hitbox.didHitTerrain)
        {
            if (!ignoreStick)
            {
                tip.isKinematic = true;
                feather.isKinematic = true;

                Stick(hitbox.hitTerrain);
            }
            else
            {
                UnparentDontDestroy();
                Destroy(tip.gameObject);
            }
            hitbox.SetActive(false);
            FXController.instance.CreateFX(FXController.FX.FX_Sparks, tip.position, Quaternion.identity, 3f, FXController.instance.clipDictionary["bow_hit"]);
        }
        else if (hitbox.victims.Count > 0)
        {
            //FXController.instance.CreateFX(FXController.FX.FX_BleedPoint, feather.position, Quaternion.identity, 3f, FXController.clipDictionary["bow_hit"]);
            UnparentDontDestroy();
            Destroy(tip.gameObject);
            allowInteract = false;
        }
        if (allowInteract)
        {
            EnablePickup();
        }
    }


    public void Stick(Collider hitCollider)
    {
        Vector3 stickPos;
        if (hitCollider is BoxCollider || hitCollider is SphereCollider || hitCollider is CapsuleCollider || (hitCollider is MeshCollider meshCollider && meshCollider.convex)) {
            stickPos = hitCollider.ClosestPoint(tip.transform.position);
        }
        else
        {
            stickPos = hitCollider.ClosestPointOnBounds(tip.transform.position);
        }

        /*ParentConstraint parentConstraint = this.GetComponent<ParentConstraint>();

        parentConstraint.constraintActive = false;
        parentConstraint.locked = false;

        while (parentConstraint.sourceCount > 0)
        {
            parentConstraint.RemoveSource(0);
        }
        parentConstraint.AddSource(new ConstraintSource() { sourceTransform = hitCollider.transform, weight = 1f });*/

        GameObject empty = new GameObject("Arrow Stick Mount");
        Destroy(empty, ARROW_DURATION);
        empty.transform.SetParent(hitbox.hitTerrain.transform, false);
        empty.transform.localScale = new Vector3(1f / hitbox.hitTerrain.transform.localScale.x, 1f / hitbox.hitTerrain.transform.localScale.y, 1f / hitbox.hitTerrain.transform.localScale.z);
        tip.transform.SetParent(empty.transform, true);
        tip.position = stickPos;
        tip.GetComponent<Collider>().isTrigger = true;
        this.GetComponentInChildren<TrailRenderer>().emitting = false;
    }

    public void EnablePickup()
    {
        if (interactable != null)
        interactable.SetActive(true);
    }
    public static new ArrowController Launch(GameObject arrowPrefab, Vector3 position, Quaternion angle, Vector3 force, Transform source, DamageKnockback damageKnockback)
    {

        GameObject arrowObj = GameObject.Instantiate(arrowPrefab, position, angle);
        ArrowController arrowController = arrowObj.GetComponent<ArrowController>();


        arrowController.hitbox = Hitbox.CreateHitbox(arrowController.tip.position, 0.1f, arrowController.tip.transform, damageKnockback, source.gameObject);

        arrowController.prefabRef = arrowPrefab;

        arrowController.Launch(position, angle, force, source, damageKnockback);
        //arrowController.hitbox.SetActive(true);

        //hitboxController.OnHit.AddListener(OnArrowHit);


        return arrowController;
    }

    public static new ArrowController Spawn(GameObject arrowPrefab, Transform source)
    {

        GameObject arrowObj = GameObject.Instantiate(arrowPrefab);
        ArrowController arrowController = arrowObj.GetComponent<ArrowController>();


        arrowController.hitbox = Hitbox.CreateHitbox(arrowController.tip.position, 0.25f, arrowController.tip.transform, new DamageKnockback(), source.gameObject);


        arrowController.gameObject.SetActive(false);
        return arrowController;
    }

    public override void Launch(Vector3 position, Quaternion angle, Vector3 force, Transform source, DamageKnockback damageKnockback)
    {
        this.hitbox.SetActive(false);
        this.transform.position = position;
        this.transform.rotation = angle;
        this.GetComponentInChildren<TrailRenderer>().Clear();
        if (gameObject.activeInHierarchy)
        {
            tip.velocity = Vector3.zero;
            tip.angularVelocity = Vector3.zero;
            tip.Sleep();
            feather.velocity = Vector3.zero;
            feather.angularVelocity = Vector3.zero;
            feather.Sleep();
        }
        this.damageKnockback = damageKnockback;
        this.origin = source.gameObject;

        this.tip.position = position;

        this.initPos = position;
        this.gameObject.SetActive(true);

        this.tip.AddForce(force, ForceMode.VelocityChange);

        this.hitbox.SetDamage(damageKnockback);
        this.initForce = force;
        launched = false;
        

        //StopCoroutine("DisableAfterDelay");
        //StartCoroutine("DisableAfterDelay");
        Destroy(this.gameObject, ARROW_DURATION);
    }

    IEnumerator DisableAfterDelay()
    {
        yield return new WaitForSeconds(ARROW_DURATION);
        tip.velocity = Vector3.zero;
        tip.angularVelocity = Vector3.zero;
        feather.velocity = Vector3.zero;
        feather.angularVelocity = Vector3.zero;
        this.GetComponentInChildren<TrailRenderer>().Clear();
        this.gameObject.SetActive(false);
    }

    public void UnparentDontDestroy()
    {
        foreach (GameObject gameObject in dontDestroy)
        {
            gameObject.transform.SetParent(null);
            Destroy(gameObject, ARROW_DURATION);
        }
    }

    public void IgnoreStick(bool ignore = true)
    {
        ignoreStick = ignore;
    }
}
