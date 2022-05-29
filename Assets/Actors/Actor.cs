using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using Animancer;

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
    public AnimancerComponent animancer;

    [HideInInspector]
    public AudioSource audioSource;

    public UnityEvent OnHurt;
    public UnityEvent OnHit;
    public UnityEvent OnDie;
    public UnityEvent OnAttack;
    public UnityEvent OnCritVulnerable;
    public UnityEvent OnDodgeSuccess;
    public UnityEvent OnBlock;
    [HideInInspector]public UnityEvent OnHealthLoss;
    [HideInInspector]public UnityEvent OnHealthGain;
    [HideInInspector]public UnityEvent OnHealthChange;

    public float lastDamageTaken;
    private  int mercyId; //hitbox

    public Vector3 moveDirection;
    public Vector3 moveAdditional;
    public Vector3 lastContactPoint;
    public Vector3 lastBlockPoint;

    public Vector3 xzVel;
    public float yVel;

    protected bool dead;
    // targets

    [SerializeField, ReadOnly] protected DamageKnockback currentDamage;

    public GameObject CombatTarget;
    public GameObject FollowTarget;
    [Header("Time Travel Data")]
    public List<TimeTravelData> timeTravelStates;
    public bool isInTimeState;
    void OnEnable()
    {

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        attributes = GetComponent<ActorAttributes>();
        OnHealthLoss = attributes.OnHealthLoss;
        OnHealthGain = attributes.OnHealthGain;
        OnHealthChange = attributes.OnHealthChange;
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
        animancer = this.GetComponent<AnimancerComponent>();
        // run after base class Start()
    }

    public void Update()
    {
        if (isInTimeState) return;

        ActorPreUpdate();

        ActorPostUpdate();
    }

    public virtual void ActorPreUpdate()
    {
        if (!isInTimeState && animancer.Layers[HumanoidAnimLayers.TimeEffects].Weight > 1f || animancer.Layers[HumanoidAnimLayers.TimeEffects].IsAnyStatePlaying())
        {
            this.GetComponent<ActorTimeTravelHandler>().EndTimeState();
        }
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

    public virtual void SetLastBlockpoint(Vector3 point)
    {
        lastBlockPoint = point;
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

    public virtual void SetCurrentDamage(DamageKnockback damageKnockback)
    {
        currentDamage = new DamageKnockback(damageKnockback);
        currentDamage.source = this.gameObject;
    }
    public virtual void OnFallOffMap()
    {
        Die();
    }
    public virtual bool IsAlive()
    {
        return attributes.health.current > 0;
    }

    public virtual bool IsInjured()
    {
        return attributes.health.current == 0;
    }

    public virtual bool IsAttacking()
    {
        return false;
    }

    public virtual bool IsFalse()
    {
        return attributes.health.current == 0;
    }
    public virtual bool IsArmored()
    {
        return false;
    }
    public virtual bool IsDodging()
    {
        return false;
    }

    public virtual bool IsHitboxActive()
    {
        return false;
    }

    public virtual bool ShouldDustOnStep()
    {
        return false;
    }

    public virtual bool IsGrounded()
    {
        return Physics.Raycast(this.transform.position, -this.transform.up, 2f, LayerMask.GetMask("Default, Terrain"));
    }
    public virtual bool IsTimeStopped()
    {
        if (!isInTimeState)
        {
            return false;
        }
        return TryGetComponent<ActorTimeTravelHandler>(out ActorTimeTravelHandler timeTravelHandler) && timeTravelHandler.IsFrozen();
    }
    public void PlayAudioClip(AudioClip audioClip)
    {
        if (audioSource != null)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }

    public virtual string GetCurrentGroundPhysicsMaterial()
    {
        return "";
    }
    public void SetAdditionalMovement(Vector3 move)
    {
        moveAdditional = move;
    }

    public virtual void Die()
    {
        if (dead) return;
        dead = true;
        OnDie.Invoke();
        StartCleanUp();
    }
    public void StartCleanUp()
    {
        StartCoroutine(CorpseClean());
    }

    public virtual bool ShouldCalcFireStrength()
    {
        return false;
    }

    public virtual List<DamageResistance> GetResistances()
    {
        return attributes.resistances;
    }

    public virtual List<DamageResistance> GetBlockResistance()
    {
        return null;
    }
    public virtual Vector3 GetLaunchVector(Vector3 origin)
    {
        return this.transform.forward;
    }

    public virtual void FlashWarning(int hand)
    {
        GameObject fx = FXController.CreateBladeWarning();
        fx.transform.position = this.transform.position;
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

    public virtual void SetToIdle()
    {
        // do nothing
    }
    public void MoveOverTime(Vector3 targetPosition, Quaternion targetRotation, float timeToReach)
    {
        StartCoroutine(MoveOverTimeRoutine(targetPosition, targetRotation, timeToReach));
    }

    IEnumerator MoveOverTimeRoutine(Vector3 targetPosition, Quaternion targetRotation, float timeToReach)
    {
        bool usingCC = this.TryGetComponent<CharacterController>(out CharacterController cc) && cc.enabled;
        
        if (timeToReach > 0f)
        {
            Vector3 startPosition = this.transform.position;
            Quaternion startRotation = this.transform.rotation;
            float t = 0f;
            float timeElapsed = 0f;
            while (timeElapsed < timeToReach)
            {
                yield return new WaitForEndOfFrame();
                timeElapsed += Time.deltaTime;
                t = Mathf.Clamp01(timeElapsed / timeToReach);
                Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, t);
                Quaternion currentRotation = Quaternion.Lerp(startRotation, targetRotation, t);
                if (usingCC)
                {
                    cc.Move(currentPosition - this.transform.position);
                }
                else
                {
                    this.transform.position = currentPosition;
                }
                this.transform.rotation = currentRotation;
            }
        }
        else
        {
            if (usingCC)
            {
                cc.Move(targetPosition - this.transform.position);
            }
            else
            {
                this.transform.position = targetPosition;
            }
            this.transform.rotation = targetRotation;
        }
    }
}
