using UnityEngine;
using UnityEngine.Events;

public class BezierProjectileController : Projectile
{
    public Rigidbody tip;
    public float duration = 1f;
    float clock;
    public DamageKnockback damageKnockback;

    public Vector3[] controlPoints;
    public Hitbox hitbox;
    public float shockwaveRadius = 1f;
    public UnityEvent OnShockwaveHit;

    BezierTimeTravelHandler timeHandler;

    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
    bool launched;

    public GameObject[] dontDestroy;
    [ReadOnly, SerializeField] Vector3 initPos;

    private static readonly float ARROW_DURATION = 30f;


    public static BezierProjectileController Launch(GameObject arrowPrefab, Vector3 position, float duration, Transform source, DamageKnockback damageKnockback, Vector3[] targetPoints)
    {
        GameObject arrowObj = GameObject.Instantiate(arrowPrefab, position, Quaternion.LookRotation(Bezier.GetTangent(0, targetPoints)));
        BezierProjectileController arrowController = arrowObj.GetComponent<BezierProjectileController>();


        arrowController.hitbox = Hitbox.CreateHitbox(arrowController.tip.position, 0.1f, arrowController.tip.transform, damageKnockback, source.gameObject);

        //arrowController.prefabRef = arrowPrefab;

        arrowController.Launch(position, duration, source, damageKnockback, targetPoints);
        //arrowController.hitbox.SetActive(true);

        //hitboxController.OnHit.AddListener(OnArrowHit);

        return arrowController;
    }
    public void Launch(Vector3 position, float duration, Transform source, DamageKnockback damageKnockback, Vector3[] targetPoints)
    {
        this.hitbox.SetActive(false);
        this.transform.position = position;
        this.transform.rotation = Quaternion.LookRotation(Bezier.GetTangent(0, targetPoints));

        /*
        if (gameObject.activeInHierarchy)
        {
            tip.velocity = Vector3.zero;
            tip.angularVelocity = Vector3.zero;
            tip.Sleep();
        }
        */

        this.damageKnockback = damageKnockback;
        this.origin = source.gameObject;

        this.initPos = position;
        this.gameObject.SetActive(true);


        this.hitbox.SetDamage(damageKnockback);
        this.duration = duration;
        this.controlPoints = targetPoints;
        launched = false;



        //StopCoroutine("DisableAfterDelay");
        //StartCoroutine("DisableAfterDelay");
        Destroy(this.gameObject, ARROW_DURATION);
    }

    public override void Launch(Vector3 position, Quaternion angle, Vector3 force, Transform source, DamageKnockback damageKnockback)
    {
        Launch(position, 10f / (Mathf.Max(force.magnitude,0.01f)), source, damageKnockback, new Vector3[4]);
    }

    public override void SetHitbox(bool active)
    {
        hitbox.SetActive(active);
    }

    // Start is called before the first frame update
    void Start()
    {
        tip.isKinematic = true;
        clock = 0f;
        timeHandler = this.GetComponent<BezierTimeTravelHandler>();
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
        if (inFlight)
        {
            float mag = hitbox.damageKnockback.kbForce.magnitude;
            hitbox.damageKnockback.kbForce = tip.velocity.normalized * mag;
        }
    }

    private void FixedUpdate()
    {

        float t = curve.Evaluate(Mathf.Clamp01(clock / duration));

        Vector3 position = Bezier.GetPoint(t, controlPoints);
        Vector3 heading = Bezier.GetTangent(t, controlPoints);

        tip.MovePosition(position);
        tip.MoveRotation(Quaternion.LookRotation(heading));

        float deltaTime = Time.fixedDeltaTime;
        if (timeHandler != null) {
            deltaTime = timeHandler.GetFixedDeltaTime();
        }
        clock += deltaTime;

        if (t >= 1)
        {
            Shockwave();
            EndFlight();
        }

    }

    private void OnArrowHitTerrain(Hitbox contactBox, Collider hitTerrain)
    {
        hitbox.SetActive(false);
        FXController.CreateFX(FXController.FX.FX_Sparks, tip.position, Quaternion.identity, 3f, SoundFXAssetManager.GetSound("Bow/Hit"));

        EndFlight();
    }

    private void OnArrowHitActor(Hitbox contactBox, IDamageable actor)
    {
        EndFlight();
    }


    void Shockwave()
    {
        Vector3 origin = tip.transform.position;

        Collider[] colliders = Physics.OverlapSphere(origin, shockwaveRadius, LayerMask.GetMask("Actors"));

        DamageKnockback shockwaveDamage = new DamageKnockback(damageKnockback);
        shockwaveDamage.source = this.hitbox.source;
        shockwaveDamage.hitboxSource = this.hitbox.gameObject;

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
        UnparentDontDestroy();
        Destroy(this.gameObject);
    }

    public void UnparentDontDestroy()
    {
        foreach (GameObject gameObject in dontDestroy)
        {
            gameObject.transform.SetParent(null, true);
            Destroy(gameObject, ARROW_DURATION);
        }
    }
}
