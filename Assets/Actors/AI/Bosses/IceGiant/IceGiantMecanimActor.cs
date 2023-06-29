using Cinemachine;
using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceGiantMecanimActor : Actor, IAttacker, IDamageable
{
    [Header("Position Reference")]
    public Transform RightHand;
    public Transform LeftHand;
    public GameObject leftLeg;
    public DamageablePoint leftLegWeakPoint;
    public GameObject rightLeg;
    public DamageablePoint rightLegWeakPoint;
    public DamageablePoint weakPoint;
    [Header("Weapons")]
    public float RightWeaponLength = 1f;
    public float RightWeaponRadius = 1f;
    [Space(15)]
    public float LeftWeaponLength = 1f;
    public float LeftWeaponRadius = 1f;
    [Header("Attacks")]
    public DamageKnockback tempDamage;
    public InputAttack stepShockwave;
    public float stepShockwaveRadius = 2f;
    public InputAttack harmlessShockwave;
    public InputAttack smallShockwave;
    public InputAttack largeShockwave;
    public float shockwaveRadius = 2f;
    public InputAttack groundShockwave;
    public float groundShockwaveRadius = 25f;
    [Space(10)]
    public float nonActorGroundedThreshold = 1f;
    [Space(20)]
    public float getupDelay = 5f;
    float getupClock = 0f;
    HitboxGroup rightHitboxes;
    DamageKnockback lastTakenDamage;
    HitboxGroup leftHitboxes;
    [Header("Particles")]
    public ParticleSystem stompParticle;
    public ParticleSystem footReformParticleLeft;
    public ParticleSystem footReformParticleRight;
    [Header("Mecanim Values")]
    [ReadOnly, SerializeField] bool Dead;
    [ReadOnly, SerializeField] bool IsFallen;
    [ReadOnly, SerializeField] bool Fall;
    public override void ActorStart()
    {
        base.ActorStart();
        animator = this.GetComponent<Animator>();
        GenerateWeapons();
        leftLegWeakPoint.OnHurt.AddListener(() => TakeDamageFromDamagePoint(leftLegWeakPoint));
        rightLegWeakPoint.OnHurt.AddListener(() => TakeDamageFromDamagePoint(rightLegWeakPoint));
        weakPoint.OnHurt.AddListener(() => TakeDamageFromDamagePoint(weakPoint));
        if (TryGetComponent<AnimationFXHandler>(out AnimationFXHandler fxHandler))
        {
            fxHandler.OnStepL.AddListener(StepShockwaveLeft);
            fxHandler.OnStepR.AddListener(StepShockwaveRight);
        }
        //EnableWeakPoint(false);
    }

    public override void ActorPostUpdate()
    {
        base.ActorPostUpdate();

        if (IsFallen)
        {
            getupClock -= Time.deltaTime;
            if (getupClock <= 0f)
            {
                GetUp();
            }
        }
        UpdateMecanimValues();
    }

    void UpdateMecanimValues()
    {
        animator.SetBool("IsFallen", IsFallen);
        animator.UpdateTrigger("Fall", ref Fall);
    }
    void GenerateWeapons()
    {
        if (rightHitboxes != null)
        {
            rightHitboxes.DestroyAll();
        }
        if (leftHitboxes != null)
        {
            leftHitboxes.DestroyAll();
        }
        rightHitboxes = Hitbox.CreateHitboxLine(RightHand.position, RightHand.up, RightWeaponLength, RightWeaponRadius, RightHand, new DamageKnockback(tempDamage), this.gameObject);
        leftHitboxes = Hitbox.CreateHitboxLine(LeftHand.position, LeftHand.up, LeftWeaponLength, LeftWeaponRadius, LeftHand, new DamageKnockback(tempDamage), this.gameObject);
    }
    public DamageKnockback GetLastDamage()
    {
        return currentDamage;
    }


    public void TakeDamageFromDamagePoint(DamageablePoint point)
    {
        attributes.health.current -= point.GetLastAmountTaken();
        if (point.hasHealth && point.health.current <= 0f)
        {
            BreakDamageablePoint(point);
        }
        lastDamageTaken = point.GetLastTakenDamage();
        SetHitParticleVectors(point.GetHitPosition(), point.GetHitDirection());
        OnHurt.Invoke();
    }

    void BreakDamageablePoint(DamageablePoint point)
    {
        point.gameObject.SetActive(false);
        if (point == rightLegWeakPoint)
        {
            rightLeg.SetActive(false);
        }
        else if (point == leftLegWeakPoint)
        {
            leftLeg.SetActive(false);
        }
        FallOver();
    }

    void FixDamageablePoint(DamageablePoint point)
    {
        point.gameObject.SetActive(true);
        if (point == rightLegWeakPoint)
        {
            rightLeg.SetActive(true);
        }
        else if (point == leftLegWeakPoint)
        {
            leftLeg.SetActive(true);
        }
        point.health.current = point.health.max;
    }

    public void FallOver()
    {
        getupClock = getupDelay;
        //EnableWeakPoint(true);
        weakPoint.StartCritVulnerability(getupDelay);
        IsFallen = true;
    }

    public void GetUp()
    {
        IsFallen = false;
        //EnableWeakPoint(false);
    }
    public void EnableWeakPoint(bool active)
    {
        weakPoint.gameObject.SetActive(active);
    }
    public void HitboxActive(int active)
    {
        if (active == 1)
        {
            rightHitboxes.SetActive(true);
            leftHitboxes.SetActive(false);
        }
        else if (active == 2)
        {
            rightHitboxes.SetActive(false);
            leftHitboxes.SetActive(true);
        }
        else if (active == 0)
        {
            rightHitboxes.SetActive(false);
            leftHitboxes.SetActive(false);
        }

    }

    public void StartReformFoot()
    {
        bool isLeft = animator.GetCurrentAnimatorStateInfo(0).IsTag("STOMP_LEFT");
        ParticleSystem particle = (isLeft) ? footReformParticleLeft : footReformParticleRight;
        particle.Play();
    }

    public void ReformFoot()
    {
        bool isLeft = animator.GetCurrentAnimatorStateInfo(0).IsTag("STOMP_LEFT");
        Transform foot = (isLeft) ? leftLeg.transform : rightLeg.transform;
        DamageablePoint point = (isLeft) ? leftLegWeakPoint : rightLegWeakPoint;
        FixDamageablePoint(point);
    }
    public void Stomp(int left)
    {
        bool isLeft = animator.GetCurrentAnimatorStateInfo(0).IsTag("STOMP_LEFT");
        Transform foot = (isLeft) ? leftLeg.transform : rightLeg.transform;

        Vector3 position = foot.position;
        position.y = this.transform.position.y;
        Shockwave(position, groundShockwaveRadius, new DamageKnockback(groundShockwave.GetDamage()), true);
        Shockwave(position, shockwaveRadius, new DamageKnockback(largeShockwave.GetDamage()), true);

        stompParticle.transform.position = position;
        stompParticle.Play();
        stompParticle.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        stompParticle.GetComponent<AudioSource>().Play();
    }

    public void HandShockwaveIn()
    {
        Vector3 position = LeftHand.position;
        position.y = this.transform.position.y;
        // activate particle only
    }
    public void HandShockwaveOut()
    {
        Vector3 position = LeftHand.position;
        position.y = this.transform.position.y;
        Shockwave(position, shockwaveRadius, new DamageKnockback(harmlessShockwave.GetDamage()), false);
    }
    public void StepShockwaveLeft()
    {
        StepShockwave(-1);
    }

    public void StepShockwaveRight()
    {
        StepShockwave(1);
    }

    public void StepShockwave(int left)
    {
        bool isLeft = left == -1;
        Transform foot = (isLeft) ? leftLeg.transform : rightLeg.transform;
        Vector3 position = foot.position;
        position.y = this.transform.position.y;


        Shockwave(position, stepShockwaveRadius, new DamageKnockback(stepShockwave.GetDamage()), false);
    }

    void Shockwave(Vector3 position, float radius, DamageKnockback damage, bool groundedOnly)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius, Hitbox.GetHitboxMask());
        List<IDamageable> victims = new List<IDamageable>();

        damage.source = this.gameObject;
        foreach (Collider c in colliders)
        {
            if (c.transform.root == this.transform) continue;
            if (c.TryGetComponent<IDamageable>(out IDamageable victim))
            {
                if (victims.Contains(victim))
                {
                    continue;
                }
                else
                {
                    victims.Add(victim);
                }
            }
        }

        foreach (IDamageable v in victims)
        {
            // check if they are grounded
            if (groundedOnly)
            {
                if (v.GetGameObject().TryGetComponent<Actor>(out Actor actor) && !actor.IsGrounded())
                {
                    continue;
                }
                else if (v.GetGameObject().transform.position.y - this.transform.position.y > nonActorGroundedThreshold)
                {
                    continue;
                }
            }
            v.TakeDamage(damage);
        }
        Debug.DrawRay(position, Vector3.forward * radius, Color.red, 5f);
        Debug.DrawRay(position, Vector3.back * radius, Color.red, 5f);
        Debug.DrawRay(position, Vector3.right * radius, Color.red, 5f);
        Debug.DrawRay(position, Vector3.left * radius, Color.red, 5f);
    }

    public void TakeDamage(DamageKnockback damage)
    {
        // do nothing, never takes damage directly
    }

    public void Recoil()
    {
        
    }

    public void StartCritVulnerability(float time)
    {
        
    }

    public bool IsCritVulnerable()
    {
        return false;
    }

    public void GetParried()
    {
        
    }

    public DamageKnockback GetLastTakenDamage()
    {
        return lastDamageTaken;
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }
}
