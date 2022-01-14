using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(ActorAttributes))]
public class Actor : MonoBehaviour
{
    [Header("Actor Details")]
    public string actorName;
    public string actorTitle;
    public int actorId;
    [Space(5), HideInInspector]
    public ActorAttributes attributes;

    [HideInInspector]
    public Animator animator;

    [HideInInspector]
    public AudioSource audioSource;

    public UnityEvent OnHurt;
    public UnityEvent OnHit;
    public UnityEvent OnDie;
    public UnityEvent OnAttack;
    public UnityEvent OnCritVulnerable;
    public float lastDamageTaken;
    private  int mercyId; //hitbox

    public Vector3 moveDirection;
    public Vector3 moveAdditional;
    public Vector3 lastContactPoint;
    // targets

    public GameObject CombatTarget;
    public GameObject FollowTarget;
    void OnEnable()
    {

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        attributes = GetComponent<ActorAttributes>();
        ActorAwake();
    }

    protected virtual void ActorAwake()
    {
        // run after base class Awake()
    }
    public void Start()
    {
        //CurrentAction = ActionsLibrary.GetInputAction(0);
        
        ActorStart();
    }

    public virtual void ActorStart()
    {
        // run after base class Start()
    }

    public void Update()
    {
        ActorPreUpdate();

        ActorPostUpdate();
    }

    public virtual void ActorPreUpdate()
    {
        // run before base class 
    }

    public virtual void ActorPostUpdate()
    {
        // run after base class 
    }

    private void OnTriggerEnter(Collider other)
    {
        HitboxController hitbox;
        if (!other.TryGetComponent<HitboxController>(out hitbox))
        {
            return;
        }

        if (hitbox.GetSource() == this.transform.root)
        {
            return;
        }

        if (hitbox.id == mercyId)
        {
            return;
        }

        mercyId = hitbox.id;
        OnHurt.Invoke();
        hitbox.lastHitActor = this;
        hitbox.OnHit.Invoke();
        OnHitboxEnter(hitbox);
    }

    protected virtual void OnHitboxEnter(HitboxController hitbox)
    {
        // do nothing by default
        ProcessDamageKnockback(hitbox.damageKnockback);
    }

    public virtual void ProcessDamageKnockback(DamageKnockback damageKnockback)
    {
        // do nothing by default
    }

    public virtual bool IsBlocking()
    {
        return false;
    }
    public GameObject GetCombatTarget()
    {
        return CombatTarget;
    }

    public virtual void SetCombatTarget(GameObject target)
    {
        CombatTarget = target;
    }
    public GameObject GetFollowTarget()
    {
        return FollowTarget;
    }

    public void SetFollowTarget(GameObject target)
    {
        FollowTarget = target;
    }

    public virtual bool IsAlive()
    {
        return attributes.health.current > 0;
    }

    public virtual bool IsInjured()
    {
        return attributes.health.current == 0;
    }

    public void PlayAudioClip(AudioClip audioClip)
    {
        if (audioSource != null)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }


    public void SetAdditionalMovement(Vector3 move)
    {
        moveAdditional = move;
    }

    public void StartCleanUp()
    {
        StartCoroutine(CorpseClean());
    }

    public virtual bool ShouldCalcFireStrength()
    {
        return false;
    }


    IEnumerator CorpseClean()
    {
        int iterations = 5;
        for (int i = 0; i < iterations; i++)
        {
            yield return new WaitForSecondsRealtime(1f);
        }
        GameObject.Destroy(this.gameObject);

    }
}
