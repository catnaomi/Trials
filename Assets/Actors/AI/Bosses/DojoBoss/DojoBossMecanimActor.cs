using Animancer;
using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class DojoBossMecanimActor : Actor, IDamageable, IAttacker
{

    Animator animator;
    DojoBossInventoryTransformingController inventory;
    bool isHitboxActive;
    HumanoidPositionReference positionReference;
    DojobossTimeTravelHandler timeHandler;
    CapsuleCollider collider;
    NavMeshAgent nav;
    [Header("Phases")]
    [SerializeField] CombatPhase currentPhase;
    public float timeInPhase;
    public float minPhaseDuration = 60f;
    public float maxPhaseDuration = 180f;
    int successesThisPhase;
    public int attackSuccessesNeeded = 2;
    public int parrySuccessesNeeded = 2;
    [Header("Animation Curves & Values")]
    public AnimationCurve lanceExtensionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public Vector2 lanceExtensionMinMax = Vector2.up;
    public float lanceExtensionDuration = 1f;
    [Header("Bow & Arrow")]
    public GameObject arrowPrefab;
    public GameObject homingArrowPrefab;
    public float bezierDuration;
    public float straightArrowForce;
    public float forceAssistDistanceRatio;
    public float heightAssistDistanceRatio;
    public float initialScatterHeightOffset = 1f;
    public float scatterArrowDelay = 1f;
    public int scatterArrowAmount = 10;
    public float scatterArrowMinimumRadius = 1f;
    public float scatterArrowMaximumRadius = 10f;
    public float scatterArrowMinimumOtherDistance = 1f;
    public float scatterArrowBezierDuration = 1f;
    public float scattarArrowBezierAdditionalDelay = 0.1f;
    [Tooltip("x: percentage along horizontal axis, y: height offset")]
    public Vector2[] bowBezierDistanceRatios;
    public DamageKnockback arrowDamage;
    [Header("Parries")]
    public string[] parryPatterns;
    int parryCurrentIndex;
    int parrySequenceIndex;
    int parrySequenceLength;
    bool inCritCoroutine;
    float critTime;
    float totalCritTime;
    [Header("Offense")]
    public int offensiveStageCount = 6;
    [Header("Navigation")]
    public float speed = 5f;
    [Header("Animancer")]
    public Animancer.ClipTransition playerParryFailAnim;
    public float freezeTimeout = 5f;
    
    [Header("Mecanim Values")]
    [ReadOnly, SerializeField] bool InCloseRange;
    [ReadOnly, SerializeField] bool InMeleeRange;
    [ReadOnly, SerializeField] bool AtLongRange;
    [ReadOnly, SerializeField] bool ParryHit;
    [ReadOnly, SerializeField] int NextParry;
    [ReadOnly, SerializeField] bool ParryFail;
    [ReadOnly, SerializeField] bool OnHeavyDamage;
    [ReadOnly, SerializeField] float xDirection;
    [ReadOnly, SerializeField] float yDirection;
    [ReadOnly, SerializeField] bool OnFlinch;
    [ReadOnly, SerializeField] bool Parried;
    [ReadOnly, SerializeField] int OffenseStage;
    [Space(10)]
    public float closeRange = 5f;
    public float meleeRange = 1f;
    public float longRange = 10f;
    public float randomCycleSpeed = 3f;
    [Header("Events")]
    public UnityEvent OnHitboxActive;
    public UnityEvent OnParryFail;
    float randomClock = 0f;
    bool shouldRealign;

    enum CombatPhase
    {
        AttackPhase,
        ParryPhase,
        PillarPhase,
        Idle
    }
    // Start is called before the first frame update
    public override void ActorStart()
    {
        base.ActorStart();
        animator = this.GetComponent<Animator>();
        inventory = this.GetComponent<DojoBossInventoryTransformingController>();
        positionReference = this.GetComponent<HumanoidPositionReference>();
        collider = this.GetComponent<CapsuleCollider>();
        CombatTarget = PlayerActor.player.gameObject;
        OnHitboxActive.AddListener(RealignToTarget);
        SetParryValue();
        arrowDamage.source = this.gameObject;
        timeHandler = this.GetComponent<DojobossTimeTravelHandler>();
        nav = this.GetComponent<NavMeshAgent>();
        nav.updatePosition = true;
        nav.updateRotation = true;
        StartCoroutine(DestinationCoroutine());
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
        float dist = Vector3.Distance(this.transform.position, CombatTarget.transform.position);
        InCloseRange = dist <= closeRange;
        InMeleeRange = dist <= meleeRange;
        AtLongRange = dist >= longRange;
        if (shouldRealign)
        {
            RealignToTarget();
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
        if (IsMoving())
        {
            nav.enabled = true;
            nav.updatePosition = true;
            nav.updateRotation = true;
        }
        else
        {
            nav.enabled = false;
            nav.updatePosition = false;
            nav.updateRotation = false;
        }
        CheckPhase();
        UpdateMecanimValues();
        if (randomClock <= 0f)
        {
            randomClock = randomCycleSpeed;
        }
    }


    void CheckPhase()
    {
        timeInPhase += Time.deltaTime;
        if (currentPhase == CombatPhase.AttackPhase)
        {
            if (timeInPhase >= minPhaseDuration && successesThisPhase >= attackSuccessesNeeded)
            {
                currentPhase = CombatPhase.ParryPhase;
                timeInPhase = 0f;
                successesThisPhase = 0;
            }
            else if (timeInPhase >= maxPhaseDuration)
            {
                //currentPhase = CombatPhase.PillarPhase;
                currentPhase = CombatPhase.ParryPhase;
                timeInPhase = 0f;
                successesThisPhase = 0;
            }
        }
        else if (currentPhase == CombatPhase.ParryPhase)
        {
            if (timeInPhase >= minPhaseDuration && successesThisPhase >= parrySuccessesNeeded)
            {
                currentPhase = CombatPhase.AttackPhase;
                timeInPhase = 0f;
                successesThisPhase = 0;
            }
            else if (timeInPhase >= maxPhaseDuration)
            {
                //currentPhase = CombatPhase.PillarPhase;
                currentPhase = CombatPhase.AttackPhase;
                timeInPhase = 0f;
                successesThisPhase = 0;
            }
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
        animator.SetBool("AtLongRange", AtLongRange);

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
        
        Vector3 dir = (CombatTarget.transform.position - this.transform.position).normalized;
        xDirection = Vector3.Dot(this.transform.right, dir);
        yDirection = Vector3.Dot(this.transform.forward, dir);

        animator.SetFloat("xDirection", xDirection);
        animator.SetFloat("yDirection", yDirection);

        if (OnHeavyDamage)
        {
            animator.SetTrigger("OnHeavyDamage");
            OnHeavyDamage = false;
            OnFlinch = false;
            animator.ResetTrigger("OnFlinch");
        }
        else if (OnFlinch)
        {
            animator.SetTrigger("OnFlinch");
            OnFlinch = false;
            OnHeavyDamage = false;
            animator.ResetTrigger("OnHeavyDamage");
        }
        
        if (Parried)
        {
            animator.SetTrigger("Parried");
            Parried = false;
        }
        animator.SetInteger("OffenseStage", OffenseStage);
        animator.SetInteger("CurrentPhase", (int)currentPhase);

    }


    IEnumerator DestinationCoroutine()
    {
        while (true)
        {
            if (CombatTarget != null && IsMoving())
            {
                nav.SetDestination(CombatTarget.transform.position);
            }
            yield return new WaitForSeconds(0.1f);
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
        if (this.IsTimeStopped()) return;
        if (CombatTarget != null)
        {
            Vector3 dir = (CombatTarget.transform.position - this.transform.position);
            dir.y = 0f;
            this.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(this.transform.forward, dir.normalized, 1080f * Time.deltaTime, Mathf.Infinity));
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
            if (this.IsTimeStopped())
            {
                yield return new WaitWhile(this.IsTimeStopped);
            }
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


    public void BowFireStraight()
    {
        RealignToTarget();
        Vector3 origin = positionReference.MainHand.transform.position + positionReference.MainHand.transform.parent.up * 1f;

        Vector3 targetPosition = CombatTarget.transform.position + Vector3.up * 0.5f;
        float dist = Vector3.Distance(targetPosition, origin);
        Vector3 heightAssist = Vector3.up * dist * heightAssistDistanceRatio;
        Vector3 launchVector = ((targetPosition + heightAssist) - origin).normalized;

        float force = straightArrowForce + dist * forceAssistDistanceRatio;
        ArrowController arrow = ArrowController.Launch(arrowPrefab, origin, Quaternion.LookRotation(launchVector), launchVector * force, this.transform, arrowDamage);

        ArrowAvoidColliders(arrow.gameObject);
    }
    public void BowFireHoming()
    {
        RealignToTarget();
        Vector3 origin = positionReference.MainHand.transform.position + positionReference.MainHand.transform.parent.up * 1f;

        Vector3[] controlPoints = new Vector3[bowBezierDistanceRatios.Length];

        for (int i = 0; i < bowBezierDistanceRatios.Length; i++)
        {
            Vector3 targetPos = CombatTarget.transform.position;
            targetPos.y = 0f;
            Vector3 point = Vector3.Lerp(origin, targetPos, bowBezierDistanceRatios[i].x);
            point.y += bowBezierDistanceRatios[i].y;
            controlPoints[i] = point;
        }




        BezierProjectileController arrow = BezierProjectileController.Launch(homingArrowPrefab, origin, bezierDuration, this.transform, arrowDamage, controlPoints);

        ArrowAvoidColliders(arrow.gameObject);
    }

    public void BowFireScatter()
    {
        RealignToTarget();
        Vector3 origin = positionReference.MainHand.transform.position + positionReference.MainHand.transform.parent.up * 1f;

        Vector3 launchVector = (CombatTarget.transform.position - origin).normalized + Vector3.up * initialScatterHeightOffset;

        ArrowController arrow = ArrowController.Launch(arrowPrefab, origin, Quaternion.LookRotation(launchVector), launchVector * straightArrowForce, this.transform, arrowDamage);
        ArrowAvoidColliders(arrow.gameObject);

        StartCoroutine(DelayScatter(arrow.gameObject));
    }

    IEnumerator DelayScatter(GameObject arrow)
    {

        yield return new WaitForSeconds(scatterArrowDelay);
        if (this.IsTimeStopped())
        {
            yield return new WaitWhile(this.IsTimeStopped);
        }
        Vector3 origin = arrow.gameObject.transform.position;
        Destroy(arrow);
        yield return null;
        Vector3 targetPosition = CombatTarget.transform.position;
        targetPosition.y = 0f;
        // create arrows
        Vector3[] arrowDestinations = new Vector3[scatterArrowAmount];
        BezierProjectileController[] arrows = new BezierProjectileController[scatterArrowAmount];
        List<Collider> colliders = new List<Collider>();
        int maxAttempts = 1000;
        int attempts = 0;
        for (int i = 0; i < scatterArrowAmount; i++)
        {
            bool distanceCheck = true;
            Vector3 arrowDestination = targetPosition;
            do
            {
                distanceCheck = true;
                Vector3 unitDirection = Random.onUnitSphere;
                unitDirection.y = 0f;
                unitDirection.Normalize();
                float offsetDistance = Random.Range(scatterArrowMinimumRadius, scatterArrowMaximumRadius);
                arrowDestination = targetPosition + (unitDirection * offsetDistance);
                
                if (i > 0)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (Vector3.Distance(arrowDestinations[j], arrowDestination) < scatterArrowMinimumOtherDistance)
                        {
                            distanceCheck = false;
                            attempts++;
                        }
                    }
                }
                
            } while (!distanceCheck && attempts < maxAttempts);

            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("Scatter Arrow Timed Out!");
            }
            arrowDestinations[i] = arrowDestination;

            Vector3[] controlPoints = new Vector3[bowBezierDistanceRatios.Length];

            for (int b = 0; b < bowBezierDistanceRatios.Length; b++)
            {
                Vector3 point = Vector3.Lerp(origin, arrowDestination, bowBezierDistanceRatios[b].x);
                point.y += bowBezierDistanceRatios[b].y;
                controlPoints[b] = point;
            }

            BezierProjectileController newArrow = BezierProjectileController.Launch(homingArrowPrefab, origin, scatterArrowBezierDuration + (scattarArrowBezierAdditionalDelay * i), this.transform, arrowDamage, controlPoints);
            arrows[i] = newArrow;
            colliders.AddRange(GetAllArrowColliders(newArrow.gameObject));
        }

        // ignore other arrow collision
        Collider colliderA;
        Collider colliderB;
        for (int a = 0; a < colliders.Count; a++)
        {
            colliderA = colliders[a];
            for (int b = a; b < colliders.Count; b++)
            {
                colliderB = colliders[b];
                if (colliderA.transform.root != colliderB.transform.root)
                {
                    Physics.IgnoreCollision(colliderA, colliderB);
                }
            }
        }
    }



    void ArrowAvoidColliders(GameObject arrowObject)
    {
        Collider[] arrowColliders = arrowObject.GetComponentsInChildren<Collider>();
        foreach (Collider actorCollider in this.transform.GetComponentsInChildren<Collider>())
        {
            foreach (Collider arrowCollider in arrowColliders)
            {
                Physics.IgnoreCollision(actorCollider, arrowCollider);
            }
        }
    }

    Collider[] GetAllArrowColliders(GameObject arrowObject)
    {
        List<Collider> colliders = new List<Collider>();
        colliders.AddRange(arrowObject.GetComponents<Collider>());
        colliders.AddRange(arrowObject.GetComponentsInChildren<Collider>());

        return colliders.ToArray();
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
        return this.gameObject;
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

    public override bool IsAttacking()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("ATTACK");
    }

    public override bool IsBlocking()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("BLOCK");
    }

    public bool IsMoving()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("MOVE");
    }


    public void GetParried()
    {
        Parried = true;
        HitboxActive(0);
        OnHurt.Invoke();
        StartCritVulnerability(5f);
        OffenseStage++;
        if (OffenseStage > offensiveStageCount)
        {
            OffenseStage = 3;
        }
        if (currentPhase == CombatPhase.AttackPhase)
        {
            successesThisPhase++;
        }
    }

    public void Recoil()
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(DamageKnockback damage)
    {
        if (!this.IsAlive()) return;
        lastDamageTaken = damage;
        float damageAmount = damage.GetDamageAmount();
        if (this.IsTimeStopped() || this.IsDodging())
        {
            if (this.IsTimeStopped())
            {
                TimeTravelController.time.TimeStopDamage(damage, this, damageAmount);
            }
            return;
        }
        bool isCrit = IsCritVulnerable() || damage.critData.alwaysCritical;
        damage.didCrit = isCrit;
        damageAmount = damage.GetDamageAmount(isCrit);

        damageAmount = DamageKnockback.GetTotalMinusResistances(damageAmount, damage.unresistedMinimum, damage.GetTypes(), this.attributes.resistances);

        bool isParrying = IsParrying();
        bool circleParrying = IsCircleParrying();
        bool crossParrying = IsCrossParrying();
        if (((isParrying) && damage.isRanged) || IsBlocking())
        {
            RealignToTarget();
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

            //damage.didCrit = true;
            this.OnHurt.Invoke();
            damage.OnCrit.Invoke();
            damage.OnBlock.Invoke();
            this.OnBlock.Invoke();
            //StartCritVulnerability(clip.MaximumDuration / clip.Speed);
        }
        else
        {
            
            if (IsCritVulnerable())
            {
                if (damage.stagger != DamageKnockback.StaggerStrength.Light)
                {
                    damage.stagger = DamageKnockback.StaggerStrength.Heavy;
                    OnHeavyDamage = true;
                }
                else
                {
                    OnFlinch = true;
                }
                damage.didCrit = true;
                damage.OnCrit.Invoke();
            }
            else
            {
                OnFlinch = true;
            }
            this.attributes.ReduceHealth(damageAmount);
            this.OnHurt.Invoke();
            damage.OnHit.Invoke();
        }
        DeactivateHitboxes();
    }


    public void StartCritVulnerability(float time)
    {
        if (totalCritTime >= DamageKnockback.MAX_CRITVULN_TIME) return;
        critTime = time;
        totalCritTime += time;
        if (!inCritCoroutine)
        {
            StartCoroutine(CriticalTimeOut());
        }
        OnCritVulnerable.Invoke();
    }

    public void StopCritVulnerability()
    {
        critTime = -1f;
        totalCritTime = 0f;
    }

    IEnumerator CriticalTimeOut()
    {
        inCritCoroutine = true;
        while (critTime > 0)
        {
            yield return null;
            if (!this.IsTimeStopped())
            {
                critTime -= Time.deltaTime;
            }
        }
        yield return new WaitForSeconds(1f);
        if (critTime <= 0)
        {
            totalCritTime = 0f;
        }
        inCritCoroutine = false;
    }

    public bool IsCritVulnerable()
    {
        bool isCritVuln = critTime > 0f;
        if (!isCritVuln) totalCritTime = 0f;
        return isCritVuln;
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
        StartCritVulnerability(5f);
        NextParrySequence();
        animator.ResetTrigger("OnHeavyDamage");
        if (currentPhase == CombatPhase.ParryPhase)
        {
            successesThisPhase++;
        }
        
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
        OnParryFail.Invoke();
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

        PlayPlayerParryFailState(actor);
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
        OnParryFail.Invoke();
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

        PlayPlayerParryFailState(actor);
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

    public void PlayPlayerParryFailState(Actor actor)
    {
        if (actor is PlayerActor player)
        {
            AnimancerState state = player.animancer.Play(playerParryFailAnim);
            StartCoroutine(PlayerParryFailStateRoutine(state, player));
        }
    }

    IEnumerator PlayerParryFailStateRoutine(AnimancerState state, PlayerActor player)
    {
        yield return new WaitForSeconds(freezeTimeout);
        if (player.animancer.States.Current == state)
        {
            player.ResetAnim();
        }
    }

    public void ParrySuccess(DamageKnockback damage, bool wasCircle)
    {
        IncrementParryIndex();
        SetParryValue();
        ParryHit = true;
        OnParrySuccess.Invoke();
    }

    void OnAnimatorMove()
    {
        
        if (IsAttacking())
        {
            Vector3 diff = animator.rootPosition - this.transform.position;
            Vector3 dirToTarget = (CombatTarget.transform.position - this.transform.position);
            dirToTarget.y = 0f;
            dirToTarget.Normalize();

            diff = Vector3.Project(diff, dirToTarget);
            float startingMagnitude = diff.magnitude * Mathf.Sign(Vector3.Dot(dirToTarget, diff));
            float distanceAfterMovement = Vector3.Distance(this.transform.position + diff, CombatTarget.transform.position);
            float minimumDistance = Mathf.Max(inventory.GetCurrentLength(),2f);
            if (distanceAfterMovement < minimumDistance)
            {
                diff = diff.normalized * (distanceAfterMovement - minimumDistance) * Time.deltaTime;
                //diff = Vector3.ClampMagnitude(diff, minimumDistance - distanceAfterMovement);
                float endMagnitude = diff.magnitude * Mathf.Sign(Vector3.Dot(dirToTarget, diff));
                Debug.Log($"adjusted root motion movement: {startingMagnitude} vs {endMagnitude}");
            }
            this.transform.position += diff;
        }
        else
        {
            this.transform.position = animator.rootPosition;
        }

    }
    public override bool IsTimeStopped()
    {
        return timeHandler != null && timeHandler.IsFrozen();
    }
}
