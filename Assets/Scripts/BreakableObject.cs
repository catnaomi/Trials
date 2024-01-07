using System.Collections;
using System.Collections.Generic;
using CustomUtilities;
using UnityEngine;
using UnityEngine.Events;

public class BreakableObject : MonoBehaviour, IDamageable, IHasHealthAttribute
{
    public DamageType brokenByElements;
    public float health = -1;
    float startingHealth;
    AttributeValue healthAttribute;
    [Space(10)]
    public GameObject particlePrefab;
    public UnityEvent OnBreak;
    public UnityEvent OnFail;
    public bool recoilOnFail = true;
    DamageKnockback lastDamage;
    bool hasBeenBroken;
    [Header("Drop Item")]
    public Item[] drops;
    public GameObject dropPrefab;
    public float dropProbability = 1f;
    public int dropPrefabAmount;
    public float forceMagnitude = 1f;
    public Vector3 angularVelocity;


    void Awake()
    {
        startingHealth = health;
        UpdateHealthAttribute();
    }
    public void Recoil()
    {
        
    }

    public void StartCritVulnerability(float time)
    {
        
    }

    public void StopCritVulnerability()
    {

    }
    public void TakeDamage(DamageKnockback damage)
    {
        lastDamage = damage; 
        if (damage.GetTypes().HasType(brokenByElements))
        {
            health -= damage.healthDamage;
            if (health <= 0)
            {
                BreakObject();             
            }
            UpdateHealthAttribute();
            return;
        }
        if (recoilOnFail)
        {
            
            if (!damage.isRanged && !damage.cannotRecoil && damage.source.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.Recoil();   
            }
            damage.OnBlock.Invoke();
        }
    }

    public void BreakObject()
    {
        if (hasBeenBroken) return;
        if (particlePrefab != null)
        {
            GameObject particle = Instantiate(particlePrefab);
            particle.transform.SetPositionAndRotation(this.transform.position, this.transform.rotation);
            Destroy(particle, 5f);
        }
        DropItems();
        OnBreak.Invoke();
        Destroy(this.gameObject, 0.01f);
        hasBeenBroken = true;
    }

    public void DropItems()
    {
        List<GameObject> droppedObjects = new List<GameObject>();

        foreach (Item item in drops)
        {
            if (Random.value > dropProbability) continue;
            LooseItem loose = LooseItem.CreateLooseItem(item);
            droppedObjects.Add(loose.gameObject);
        }
        if (dropPrefab != null)
        {
            for (int i = 0; i < dropPrefabAmount; i++)
            {
                if (Random.value > dropProbability) continue;
                droppedObjects.Add(Instantiate(dropPrefab));
            }
        }

        foreach (GameObject droppedObject in droppedObjects)
        {
            droppedObject.transform.position = this.transform.position + Random.insideUnitSphere;
            if (droppedObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
            {
                rigidbody.velocity = Vector3.up * forceMagnitude;
                rigidbody.angularVelocity = angularVelocity;
                //rigidbody.AddExplosionForce(forceMagnitude, this.transform.position + Vector3.up * -0.5f, 1f);
            }
        }
    }
    public void SetHitParticleVectors(Vector3 position, Vector3 direction)
    {
        //throw new System.NotImplementedException();
    }

    public DamageKnockback GetLastTakenDamage()
    {
        return lastDamage;
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
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

    public AttributeValue GetHealth()
    {
        UpdateHealthAttribute();
        return healthAttribute;
    }

    void UpdateHealthAttribute()
    {
        if (healthAttribute == null)
        {
            healthAttribute = new AttributeValue(startingHealth, startingHealth, startingHealth);
        }
        healthAttribute.current = health;
    }

    public float GetSmoothedHealth()
    {
        return health;
    }

    public void SetHealth(float health)
    {
        this.health = health;
        UpdateHealthAttribute();
    }
}