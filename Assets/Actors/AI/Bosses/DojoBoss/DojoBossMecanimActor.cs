using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DojoBossMecanimActor : Actor, IDamageable, IAttacker
{

    Animator animator;
    DojoBossInventoryTransformingController inventory;
    bool isHitboxActive;
    [Header("Animation Curves & Values")]
    public AnimationCurve lanceExtensionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public Vector2 lanceExtensionMinMax = Vector2.up;
    public float lanceExtensionDuration = 1f;
    [Header("Parries")]
    public string[] parryPatterns;
    int parryCurrentIndex;
    int parrySequenceIndex;
    [Header("Mecanim Values")]
    [ReadOnly, SerializeField] bool InCloseRange;
    [ReadOnly, SerializeField] bool InMeleeRange;
    [ReadOnly, SerializeField] bool ParryHit;
    [ReadOnly, SerializeField] int NextParry;
    [ReadOnly, SerializeField] bool ParryFail;
    [Space(10)]
    public float closeRange = 5f;
    public float meleeRange = 1f;
    public float randomCycleSpeed = 3f;
    [Header("Events")]
    public UnityEvent OnHitboxActive;
    public UnityEvent OnParrySuccess;
    float randomClock = 0f;
    bool shouldRealign;
    // Start is called before the first frame update
    public override void ActorStart()
    {
        base.ActorStart();
        animator = this.GetComponent<Animator>();
        inventory = this.GetComponent<DojoBossInventoryTransformingController>();
        CombatTarget = PlayerActor.player.gameObject;
        OnHitboxActive.AddListener(RealignToTarget);
        SetParryValue();
    }

    // Update is called once per frame
    public override void ActorPostUpdate()
    {
        /*
        if (!inventory.IsMainDrawn())
        {
            //inventory.SetDrawn(Inventory.MainType, true);
        }
        */
        randomClock -= Time.deltaTime;
        InCloseRange = Vector3.Distance(this.transform.position, CombatTarget.transform.position) <= closeRange;
        InMeleeRange = Vector3.Distance(this.transform.position, CombatTarget.transform.position) <= meleeRange;
        if (shouldRealign)
        {
            this.transform.LookAt(CombatTarget.transform, Vector3.up);
            shouldRealign = false;
        }
        else if (IsParrying())
        {
            RotateTowardsTarget();
        }
        if (randomClock <= 0f)
        {
            OnCycle();
        }
        UpdateMecanimValues();
        if (randomClock <= 0f)
        {
            randomClock = randomCycleSpeed;
        }
    }


    void UpdateMecanimValues()
    {
        if (randomClock <= 0f)
        {
            animator.SetFloat("Random", Random.value);
        }

        animator.SetBool("InCloseRange", InCloseRange);
        animator.SetBool("InMeleeRange", InMeleeRange);

        animator.SetInteger("NextParry", NextParry);
        if (ParryHit)
        {
            animator.SetTrigger("ParryHit");
            ParryHit = false;
        }
        if (ParryFail)
        {
            animator.SetTrigger("ParryFail");
            ParryFail = false;
        }
    }

    void OnCycle()
    {
        if (!IsParrying())
        {

        }
    }

    public void RotateTowardsTarget()
    {
        if (CombatTarget != null)
        {
            this.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(this.transform.forward, (CombatTarget.transform.position - this.transform.position).normalized, 360f * Time.deltaTime, Mathf.Infinity));
        }
    }

    public void StartLanceExtension(float duration)
    {
        if (inventory.weaponMainInstance == null || inventory.weaponOffInstance == null) return;

        StartCoroutine(ExtendLanceRoutine(lanceExtensionDuration, lanceExtensionMinMax.x, lanceExtensionMinMax.y));
    }

    public void RevertLanceExtension()
    {
        if (inventory.weaponMainInstance == null || inventory.weaponOffInstance == null) return;

        StartCoroutine(ExtendLanceRoutine(lanceExtensionDuration, lanceExtensionMinMax.y, lanceExtensionMinMax.x));
    }

    void ExtendLance(float length, GameObject model, Transform tip, Transform extend)
    {
        float y_local = extend.localScale.y;
        float y_world = extend.lossyScale.y;

        float y_mult = (y_local * length) / y_world;

        extend.localScale = new Vector3(extend.localScale.x, y_mult * 0.5f, extend.localScale.z);

        Vector3 farPoint = model.transform.position + model.transform.up * length;

        extend.transform.position = (model.transform.position + farPoint) / 2f;
        tip.transform.position = farPoint;
        Debug.DrawRay(model.transform.position, model.transform.up * length, Color.cyan, 5f);
    }

    IEnumerator ExtendLanceRoutine(float duration, float startLength, float endLength)
    {
        GameObject model = inventory.GetWeaponModel();
        Transform tip = model.transform.FindRecursively("_tip");
        Transform extend = model.transform.FindRecursively("_extend");

        if (tip == null || extend == null)
        {
            yield break;
        }
        float clock = 0f;

        float length;

        List<GameObject> struck = new List<GameObject>();
        RaycastHit[] results = new RaycastHit[16];
        while (clock < duration)
        {
            length = Mathf.Lerp(startLength, endLength, lanceExtensionCurve.Evaluate(Mathf.Clamp01(clock / duration)));
            ExtendLance(length, model, tip, extend);
            CheckLanceHitbox(length, inventory.weaponMainInstance.width, model.transform.position, model.transform.up, struck, results);
            yield return null;
            clock += Time.deltaTime;

        }
    }

    void CheckLanceHitbox(float length, float radius, Vector3 start, Vector3 direction, List<GameObject> struck, RaycastHit[] results)
    {
        Vector3 farPoint = start + direction * length;
        int hits = Physics.CapsuleCastNonAlloc(start, farPoint, radius, direction, results, length, LayerMask.GetMask("Actors") | MaskReference.Terrain);

        GameObject hitObj;
        for (int i = 0; i < hits; i++)
        {
            hitObj = results[i].collider.gameObject;
            if (!struck.Contains(hitObj))
            {
                struck.Add(hitObj);
                if (hitObj.transform.root != this.transform)
                {
                    if (hitObj.TryGetComponent<IDamageable>(out IDamageable damageable))
                    {
                        damageable.TakeDamage(currentDamage);
                    }
                }
            }
        }
    }

    public override void RealignToTarget()
    {
        base.RealignToTarget();
        shouldRealign = true;
    }

    /*
   * triggered by animation:
   * 0 = deactivate hitboxes
   * 1 = main weapon
   * 2 = off weapon, if applicable
   * 3 = both, if applicable
   * 4 = ranged weapon
   */
    public void HitboxActive(int active)
    {
        EquippableWeapon mainWeapon = inventory.weaponMainInstance;
        EquippableWeapon offHandWeapon = inventory.weaponOffInstance;
        EquippableWeapon rangedWeapon = null;// inventory.GetRangedWeapon();
        bool main = (mainWeapon != null && mainWeapon is IHitboxHandler);
        bool off = (offHandWeapon != null && offHandWeapon is IHitboxHandler);
        bool ranged = (rangedWeapon != null && rangedWeapon is IHitboxHandler);
        if (active == 0)
        {
            if (main)
            {
                ((IHitboxHandler)mainWeapon).HitboxActive(false);
            }
            if (off)
            {
                ((IHitboxHandler)offHandWeapon).HitboxActive(false);
            }
            isHitboxActive = false;
        }
        else if (active == 1)
        {
            if (main)
            {
                ((IHitboxHandler)mainWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }
        else if (active == 2)
        {
            if (off)
            {
                ((IHitboxHandler)offHandWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }
        else if (active == 3)
        {
            if (main)
            {
                ((IHitboxHandler)mainWeapon).HitboxActive(true);
            }
            if (off)
            {
                ((IHitboxHandler)offHandWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }
        else if (active == 4)
        {
            if (ranged)
            {
                ((IHitboxHandler)rangedWeapon).HitboxActive(true);
            }
            isHitboxActive = true;
            OnHitboxActive.Invoke();
        }


    }

    public bool IsCircleParrying()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("PARRY_CIRCLE");
    }

    public bool IsCrossParrying()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("PARRY_CROSS");
    }

    public bool IsParrying()
    {
        return IsCircleParrying() || IsCrossParrying();
    }
    public GameObject GetGameObject()
    {
        throw new System.NotImplementedException();
    }

    public DamageKnockback GetLastDamage()
    {
        return currentDamage;
    }

    public DamageKnockback GetLastTakenDamage()
    {
        return lastDamageTaken;
    }
    public override bool IsDodging()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("DODGE");
    }

    public void GetParried()
    {
        throw new System.NotImplementedException();
    }

    public void Recoil()
    {
        throw new System.NotImplementedException();
    }

    public void StartCritVulnerability(float time)
    {

        //throw new System.NotImplementedException();
    }

    public void TakeDamage(DamageKnockback damage)
    {
        if (!this.IsAlive()) return;
        lastDamageTaken = damage;
        if (this.IsTimeStopped() || this.IsDodging())
        {
            //damageHandler.TakeDamage(damage);
            return;
        }

        bool isParrying = IsParrying();
        bool circleParrying = IsCircleParrying();
        bool crossParrying = IsCrossParrying();
        if ((isParrying) && damage.isRanged)
        {
            damage.OnBlock.Invoke();
            this.OnBlock.Invoke();
        }
        else if (crossParrying && !damage.isSlash)
        {
            if (damage.source.TryGetComponent<IDamageable>(out IDamageable damageable) && !damage.cannotRecoil)
            {
                damageable.StartCritVulnerability(3f);
                damageable.Recoil();
            }
            CrossParryFail(damage);

            OnBlock.Invoke();
            damage.OnBlock.Invoke();
        }
        else if (circleParrying && !damage.isThrust)
        {
            if (damage.source.TryGetComponent<IDamageable>(out IDamageable damageable) && !damage.cannotRecoil)
            {
                damageable.StartCritVulnerability(3f);
                damageable.Recoil();
            }
            CircleParryFail(damage);

            OnBlock.Invoke();
            damage.OnBlock.Invoke();
        }
        else if ((crossParrying && damage.isSlash) || (circleParrying && damage.isThrust))
        {
            ParrySuccess(damage, circleParrying);
            this.OnHurt.Invoke();
            damage.OnCrit.Invoke();
            damage.OnBlock.Invoke();
            this.OnBlock.Invoke();
            //StartCritVulnerability(clip.MaximumDuration / clip.Speed);
        }
        else
        {
            //damageHandler.TakeDamage(damage);
        }
        DeactivateHitboxes();
    }


    public void SetParryValue()
    {
        string sequence = parryPatterns[parrySequenceIndex];
        string[] charSequence = sequence.Split(" ");

        if (parryCurrentIndex < charSequence.Length)
        {
            if (charSequence[parryCurrentIndex].ToUpper() == "O")
            {
                NextParry = 1;
            }
            else if (charSequence[parryCurrentIndex].ToUpper() == "X")
            {
                NextParry = -1;
            }
            else
            {
                NextParry = 0;
            }
        }
        else
        {
            NextParry = 0;
        }
    }

    public void OnGuardBreak()
    {
        NextParrySequence();
    }
    public void IncrementParryIndex()
    {
        parryCurrentIndex++;
    }

    public void NextParrySequence()
    {
        parrySequenceIndex++;
        parrySequenceIndex %= parryPatterns.Length;
        parryCurrentIndex = 0;
    }
    public void CrossParryFail(DamageKnockback damage)
    {
        OnParrySuccess.Invoke();
        OnBlock.Invoke();
        damage.OnBlock.Invoke();
        Actor actor = damage.source.GetComponent<Actor>();

        /*
        float otherSpeed = 0f;
        if (actor.TryGetComponent<Animancer.AnimancerComponent>(out Animancer.AnimancerComponent otherAnimancer))
        {
            otherSpeed = otherAnimancer.States.Current.Speed;
            otherAnimancer.States.Current.Speed = 0.25f;
        }
        */
        actor.DeactivateHitboxes();
        ParryFail = true;
        /*
        animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
        AnimancerState hit = animancer.Play(CrossParryHit);

        hit.Events.OnEnd = () =>
        {
            RealignToTarget();
            cstate.attack = CrossParryFollowup.ProcessHumanoidAction(this, _MoveOnEnd);
            if (otherAnimancer != null)
            {
                otherAnimancer.States.Current.Speed = otherSpeed;
            }
            crossParrying = false;
        };
        */
    }

    public void CircleParryFail(DamageKnockback damage)
    {
        OnParrySuccess.Invoke();
        OnBlock.Invoke();
        damage.OnBlock.Invoke();
        Actor actor = damage.source.GetComponent<Actor>();

        /*
        float otherSpeed = 0f;
        if (actor.TryGetComponent<Animancer.AnimancerComponent>(out Animancer.AnimancerComponent otherAnimancer))
        {
            otherSpeed = otherAnimancer.States.Current.Speed;
            otherAnimancer.States.Current.Speed = 0.25f;
        }
        */
        actor.DeactivateHitboxes();
        ParryFail = true;
        /*
        animancer.Layers[HumanoidAnimLayers.UpperBody].Stop();
        AnimancerState hit = animancer.Play(CrossParryHit);

        hit.Events.OnEnd = () =>
        {
            RealignToTarget();
            cstate.attack = CrossParryFollowup.ProcessHumanoidAction(this, _MoveOnEnd);
            if (otherAnimancer != null)
            {
                otherAnimancer.States.Current.Speed = otherSpeed;
            }
            crossParrying = false;
        };
        */
    }

    public void ParrySuccess(DamageKnockback damage, bool wasCircle)
    {
        IncrementParryIndex();
        SetParryValue();
        ParryHit = true;
    }
}
