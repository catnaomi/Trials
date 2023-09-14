using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IceShieldGolemMecanimcActor : Actor, IAttacker, IDamageable
{
    HumanoidNPCInventory inventory;
    CapsuleCollider collider;
    CharacterController cc;
    HumanoidPositionReference positionReference;
    ActorTimeTravelHandler timeTravelHandler;

    public bool isHitboxActive;
    public UnityEvent OnHitboxActive;

    [Header("Mecanim Values")]
    [ReadOnly, SerializeField] bool InCloseRange;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(DamageKnockback damage)
    {
        throw new System.NotImplementedException();
    }

    public void Recoil()
    {
        throw new System.NotImplementedException();
    }

    public void StartCritVulnerability(float time)
    {
        throw new System.NotImplementedException();
    }

    public bool IsCritVulnerable()
    {
        throw new System.NotImplementedException();
    }

    public void GetParried()
    {
        throw new System.NotImplementedException();
    }

    public DamageKnockback GetLastTakenDamage()
    {
        throw new System.NotImplementedException();
    }

    public GameObject GetGameObject()
    {
        throw new System.NotImplementedException();
    }
}
