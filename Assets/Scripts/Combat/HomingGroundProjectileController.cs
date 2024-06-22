using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HomingGroundProjectileController : Projectile
{
    public Rigidbody tip;
    public float velocity = 1f;
    public float accel = 0f;
    public float angularVelocity = 90f;
    public float angularAccel = 0f;
    public DamageKnockback damageKnockback;

    public Vector3 targetPoint;
    public Hitbox hitbox;
    public DamageKnockback shockwaveDamage;
    public float shockwaveRadius = 1f;
    public UnityEvent OnShockwaveHit;

    bool launched;

    [ReadOnly, SerializeField] Vector3 initPos;

    private static readonly float ARROW_DURATION = 30f;


    public static HomingGroundProjectileController Launch(GameObject arrowPrefab, Vector3 position, Quaternion angle, Vector3 force, Transform source, DamageKnockback damageKnockback, Vector3 targetPoint)
    {
        GameObject arrowObj = GameObject.Instantiate(arrowPrefab, position, angle);
        HomingGroundProjectileController arrowController = arrowObj.GetComponent<HomingGroundProjectileController>();


        arrowController.hitbox = Hitbox.CreateHitbox(arrowController.tip.position, 0.1f, arrowController.tip.transform, damageKnockback, source.gameObject);

        //arrowController.prefabRef = arrowPrefab;

        arrowController.Launch(position, angle, force, source, damageKnockback, targetPoint);
        //arrowController.hitbox.SetActive(true);

        //hitboxController.OnHit.AddListener(OnArrowHit);


        return arrowController;
    }
    public void Launch(Vector3 position, Quaternion angle, Vector3 force, Transform source, DamageKnockback damageKnockback, Vector3 targetPoint)
    {
        this.hitbox.SetActive(false);
        this.transform.position = position;
        this.transform.rotation = angle;

        if (gameObject.activeInHierarchy)
        {
            tip.velocity = Vector3.zero;
            tip.angularVelocity = Vector3.zero;
            tip.Sleep();
        }

        this.damageKnockback = damageKnockback;
        this.origin = source.gameObject;

        this.initPos = position;
        this.gameObject.SetActive(true);


        this.hitbox.SetDamage(damageKnockback);
        this.velocity = force.magnitude;
        this.targetPoint = targetPoint;
        launched = false;



        //StopCoroutine("DisableAfterDelay");
        //StartCoroutine("DisableAfterDelay");
        Destroy(this.gameObject, ARROW_DURATION);
    }

    public override void Launch(Vector3 position, Quaternion angle, Vector3 force, Transform source, DamageKnockback damageKnockback)
    {
        Launch(position, angle, force, source, damageKnockback, Vector3.zero);
    }

    public override void SetHitbox(bool active)
    {
        hitbox.SetActive(active);
    }

    // Start is called before the first frame update
    void Start()
    {
        tip.isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!launched)
        {
            hitbox.SetActive(true);
            hitbox.events.OnHitActor.AddListener(OnArrowHitActor);
            hitbox.events.OnHitTerrain.AddListener(OnArrowHitTerrain);
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

    private void FixedUpdate()
    {
        Vector3 currentHeading = tip.transform.forward;
        Vector3 desiredHeading = (targetPoint - tip.transform.position).normalized;
        Vector3 newHeading = Vector3.RotateTowards(currentHeading, desiredHeading, angularVelocity * Mathf.Deg2Rad * Time.fixedDeltaTime, 1f);

        tip.MoveRotation(Quaternion.LookRotation(newHeading));
        tip.MovePosition(tip.transform.position + tip.transform.forward * velocity * Time.fixedDeltaTime);

        velocity += accel * Time.deltaTime;
        angularVelocity += angularAccel * Time.deltaTime;
        if (Vector3.Distance(tip.transform.position, targetPoint) < shockwaveRadius)
        {
            Shockwave();
            EndFlight();
        }
    }

    void CheckShockwave()
    {
        if (Vector3.Distance(tip.transform.position, targetPoint) < shockwaveRadius)
        {
            Shockwave();
        }
    }

    private void OnArrowHitTerrain(Hitbox contactBox, Collider hitTerrain)
    {
        CheckShockwave();
        hitbox.SetActive(false);
        FXController.CreateFX(FXController.FX.FX_Sparks, tip.position, Quaternion.identity, 3f, SoundFXAssetManager.GetSound("Bow/Hit"));

        EndFlight();
    }

    private void OnArrowHitActor(Hitbox contactBox, IDamageable actor)
    {
        CheckShockwave();
        EndFlight();
    }

    void Shockwave()
    {
        Vector3 origin = tip.transform.position;

        Collider[] colliders = Physics.OverlapSphere(origin, shockwaveRadius, LayerMask.GetMask("Actors"));
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.TakeDamage(shockwaveDamage);
            }
        }
        OnShockwaveHit.Invoke();
    }
    private void EndFlight()
    {
        inFlight = false;
        Destroy(this.gameObject);
    }

}
