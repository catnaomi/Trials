using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyDamageHandler : MonoBehaviour, IDamageable, IAttacker
{
    Rigidbody rigidbody;
    RigidbodyTimeTravelHandler timeTravelHandler;
    bool hasTimeTravelHandler;
    Carryable carryable;
    bool hasCarryable;
    List<DamageKnockback> timeStopDamages;
    AudioSource audioSource;
    bool hasAudio;
    public bool sparkOnContact;
    [Tooltip("Base Damage is multiplied by force!")]
    public DamageKnockback damage = DamageKnockback.GetDefaultDamage();
    DamageKnockback lastDamage;
    DamageKnockback lastDamageTaken;
    public float minimumForceForDamage = 10f;
    public bool damagesSelf;
    public float forceMultiplier = 0.001f;
    public float maximumTimeStopMagnitude = 100f;
    public float soundMinMagnitude = 500f;
    public float soundMaxMagnitude = 2000f;

    public Vector3 hitParticlePosition;
    public Vector3 hitParticleDirection;
    // Use this for initialization
    void Start()
    {
        rigidbody = this.GetComponent<Rigidbody>();
        timeTravelHandler = this.GetComponent<RigidbodyTimeTravelHandler>();
        hasTimeTravelHandler = timeTravelHandler != null;
        carryable = this.GetComponent<Carryable>();
        hasCarryable = carryable != null;
        audioSource = this.GetComponent<AudioSource>();
        hasAudio = audioSource != null;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        if (rigidbody.isKinematic) return;
        Vector3 force = collision.impulse / Time.fixedDeltaTime;
        //Debug.Log(collision.collider + "----" + force.magnitude);
        if (collision.collider.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            
            if (force.magnitude >= minimumForceForDamage)
            {
                DamageKnockback damage = new DamageKnockback(this.damage);
                damage.originPoint = collision.GetContact(0).point;
                damage.source = this.gameObject;
                damageable.TakeDamage(damage);
                if (collision.rigidbody == null)
                {
                    damage.kbForce = force;
                }
                lastDamage = damage;
            }        
        }
        if (damagesSelf && force.magnitude >= minimumForceForDamage)
        {
            DamageKnockback selfDamage = new DamageKnockback(this.damage);
            selfDamage.originPoint = collision.GetContact(0).point;
            selfDamage.source = this.gameObject;
            this.GetComponent<IDamageable>().TakeDamage(selfDamage);
        }
        if (hasAudio && force.magnitude > soundMinMagnitude)
        {
            audioSource.volume = Mathf.SmoothStep(soundMinMagnitude, soundMaxMagnitude, force.magnitude);
            audioSource.Play();
            if (sparkOnContact)
            {
                ContactPoint[] points = new ContactPoint[collision.contactCount];
                collision.GetContacts(points);
                foreach (ContactPoint point in points)
                {
                    FXController.instance.CreateFX(FXController.FX.FX_Sparks, point.point, Quaternion.identity, 1f, null);
                }
                
            }
        }
    }
    public DamageKnockback GetLastDamage()
    {
        return lastDamage;
    }

    public DamageKnockback GetLastTakenDamage()
    {
        return lastDamageTaken;
    }
    public void Recoil()
    {
        // do nothing
    }

    public void TakeDamage(DamageKnockback damage)
    {
        lastDamageTaken = damage;
        if (damage.kbForce != Vector3.zero)
        {
            if (!hasTimeTravelHandler || !timeTravelHandler.IsFrozen())
            {
                rigidbody.AddForce(damage.kbForce);
                
            }

        }
        if (sparkOnContact)
        {
            Vector3 point = this.GetComponent<Collider>().ClosestPoint(damage.originPoint);
            FXController.instance.CreateFX(FXController.FX.FX_Sparks, point, Quaternion.identity, 1f);
        }
        if (hasAudio)
        {
            audioSource.volume = 1f;
            audioSource.Play();
        }
    }

    public void StartCritVulnerability(float time)
    {
        // do nothing
    }

    public void StopCritVulnerability()
    {
        // do nothing
    }
    public void SetHitParticleVectors(Vector3 position, Vector3 direction)
    {
        
    }

    public GameObject GetGameObject()
    {
        return rigidbody.gameObject;
    }

    public void GetParried()
    {
        throw new System.NotImplementedException();
    }

    public bool IsCritVulnerable()
    {
        return false;
    }

    public void StartInvulnerability(float duration)
    {
        throw new System.NotImplementedException();
    }

    public bool IsInvulnerable()
    {
        return false; //TODO: implement invulnerability?
    }
}