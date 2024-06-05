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
    public UnityEvent OnParrySuccess;
    public UnityEvent OnBlock;
    public UnityEvent OnDodge;
    public UnityEvent OnCorpseClean;
    [HideInInspector]public UnityEvent OnHealthLoss;
    [HideInInspector]public UnityEvent OnHealthGain;
    [HideInInspector]public UnityEvent OnHealthChange;

    public float lastDamageAmountTaken;
    public DamageKnockback lastDamageTaken;
    private  int mercyId; //hitbox

    public Vector3 moveAdditional;
    public Vector3 lastContactPoint;
    public Vector3 lastBlockPoint;

    public Vector3 xzVel;
    public float yVel;

    [HideInInspector]
    public Vector3 hitParticlePosition;
    [HideInInspector]
    public Vector3 hitParticleDirection;
    protected bool dead;

    // targets

    [SerializeField, ReadOnly] protected DamageKnockback currentDamage;
    [SerializeField, ReadOnly] protected Consumable currentConsumable;
    public GameObject CombatTarget;
    public GameObject FollowTarget;
    [Header("Time Travel Data")]
    public List<TimeTravelData> timeTravelStates;
    public bool isInTimeState;
    public bool isInTimelineState;
    void OnEnable()
    {

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        attributes = GetComponent<ActorAttributes>();
        OnHealthLoss = attributes.OnHealthLoss;
        OnHealthGain = attributes.OnHealthGain;
        OnHealthChange = attributes.OnHealthChange;
        ActorOnEnable();
    }

    protected virtual void ActorOnEnable()
    {
        // run after base class Awake()
    }
    public void Start()
    {
        //CurrentAction = ActionsLibrary.GetInputAction(0);

        ActorManager.Register(this);
        ActorStart();

    }

    public virtual void ActorStart()
    {
        InitAnimancer();
        // run after base class Start()
    }

    public virtual void Update()
    {
        if (!CanUpdate()) return;

        ActorPreUpdate();

        ActorPostUpdate();
    }


    public bool CanUpdate()
    {
        return !isInTimeState && !isInTimelineState;
    }

    public void SetInTimeline(bool inTimeline)
    {
        isInTimelineState = inTimeline;
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

    protected virtual void InitAnimancer()
    {
        animancer = this.GetComponent<AnimancerComponent>();
        HumanoidAnimLayers.InitLayers(animancer);
    }

    private void OnDestroy()
    {
        ActorManager.Deregister(this);
        ActorOnDestroy();
    }
    public virtual void ActorOnDestroy()
    {

    }

    public virtual void ProcessDamageKnockback(DamageKnockback damageKnockback)
    {
        // do nothing by default
    }

    public virtual bool IsBlocking()
    {
        return false;
    }

    public virtual bool IsFalling()
    {
        return false;
    }
    public virtual void SetLastBlockpoint(Vector3 point)
    {
        lastBlockPoint = point;
    }

    public virtual Vector3 GetBlockpoint(Vector3 hitPoint)
    {
        return hitPoint;
    }

    public virtual void SetHitParticleVectors(Vector3 position, Vector3 direction)
    {
        hitParticlePosition = position;
        hitParticleDirection = direction;
    }

    public virtual IInventory GetInventory()
    {
        return null;
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

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }
    public void SetFollowTarget(GameObject target)
    {
        FollowTarget = target;
    }

    public virtual void SetCurrentDamage(DamageKnockback damageKnockback)
    {
        if (currentDamage != null)
        {
            currentDamage.OnHit.RemoveListener(RegisterHit);
        }
        currentDamage = new DamageKnockback(damageKnockback);
        currentDamage.friendlyGroup = this.attributes.friendlyGroup;
        currentDamage.source = this.gameObject;
        currentDamage.OnHit.AddListener(RegisterHit);

    }

    public virtual DamageKnockback GetLastDamage()
    {
        return currentDamage;
    }

    public void RegisterHit()
    {
        OnHit.Invoke();
    }
    public virtual void SetCurrentConsumable(Consumable consumable)
    {
        if (consumable != null)
        {
            currentConsumable = consumable;
        }
    }

    public virtual void RealignToTarget()
    {
        this.transform.rotation = Quaternion.LookRotation(GetDirectionToTarget());
    }

    public virtual void RotateTowardsTarget(float maxDegreesDelta)
    {
        this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(GetDirectionToTarget()), maxDegreesDelta);
    }
    public virtual void RealignAwayTarget()
    {
        this.transform.rotation = Quaternion.LookRotation(-GetDirectionToTarget());
    }

    public Vector3 GetDirectionToTarget()
    {
        if (CombatTarget != null)
        {
            Vector3 dir = CombatTarget.transform.position - this.transform.position;
            dir.y = 0f;
            dir.Normalize();
            return dir;
        }
        return this.transform.forward;
    }
    public virtual void OnFallOffMap()
    {
        DieImmediate();
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

    public virtual bool IsClimbing()
    {
        return false;
    }

    public virtual bool IsJumping()
    {
        return !IsGrounded();
    }

    public virtual bool IsGrounded()
    {
        return Physics.Raycast(this.transform.position, -this.transform.up, 2f, MaskReference.Terrain);
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
        return "Default";
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

    public virtual void DieImmediate()
    {
        Die();
        OnCorpseClean.Invoke();
        GameObject.Destroy(this.gameObject);
    }

    public void StartCleanUp()
    {
        StartCoroutine(CorpseClean(attributes.cleanUpTime));
    }

    public void StartCleanUp(float duration)
    {
        StartCoroutine(CorpseClean(duration));
    }

    IEnumerator CorpseClean(float duration) 
    {
        yield return new WaitForSecondsRealtime(duration);
        OnCorpseClean.Invoke();
        GameObject.Destroy(this.gameObject);
    }

    public virtual bool ShouldCalcFireStrength()
    {
        return false;
    }

    public virtual DamageResistance GetResistances()
    {
        return attributes.resistances;
    }

    public virtual DamageResistance GetBlockResistance()
    {
        return null;
    }
    public virtual Vector3 GetLaunchVector(Vector3 origin)
    {
        return this.transform.forward;
    }

    public virtual float GetLaunchVectorSmoothDistance()
    {
        return 10f;
    }

    public virtual void DeactivateHitboxes()
    {
        Debug.LogWarning("DeactivateHitboxes not implemented!");
    }
    public virtual void FlashWarning(int hand)
    {
        GameObject fx = FXController.CreateBladeWarning();
        fx.transform.position = this.transform.position;
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
